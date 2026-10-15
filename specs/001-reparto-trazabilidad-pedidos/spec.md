# Feature Specification: Reparto y Trazabilidad de Pedidos (DeliverySac)

**Feature Branch**: `001-reparto-trazabilidad-pedidos`

**Created**: 2026-10-14

**Status**: Draft

**Input**: User description: "alimentado con tu PRD" (PRD-001: DeliverySac - Reparto y Trazabilidad de Pedidos)

## Clarifications

### Session 2026-10-14

- Q: Cuando un cambio de estado guardado sin conexión se reintenta y el servidor lo rechaza porque el pedido ya cambió (avanzó de estado, fue reasignado o alcanzó estado final), ¿qué debe hacer el sistema? → A: El servidor es la fuente de verdad: descartar el cambio local, notificar al repartidor y refrescar el estado real del pedido desde el servidor.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Autenticación por rol y sesión (Priority: P1)

Un usuario del sistema (Administrador de reparto, Repartidor o Supervisor) inicia sesión con
usuario y contraseña. Al autenticarse, el sistema lo redirige a la pantalla inicial propia de su
rol y muestra en la barra superior su usuario/rol con la acción de cerrar sesión.

**Why this priority**: Todo el resto del sistema depende de saber quién es el usuario y qué rol
tiene; sin autenticación y autorización por rol no existe control de acceso ni trazabilidad
atribuible.

**Independent Test**: Se puede probar creando credenciales para cada rol, iniciando sesión y
verificando la redirección por rol, el usuario/rol mostrado y que "Cerrar sesión" invalida la
sesión y devuelve al login.

**Acceptance Scenarios**:

1. **Given** un usuario con credenciales válidas, **When** inicia sesión, **Then** el sistema lo
   redirige según su rol: ADMINISTRADOR al listado de pedidos del día, REPARTIDOR a su ruta de la
   fecha actual, SUPERVISOR al listado de pedidos del día en solo lectura (sin edición ni
   asignación).
2. **Given** un usuario autenticado (cualquier rol), **When** ejecuta "Cerrar sesión", **Then** se
   eliminan sus credenciales locales y es redirigido al login; al volver atrás no accede a
   pantallas protegidas sin autenticarse de nuevo.
3. **Given** credenciales inválidas, **When** el usuario intenta iniciar sesión, **Then** el
   sistema muestra un mensaje de error inline y no concede acceso.

---

### User Story 2 - Registro de clientes y creación de pedidos con total (Priority: P1)

El Administrador registra clientes (nombre y dirección obligatorios) y crea pedidos compuestos por
una cabecera (cliente, fecha de reparto, comentario) y una o más líneas de detalle (producto,
cantidad, precio unitario y descripción). El sistema calcula el monto total del pedido.

**Why this priority**: Es la entrada de datos sin la cual no hay nada que asignar ni repartir; el
total y las validaciones son reglas de negocio centrales.

**Independent Test**: Se puede probar registrando un cliente, creando un pedido con dos líneas y
verificando que el total es la suma de `cantidad * precio unitario` y que un pedido inválido es
rechazado.

**Acceptance Scenarios**:

1. **Given** un administrador autenticado, **When** registra un cliente con nombre y dirección
   válidos, **Then** el cliente aparece en el listado de clientes y es recuperable por su ID.
2. **Given** un administrador que intenta guardar un pedido sin líneas o con cantidades en 0,
   **When** confirma, **Then** el sistema rechaza la creación con HTTP 400.
3. **Given** un pedido con 2 líneas (2 x 2.50 y 1 x 7.00), **When** se consulta el pedido, **Then**
   el total devuelto es exactamente 12.00.

---

### User Story 3 - Asignación y ruta del repartidor con cambio de estado trazable (Priority: P1)

El Administrador asigna un pedido `PENDIENTE` a un repartidor y una unidad en una fecha. El
Repartidor ve únicamente sus pedidos de la fecha actual, su cabecera y líneas, y marca los estados
según la máquina de estados; el sistema registra automáticamente la hora y el usuario responsable de
cada cambio.

