# Phase 1 UI Contract: Patrones PrimeNG / Sakai (DeliverySac)

**Feature**: `001-reparto-trazabilidad-pedidos` | **Date**: 2026-10-14

Contrato de interfaz de usuario derivado de `PRD.md` §7 y de las convenciones de `AGENTS.md`. Define
el mapeo obligatorio operación → componente → contenedor → interacción. Aplica a todo formulario,
tabla o selector del sistema.

## Shell y layout (PRD §7.1)

- **Admin/Supervisor**: plantilla Sakai con `app-layout` (`app-topbar` + `app-sidebar` colapsable +
  `app-footer`).
- **Repartidor**: sin `app-sidebar`; maquetación de una columna mobile-first desde 320 px.
- **Tokens**: consumir solo variables CSS globales (`--primary-color`, `--surface-card`,
  `--text-color`). Prohibido hardcodear hexadecimales o usar `style="..."` inline.
- **Navegación**: cada pantalla muestra título + breadcrumb; el menú resalta la sección activa; una
  pantalla y una opción de menú por función/CRUD.

## Matriz de patrones (PRD §7.2)

| Operación / Contexto | Componente | Contenedor | Regla de interacción |
|---|---|---|---|
| Selección con alta carga (Clientes, Unidades, Repartidores) | `p-autoComplete` | Inline / modal con `p-table` | Búsqueda en servidor con debounce 300 ms; `pTemplate="item"` para info de apoyo. Prohibido el ingreso manual de IDs. |
| Altas y formularios cortos (≤ 4 campos) | `p-dialog` | Modal centrado (width 450 px) | `modal="true"`, foco en el primer input, opción "Guardar y crear otro" (`p-checkbox`). |
| Edición y formularios extensos (> 5 campos) | `p-sidebar` | Panel lateral (`position="right"`, width 450 px) | Desliza desde la derecha manteniendo visible el listado. |
| Acciones irreversibles (reasignar, marcar estado final) | `p-confirmDialog` | Modal centrado small | Invocado vía `ConfirmationService`, botón primario `severity="danger"`. |
| Listados principales | `p-table` | Tarjeta Sakai (`.card`) | Paginación por servidor `[lazy]="true"` con `page`/`size`. |

## Feedback, carga y estados (PRD §7.3 / §7.5)

- **Notificaciones**: todo evento de API (éxito, validación, error de servidor) se emite con
  `MessageService` (`<p-toast>`) en la esquina superior derecha. Prohibido `alert()` u otros diálogos
  nativos.
- **Estados de espera**: `p-skeleton` en tablas/formularios durante la carga y `[loading]="true"` en
  `p-button`.
- **Máquina de estados de UI**:
  - `[ Sin Datos ]` → `p-skeleton`/ProgressSpinner durante la carga.
  - `[ Con Datos ]` → `p-table` con paginador `page`/`size`.
  - `[ Lista Vacía ]` → mensaje "No se encontraron registros" + botón de acción principal.
  - `[ Error API ]` → Toast `severity="error"` + conservar datos del formulario para reintento.

## Accesibilidad y teclado (PRD §7.3 / §7.4)

- **RF-UX-19 / AC-UX-01**: en `p-dialog`/`p-sidebar`, Enter ejecuta el guardado principal y Esc
  cancela/cierra descartando cambios no guardados.
- **RF-UX-14 / AC-UX-04**: `label for` asociado, anillo de foco del tema visible, contraste AA y
  navegación por teclado; sin scroll horizontal desde 320 px en ninguna vista operativa.

## Criterios de aceptación de UI (PRD §7.4)

- **AC-UX-01**: manejo de Enter/Esc en diálogos y paneles laterales.
- **AC-UX-02**: cero exposición/captura de IDs en formularios de registro/asignación.
- **AC-UX-03**: consistencia de notificaciones vía `<p-toast>`.
- **AC-UX-04**: diseño adaptable del repartidor (≥ 320 px, sin scroll horizontal).
- **AC-UX-05**: consistencia temática; todos los componentes heredan tokens e iconografía PrimeIcons.
