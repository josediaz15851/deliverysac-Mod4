---
description: "Task list for DeliverySac - Reparto y Trazabilidad de Pedidos"
---

# Tasks: Reparto y Trazabilidad de Pedidos (DeliverySac)

**Input**: Design documents from `/specs/001-reparto-trazabilidad-pedidos/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Incluidos. La constitución (Principio I, Spec-First & Test-First) es NON-NEGOTIABLE, por lo
que cada implementación (front y back) se precede de su prueba (rojo → verde → refactor).

**Organization**: Tareas agrupadas por historia de usuario (US1–US6). **Orden por pantalla: primero
el front (con su prueba), luego el back de esa misma pantalla.** Tareas pequeñas y verificables.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: US1..US6 (trazabilidad a `spec.md`)
- Cada tarea incluye rutas de archivo exactas

## Path Conventions

- Web app: `backend/src/DeliverySac.Api/`, `backend/tests/DeliverySac.Tests/`, `frontend/src/app/`
- Backend: .NET 8 + EF Core (comandos) + Dapper (consultas) + SQL Server Express; tests xUnit
- Frontend: Angular + PrimeNG + plantilla Sakai + PrimeFlex + PWA; tests con el runner de Angular

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicialización de proyectos y estructura base.

- [x] T001 Crear solución backend `backend/DeliverySac.sln` con `src/DeliverySac.Api` (Web API .NET 8) y `tests/DeliverySac.Tests` (xUnit).
- [x] T002 Crear carpetas backend `Controllers/`, `Domain/`, `Application/`, `Infrastructure/`, `Security/`, `Contracts/` en `backend/src/DeliverySac.Api/`.
- [x] T003 [P] Inicializar frontend Angular (última estable) en `frontend/` y borrar el boilerplate inicial de demo.
- [x] T004 [P] Instalar y configurar PrimeNG + PrimeFlex + PrimeIcons y base de layout Sakai (`app-layout`, `app-topbar`, `app-sidebar`, `app-footer`) en `frontend/`.
- [x] T005 [P] Configurar `@angular/pwa` (manifest + service worker) en `frontend/`.
- [x] T006 Personalizar el tema PrimeNG (tokens `--primary-color`/`--surface-card`/`--text-color`, primario `#1976D2`) en `frontend/src/theme/`.
- [x] T007 [P] Configurar xUnit + `Microsoft.AspNetCore.Mvc.Testing` y dejar una prueba trivial en verde en `backend/tests/DeliverySac.Tests/`.
- [x] T008 [P] Configurar `.gitignore` (raíz) y formato/lint en `backend/DeliverySac.sln` y `frontend/angular.json`; habilitar el runner de tests del front (`ng test`).

**Checkpoint**: Proyectos compilan (`dotnet build`, `npm run build`) y runners de test operativos.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestructura común que bloquea TODAS las historias.

**⚠️ CRITICAL**: Ninguna historia comienza hasta completar esta fase.

- [x] T009 [P] Crear entidades de dominio y enums `EstadoPedido`, `RolUsuario`, `TipoUnidad` en `backend/src/DeliverySac.Api/Domain/`.
- [x] T010 Crear `DeliverySacDbContext` (EF Core) con DbSets y configuraciones (únicos y filtrados del `data-model.md`) en `backend/src/DeliverySac.Api/Infrastructure/`.
- [x] T011 Crear migración inicial y aplicarla a SQL Server Express (conexión por `DELIVERYSAC_CONNECTION`) en `backend/src/DeliverySac.Api/Infrastructure/Migrations/`.
- [x] T012 Sembrar datos base en Development (usuarios por rol, 3 motos lineales + 2 furgonetas, un cliente) leyendo `DELIVERYSAC_ADMIN_PASSWORD` en `backend/src/DeliverySac.Api/Infrastructure/Seed/`.
- [x] T013 Configurar `appsettings` y lectura de secreto por entorno sin credenciales versionadas en `backend/src/DeliverySac.Api/Program.cs`.
- [x] T014 Configurar middleware de errores uniforme (ProblemDetails 400/401/403/404/405) y CORS en `backend/src/DeliverySac.Api/Program.cs`.
- [x] T015 Implementar hashing bcrypt (coste >= 10) y autenticación/autorización JWT por rol en `backend/src/DeliverySac.Api/Security/`.
- [x] T016 Configurar `environment.ts`/`environment.prod.ts` + `ConfigService` con `apiUrl` (sin URLs hardcodeadas) y locales `es-PE` (FR-039) en `frontend/src/app/core/`.
- [x] T017 [P] Crear `AuthService` + `authInterceptor` (Bearer) + guard por rol en `frontend/src/app/core/` (test en T023).
- [x] T018 Configurar `MessageService`/`<p-toast>` y `ConfirmationService`/`p-confirmDialog` globales en `frontend/src/app/layout/`.
- [x] T019 [P] **Front test**: componente `data-state` (skeleton en carga, estado vacío "No se encontraron registros", contenido con datos) en `frontend/src/app/shared/data-state/data-state.component.spec.ts` (debe FALLAR primero).
- [x] T020 **Front**: componente compartido `data-state` (carga/vacío, FR-033) reutilizable en `frontend/src/app/shared/data-state/`.
- [x] T021 Implementar shell Sakai (`app-topbar`, `app-sidebar` colapsable, `app-footer`) con título de página + breadcrumb y resaltado de sección activa en `frontend/src/app/layout/`.
- [x] T022 Configurar routing base y menú lateral por rol (una opción por función/CRUD) en `frontend/src/app/app.routes.ts`.