**Why this priority**: Es el corazón operativo del sistema y la base del objetivo de negocio (100%
de pedidos con estado final registrado con hora y usuario).

**Independent Test**: Se puede probar asignando un pedido, confirmando que aparece solo en la ruta
de ese repartidor, y recorriendo la máquina de estados `PENDIENTE → ASIGNADO → EN_RUTA →
ENTREGADO` verificando que cada cambio queda trazado con hora y usuario.

**Acceptance Scenarios**:

1. **Given** un pedido en estado `PENDIENTE`, **When** el administrador lo asigna a un repartidor y
   unidad, **Then** el pedido pasa a `ASIGNADO` y aparece exclusivamente en la ruta de dicho
   repartidor.
2. **Given** un pedido asignado, **When** el repartidor lo marca como `EN_RUTA`, **Then** el estado
   cambia y el sistema registra automáticamente la hora exacta y el ID del usuario, visible para el
   administrador en su listado dentro de 5 segundos.
3. **Given** un pedido en `EN_RUTA`, **When** el repartidor lo marca como `ENTREGADO`, **Then** el
   estado cambia con hora e ID registrados automáticamente.
4. **Given** un pedido asignado, **When** el repartidor intenta marcarlo como `NO_ENTREGADO` o
   `REPROGRAMADO` sin motivo, **Then** el sistema bloquea la acción y exige el motivo.
5. **Given** un pedido en estado `PENDIENTE`, **When** el repartidor intenta cambiarlo directamente
   a `ENTREGADO`, **Then** el sistema rechaza la acción con HTTP 400.
6. **Given** un pedido en estado `ENTREGADO`, **When** el repartidor intenta cambiar a otro estado,
   **Then** el sistema rechaza la acción con HTTP 400.

---

### User Story 4 - Ruta móvil del repartidor con operación offline (Priority: P2)

El Repartidor usa su celular en ruta (mobile-first, una sola columna, sin menú lateral, con acciones
táctiles grandes). Si la señal 4G se interrumpe, los cambios de estado se almacenan localmente y se
sincronizan automáticamente al recuperar la conexión.

**Why this priority**: Garantiza que la operación no se detenga por intermitencia de red, mitigando
el principal riesgo operativo del negocio.

**Independent Test**: Se puede probar actualizando el estado sin conexión, restableciendo la señal y
verificando que el cambio se sincroniza solo en segundo plano y queda reflejado para el
administrador.

**Acceptance Scenarios**:

1. **Given** un repartidor que actualiza el estado de un pedido sin conexión, **When** el celular
   recupera la señal, **Then** el sistema sincroniza el cambio pendiente en segundo plano sin
   intervención del usuario en un plazo máximo de 10 segundos y el cambio queda reflejado en el
   panel del administrador.
2. **Given** un repartidor autenticado con pedidos asignados en una pantalla de 320px, **When**
   abre el detalle de un pedido, **Then** visualiza cabecera, todas las líneas y sus valores
   unitarios sin scroll horizontal.

---

### User Story 5 - Panel del día y supervisión (Priority: P2)

El Administrador y el Supervisor consultan el listado paginado de los pedidos del día con estado
actual, hora de última modificación y motivo en caso de incidencias, incluyendo los pedidos
`REPROGRAMADO` de días anteriores. El listado se refresca automáticamente cada 5 segundos si hay
conexión, y ante falta de conexión se muestra la última versión local más un indicador visible.

**Why this priority**: Da visibilidad operativa al administrador y al supervisor para tomar
decisiones sin llamadas telefónicas, cerrando el ciclo de trazabilidad.

**Independent Test**: Se puede probar con un pedido del día y un pedido `REPROGRAMADO` de días
anteriores, verificando que ambos aparecen, que el listado se refresca cada 5 segundos y que al
perder la conexión aparece el banner "SIN CONEXIÓN" y se muestran los datos locales.

