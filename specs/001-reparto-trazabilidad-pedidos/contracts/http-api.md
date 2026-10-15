# Phase 1 API Contract: DeliverySac HTTP API

**Feature**: `001-reparto-trazabilidad-pedidos` | **Date**: 2026-10-14

Contrato REST del backend. Base URL de desarrollo: `http://localhost:5210`. Todas las rutas usan el
prefijo `/api`. Formato de error uniforme: `application/problem+json` (ProblemDetails).

**Autorización**: `[ADMIN]` = Administrador, `[REP]` = Repartidor, `[SUP]` = Supervisor. Las rutas
marcadas requieren `Authorization: Bearer <token>`. Un rol no autorizado recibe `403`.

## Convenciones

- Paginación: query `page` (1-based) y `size`; respuesta `{ items, page, size, total }` (FR-021).
- Fechas de reparto/asignación: string `yyyy-MM-dd` en hora America/Lima (sin UTC) (FR-029).
- Marcas de tiempo de trazabilidad: ISO-8601 con offset, mostradas en America/Lima.
- Idempotencia de cambios de estado: `operacionId` (uuid) enviado por el cliente.

## Autenticación

### POST /api/auth/login

- **Auth**: ninguna.
- **Request**: `{ "usuario": "string", "contrasena": "string" }`
- **200**: `{ "token": "string", "usuario": { "id": 1, "nombreUsuario": "string", "rol": "Administrador|Repartidor|Supervisor" } }`
- **401**: credenciales inválidas (FR-001).

## Clientes `[ADMIN]`

### POST /api/clientes

- **Request**: `{ "nombre": "string", "direccion": "string" }`
- **201**: `{ "id": 1, "nombre": "string", "direccion": "string" }`
- **400**: `nombre` o `direccion` vacíos (FR-003).

### GET /api/clientes?page=&size=&q=

- **200**: paginado de clientes (FR-021).

### GET /api/clientes/buscar?q=

- **200**: `[{ "id": 1, "nombre": "string", "direccion": "string" }]` — máximo ~20 resultados, para
  autocompletar (FR-031, sin exponer IDs como entrada).

### GET /api/clientes/{id}

- **200**: cliente; **404** si no existe.

## Pedidos `[ADMIN]`

### POST /api/pedidos

- **Request**:
  ```json
  {
    "clienteId": 1,
    "fechaReparto": "2026-10-14",
    "comentario": "string?",
    "lineas": [
      { "producto": "string", "cantidad": 2, "precioUnitario": 2.50, "descripcion": "string" }
    ]
  }
  ```
- **201**: pedido creado con `estado = "PENDIENTE"` y `total` calculado.
- **400**: sin líneas o líneas inválidas (`cantidad > 0`, `precioUnitario >= 0`, `descripcion`
  obligatoria) (FR-004/FR-005).

### GET /api/pedidos/{id}

- **200**: cabecera + líneas (con precio unitario) + `total` + `estado`; **404** si no existe.

### GET /api/pedidos?page=&size=

- **200**: listado de gestión con todos los pedidos (incluye `PENDIENTE` sin asignación); cada ítem
  `{ id, cliente, fechaReparto, estado, total }` (FR-020, AC-22).

### GET /api/pedidos/{id}/trazabilidad

- **200**: `[{ "estadoAnterior", "estadoNuevo", "fechaHora", "usuario", "motivo" }]` — solo lectura.
- **405**: cualquier intento de modificar/eliminar trazas (no existe endpoint de escritura) (FR-017).

## Listado del día `[ADMIN]` `[SUP]`

### GET /api/pedidos/dia?page=&size=

- **200**: pedidos de la fecha actual (según `fechaReparto`, America/Lima) más los `REPROGRAMADO` de
  días anteriores; cada ítem `{ id, cliente, fechaReparto, estado, horaUltimaModificacion, motivo }`
  (FR-018/FR-019, AC-12). El supervisor obtiene la misma data en solo lectura.

## Asignación `[ADMIN]`

### POST /api/pedidos/{pedidoId}/asignaciones

- **Request**: `{ "repartidorId": 1, "unidadId": 1, "fecha": "2026-10-14" }`
- **200**: asignación creada; el pedido pasa a `ASIGNADO` (o vuelve a `ASIGNADO` desde `REPROGRAMADO`).
- **400**: el pedido no está en `PENDIENTE` ni `REPROGRAMADO` (FR-010, AC-06).

### GET /api/pedidos/candidatos?page=&size=

- **200**: paginado de pedidos en estado `PENDIENTE` o `REPROGRAMADO`, candidatos para asignar
  (FR-020).

## Ruta del repartidor `[REP]`

### GET /api/repartidor/pedidos

- **200**: pedidos asignados al repartidor autenticado en la fecha actual; cada ítem incluye cabecera
  y líneas con precio unitario (FR-011/FR-012, AC-08).
- **403**: si un repartidor intenta acceder a pedidos de otro (FR-011, AC-07).

### POST /api/pedidos/{pedidoId}/estados

- **Request**: `{ "estadoNuevo": "EN_RUTA", "motivo": "string?", "operacionId": "uuid" }`
- **200**: traza registrada; el servidor asigna `fechaHora` y `usuario` (FR-016).
- **400**: transición inválida según la máquina de estados (FR-013/FR-015) o `motivo` faltante en
  `NO_ENTREGADO`/`REPROGRAMADO` (FR-014).
- **403**: el pedido no está asignado al repartidor autenticado (FR-011).
- **409/200** (idempotencia): si `operacionId` ya fue procesado, no se duplica la traza.

## Catálogos `[ADMIN]`

### GET /api/repartidores

- **200**: `[{ "id": 1, "nombreCompleto": "string" }]` — conjunto pequeño y fijo; puede usarse combo
  (FR-031).

### GET /api/unidades

- **200**: `[{ "id": 1, "identificador": "string", "tipo": "MotoLineal|Furgoneta" }]`.

## Códigos de estado transversales

- **400** validación/regla de negocio; **401** no autenticado; **403** no autorizado; **404** no
  encontrado; **405** método no permitido (p. ej. intento de editar trazabilidad).
- Comunicación por HTTPS/TLS 1.2+ en entornos no locales (FR-027).
