# PRD-001: DeliverySac - Reparto y Trazabilidad de Pedidos.

## Contexto y Problema
En la empresa DeliverySac repartimos con 5 unidades (3 motos lineales y 2 furgonetas). Hoy el administrador asigna clientes y pedidos manualmente, sin visibilidad de dónde están las unidades, qué pedidos se entregaron ni a qué hora. Al final del día solo hay control cuando retornan las unidades: demoras, pérdida de trazabilidad y cero métricas. No tenemos presupuesto ni tiempo para un ERP de reparto; necesitamos algo simple que se haga cargo del control operativo diario.

**Personas:**
* **Administrador de reparto:** Asigna pedidos cada mañana y quiere ver el estado y la hora exacta de actualización sin llamar a nadie.
* **Repartidor:** Necesita ver su ruta en el celular, marcar entregas rápido y sincronizar estados incluso ante intermitencias de señal en ruta.
* **Supervisor:** Consulta el reporte del día y los motivos de no entrega para tomar decisiones operativas.

---

## Objetivos
Que al final del día el 100% de los pedidos asignados tengan un estado final (entregado, no entregado o reprogramado) registrado en el sistema con su respectiva hora y usuario, sin llamadas telefónicas ni dudas.

*Flujo operativo:* Creación de pedido -> Asignación -> Repartidor marca estado (con soporte offline/reintento) -> Administrador visualiza el estado y la hora actualizados.

---

## Requerimientos Funcionales (RF)
* **RF-01:** El sistema debe permitir iniciar sesión con usuario y contraseña según rol (Administrador, Repartidor, Supervisor).
* **RF-02:** El sistema debe permitir al administrador registrar un cliente con nombre y dirección obligatorios.
* **RF-03:** El sistema debe permitir al administrador crear un pedido con cabecera (cliente, fecha de reparto, comentario) y una o más líneas de detalle. *Regla de negocio:* Cada línea requiere producto, cantidad > 0, precio unitario >= 0 y descripción obligatoria. Si no cumple, el sistema rechaza la solicitud respondiendo HTTP 400.
* **RF-04:** El sistema debe calcular automáticamente el monto total del pedido como la suma de cantidad por precio unitario de sus líneas.
* **RF-05:** El sistema debe permitir al administrador asignar un pedido en estado `PENDIENTE` a un repartidor y una unidad en una fecha. *Reglas de negocio:*
  * Un pedido solo puede pertenecer a una asignación activa (si se reasigna, se invalida la anterior automáticamente).
  * La reasignación solo es válida mientras el pedido esté en `PENDIENTE` o `REPROGRAMADO`. Al reasignar un pedido `REPROGRAMADO` se crea una nueva asignación activa y el pedido vuelve a la ruta como `ASIGNADO`, conservando su historial.
  * La edición de pedidos no es una funcionalidad del sistema: cualquier intento del backend de modificar o reasignar un pedido cuyo estado sea diferente de `PENDIENTE` o `REPROGRAMADO` debe rechazarse respondiendo HTTP 400.