**Acceptance Scenarios**:

1. **Given** el panel del administrador con conectividad y al menos un pedido `REPROGRAMADO` de un
   día anterior, **When** consulta el listado del día, **Then** ve estado, hora de última
   actualización y motivos, y el pedido `REPROGRAMADO` figura en el listado; el listado se refresca
   cada 5 segundos.
2. **Given** el panel del administrador sin conectividad, **When** intenta consultar el listado,
   **Then** visualiza la última versión guardada localmente y un banner rojo fijo "SIN CONEXIÓN".
3. **Given** un panel con el banner "SIN CONEXIÓN" visible, **When** el navegador recupera la
   conexión, **Then** el banner desaparece en menos de 2 segundos.
4. **Given** un administrador que acaba de crear un pedido `PENDIENTE` sin asignación, **When** abre
   la pantalla de Pedidos, **Then** el pedido aparece con cliente, fecha de reparto, estado y total,
   y figura como candidato en la pantalla de Asignación.
5. **Given** el listado con todos los pedidos, **When** se envía la petición con los parámetros
   `page` y `size`, **Then** el backend devuelve estrictamente un subconjunto paginado respetando
   los límites solicitados.

---

### User Story 6 - Configuración por entorno y despliegue seguro (Priority: P3)

El frontend obtiene la URL base de la API desde configuración por entorno (desarrollo vs.
producción), sin URLs hardcodeadas en los componentes; el sistema opera sin dependencias de
servicios de terceros y las fechas se manejan en la zona horaria de Perú.

**Why this priority**: Habilita promover la solución entre entornos y cumplir restricciones
técnicas y de seguridad, aunque no es visible para el usuario final en el día a día.

**Independent Test**: Se puede probar construyendo para producción y verificando que el bundle usa
la URL de producción sin modificar componentes, y que una petición se resuelve contra la URL
base del entorno activo.

**Acceptance Scenarios**:

1. **Given** el código fuente con configuración de desarrollo y de producción, **When** se construye
   para producción, **Then** el bundle usa la URL de producción sin modificar componentes.
2. **Given** un componente que realiza una petición HTTP, **When** la ejecuta, **Then** en el panel
   de red la petición se resuelve contra la URL base definida en la configuración del entorno
   activo, sin URLs hardcodeadas.
3. **Given** un administrador que asigna un pedido con la fecha de hoy (p. ej. 20:00 hora Perú) o
   desde un navegador en otra zona horaria, **When** guarda, **Then** la fecha enviada corresponde
   al día operativo en America/Lima y el pedido aparece en la ruta del repartidor de ese día, sin
   corrimiento por conversión a UTC.

---

### Edge Cases

- ¿Qué ocurre cuando un pedido `REPROGRAMADO` se reasigna? Se crea una nueva asignación activa, el
  pedido vuelve a la ruta como `ASIGNADO` y conserva su historial.
- ¿Qué ocurre cuando se reasigna un pedido con una asignación activa? La asignación anterior se
  invalida automáticamente.
- ¿Qué ocurre al intentar editar o reasignar un pedido `ASIGNADO`, `EN_RUTA`, `ENTREGADO` o
  `NO_ENTREGADO`? El sistema rechaza con HTTP 400 (la edición de pedidos no es una funcionalidad;
  solo se permite reasignar `PENDIENTE` o `REPROGRAMADO`).
- ¿Qué ocurre cuando un repartidor intenta consultar o modificar por URL/API un pedido de otro
  compañero? El sistema deniega el acceso con HTTP 403.
- ¿Qué ocurre cuando un repartidor llama directamente al endpoint de listado del administrador? El
  sistema responde HTTP 403 y no devuelve datos.
- ¿Qué ocurre cuando cualquier rol intenta actualizar la hora o el usuario responsable de un cambio
  de estado existente? El sistema responde HTTP 405 (no existe endpoint de edición de trazabilidad).
