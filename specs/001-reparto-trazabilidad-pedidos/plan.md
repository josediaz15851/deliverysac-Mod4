# Implementation Plan: Reparto y Trazabilidad de Pedidos (DeliverySac)

**Branch**: `001-reparto-trazabilidad-pedidos` | **Date**: 2026-10-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-reparto-trazabilidad-pedidos/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

DeliverySac es un sistema web (PWA mobile-first) para reparto y trazabilidad de pedidos. Un
Administrador registra clientes y pedidos, los asigna a repartidores y unidades; el Repartidor ve su
ruta del día en el celular, marca estados (con cola offline y reintento) y el sistema registra de
forma inmutable la hora y el usuario de cada cambio; el Administrador y el Supervisor consultan el
listado del día. El enfoque técnico usa el stack fijado por `AGENTS.md`: backend .NET 8 Web API con
EF Core para comandos y Dapper para consultas sobre SQL Server Express, frontend Angular con las
plantillas PrimeNG/Sakai, y pruebas xUnit bajo ciclo rojo-verde-refactor.

## Technical Context

**Language/Version**: C# / .NET 8 (LTS) en el backend; TypeScript sobre Angular (última estable
soportada) en el frontend.

**Primary Dependencies**:
- Backend: ASP.NET Core 8 Web API, Entity Framework Core 8 (comandos/escritura), Dapper (consultas/
  lectura), SQL Server Express, `BCrypt.Net-Next` (hash de contraseñas), autenticación JWT Bearer.
- Frontend: Angular (última estable), PrimeNG (versión compatible), plantilla oficial Sakai
  (`primefaces/sakai-ng` con los módulos `app-layout`, `app-topbar`, `app-sidebar`, `app-footer`),
  PrimeFlex, PrimeIcons, `@angular/pwa` (service worker) para soporte offline (RF-11).
  Componentes PrimeNG por patrón (PRD §7.2): `p-autoComplete` (búsqueda en servidor, debounce 300 ms),
  `p-dialog` (modal centrado, width 450 px), `p-sidebar` (panel lateral derecho, width 450 px),
  `p-confirmDialog` (vía `ConfirmationService`, `severity="danger"`), `p-table` (paginación lazy
  `page`/`size`), `p-toast` (vía `MessageService`), `p-skeleton`, `p-button`.
  Fuentes del stack: `AGENTS.md` (Stack y UI/UX) y `PRD.md` (§7.1–§7.5, RF-UX-17/18/19).
- Tests: xUnit + `Microsoft.AspNetCore.Mvc.Testing` (integración) en backend.

**Storage**: SQL Server Express. Conexión por variable de entorno `DELIVERYSAC_CONNECTION` o archivo
local no versionado (`backend/src/DeliverySac.Api/appsettings.Development.local.json`).

**Testing**: `dotnet test` (xUnit). Pruebas unitarias y de integración (WebApplicationFactory).
Frontend: pruebas con el runner por defecto de Angular (`ng test`) y verificación de accesibilidad/
responsive en el flujo de validación (quickstart).

**Target Platform**: Servidor Windows/Linux (backend); navegadores modernos de escritorio y móviles
(>= 320px) con soporte PWA (frontend).

**Project Type**: Web application (frontend Angular + backend API).

**Performance Goals**: 95% de las peticiones HTTP <= 2 s en condiciones normales de red (SC-002);
refresco del panel del administrador cada 5 s (SC-003); sincronización offline <= 10 s tras recuperar
señal (SC-004); ocultamiento del banner "SIN CONEXIÓN" en < 2 s (SC-005).

**Constraints**: Sin dependencias de servicios externos de terceros (FR-028/RNF-05); HTTPS con TLS
1.2+ (FR-027); contraseñas con bcrypt coste >= 10 (FR-026); fechas de reparto/asignación en
`yyyy-MM-dd` hora America/Lima sin conversión a UTC (FR-029); UI exclusivamente con componentes
PrimeNG y tema propio (FR-030, FR-038); sin scroll horizontal desde 320px (FR-037/SC-006); trazabilidad
inmutable (FR-017).
- **Tema y tokens (PRD §7.1)**: consumir solo variables CSS globales de Sakai/PrimeNG
  (`--primary-color`, `--surface-card`, `--text-color`); prohibido hardcodear hexadecimales o estilos
  inline. Primario institucional `#1976D2`; etiquetas de color por estado (Tag).