* **RF-06:** El sistema debe mostrar al repartidor únicamente los pedidos asignados a él en la fecha actual, bloqueando estrictamente el acceso a pedidos de otros usuarios (retornando HTTP 403 en intentos no autorizados).
* **RF-07:** El sistema debe mostrar al repartidor, para cada pedido, la cabecera completa y el detalle de sus líneas de productos con precio unitario.
* **RF-08:** El sistema debe permitir al repartidor cambiar el estado de un pedido según la máquina de estados: `PENDIENTE` → `ASIGNADO` → `EN_RUTA` → (`ENTREGADO` | `NO_ENTREGADO` | `REPROGRAMADO`). *Regla de negocio:* Si el estado seleccionado es `NO_ENTREGADO` o `REPROGRAMADO`, el ingreso del motivo del suceso es de carácter obligatorio.
* **RF-09:** El sistema debe registrar de forma automática e inalterable la hora exacta y el ID del usuario responsable cada vez que ocurra un cambio de estado en el pedido.
* **RF-10:** El sistema debe mostrar al administrador y supervisor el listado paginado de los pedidos del día con su estado actual, hora de última modificación y motivo en caso de incidencias.
* **RF-10a:** El sistema debe incluir en el listado de la fecha actual los pedidos con estado `REPROGRAMADO` de días anteriores.
* **RF-10b:** El frontend debe verificar conectividad y refrescar los datos locales cada 5 segundos si hay conexión disponible.
* **RF-10c:** El sistema debe mostrar al administrador un listado de gestión de pedidos con todos los pedidos registrados (incluidos los `PENDIENTE` sin asignación), mostrando cliente, fecha de reparto, estado y total, paginado. La pantalla de asignación debe listar como candidatos los pedidos en estado `PENDIENTE` o `REPROGRAMADO`.
* **RF-11:** El celular del repartidor debe almacenar localmente los cambios de estado si la red 4G se interrumpe momentáneamente, reintentando la sincronización en segundo plano de forma automática al recuperar la conectividad.
* **RF-12:** El sistema debe paginar todos los listados principales utilizando los parámetros de consulta `page` y `size` por defecto.
* **RF-13:** El sistema debe mostrar un indicador visual prominente en el panel del administrador y supervisor cuando no hay conexión disponible.
* **RF-14:** El frontend debe obtener la URL base de la API desde una configuración por entorno (desarrollo vs. producción), sin hardcodear URLs en los componentes. *Aceptación:* Cambiar el puerto o dominio de la API sin editar ningún componente.

---

## Requerimientos No Funcionales (RNF)
* **RNF-01:** El 95% de las peticiones HTTP debe responder en <= 2 segundos bajo condiciones normales de red.
* **RNF-02:** La interfaz del repartidor debe ser responsive y no debe requerir scroll horizontal en pantallas de 320px de ancho (diseño mobile-first), verificado por AC-08.
* **RNF-03:** Las contraseñas de usuario deben almacenarse de forma segura utilizando hash bcrypt con coste >= 10.
* **RNF-04:** Toda comunicación cliente-servidor debe efectuarse mediante HTTPS utilizando TLS 1.2 o superior.
* **RNF-05:** El sistema no debe realizar llamadas a servicios externos de terceros (0 dependencias externas de pago o de terceros: pasarelas, autenticación externa, etc.). Verificable: la lista de dominios externos en la configuración debe estar vacía y todas las peticiones del cliente deben apuntar únicamente a APIs propias.
* **RNF-06:** Las fechas de reparto y de asignación deben interpretarse en la zona horaria de Perú (America/Lima); el cliente debe enviarlas como `yyyy-MM-dd` en hora local, sin conversión a UTC, para evitar desfases de un día entre la fecha de asignación y el día en que el repartidor ve el pedido en su ruta.

---