- ¿Qué ocurre cuando cambia el puerto o dominio de la API? Se ajusta solo la configuración del
  entorno, sin editar componentes.
- ¿Qué ocurre cuando un cambio de estado guardado sin conexión se sincroniza y el servidor lo
  rechaza por conflicto? Se aplica la política definida en FR-022 (registrada en `## Clarifications`).

## Requirements *(mandatory)*

### Functional Requirements

**Autenticación y acceso**

- **FR-001**: El sistema MUST permitir iniciar sesión con usuario y contraseña y autorizar según rol
  (Administrador, Repartidor, Supervisor).
- **FR-002**: El sistema MUST, tras autenticar, redirigir a cada rol a su pantalla inicial y mostrar
  en la barra superior el usuario/rol con la acción de cerrar sesión; cerrar sesión MUST eliminar
  las credenciales locales y bloquear el acceso a pantallas protegidas sin nueva autenticación.

**Clientes y pedidos**

- **FR-003**: El sistema MUST permitir al administrador registrar un cliente con nombre y dirección
  obligatorios, y recuperarlo por su identificador.
- **FR-004**: El sistema MUST permitir al administrador crear un pedido con cabecera (cliente, fecha
  de reparto, comentario) y una o más líneas de detalle.
- **FR-005**: El sistema MUST validar cada línea (producto, cantidad > 0, precio unitario >= 0 y
  descripción obligatoria) y rechazar con HTTP 400 las solicitudes que no cumplan.
- **FR-006**: El sistema MUST calcular automáticamente el monto total del pedido como la suma de
  cantidad por precio unitario de sus líneas, con precisión de 2 decimales (redondeo bancario).

**Asignación**

- **FR-007**: El sistema MUST permitir al administrador asignar un pedido en estado `PENDIENTE` a un
  repartidor y una unidad en una fecha.
- **FR-008**: El sistema MUST garantizar que un pedido pertenezca a una sola asignación activa; al
  reasignar, MUST invalidar automáticamente la asignación anterior.
- **FR-009**: El sistema MUST permitir reasignar únicamente pedidos en `PENDIENTE` o `REPROGRAMADO`;
  al reasignar un `REPROGRAMADO`, MUST crear una nueva asignación activa y devolver el pedido a la
  ruta como `ASIGNADO`, conservando su historial.
- **FR-010**: El sistema MUST rechazar con HTTP 400 cualquier intento de modificar o reasignar un
  pedido cuyo estado sea distinto de `PENDIENTE` o `REPROGRAMADO`.

**Ruta del repartidor y cambio de estado**

- **FR-011**: El sistema MUST mostrar al repartidor únicamente los pedidos asignados a él en la fecha
  actual, bloqueando estrictamente el acceso a pedidos de otros usuarios con HTTP 403.
- **FR-012**: El sistema MUST mostrar al repartidor, para cada pedido, la cabecera completa y el
  detalle de sus líneas con precio unitario.
- **FR-013**: El sistema MUST permitir al repartidor cambiar el estado de un pedido según la máquina
  de estados `PENDIENTE → ASIGNADO → EN_RUTA → (ENTREGADO | NO_ENTREGADO | REPROGRAMADO)`, y MUST
  rechazar con HTTP 400 las transiciones inválidas.
- **FR-014**: El sistema MUST exigir el motivo (texto libre obligatorio, de 5 a 200 caracteres)
  cuando el estado seleccionado sea `NO_ENTREGADO` o `REPROGRAMADO`.
- **FR-015**: El sistema MUST rechazar con HTTP 400 cualquier intento de cambiar el estado de un
  pedido en estado final (`ENTREGADO`).
- **FR-016**: El sistema MUST registrar de forma automática e inalterable la hora del cambio de estado
  con precisión de segundos (almacenada en UTC como `DateTimeOffset` y mostrada en America/Lima) y el
  ID del usuario responsable en cada cambio de estado.
