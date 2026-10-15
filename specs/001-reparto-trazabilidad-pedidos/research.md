# Phase 0 Research: Reparto y Trazabilidad de Pedidos (DeliverySac)

**Feature**: `001-reparto-trazabilidad-pedidos` | **Date**: 2026-10-14

Todas las decisiones respetan el stack fijado en `AGENTS.md` y la constitución del proyecto. No
quedan `NEEDS CLARIFICATION` en el Technical Context del plan.

## R1. Estilo de API (controladores vs minimal APIs)

- **Decision**: Usar controladores ASP.NET Core (`[ApiController]`) organizados por recurso.
- **Rationale**: El PRD exige códigos de estado específicos (400/403/405) y autorización por rol; los
  controladores con filtros/atributos (`[Authorize(Roles=...)]`) hacen explícito y testeable el
  comportamiento. Facilita agrupar por recurso (clientes, pedidos, asignaciones, repartidor).
- **Alternatives considered**: Minimal APIs (menos ceremonia, pero las reglas de autorización y
  formato de errores quedan más dispersas).

## R2. Separación de lectura/escritura (EF Core vs Dapper)

- **Decision**: EF Core para comandos (altas, cambios de estado) y Dapper para consultas (listados
  paginados y proyecciones).
- **Rationale**: `AGENTS.md` lo establece explícitamente. Dapper ofrece consultas ligeras y paginadas
  eficientes; EF Core gestiona el modelo de escritura y el mapeo de entidades.
- **Alternatives considered**: EF Core para todo (consultas más pesadas y menos control del SQL de
  paginación).

## R3. Autenticación y autorización

- **Decision**: Autenticación con usuario/contraseña que emite un JWT Bearer con el rol en los
  claims; contraseñas con `BCrypt.Net-Next` (coste >= 10); autorización por rol en endpoints.
- **Rationale**: Cumple FR-001/FR-002/FR-026 sin dependencias externas (FR-028). El token es
  stateless y el cliente lo envía en `Authorization: Bearer`.
- **Alternatives considered**: Cookies de sesión (válido, pero JWT simplifica el consumo desde la PWA
  y las pruebas de integración).

## R4. Formato de paginación

- **Decision**: Parámetros de consulta `page` (1-based) y `size`; respuesta con envoltura
  `{ items, page, size, total }`.
- **Rationale**: Cubre FR-021/FR-012 del PRD y SC-010. Uniforme para todos los listados.
- **Alternatives considered**: Paginación por cursor (innecesaria para el volumen bajo).

## R5. Máquina de estados del pedido

- **Decision**: La transición `PENDIENTE → ASIGNADO → EN_RUTA → (ENTREGADO | NO_ENTREGADO |
  REPROGRAMADO)` se valida en el dominio (backend) y se rechaza con HTTP 400 toda transición
  inválida; `ENTREGADO` es terminal.
- **Rationale**: FR-013/FR-015; concentra las reglas en un único punto testeable con xUnit.
- **Alternatives considered**: Validación solo en frontend (insegura; el PRD exige rechazo en
  backend).

## R6. Inmutabilidad de la trazabilidad

- **Decision**: La tabla de cambios de estado es append-only; no existen endpoints de edición ni
  borrado. Cualquier intento de modificar hora/usuario existente responde HTTP 405.
- **Rationale**: FR-016/FR-017 y Principio III de la constitución. La hora y el usuario se asignan en
  el servidor.
- **Alternatives considered**: Actualización con auditoría (prohibida por el PRD).

## R7. Zona horaria y fechas

- **Decision**: `fechaReparto` y `fechaAsignacion` como `DateOnly` (formato `yyyy-MM-dd`, hora local
  America/Lima, sin conversión a UTC). Las marcas de tiempo de trazabilidad se almacenan como
  `DateTimeOffset` (UTC) y se muestran en America/Lima con locale `es-PE`.
- **Rationale**: FR-029/RNF-06 y AC-23 evitan el desfase de un día; el locale cubre FR-039.
- **Alternatives considered**: Guardar fechas como `DateTime` UTC (riesgo de corrimiento de día,
  explícitamente prohibido).

## R8. Cola offline y reintento (RF-011)

- **Decision**: PWA con `@angular/pwa`; una cola de operaciones persistida en IndexedDB en el
  navegador; reintento automático en segundo plano (evento `online` + intervalo) con backoff. Cada
  cambio de estado lleva un identificador de operación idempotente generado en el cliente.
- **Rationale**: FR-022/SC-004. El id de operación permite deduplicar reintentos sin duplicar trazas.
- **Alternatives considered**: Solo `localStorage` (límites y bloqueo del hilo); app nativa (fuera de
  alcance).

## R9. Resolución de conflictos offline

- **Decision**: El servidor es la fuente de verdad. Si un cambio encolado es rechazado (el pedido ya
  avanzó, fue reasignado o llegó a estado final), el cliente descarta la operación local, notifica al
  repartidor con Toast y refresca el estado real del pedido desde el servidor.
- **Rationale**: Clarificación registrada en el spec (RF-022). Evita sobrescribir trazabilidad válida.
- **Alternatives considered**: Last-write-wins (peligroso para la trazabilidad); reintento indefinido
  (bloquea la cola).

## R10. Selección de entidades (sin IDs)

- **Decision**: Endpoints de búsqueda por nombre (`/api/clientes/buscar?q=`, `/api/repartidores`,
  `/api/unidades`) consumidos por autocompletar con debounce 300 ms; modal con tabla paginada +
  búsqueda cuando se requiere explorar. Combos solo para conjuntos pequeños y fijos.