## Diseño de Interfaz (UX)
* **RF-UX-01:** Los paneles del administrador y del supervisor deben presentar un menú lateral de navegación que dé acceso a las secciones habilitadas para cada rol (administrador: listado del día, clientes, pedidos, asignación; supervisor: listado del día en solo lectura).
* **RF-UX-02:** El panel del repartidor debe usar un diseño de una sola columna, mobile-first, sin menú lateral, priorizando acciones táctiles (botones grandes) para marcar estados.
* **RF-UX-03:** La interfaz debe usar una paleta institucional coherente (primario azul `#1976D2`, éxito verde, advertencia ámbar, error rojo) con tipografía legible; cada estado del pedido debe mostrarse con una etiqueta de color distinta.
* **RF-UX-04:** El shell debe mostrar una barra superior con el nombre del sistema y el usuario/rol con la acción de cerrar sesión.
* **RF-UX-05:** Cada función del sistema se expone como una opción del menú lateral y cada CRUD (clientes, pedidos, asignación, listado del día, etc.) tiene su propia pantalla dedicada. Queda prohibido concentrar varias funciones en una sola pantalla.
* **RF-UX-06:** Los listados y los formularios de alta/edición de una entidad son pantallas separadas: el listado en su pantalla y el alta/edición en su propia pantalla (o modal dedicado).
* **RF-UX-07:** La selección de entidades relacionadas (cliente, repartidor, unidad) se realiza mediante **búsqueda predictiva** (`p-autoComplete` con búsqueda en el servidor y *debounce* 300ms) o, cuando se requiere explorar/paginar un conjunto amplio, mediante un **modal con tabla paginada y búsqueda** (`p-table`). Los **combos** (`p-dropdown`) se usan **solo** cuando el conjunto de opciones es pequeño y fijo. Queda **prohibido exponer identificadores (IDs) internos de entidades relacionadas como campo de entrada**: el usuario selecciona por nombre y el formulario muestra el nombre del elemento elegido.
* **RF-UX-08:** Todas las tablas de datos usan el componente de tabla de PrimeNG con paginación integrada (parámetros `page`/`size`).
* **RF-UX-09:** La interfaz usa exclusivamente componentes de PrimeNG (botones, inputs, tablas, diálogos, calendarios, etc.). Queda prohibido usar controles HTML nativos o estilizados a mano.
* **RF-UX-10:** La apariencia se define con un tema PrimeNG personalizado (paleta institucional, tipografía, radios y espaciado como parte del tema); no se usan colores ni estilos sueltos por componente.
* **RF-UX-11:** La retroalimentación es uniforme: los mensajes de éxito/error se muestran con el componente de notificaciones (Toast) de PrimeNG, y las acciones irreversibles (reasignar, marcar estado final) piden confirmación mediante el diálogo de confirmación de PrimeNG.
* **RF-UX-12:** Cada pantalla con datos define estados de carga (Skeleton/ProgressSpinner) y estado vacío ("sin datos") de forma consistente.
* **RF-UX-13:** Los formularios son consistentes: etiquetas visibles, validación con mensajes de error, agrupación en tarjetas y botones con la misma disposición entre pantallas.
* **RF-UX-14:** Accesibilidad: cada control tiene etiqueta asociada, foco visible, contraste suficiente (AA) y navegación por teclado.
* **RF-UX-15:** Navegación consistente: cada pantalla muestra su título y una ruta de navegación (breadcrumb); el menú lateral resalta la sección activa.
* **RF-UX-16:** La iconografía usa exclusivamente PrimeIcons, con tamaños uniformes.
* **RNF-UX-01:** La interfaz se construye con PrimeNG sobre Angular usando la plantilla oficial **Sakai** (`primefaces/sakai-ng`) como base de layout, con PrimeFlex y un tema PrimeNG personalizado.
* **RNF-UX-02:** El menú lateral debe ser colapsable en pantallas pequeñas (<= 640px) para no romper el diseño mobile-first.
* **RNF-UX-03:** La interfaz es responsive con breakpoints definidos; las tablas usan el modo responsive/scroll de PrimeNG.
* **RNF-UX-04:** Las fechas y números se formatean en el *locale* `es-PE`, coherente con la zona horaria America/Lima.
* **RNF-UX-05:** El modo oscuro y la densidad (compacta/normal) son **opcionales** y no bloquean la entrega.

### Base de Layout: Plantilla Sakai (PrimeNG)

Se toma como base la plantilla oficial **Sakai** de PrimeNG (`primefaces/sakai-ng`), que aporta el shell de la aplicación: barra superior (*topbar*), menú lateral colapsable, sistema de temas y utilidades PrimeFlex, además de pantallas de autenticación (login).

Como **Sakai es blanco por defecto**, se personaliza para reflejar la identidad de DeliverySac:
* **Menú lateral:** fondo oscuro con gradiente azul institucional y acentos en el color primario `#1976D2`; textos claros de alto contraste.
* **Barra superior (*topbar*):** color de marca y contraste, mostrando el usuario/rol y la acción de cerrar sesión (RF-UX-04).
* **Tema:** se personaliza el tema de PrimeNG (o se sobreescribe) para usar el primario de DeliverySac en botones, foco y elementos activos.
* **Etiquetas de estado:** los estados del pedido usan los colores definidos en RF-UX-03.
* **Login:** la pantalla de autenticación de Sakai se adapta al diseño especificado en "Pantalla de Inicio de Sesión".
* **Repartidor:** no usa el menú lateral; usa el layout de una columna definido en RF-UX-02.