- **FR-017**: El sistema MUST impedir la modificación o eliminación de trazas históricas; cualquier
  intento de alterar hora o usuario de un cambio existente MUST responder HTTP 405.

**Listados y visibilidad**

- **FR-018**: El sistema MUST mostrar al administrador y al supervisor el listado paginado de los
  pedidos del día (entendiendo "del día" según la fecha de reparto en America/Lima) con su estado
  actual, hora de última modificación y motivo en caso de incidencias.
- **FR-019**: El sistema MUST incluir en el listado de la fecha actual los pedidos con estado
  `REPROGRAMADO` de días anteriores.
- **FR-020**: El sistema MUST exponer al administrador un listado de gestión con todos los pedidos
  registrados (incluidos los `PENDIENTE` sin asignación), mostrando cliente, fecha de reparto,
  estado y total, paginado; la pantalla de asignación MUST listar como candidatos los pedidos en
  `PENDIENTE` o `REPROGRAMADO`.
- **FR-021**: El sistema MUST paginar todos los listados principales usando los parámetros `page` y
  `size`, devolviendo estrictamente el subconjunto solicitado.

**Operación offline y conectividad**

- **FR-022**: El sistema MUST almacenar localmente en el celular del repartidor los cambios de estado
  ante interrupción de red y reintentar la sincronización en segundo plano de forma automática al
  recuperar la conectividad (plazo máximo de 10 segundos, contados desde que se recupera la conexión),
  sin intervención del usuario. Ante un
  rechazo por conflicto (el pedido ya cambió, fue reasignado o alcanzó un estado final), el servidor
  es la fuente de verdad: el sistema MUST descartar el cambio local, notificar al repartidor y
  refrescar el estado real del pedido desde el servidor, sin sobrescribir la trazabilidad válida.
- **FR-023**: El frontend MUST verificar conectividad y refrescar los datos locales cada 5 segundos
  cuando haya conexión disponible (mediante sondeo periódico; el sondeo se pausa mientras la pestaña
  no está visible).
- **FR-024**: El sistema MUST mostrar un indicador visual prominente en el panel del administrador y
  del supervisor cuando no haya conexión (banner rojo fijo con el texto "SIN CONEXIÓN"), y retirarlo
  en menos de 2 segundos al recuperarla; sin conexión MUST mostrar la última versión guardada
  localmente.

**Configuración, seguridad y zona horaria**

- **FR-025**: El frontend MUST obtener la URL base de la API desde configuración por entorno
  (desarrollo vs. producción), sin hardcodear URLs en los componentes.
- **FR-026**: El sistema MUST almacenar las contraseñas usando hash seguro bcrypt con coste >= 10.
- **FR-027**: Toda comunicación cliente-servidor MUST efectuarse mediante HTTPS con TLS 1.2 o
  superior.
- **FR-028**: El sistema MUST NOT realizar llamadas a servicios externos de terceros (0 dependencias
  externas de pago o de terceros); todas las peticiones del cliente MUST apuntar únicamente a APIs
  propias.
- **FR-029**: El sistema MUST interpretar las fechas de reparto y de asignación en la zona horaria de
  Perú (America/Lima); el cliente MUST enviarlas como `yyyy-MM-dd` en hora local sin conversión a
  UTC, y el backend MUST operar el día operativo en dicha zona.

**Interfaz y experiencia (UI/UX)**

- **FR-030**: La interfaz MUST usar exclusivamente la biblioteca de componentes de UI institucional y
  la plantilla de layout aprobada en el PRD, con un tema propio (paleta, tipografía, radios y
  espaciado como tokens); queda prohibido el uso de controles HTML nativos estilizados a mano o
  estilos sueltos por componente.
- **FR-031**: La selección de entidades relacionadas (cliente, repartidor, unidad) MUST realizarse por
  nombre mediante búsqueda predictiva o modal con tabla paginada y búsqueda; queda prohibido exponer
  o capturar identificadores internos como campo de entrada (se prohíben los combos salvo conjuntos
  pequeños y fijos, entendidos como hasta 20 opciones).