**Checkpoint**: Shell navegable con tema, apiUrl por entorno, componente `data-state` y `dotnet test` verde. Historias habilitadas.

---

## Phase 3: User Story 1 - Autenticación por rol y sesión (Priority: P1) 🎯 MVP

**Goal**: Login usuario/contraseña con redirección por rol, topbar con usuario/rol y cierre de sesión.

**Independent Test**: Iniciar sesión con cada rol y verificar redirección; "Cerrar sesión" invalida la sesión (AC-01, AC-24).

- [x] T023 [US1] **Front test**: `AuthService` (guarda token, maneja 401) y componente de login (validación, error inline) en `frontend/src/app/features/auth/login/login.component.spec.ts` (debe FALLAR primero).
- [x] T024 [US1] **Front**: pantalla de login split-screen glassmorphism (tabs por rol, validación, error inline) en `frontend/src/app/features/auth/login/`.
- [x] T025 [US1] **Front**: conectar formulario de login a `AuthService` (guarda token, maneja 401 con Toast) en `frontend/src/app/features/auth/login/` — cableado preparado para el back de T027.
- [x] T026 [P] [US1] **Test back**: integración de login (credenciales válidas → token con rol; inválidas → 401) en `backend/tests/DeliverySac.Tests/Integration/AuthEndpointsTests.cs` (debe FALLAR primero).
- [x] T027 [US1] **Back**: endpoint `POST /api/auth/login` (JWT con rol, 401) en `backend/src/DeliverySac.Api/Controllers/AuthController.cs`.
- [x] T028 [US1] **Front**: mostrar usuario/rol en topbar y acción "Cerrar sesión" (limpia credenciales, redirige a login; sin retroceso) en `frontend/src/app/layout/`.

**Checkpoint**: US1 funcional de punta a punta y testeable independientemente.

---

## Phase 4: User Story 2 - Clientes y pedidos con total (Priority: P1)

**Goal**: Alta de clientes y creación de pedidos con líneas y total calculado.

**Independent Test**: Registrar cliente; crear pedido (2 × 2.50 + 1 × 7.00 → total 12.00); pedido inválido → HTTP 400 (AC-02/03/04/22).