- **Feedback y estados (PRD §7.3/§7.5)**: `MessageService`/`p-toast` para todo evento de API
  (prohibido `alert()`); `p-skeleton`/`[loading]` en cargas; estados vacío ("No se encontraron
  registros") y error definidos de forma consistente.
- **Interacción modal (PRD §7.3/§7.4)**: Enter ejecuta el guardado principal y Esc cancela en
  `p-dialog`/`p-sidebar`; foco en el primer input; `label for` asociado y anillo de foco visible.

**Scale/Scope**: Operación diaria de ~5 unidades (3 motos lineales, 2 furgonetas) y un administrador
operativo. 3 roles, ~6 pantallas CRUD/listado + ruta móvil del repartidor; volumen de datos bajo
(decenas de pedidos por día) con paginación `page`/`size` en todos los listados.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Gate | Estado en el plan |
|-----------|------|-------------------|
| I. Spec-First & Test-First (TDD) | Cada comportamiento tiene spec + pruebas xUnit escritas antes (rojo-verde-refactor). | PASS — el plan define estrategia de pruebas por capa y el flujo exige `dotnet test` antes de commitear. |
| II. Aislamiento Absoluto de IA | Ninguna lógica de IA mezclada con negocio; el PRD no incluye funciones de IA. | PASS — no se introduce ningún componente de IA; el alcance se limita a reglas determinísticas. |
| III. Inmutabilidad y Zona Horaria (America/Lima) | Trazas inmutables; fechas en America/Lima. | PASS — no hay endpoints de edición/borrado de trazas (FR-017); fechas `DateOnly` y marcas `DateTimeOffset` con conversión a Lima. |
| IV. Cero Secretos en el Código | Sin credenciales/tokens/URLs hardcodeadas. | PASS — conexión y secretos por variables de entorno / archivo local no versionado; URL base de API por `environment` (FR-025). |
| V. Alineación Estricta al PRD | Sin funcionalidad fuera del PRD. | PASS — el plan cubre exactamente RF-01..RF-14, RNF-01..06 y RF-UX-01..19; fuera de alcance respetado. |

No se identifican violaciones; **Complexity Tracking** queda sin entradas.

## Project Structure

### Documentation (this feature)

```text
specs/001-reparto-trazabilidad-pedidos/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── http-api.md          # Contratos REST (endpoints, payloads, códigos de estado)
│   └── ui-ux-patterns.md    # Contrato UI: matriz PrimeNG/Sakai, tokens, feedback, a11y
├── checklists/          # Checklists de calidad (requirements.md, spec-quality.md)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── DeliverySac.sln
├── src/
│   └── DeliverySac.Api/
│       ├── Controllers/          # Auth, Clientes, Pedidos, Asignaciones, Repartidor, Catalogos
│       ├── Domain/               # Entidades, enums (EstadoPedido), reglas de la máquina de estados
│       ├── Application/          # Casos de uso / servicios (comandos con EF Core)
│       ├── Infrastructure/       # DbContext (EF Core), consultas Dapper, repositorios, seed
│       ├── Security/             # JWT, hashing bcrypt, autorización por rol
│       ├── Contracts/            # DTOs de request/response
│       └── Program.cs
└── tests/
    └── DeliverySac.Tests/
        ├── Unit/                 # Reglas de negocio (máquina de estados, totales, validaciones)
        └── Integration/          # WebApplicationFactory: endpoints, 400/403/405, paginación

frontend/
├── src/
│   ├── app/
│   │   ├── core/                 # AuthService, ConfigService (apiUrl por entorno), HttpInterceptor, OfflineService
│   │   ├── layout/               # Shell Sakai: app-topbar, app-sidebar, app-footer
│   │   ├── features/
│   │   │   ├── auth/             # Login split-screen glassmorphism
│   │   │   ├── clientes/         # listado + alta/edición (pantallas/modales separados)
│   │   │   ├── pedidos/          # listado gestión + crear pedido (con líneas)
│   │   │   ├── asignacion/       # asignación/reasignación (candidatos PENDIENTE/REPROGRAMADO)
│   │   │   ├── listado-dia/      # panel admin/supervisor + indicador sin conexión
│   │   │   └── repartidor/       # ruta del día, una columna, mobile-first
│   │   └── shared/               # componentes PrimeNG reutilizables (etiquetas de estado, selectores)
│   ├── environments/             # environment.ts / environment.prod.ts (URL base de API)
│   └── theme/                    # tema PrimeNG personalizado (tokens, primario #1976D2)
└── src/manifest.webmanifest + service worker (PWA)

docs/
└── (documentación del proyecto)
```

**Structure Decision**: Aplicación web con dos proyectos independientes: `backend/` (solución .NET 8
`DeliverySac.sln` con `src/DeliverySac.Api` y `tests/DeliverySac.Tests`) y `frontend/` (proyecto
Angular con plantilla Sakai y PWA). Esta separación coincide con el layout referenciado en `AGENTS.md`
(`backend/src/DeliverySac.Api/...`) y en la spec (frontend con configuración por entorno).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No hay violaciones de la constitución; no aplica.