- **Rationale**: FR-031 y RF-UX-07/17; AC-UX-02 prohíbe capturar IDs.
- **Alternatives considered**: Enviar el catálogo completo al cliente (no escala y dificulta la
  búsqueda en servidor).

## R11. Configuración por entorno (frontend)

- **Decision**: `environment.ts` (dev, `http://localhost:5210`) y `environment.prod.ts`; un
  `ConfigService` inyectable expone `apiUrl`; un interceptor HTTP antepone la base URL. Sin URLs
  hardcodeadas en componentes.
- **Rationale**: FR-025 y AC-20/AC-21.
- **Alternatives considered**: Variable global en el código (prohibido por la nota del PRD).

## R12. Datos base y secretos

- **Decision**: En Development se siembran usuarios, unidades y un cliente base leyendo
  `DELIVERYSAC_ADMIN_PASSWORD`. La cadena de conexión proviene de `DELIVERYSAC_CONNECTION` o de
  `appsettings.Development.local.json` (no versionado).
- **Rationale**: `AGENTS.md` y Principio IV. No se versionan credenciales.
- **Alternatives considered**: Sembrado manual (menos reproducible).

## R13. Estrategia de pruebas (TDD)

- **Decision**: xUnit con dos capas: `Unit` (máquina de estados, cálculo de total, validaciones de
  líneas) e `Integration` (`WebApplicationFactory`: login por rol, 400/403/405, paginación,
  trazabilidad). Cada comportamiento se prueba primero (rojo) antes de implementarse.
- **Rationale**: Principio I de la constitución; `dotnet test` es el comando de verificación.
- **Alternatives considered**: Solo pruebas de integración (más lentas y menos precisas para reglas).

## R14. Tema y layout institucional

- **Decision**: Plantilla Sakai con tema PrimeNG personalizado (tokens; primario `#1976D2`), barra
  superior + menú lateral colapsable para admin/supervisor, y layout de una columna para el
  repartidor. Etiquetas de color por estado (Tag de PrimeNG).
- **Rationale**: FR-030/FR-036/FR-037 y RNF-UX-01/03.
- **Alternatives considered**: Tema por defecto de PrimeNG (prohibido por `AGENTS.md`).

## R15. Asignación activa única

- **Decision**: Índice único filtrado en la base (por pedido, cuando la asignación está activa) para
  garantizar una sola asignación activa; la reasignación invalida la anterior en una transacción.
- **Rationale**: FR-008/FR-009; previene inconsistencias por concurrencia.
- **Alternatives considered**: Validación solo por aplicación (vulnerable a condiciones de carrera).

## R16. Manejo uniforme de errores de API

- **Decision**: Respuestas de error con forma uniforme (`application/problem+json` /
  ProblemDetails) y códigos 400/401/403/404/405 según corresponda; el frontend los traduce a Toast.
- **Rationale**: FR-032 y RF-UX-18; facilita pruebas de integración.
- **Alternatives considered**: Formatos ad-hoc por endpoint (inconsistente).

## R17. Matriz de contenedores UI (PRD §7.2)

- **Decision**: Mapear cada operación a un patrón fijo de PrimeNG:
  - Selección con alta carga (Clientes, Unidades, Repartidores) → `p-autoComplete` (búsqueda en
    servidor, debounce 300 ms) o modal con `p-table` paginada; prohibido el ingreso manual de IDs.
  - Altas y formularios cortos (≤ 4 campos) → `p-dialog` modal centrado (width 450 px), foco en el
    primer input, opción "Guardar y crear otro" (`p-checkbox`).
  - Edición y formularios extensos (> 5 campos) → `p-sidebar` lateral derecho (width 450 px)
    manteniendo el listado visible.
  - Acciones irreversibles (reasignar, marcar estado final) → `p-confirmDialog` vía
    `ConfirmationService` con botón `severity="danger"`.
  - Listados principales → `p-table` en tarjeta Sakai (`.card`) con paginación lazy `page`/`size`.
- **Rationale**: PRD §7.2 y AGENTS.md (una pantalla por función, listados y formularios separados);
  uniformiza la implementación y las pruebas de UI.
- **Alternatives considered**: Contenedores ad-hoc por pantalla (prohibido por el PRD).

## R18. Tema, tokens y notificaciones (PRD §7.1/§7.3/§7.5)

- **Decision**: Definir un tema PrimeNG propio sobre Sakai con tokens (`--primary-color`,
  `--surface-card`, `--text-color`), primario `#1976D2` y etiquetas de color por estado (Tag). Todo
  feedback de API se emite con `MessageService` (`p-toast`); las cargas usan `p-skeleton` y
  `[loading]` en `p-button`; estados vacío y error son consistentes. Prohibido `alert()` y estilos
  inline/hex hardcodeados.
- **Rationale**: FR-030/FR-032/FR-033/FR-036; PRD §7.1/§7.3/§7.5 y AGENTS.md (tema propio, feedback
  uniforme, estados de carga/vacío).
- **Alternatives considered**: Tema PrimeNG por defecto (prohibido); diálogos nativos (prohibido).

## R19. Accesibilidad y atajos de teclado (PRD §7.3/§7.4)

- **Decision**: Enter ejecuta la acción principal de guardado y Esc cancela/cierra en `p-dialog` y
  `p-sidebar`; etiquetas `<label for>` asociadas, anillo de foco del tema preservado y navegación por
  teclado.
- **Rationale**: FR-034 y RF-UX-19; AC-UX-01/AC-UX-04; permite pruebas de accesibilidad en el flujo
  de validación.
- **Alternatives considered**: Manejo de teclado no uniforme (inconsistente con el PRD).