- [x] T029 [US2] **Front test**: listado de clientes (`p-table` lazy) y formulario de alta (validación `nombre`/`direccion` obligatorios) en `frontend/src/app/features/clientes/clientes-list/clientes-list.component.spec.ts` (debe FALLAR primero).
- [x] T030 [US2] **Front**: listado de clientes con `p-table` lazy (`page`/`size`) en `frontend/src/app/features/clientes/clientes-list/`.
- [x] T031 [US2] **Front**: alta/edición de cliente en `p-dialog` (modal 450 px, foco primer input) en `frontend/src/app/features/clientes/cliente-form/`.
- [x] T032 [P] [US2] **Test back**: integración de clientes (`POST /api/clientes`; `nombre`/`direccion` obligatorios → 400) en `backend/tests/DeliverySac.Tests/Integration/ClientesEndpointsTests.cs` (debe FALLAR primero).
- [x] T033 [US2] **Back**: endpoints `POST /api/clientes`, `GET /api/clientes` (paginado) y `GET /api/clientes/buscar?q=` (autocompletar) en `backend/src/DeliverySac.Api/Controllers/ClientesController.cs`.
- [x] T034 [US2] **Front test**: formulario de pedido (líneas dinámicas, total, selección de cliente sin campos de ID) en `frontend/src/app/features/pedidos/pedido-form/pedido-form.component.spec.ts` (debe FALLAR primero).
- [x] T035 [US2] **Front**: pantalla crear pedido con líneas dinámicas y selección de cliente por autocompletar (sin campos de ID) en `frontend/src/app/features/pedidos/pedido-form/`.
- [x] T036 [P] [US2] **Test back**: integración AC-03/AC-04/AC-22 (rechazo HTTP 400, total exacto, pedido PENDIENTE en gestión) en `backend/tests/DeliverySac.Tests/Integration/PedidosEndpointsTests.cs` (debe FALLAR primero).
- [x] T037 [P] [US2] **Test back**: unit de validación de líneas (`cantidad > 0`, `precioUnitario >= 0`, `descripcion` obligatoria) y cálculo del total en `backend/tests/DeliverySac.Tests/Unit/PedidoTests.cs` (debe FALLAR primero).
- [x] T038 [US2] **Back**: endpoint `POST /api/pedidos` (rechaza con HTTP 400 sin líneas o líneas inválidas; calcula `total` = Σ(cantidad × precioUnitario)) en `backend/src/DeliverySac.Api/Controllers/PedidosController.cs`.
- [x] T039 [US2] **Back**: endpoints `GET /api/pedidos` (gestión paginada, incluye `PENDIENTE` sin asignación) y `GET /api/pedidos/{id}` (cabecera + líneas + total) en `backend/src/DeliverySac.Api/Controllers/PedidosController.cs`.
- [x] T040 [US2] **Front test**: listado de gestión de pedidos (cliente, fecha, estado como Tag, total) en `frontend/src/app/features/pedidos/pedidos-list/pedidos-list.component.spec.ts` (debe FALLAR primero).
- [x] T041 [US2] **Front**: listado de gestión de pedidos (cliente, fecha de reparto, estado como Tag, total) en `frontend/src/app/features/pedidos/pedidos-list/`.

**Checkpoint**: US1 y US2 funcionales e independientes.

---

## Phase 5: User Story 3 - Asignación, ruta y cambio de estado trazable (Priority: P1)

**Goal**: Asignar pedido a repartidor/unidad, ruta del día y cambio de estado con traza inmutable.

**Independent Test**: Asignar pedido → aparece solo en la ruta del repartidor; recorrer
`ASIGNADO → EN_RUTA → ENTREGADO` con hora/usuario; transiciones inválidas y accesos ajenos rechazados
(AC-05/06/07/07a/09/10/11/14/15).