### Pantalla de Inicio de Sesión

**Concepto de diseño general.** Estética moderna de "vidrio esmerilado" (*glassmorphism*) / "tarjeta flotante" sobre un fondo texturizado, con layout de pantalla dividida (*split screen*) que equilibra la marca con la interacción.

**Panel de marca (izquierda, ~50%).**
* **Fondo y paleta:** gradiente azul profundo (de azul medio a oscuro) para proyectar un tono profesional y tecnológico.
* **Patrones gráficos:** patrón de puntos sutil (matriz de puntos en esquinas opuestas) para dar textura sin distraer.
* **Jerarquía de texto:**
  * **Encabezado grande** en negrita con el propósito del portal (texto: "DeliverySac — Reparto y trazabilidad de pedidos").
  * **Lista de características:** 5 puntos clave, cada uno precedido por un icono circular monocromático y un texto breve (p. ej. "Asignación de pedidos", "Trazabilidad con hora y responsable", "Ruta móvil del repartidor", "Funciona sin conexión", "Panel del supervisor").
* **Color de texto:** blanco de alto contraste contra el fondo oscuro.

**Panel de formulario (derecha, ~50%).**
* **Fondo:** blanco limpio o gris muy claro.
* **Encabezado local:** isotipo discreto en la esquina superior izquierda; botón flotante tipo *pill* en la superior derecha para una acción secundaria (p. ej. "Ayuda").
* **Título del formulario:** "Iniciar sesión", claro y directo sobre los campos.
* **Selector de tipo de acceso (tabs):** interruptor dual para elegir el contexto (Administrador / Repartidor / Supervisor); el estado activo debe distinguirse visualmente. Es apoyo visual: la autorización real la determina el servidor según las credenciales.
* **Campos de entrada:** etiqueta sutil sobre cada campo; inputs de ancho completo, esquinas redondeadas y *placeholder* ("Usuario", "Contraseña").
* **Opciones de cuenta:** checkbox "Recordar usuario" y enlace "Olvidé mi contraseña" alineado a la derecha (enlace informativo; su flujo no está en alcance).
* **Llamado a la acción principal (CTA):** botón de ancho completo, esquinas redondeadas y color primario institucional, con el texto "Ingresar".
* **Retroalimentación:** mensaje de error inline ante credenciales inválidas.

**Ajustes respecto al texto de referencia.** No se incluye botón "REGISTER" (el registro público de usuarios está fuera de alcance) ni campo "Email" (el acceso es por nombre de usuario y contraseña).

---

## 7. Estándares de Interfaz y Experiencia de Usuario (UI/UX) — Sakai & PrimeNG

El desarrollo del frontend sobre Angular debe alinearse estrictamente a la arquitectura de la plantilla Sakai (`sakai-ng`) y al conjunto de componentes de PrimeNG, garantizando un entorno visual profesional, uniforme y orientado a la velocidad operativa.

### 7.1. Arquitectura de Layout y Estilo (Sakai)

* **Shell de Administración:** El panel administrativo y de supervisión utiliza la estructura base de Sakai (`app-layout`), conservando los módulos `app-topbar`, `app-sidebar` (menú lateral colapsable) y `app-footer`.
* **Vista Móvil del Repartidor:** La interfaz para el rol de repartidor prescinde del `app-sidebar` para adoptar una maquetación de 1 sola columna (Mobile-First) optimizada para pantallas táctiles a partir de 320px.
* **Tokens de Diseño:** Todas las vistas deben consumir exclusivamente las variables CSS globales de Sakai/PrimeNG (e.g., `--primary-color`, `--surface-card`, `--text-color`). Queda prohibido hardcodear colores hexadecimales o aplicar estilos inline (`style="..."`) en los componentes.

### 7.2. Matriz de Patrones UI y Contenedores PrimeNG

Cualquier formulario, tabla o selector dentro del sistema debe implementarse siguiendo la siguiente matriz de patrones:

| Operación / Contexto | Componente PrimeNG | Contenedor UI / Modal | Regla de Interacción Esperada |
|---|---|---|---|
| Selección con alta carga de datos (Clientes, Unidades) | `p-autoComplete` | Inline / Modal con `p-table` | Búsqueda predictiva (debounce 300ms) o tabla paginada. Prohibido el ingreso manual de IDs. |
| Altas y Formularios Cortos (≤ 4 campos) | `p-dialog` | Modal Centrado (width: 450px) | Modalidad activa (`modal="true"`), foco en primer input y opción de "Guardar y crear otro" (`p-checkbox`). |
| Edición y Formularios Extensos (> 5 campos) | `p-sidebar` | Panel Lateral (`position="right"`) | Desliza desde la derecha (width: 450px), manteniendo el listado principal visible de fondo. |
| Acciones Irreversibles (Reasignar, Anular) | `p-confirmDialog` | Modal Centrado Small | Invocado vía `ConfirmationService` con botón primario de peligro (`severity="danger"`). |
| Listados Principales | `p-table` | Tarjeta Sakai (`.card`) | Paginación obligatoria por servidor (`[lazy]="true"`, parámetros `page`/`size`). |

### 7.3. Directrices Específicas de Implementación

* **RF-UX-17 (Selección de Entidades y Autocomplete):** Todo campo de selección de entidades relacionadas (clientes, unidades, repartidores) debe omitir inputs numéricos de ID. La opción por defecto es `p-autoComplete` con búsqueda en el servidor (*debounce* 300ms) y la directiva `pTemplate="item"` para desplegar información de apoyo (ej. Nombre de Cliente + DNI/RUC) en cada opción; cuando se requiera explorar/paginar un conjunto amplio, se usa el modal con `p-table` paginada y búsqueda. Mismo criterio de RF-UX-07.
* **RF-UX-18 (Sistema de Notificaciones y Retroalimentación):**
  * **Feedback Asíncrono:** Todas las confirmaciones de operaciones y errores de API deben emitirse mediante el servicio global `MessageService` (`<p-toast>`) configurado en el layout principal. Queda prohibido el uso de diálogos nativos del navegador (`alert()`).
  * **Estados de Espera:** Durante la carga de datos se renderizarán componentes `<p-skeleton>` en las tablas o formularios, y la propiedad `[loading]="true"` en los botones de acción (`p-button`).
* **RF-UX-19 (Accesibilidad y Atajos por Teclado):**
  * Todo formulario dentro de un `p-dialog` o `p-sidebar` debe responder de forma nativa a la tecla **Enter** para ejecutar la acción de guardado principal y a la tecla **Esc** para cancelar/cerrar la capa activa.
  * Todos los controles interactivos deben preservar el anillo de foco (*focus ring*) provisto por el tema de Sakai y contar con etiquetas (`<label>`) asociadas explícitamente mediante el atributo `for`.

### 7.4. Criterios de Aceptación de UI/UX (AC-UX)

* **AC-UX-01 (Manejadores de Diálogo):** Dado un usuario en cualquier formulario modal (`p-dialog` o `p-sidebar`), cuando presiona la tecla Enter, el sistema procesa la acción principal de guardado; cuando presiona Esc, el modal se cierra descartando los cambios no guardados.
* **AC-UX-02 (Cero Exposición de IDs):** Dado cualquier formulario de registro o asignación, cuando se requiere vincular un cliente o entidad relacionada, la interfaz no presenta ningún campo para escribir IDs manualmente; la selección se realiza mediante búsqueda predictiva (`p-autoComplete`) o tabla en modal con paginación.
* **AC-UX-03 (Consistencia de Notificaciones):** Dado un evento de API (creación exitosa, error de validación o fallo de servidor), el sistema muestra la notificación correspondiente mediante `<p-toast>` en la esquina superior derecha sin interrumpir el flujo del usuario ni usar diálogos invasivos.
* **AC-UX-04 (Diseño Adaptable del Repartidor):** Dado un repartidor accediendo desde un dispositivo móvil (ancho ≥ 320px), la interfaz ajusta automáticamente los componentes de PrimeNG para no requerir desplazamiento (*scroll*) horizontal en ninguna vista operativa.
* **AC-UX-05 (Consistencia Temática):** Dado el cambio de tema o ajuste de densidad en la plantilla Sakai, todos los componentes de PrimeNG (`p-table`, `p-button`, `p-dialog`, `p-autoComplete`) heredan los tokens de color e iconografía de PrimeIcons sin desalineaciones visuales.