- **FR-032**: La retroalimentación MUST ser uniforme: notificaciones tipo Toast para éxito/error y
  confirmación mediante diálogo para acciones irreversibles (reasignar, marcar estado final); queda
  prohibido el uso de diálogos nativos del navegador.
- **FR-033**: Cada pantalla con datos MUST definir estados de carga y estado vacío ("sin datos") de
  forma consistente, y los formularios MUST ser consistentes (etiquetas visibles, validación con
  mensajes de error, agrupación en tarjetas y misma disposición de botones).
- **FR-034**: La interfaz MUST cumplir accesibilidad: etiqueta asociada a cada control, foco visible,
  contraste AA y navegación por teclado; los formularios modales MUST responder a Enter para guardar
  y a Esc para cancelar.
- **FR-035**: La navegación MUST ser consistente: cada pantalla muestra su título y breadcrumb, el
  menú lateral resalta la sección activa y cada función/CRUD se expone como una opción de menú con su
  propia pantalla; listados y formularios de alta/edición MUST ser pantallas separadas (o modal
  dedicado).
- **FR-036**: La interfaz institucional MUST usar una paleta coherente (primario azul `#1976D2`,
  éxito verde, advertencia ámbar, error rojo) y mostrar cada estado del pedido con una etiqueta de
  color distinta.
- **FR-037**: El panel del repartidor MUST usar una maquetación de una sola columna, mobile-first
  desde 320px, sin menú lateral y con acciones táctiles grandes (área táctil mínima de 44×44 px);
  los paneles de administrador y supervisor MUST usar menú lateral colapsable (colapsable en
  <= 640px).
- **FR-038**: Los listados MUST usar el componente de tabla institucional con paginación integrada
  (`page`/`size`) y modo responsive; queda prohibido el uso de tablas HTML nativas.
- **FR-039**: Las fechas y números MUST formatearse en el locale `es-PE`, coherente con la zona
  horaria America/Lima.

**Concurrencia y configuración sensible**

- **FR-040**: El sistema MUST hacer idempotentes los cambios de estado: reenviar la misma operación
  (mismo identificador) no MUST duplicar la traza ni el efecto sobre el estado del pedido.
- **FR-041**: El sistema MUST obtener la cadena de conexión y cualquier secreto desde variables de
  entorno o archivos locales no versionados, sin credenciales en el código ni en archivos
  versionados.

### Key Entities *(include if feature involves data)*

- **Usuario**: cuenta del sistema con rol (Administrador, Repartidor, Supervisor), credenciales y
  contraseña con hash; es el responsable atribuible de cada cambio de estado.
- **Cliente**: destinatario del pedido con nombre y dirección obligatorios.
- **Pedido**: cabecera con cliente, fecha de reparto, comentario, estado actual y monto total
  calculado; agrupa una o más líneas.
- **Línea de Pedido**: detalle del pedido con producto, cantidad (> 0), precio unitario (>= 0) y
  descripción obligatoria.
- **Asignación**: vínculo entre un pedido, un repartidor, una unidad y una fecha; solo una activa por
  pedido.
- **Unidad**: vehículo de reparto (motos lineales y furgonetas).
- **Cambio de Estado (traza)**: evento inmutable con estado, hora exacta (America/Lima), usuario
  responsable y motivo (obligatorio en `NO_ENTREGADO` y `REPROGRAMADO`).
- **Estado de Pedido**: `PENDIENTE`, `ASIGNADO`, `EN_RUTA`, `ENTREGADO`, `NO_ENTREGADO`,
  `REPROGRAMADO`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Al final del día, el 100% de los pedidos asignados tienen un estado final (entregado,
  no entregado o reprogramado) registrado con hora y usuario, sin llamadas telefónicas.
- **SC-002**: El 95% de las peticiones de lectura/listado paginadas (endpoints de consulta) responde
  en 2 segundos o menos (P95 ≤ 2 s) bajo condiciones normales de red.