- [x] T042 [US3] **Front test**: pantalla de asignación (selección de repartidor/unidad, listado de candidatos `PENDIENTE`/`REPROGRAMADO`) en `frontend/src/app/features/asignacion/asignacion-form/asignacion-form.component.spec.ts` (debe FALLAR primero).
- [x] T043 [US3] **Front**: pantalla de asignación con selección de repartidor/unidad (autocompletar/combo para conjunto pequeño) y listado de candidatos en `frontend/src/app/features/asignacion/asignacion-form/`.
- [x] T044 [US3] **Front**: confirmación (`p-confirmDialog`) al reasignar y Toast de resultado en `frontend/src/app/features/asignacion/asignacion-form/`.
- [x] T045 [P] [US3] **Test back**: integración de asignación (única activa; `POST /api/pedidos/{id}/asignaciones` → 400 si el estado no es `PENDIENTE` ni `REPROGRAMADO`) en `backend/tests/DeliverySac.Tests/Integration/AsignacionesEndpointsTests.cs` (debe FALLAR primero).
- [x] T046 [US3] **Back**: endpoints `POST /api/pedidos/{pedidoId}/asignaciones`, `GET /api/pedidos/candidatos`, `GET /api/repartidores`, `GET /api/unidades` en `backend/src/DeliverySac.Api/Controllers/AsignacionesController.cs`.
- [x] T047 [US3] **Front test**: componente `estado-tag` (color correcto por cada estado) en `frontend/src/app/shared/estado-tag/estado-tag.component.spec.ts` (debe FALLAR primero).
- [x] T048 [US3] **Front**: ruta del día del repartidor (una columna, mobile-first, sin menú lateral, acciones táctiles grandes) en `frontend/src/app/features/repartidor/ruta-dia/`.
- [x] T049 [US3] **Front**: componente compartido `estado-tag` (etiqueta de color por estado) en `frontend/src/app/shared/estado-tag/`.
- [x] T050 [P] [US3] **Test back**: unit de la máquina de estados (`PENDIENTE → ASIGNADO → EN_RUTA → (ENTREGADO|NO_ENTREGADO|REPROGRAMADO)`, `ENTREGADO` terminal, motivo obligatorio) en `backend/tests/DeliverySac.Tests/Unit/EstadoPedidoTests.cs` (debe FALLAR primero).
- [x] T051 [P] [US3] **Test back**: integración AC-05/06/07/07a/09/10/11/14/15 en `backend/tests/DeliverySac.Tests/Integration/EstadosEndpointsTests.cs` (debe FALLAR primero).
- [x] T052 [US3] **Back**: implementar la máquina de estados y el registro inmutable de traza (`FechaHora` y `UsuarioId` asignados por el servidor) en `backend/src/DeliverySac.Api/Application/EstadoPedidoService.cs`.
- [x] T053 [US3] **Back**: endpoints `POST /api/pedidos/{id}/estados` (400 transición inválida / 400 sin motivo / 403 no propietario) y `GET /api/pedidos/{id}/trazabilidad` (solo lectura; 405 ante intentos de edición) en `backend/src/DeliverySac.Api/Controllers/PedidosController.cs`.
- [x] T054 [US3] **Back**: endpoint `GET /api/repartidor/pedidos` (solo pedidos propios del día; 403 en accesos ajenos) en `backend/src/DeliverySac.Api/Controllers/RepartidorController.cs`.
- [x] T055 [US3] **Front test**: botones de cambio de estado en la ruta (motivo obligatorio en `NO_ENTREGADO`/`REPROGRAMADO`, confirmación para estados finales) en `frontend/src/app/features/repartidor/ruta-dia/ruta-dia.component.spec.ts` (debe FALLAR primero).
- [x] T056 [US3] **Front**: botones de cambio de estado en la ruta con confirmación para estados finales y campo de motivo obligatorio cuando aplique en `frontend/src/app/features/repartidor/ruta-dia/`.

**Checkpoint**: US1, US2 y US3 funcionales; el flujo operativo principal (crear → asignar → marcar → visualizar) funciona.

---

## Phase 6: User Story 4 - Operación offline del repartidor (Priority: P2)

**Goal**: Cola local de cambios de estado con reintento automático al recuperar señal.

**Independent Test**: Marcar estado sin conexión y, al recuperar señal, verificar sincronización
automática <= 10 s (AC-16); conflicto → descartar, notificar y refrescar.

- [x] T057 [US4] **Front test**: `OfflineService` (cola en IndexedDB, idempotencia por `operacionId`, política de conflicto) en `frontend/src/app/core/offline/offline.service.spec.ts` (debe FALLAR primero).
- [ ] T058 [US4] **Front**: `OfflineService` con cola persistida en IndexedDB y `operacionId` idempotente por cambio en `frontend/src/app/core/offline/`.
- [ ] T059 [US4] **Front**: sincronización en segundo plano (evento `online` + reintento con backoff) y política de conflicto (descartar cambio local, Toast, refrescar estado) en `frontend/src/app/core/offline/`.
- [ ] T060 [US4] **Front**: habilitar service worker PWA para operación sin conexión en `frontend/src/app/core/offline/`.
- [ ] T061 [P] [US4] **Test back**: unit de idempotencia (`operacionId` repetido no duplica traza) en `backend/tests/DeliverySac.Tests/Unit/IdempotenciaTests.cs` (debe FALLAR primero).
- [ ] T062 [P] [US4] **Test back**: integración AC-16 determinística (simular offline→reconexión y asertar que el cambio encolado se envía y aparece en el panel, sin medir el tiempo real) en `backend/tests/DeliverySac.Tests/Integration/OfflineSyncTests.cs` (debe FALLAR primero).
- [ ] T063 [US4] **Back**: idempotencia en el endpoint de estados (índice único `OperacionId`) en `backend/src/DeliverySac.Api/Application/EstadoPedidoService.cs`.