### 7.5. Manejo de Estados de Interfaz

```
[ Sin Datos ]   --->  Mostrar <p-skeleton> / ProgressSpinner durante la carga
[ Con Datos ]   --->  Renderizar en p-table con paginador (page / size)
[ Lista Vacía ] --->  Mostrar mensaje explicativo "No se encontraron registros" + Botón de acción principal
[ Error API ]   --->  Lanzar Toast (severity="error") + Mantener datos en formulario para reintento
```

---

## Criterios de Aceptación (AC)
* **AC-01 (RF-01):** Dado un usuario autenticado con credenciales válidas, cuando inicia sesión, entonces el sistema lo redirige según su rol: ADMINISTRADOR ve el listado de pedidos del día, REPARTIDOR ve su ruta de la fecha actual, SUPERVISOR ve el listado de pedidos del día sin botones de edición ni asignación.
* **AC-02 (RF-02):** Dado un administrador autenticado, cuando registra un cliente con nombre y dirección válidos, entonces el cliente aparece en el listado de clientes del administrador y es recuperable por su ID mediante la API.
* **AC-03 (RF-03):** Dado un administrador que intenta guardar un pedido sin líneas o con cantidades en 0, cuando confirma, entonces el sistema responde HTTP 400 y rechaza la creación.
* **AC-04 (RF-04):** Dado un pedido con líneas creadas, cuando se consulta ese pedido por API, entonces el campo `total` devuelto es exactamente igual a la sumatoria de `cantidad * precioUnitario` de todas sus líneas (verificable con 2 líneas de ejemplo: 2x2.50 + 1x7.00 => total 12.00).
* **AC-05 (RF-05):** Dado un pedido en estado `PENDIENTE`, cuando el administrador lo asigna a un repartidor y unidad, entonces el pedido cambia a estado `ASIGNADO` y aparece reflejado exclusivamente en la ruta de dicho repartidor.
* **AC-06 (RF-05 — Rechazo de edición/reasignación):** Dado un pedido en estado `ASIGNADO`, `EN_RUTA`, `ENTREGADO` o `NO_ENTREGADO`, cuando el administrador intenta editarlo o reasignarlo, entonces el sistema responde HTTP 400 rechazando la operación. Un pedido en `REPROGRAMADO` sí puede reasignarse (vuelve a `ASIGNADO` con nueva asignación activa); su edición sigue rechazada.
* **AC-07 (RF-06 — Control de acceso):** Dado un repartidor autenticado que intenta consultar o modificar mediante URL/API el pedido asignado a otro compañero, entonces el sistema deniega el acceso respondiendo con HTTP 403.
* **AC-07a (RF-06 — Aislamiento por rol en API):** Dado un repartidor autenticado, cuando llama directamente por API al endpoint de listado de pedidos del administrador, entonces el sistema responde HTTP 403 y no devuelve datos.
* **AC-08 (RF-07):** Dado un repartidor autenticado con pedidos asignados en un dispositivo con pantalla de 320px, cuando abre el detalle de un pedido, entonces visualiza la cabecera, todas las líneas de productos y sus valores unitarios sin scroll horizontal.
* **AC-09 (RF-08 — Cambio de estado):** Dado un pedido asignado, cuando el repartidor lo marca como `EN_RUTA`, entonces el estado cambia, el sistema registra automáticamente la hora exacta y el ID del usuario, y el administrador lo visualiza actualizado en su listado dentro de 5 segundos.
* **AC-09a (RF-09 — Trazabilidad inalterable):** Dado un pedido con cambios de estado registrados, cuando cualquier rol intenta actualizar la hora o el usuario responsable de un cambio existente mediante API, entonces el sistema responde HTTP 405 (método no permitido), ya que no existe endpoint de edición de la trazabilidad.
* **AC-10 (RF-08 — Cambio a ENTREGADO):** Dado un pedido en estado `EN_RUTA`, cuando el repartidor lo marca como `ENTREGADO`, entonces el estado cambia, se registran hora e ID automáticamente, y el pedido aparece con estado `ENTREGADO` en el listado del administrador dentro de 5 segundos.
* **AC-11 (RF-08 — Motivo obligatorio):** Dado un pedido asignado, cuando el repartidor intenta marcarlo como `NO_ENTREGADO` o `REPROGRAMADO` sin especificar un motivo, entonces el sistema bloquea la acción y exige el texto del motivo.
* **AC-12 (RF-10, RF-10a):** Dado el panel del administrador con conectividad 4G estable y al menos un pedido `REPROGRAMADO` de un día anterior, cuando consulta el listado del día, entonces visualiza los pedidos con estado, hora de última actualización y motivos de incidencias, y el pedido `REPROGRAMADO` de días anteriores figura en el listado; el listado se refresca automáticamente cada 5 segundos.
* **AC-13 (RF-10 — Sin conexión):** Dado el panel del administrador sin conectividad, cuando intenta consultar el listado, entonces visualiza la última versión guardada localmente.
* **AC-14 (RF-08 — Transiciones inválidas):** Dado un pedido en estado `PENDIENTE`, cuando el repartidor intenta cambiar directamente a `ENTREGADO` (saltando ASIGNADO y EN_RUTA), entonces el sistema rechaza la acción respondiendo HTTP 400.
* **AC-15 (RF-08 — Estados finales irreversibles):** Dado un pedido en estado `ENTREGADO`, cuando el repartidor intenta cambiar a cualquier otro estado, entonces el sistema rechaza la acción respondiendo HTTP 400.
* **AC-16 (RF-11):** Dado un repartidor que actualiza el estado de un pedido sin conexión 4G, cuando el celular recupera la señal, entonces el sistema sincroniza automáticamente el cambio pendiente en segundo plano sin intervención del usuario en un plazo máximo de 10 segundos, y el cambio queda reflejado en el panel del administrador.
* **AC-17 (RF-12):** Dados más de 20 pedidos registrados en el sistema, cuando se realiza la petición de listado enviando los parámetros `page` y `size`, entonces el backend devuelve estrictamente un subconjunto paginado respetando los límites solicitados.
* **AC-18 (RF-13 — Indicador de desconexión):** Dado el panel del administrador o supervisor sin conectividad, cuando pierde la conexión, entonces se muestra un banner rojo fijo con el texto "SIN CONEXIÓN" en la pantalla.
* **AC-19 (RF-13 — Recuperación de conexión):** Dado un panel con el banner "SIN CONEXIÓN" visible, cuando el navegador recupera la conexión, entonces el banner desaparece en menos de 2 segundos.
* **AC-20 (RF-14 — Configuración por entorno):** Dado el código fuente del frontend con configuración de desarrollo (localhost:5210) y de producción, cuando se construye con `ng build --configuration production`, entonces el bundle utiliza la URL de producción sin requerir modificación de componentes.
* **AC-21 (RF-14 — URL desde configuración):** Dado un componente del frontend que consume la configuración por entorno, cuando realiza una petición HTTP a la ruta `/api/pedido`, entonces en el panel de red del navegador la petición se resuelve contra la URL base definida en la configuración del entorno activo (desarrollo o producción), sin URLs hardcodeadas en el código del componente.
* **AC-22 (RF-10c — Listado de gestión de pedidos):** Dado un administrador que acaba de crear un pedido (estado `PENDIENTE`, sin asignación), cuando abre la pantalla de Pedidos, entonces el pedido aparece en el listado con su cliente, fecha de reparto, estado y total; y figura como candidato en la pantalla de Asignación para poder ser asignado.
* **AC-23 (RNF-06 — Fechas en hora local):** Dado un administrador que asigna un pedido con la fecha de hoy seleccionada, cuando lo hace en cualquier hora del día (p. ej. 20:00 hora Perú) o desde un navegador en otra zona horaria, entonces la fecha enviada corresponde al día operativo en America/Lima, el pedido aparece en la ruta del repartidor de ese día y las fechas mostradas coinciden con el día real (sin corrimiento por conversión a UTC).
* **AC-24 (RF-UX-04 — Cerrar sesión):** Dado un usuario autenticado (administrador, repartidor o supervisor), cuando ejecuta la acción "Cerrar sesión" desde la barra superior, entonces se eliminan sus credenciales locales y es redirigido a la pantalla de login; al volver atrás no accede a pantallas protegidas sin autenticarse de nuevo.
* **AC-25 (RF-UX-07 — Sin captura de IDs):** Dado el formulario de crear pedido, cuando el administrador elige el cliente, entonces **no existe ningún campo que solicite un ID**; la selección se realiza por nombre desde un modal con búsqueda (tabla paginada) y el formulario muestra el nombre del cliente elegido. La verificación se extiende a todos los formularios (repartidor, unidad u otras relaciones).