- **SC-003**: Los cambios de estado marcados por el repartidor se reflejan en el panel del
  administrador en 5 segundos o menos.
- **SC-004**: Los cambios marcados sin conexión se sincronizan automáticamente en 10 segundos o menos
  tras recuperar la señal, sin intervención del usuario.
- **SC-005**: El indicador de desconexión aparece al perder conexión y se oculta en menos de 2
  segundos al recuperarla.
- **SC-006**: Un repartidor puede ver el detalle de un pedido en una pantalla de 320px sin recorrer
  horizontalmente (0 scroll horizontal) en ninguna vista operativa.
- **SC-007**: Un administrador puede asignar un pedido y el repartidor verlo en su ruta el mismo día
  operativo (sin desfase de fecha), para el 100% de las asignaciones.
- **SC-008**: La solución no registra ninguna dependencia o llamada a servicios externos de terceros
  (lista de dominios externos vacía).
- **SC-009**: Cambiar el puerto o dominio de la API no requiere editar ningún componente del
  frontend.
- **SC-010**: El 100% de los pedidos de un listado paginado respeta los límites solicitados en
  `page`/`size`.
- **SC-011**: El 100% de los accesos no autorizados (rol incorrecto o pedido ajeno) se rechaza con
  HTTP 403, y toda edición/reasignación inválida se rechaza con HTTP 400.

## Assumptions

- Se parte del PRD-001 vigente como fuente de verdad de alcance y reglas; toda necesidad no cubierta
  se trata primero como enmienda al PRD.
- El PRD define la arquitectura tecnológica obligatoria: backend .NET 8 con EF Core (comandos) y
  Dapper (consultas) sobre SQL Server Express; frontend Angular con la plantilla Sakai de PrimeNG;
  pruebas con xUnit. Estos detalles no son requisitos funcionales aquí, pero condicionan la
  implementación.
- Existe un único administrador operativo por día que crea pedidos y asigna; no se contempla
  arquitectura multiempresa/multi-sucursal.
- Los usuarios, las unidades (3 motos lineales y 2 furgonetas) y un cliente base se siembran en el
  entorno de desarrollo; **no existe CRUD administrativo de repartidores ni de unidades** (solo
  catálogos de consulta). Los clientes sí tienen alta/edición (FR-003).
- No se modela disponibilidad ni programación de unidades/repartidores: si un repartidor o unidad no
  está disponible, el administrador reasigna el pedido (FR-009).
- La cola offline del repartidor vive en el almacenamiento del navegador móvil (PWA/Web móvil) y su
  tamaño es acotado por el volumen diario (decenas de pedidos); no se define expulsión más allá de
  descartar operaciones inválidas (FR-022). No se desarrolla una aplicación móvil nativa.
- No se define retención ni borrado automático de pedidos y trazas: los datos se conservan por
  auditoría; la depuración/archivado está fuera de alcance.
- No se definen requisitos de observabilidad (logging/métricas/trazado APM) por estar fuera del
  alcance del PRD; se usa el logging por defecto de la plataforma.
- Volumen esperado: un administrador operativo, ~5 unidades y decenas de pedidos por día; los
  listados paginan con `page`/`size` (FR-021).
- El servidor backend está disponible en el horario operativo de reparto (06:00 a 20:00 horas).
- La red móvil celular (4G/3G) tiene estabilidad básica para las sincronizaciones periódicas en ruta.
- El sistema se opera bajo la zona horaria de Perú (America/Lima) y el locale `es-PE`.
- El modo oscuro y la densidad (compacta/normal) son opcionales y no bloquean la entrega.
- Está fuera de alcance: reportes PDF/Excel, cierre de día automatizado, registro público de
  usuarios, GPS/mapa en tiempo real, auditoría avanzada de BD, multiempresa/multi-sucursal, app
  nativa, notificaciones push masivas, chat interno, integraciones con ERP/WhatsApp/pasarelas, y la
  edición de pedidos ya creados.
