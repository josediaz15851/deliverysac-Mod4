# Phase 1 Data Model: Reparto y Trazabilidad de Pedidos (DeliverySac)

**Feature**: `001-reparto-trazabilidad-pedidos` | **Date**: 2026-10-14

Modelo derivado de la spec (Key Entities) y de las reglas de negocio del PRD. Tipos orientativos para
.NET 8 + SQL Server Express.

## Enumeraciones

### EstadoPedido

`PENDIENTE`, `ASIGNADO`, `EN_RUTA`, `ENTREGADO`, `NO_ENTREGADO`, `REPROGRAMADO`.

- Estado inicial al crear un pedido: `PENDIENTE`.
- Estado terminal: `ENTREGADO` (irreversible, FR-015).

### RolUsuario

`Administrador`, `Repartidor`, `Supervisor`.

### TipoUnidad

`MotoLineal`, `Furgoneta`.

## Entidades

### Usuario

| Campo | Tipo | Reglas |
|-------|------|--------|
| Id | int (PK) | Autogenerado |
| NombreUsuario | string | Requerido, único |
| NombreCompleto | string | Requerido |
| PasswordHash | string | Requerido; bcrypt coste >= 10 (FR-026) |
| Rol | RolUsuario | Requerido |
| Activo | bool | Por defecto `true` |

- Responsable atribuible de cada cambio de estado (FR-016).
- Relaciones: 1 → N `CambioEstado`; 1 → N `Asignacion` (como repartidor).

### Cliente

| Campo | Tipo | Reglas |
|-------|------|--------|
| Id | int (PK) | Autogenerado |
| Nombre | string | Requerido (FR-003) |
| Direccion | string | Requerido (FR-003) |

- Relaciones: 1 → N `Pedido`.

### Unidad

| Campo | Tipo | Reglas |
|-------|------|--------|
| Id | int (PK) | Autogenerado |
| Identificador | string | Requerido, único (p. ej. placa/código) |
| Tipo | TipoUnidad | Requerido |
| Activo | bool | Por defecto `true` |

- Datos sembrados (3 motos lineales, 2 furgonetas); sin CRUD administrativo (ver Assumptions del spec).
- Relaciones: 1 → N `Asignacion`.

### Pedido

| Campo | Tipo | Reglas |
|-------|------|--------|
| Id | int (PK) | Autogenerado |
| ClienteId | int (FK) | Requerido |
| FechaReparto | DateOnly | Requerido; `yyyy-MM-dd` hora America/Lima sin conversión a UTC (FR-029) |
| Comentario | string? | Opcional |
| Estado | EstadoPedido | Requerido; inicia en `PENDIENTE` |
| Total | decimal(18,2) | Calculado = Σ(cantidad × precioUnitario) (FR-006); solo lectura |
| CreatedAt | DateTimeOffset | Hora de creación (UTC) |

- Relaciones: N → 1 `Cliente`; 1 → N `PedidoLinea`; 1 → N `Asignacion` (una activa); 1 → N
  `CambioEstado`.

### PedidoLinea

| Campo | Tipo | Reglas |
|-------|------|--------|
| Id | int (PK) | Autogenerado |
| PedidoId | int (FK) | Requerido |
| Producto | string | Requerido |
| Cantidad | int | Requerido; `> 0` (FR-005) |
| PrecioUnitario | decimal(18,2) | Requerido; `>= 0` (FR-005) |
| Descripcion | string | Requerido (FR-005) |

- Un pedido requiere una o más líneas; sin líneas o con valores inválidos se responde HTTP 400.

### Asignacion

| Campo | Tipo | Reglas |
|-------|------|--------|
| Id | int (PK) | Autogenerado |
| PedidoId | int (FK) | Requerido |
| RepartidorId | int (FK Usuario) | Requerido; rol `Repartidor` |
| UnidadId | int (FK Unidad) | Requerido |
| Fecha | DateOnly | Requerido; `yyyy-MM-dd` hora America/Lima |
| Activa | bool | Una sola activa por pedido (índice único filtrado) (FR-008) |
| CreatedAt | DateTimeOffset | Hora de creación (UTC) |

- Reasignación: válida solo si el pedido está en `PENDIENTE` o `REPROGRAMADO` (FR-009/FR-010). Al
  reasignar, la asignación anterior pasa a `Activa = false` y se crea una nueva; el pedido vuelve a
  `ASIGNADO` si estaba `REPROGRAMADO`, conservando su historial.

### CambioEstado (traza — inmutable)

| Campo | Tipo | Reglas |
|-------|------|--------|
| Id | long (PK) | Autogenerado |
| PedidoId | int (FK) | Requerido |
| EstadoAnterior | EstadoPedido? | Nulo en el primer registro si aplica |
| EstadoNuevo | EstadoPedido | Requerido |
| FechaHora | DateTimeOffset | Requerido; asignado por el servidor (no editable) (FR-016) |
| UsuarioId | int (FK Usuario) | Requerido; asignado por el servidor (no editable) (FR-016) |
| Motivo | string? | Obligatorio cuando `EstadoNuevo` ∈ {`NO_ENTREGADO`, `REPROGRAMADO`} (FR-014) |
| OperacionId | guid | Identificador idempotente enviado por el cliente (evita trazas duplicadas al reintentar, R8) |

- **Inmutable**: sin operaciones de actualización/eliminación; no existen endpoints que la modifiquen
  (FR-017, HTTP 405).

## Máquina de estados (transiciones válidas)

| Desde | Hacia | Disparador | Notas |
|-------|-------|------------|-------|
| `PENDIENTE` | `ASIGNADO` | Asignación del administrador | Crea asignación activa (FR-007) |
| `ASIGNADO` | `EN_RUTA` | Repartidor | Registra traza |
| `EN_RUTA` | `ENTREGADO` | Repartidor | Estado terminal (FR-015) |
| `EN_RUTA` | `NO_ENTREGADO` | Repartidor | Requiere `Motivo` (FR-014) |
| `EN_RUTA` | `REPROGRAMADO` | Repartidor | Requiere `Motivo` (FR-014) |
| `REPROGRAMADO` | `ASIGNADO` | Reasignación del administrador | Nueva asignación activa; conserva historial (FR-009) |

Transiciones no listadas (p. ej. `PENDIENTE → ENTREGADO`, cualquiera desde `ENTREGADO`) se rechazan
con HTTP 400 (FR-013/FR-015). El repartidor solo puede operar pedidos asignados a él y del día
actual (FR-011, HTTP 403 en caso contrario).

## Reglas de validación (resumen trazable)

- Cliente: `Nombre` y `Direccion` obligatorios (FR-003).
- Pedido: al menos una línea; cada línea `Cantidad > 0`, `PrecioUnitario >= 0`, `Descripcion`
  obligatoria; si no, HTTP 400 (FR-004/FR-005).
- Total: `Σ(Cantidad × PrecioUnitario)`, precisión 2 decimales (FR-006).
- Estado: respeta la máquina de estados; `Motivo` obligatorio en `NO_ENTREGADO`/`REPROGRAMADO`
  (FR-013/FR-014).
- No reasignar/editar si el estado no es `PENDIENTE` o `REPROGRAMADO` (HTTP 400, FR-010).

## Índices y restricciones

- `Usuario(NombreUsuario)` único.
- `Asignacion(PedidoId)` con índice único filtrado sobre `Activa = 1` (una asignación activa por
  pedido).
- `CambioEstado(PedidoId, FechaHora)` para el listado de trazabilidad.
- `Pedido(FechaReparto, Estado)` para el listado del día.
- `CambioEstado(OperacionId)` único para idempotencia de sincronización offline.