---

## Fuera de Alcance
Reportes PDF/Excel · cierre de día automatizado · registro público de usuarios · GPS o ubicación en tiempo real por mapa · auditoría avanzada de bases de datos · arquitectura multiempresa/multi-sucursal · aplicación móvil nativa (se emplea PWA/Web móvil) · notificaciones push masivas · chat interno · integraciones con ERP, WhatsApp o pasarelas de terceros · edición de pedidos ya creados (solo se permite crear, asignar y cambiar estado).

---

## Riesgos y Dependencias
* **Riesgo:** Intermitencia o pérdida de señal 4G mientras el repartidor se desplaza en moto.
  * *Mitigación:* Mecanismo de cola local en el almacenamiento del navegador móvil que reintenta automáticamente el envío del cambio de estado al recuperar la conexión (implementado en RF-11 y verificado en AC-16).
* **Riesgo:** Que un administrador intente editar un pedido ya en curso (`ASIGNADO`, `EN_RUTA`, etc.), alterando la trazabilidad operativa.
  * *Mitigación:* Validación estricta en el backend que rechaza cualquier edición si el estado actual es diferente de `PENDIENTE` (y la reasignación si no está en `PENDIENTE` o `REPROGRAMADO`), respondiendo HTTP 400 (regla incorporada en RF-05 y verificada en AC-06).