**Checkpoint**: La operación en ruta tolera intermitencia de red.

---

## Phase 7: User Story 5 - Panel del día y supervisión (Priority: P2)

**Goal**: Listado del día para administrador/supervisor con estado, hora y motivo, refresco cada 5 s e indicador de desconexión.

**Independent Test**: Con un pedido del día y uno `REPROGRAMADO` de días anteriores, ambos aparecen;
refresco cada 5 s; banner "SIN CONEXIÓN" al perder red (AC-12/13/18/19).

- [ ] T064 [US5] **Front test**: panel del día (render estado/hora/motivo, refresco 5 s, banner "SIN CONEXIÓN") en `frontend/src/app/features/listado-dia/listado-dia.component.spec.ts` (debe FALLAR primero).
- [ ] T065 [US5] **Front**: panel del día (admin/supervisor, `p-table` lazy) con estado (Tag), hora de última modificación y motivo, en `frontend/src/app/features/listado-dia/`.
- [ ] T066 [US5] **Front**: refresco automático cada 5 s e indicador visual "SIN CONEXIÓN" (oculta en < 2 s al recuperar) en `frontend/src/app/features/listado-dia/`.
- [ ] T067 [P] [US5] **Test back**: integración AC-12/AC-13 (incluye `REPROGRAMADO` de días previos; sin conexión usa cache local) en `backend/tests/DeliverySac.Tests/Integration/ListadoDiaTests.cs` (debe FALLAR primero).
- [ ] T068 [US5] **Back**: endpoint `GET /api/pedidos/dia` (pedidos de hoy + `REPROGRAMADO` de días anteriores; rol admin/supervisor) en `backend/src/DeliverySac.Api/Controllers/PedidosController.cs`.

**Checkpoint**: Administrador y supervisor consultan el día sin llamadas telefónicas.

---

## Phase 8: User Story 6 - Configuración por entorno y despliegue seguro (Priority: P3)

**Goal**: URL base por entorno sin hardcodeo y cumplimiento de restricciones de seguridad/zona horaria.

**Independent Test**: `ng build --configuration production` usa la URL de producción sin editar
componentes (AC-20/AC-21); fechas en America/Lima sin desfase (AC-23).

- [ ] T069 [US6] **Front**: verificar que build de producción resuelve `apiUrl` desde el entorno activo sin editar componentes en `frontend/src/environments/`.
- [ ] T070 [P] [US6] **Back**: forzar HTTPS/TLS 1.2+ y comprobar que no hay dominios externos configurados (FR-027/FR-028) en `backend/src/DeliverySac.Api/Program.cs`.
- [ ] T071 [P] [US6] **Test back**: integración RNF-06/AC-23 (fecha enviada `yyyy-MM-dd` corresponde al día operativo America/Lima, sin corrimiento UTC) en `backend/tests/DeliverySac.Tests/Integration/FechasTests.cs`.

**Checkpoint**: Todas las historias funcionales e independientes.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Mejoras transversales.

- [ ] T072 [P] **Front test**: accesibilidad (Enter guarda / Esc cancela, `label for`, foco visible) en modales en `frontend/src/app/shared/a11y.spec.ts` (debe FALLAR primero).
- [ ] T073 [P] **Front**: accesibilidad en `p-dialog`/`p-sidebar` (Enter/Esc, `label for`, foco visible) en `frontend/src/app/shared/`.
- [ ] T074 [P] **Front test**: consistencia temática y de etiquetas de estado en `frontend/src/app/shared/theme.spec.ts`.
- [ ] T075 [P] **Front**: consistencia temática y etiquetas de estado aplicadas en todas las pantallas (FR-030/FR-036) en `frontend/src/app/shared/`.
- [ ] T076 [P] **Back test**: verificación de rendimiento (P95 ≤ 2 s en listados paginados bajo condiciones normales; SC-002) en `backend/tests/DeliverySac.Tests/Integration/PerformanceTests.cs`.
- [ ] T077 [P] **Front test**: verificación responsive sin scroll horizontal a 320px en vistas operativas (SC-006) en `frontend/src/app/shared/responsive.spec.ts`.
- [ ] T078 [P] Documentación en `docs/` y ejecución de la validación de `quickstart.md`.
- [ ] T079 Revisión final de la constitución (`.specify/memory/constitution.md`): sin lógica de IA mezclada, sin secretos versionados, trazas inmutables, sin funcionalidad fuera del PRD.

---

## Fase 10 – Incidencias de demo

- [x] T080 [P] **Back**: dejar el CORS libre para la demo permitiendo cualquier origen (`AllowAnyOrigin` + `AllowAnyHeader` + `AllowAnyMethod`) en la política `Frontend` de `backend/src/DeliverySac.Api/Program.cs` (idealmente condicionado a Development); documentar que se endurece en producción.
- [ ] T081 [P] **Front/Infra**: diagnosticar y corregir el arranque en Opera (pestaña con carga indefinida): verificar/limpiar Service Worker y caché, confirmar compatibilidad del PWA/SW y validar el login en Opera (Brave/Chrome ya funcionan).
- [x] T082 **Docs**: documentar en `AGENTS.md` y `PRD.md` el CORS libre de demo y la nota de compatibilidad de navegadores (Opera vs Brave/Chrome y limpieza de Service Worker/caché).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Fase 1)**: sin dependencias.
- **Foundational (Fase 2)**: depende de Setup; BLOQUEA todas las historias.
- **User Stories (Fases 3–8)**: dependen de Foundational. Pueden hacerse en paralelo (si hay equipo) o
  secuencialmente por prioridad (US1 → US2 → US3 → US4 → US5 → US6).
- **Polish (Fase 9)**: depende de las historias deseadas.

### User Story Dependencies

- **US1 (P1)**: sin dependencias (tras Foundational).
- **US2 (P1)**: sin dependencias; usa el AuthService de US1 para autorizar.
- **US3 (P1)**: sin dependencias; integra clientes/pedidos de US2 para asignar.
- **US4 (P2)**: extiende el cambio de estado de US3.
- **US5 (P2)**: consume datos de US2/US3.
- **US6 (P3)**: transversal; aprovecha la config de Foundational.

### Within Each User Story (front-first, test-first)

- Test front → Front (pantalla) → Test back → Back → cableado/integración.
- Las pruebas (front y back) MUST escribirse y FALLAR antes de su implementación.
- Story completa antes de pasar a la siguiente prioridad.

### Parallel Opportunities

- Setup y Foundational: tareas `[P]` en paralelo.
- Tras Foundational, las historias pueden ir en paralelo entre desarrolladores.
- Dentro de cada historia, los tests `[P]` y las tareas de archivos distintos pueden ir en paralelo.

---

## Parallel Example: User Story 3

```bash
# Tests back de US3 (archivos distintos, en paralelo):
Task: "unit de la máquina de estados en backend/tests/DeliverySac.Tests/Unit/EstadoPedidoTests.cs"
Task: "integración AC-05..15 en backend/tests/DeliverySac.Tests/Integration/EstadosEndpointsTests.cs"

# Front y back desacoplados de US3:
Task: "ruta del día del repartidor en frontend/src/app/features/repartidor/ruta-dia/"
Task: "endpoint GET /api/repartidor/pedidos en backend/src/DeliverySac.Api/Controllers/RepartidorController.cs"
```

---

## Implementation Strategy

### MVP First (US1 → US2 → US3)

1. Completar Fase 1 (Setup) y Fase 2 (Foundational).
2. Completar US1 (login: front + back) → validar.
3. Completar US2 (clientes/pedidos: front + back) → validar.
4. Completar US3 (asignación/ruta/estado: front + back) → validar el flujo operativo completo (MVP).

### Incremental Delivery

1. Setup + Foundational → base lista (shell con tema, `data-state` y navegación).
2. Añadir US1 → probar → demo.
3. Añadir US2 → probar → demo.
4. Añadir US3 → probar → demo (MVP operativo).
5. Añadir US4 → US5 → US6 → polish, cada una sin romper las anteriores.

---

## Notes

- [P] = archivos distintos, sin dependencias pendientes.
- Cada pantalla avanza en pares: **test front → front**, luego **test back → back** (ver US1).
- Verificar que cada prueba FALLA antes de implementar (rojo-verde-refactor).
- Una subtarea = un commit (ver `AGENTS.md`); marcar `[ ]` → `[x]` al completar.
- Evitar: tareas vagas, conflictos de archivo, dependencias cruzadas entre historias que rompan la independencia.