* **Riesgo:** Desfase de un día por conversión de fechas a UTC al crear o asignar pedidos, de modo que el repartidor no vea la asignación del día.
  * *Mitigación:* El frontend construye la fecha en componentes locales (`aFechaIsoLocal`) y las muestra sin reinterpretarlas como UTC; el backend opera el día operativo en America/Lima (regla incorporada en RNF-06 y verificada en AC-23).
* **Dependencia:** Estabilidad básica de la red móvil celular (4G/3G) para las sincronizaciones periódicas en ruta.
* **Dependencia:** Disponibilidad del servidor backend durante el horario operativo de reparto (06:00 a 20:00 horas).

---

## Nota de Decisiones Técnicas (fuera del alcance funcional del PRD)
Derivadas de RF-14, para el equipo de desarrollo (no son requerimientos):
* `environment.ts` (desarrollo) y `environment.prod.ts` (producción) como fuentes de la URL base.
* `ConfigService` inyectable que expone `apiUrl`; todos los componentes consumen `this.config.apiUrl` en lugar de valores fijos.

Decisiones de despliegue para la demo (no afectan el alcance funcional):
* **CORS libre:** por tratarse de un entorno de demo, la API permite cualquier origen (`AllowAnyOrigin`) para probar desde cualquier navegador/host; en producción se restringe a orígenes conocidos.
* **Compatibilidad de navegadores:** la app debe cargar en los navegadores objetivo (Chrome, Brave y Opera). Si una pestaña queda con carga indefinida por un Service Worker/caché de un build anterior, se limpian los datos del sitio y el Service Worker antes de reintentar.
