# Quickstart: Reparto y Trazabilidad de Pedidos (DeliverySac)

**Feature**: `001-reparto-trazabilidad-pedidos` | **Date**: 2026-10-14

Guía para levantar el sistema y validar el comportamiento de punta a punta. Los detalles de
contratos están en [contracts/http-api.md](./contracts/http-api.md) y el modelo en
[data-model.md](./data-model.md).

## Prerequisites

- .NET 8 SDK; SQL Server Express.
- Node.js LTS + Angular CLI (`npm install -g @angular/cli`).
- Variables de entorno (no versionar valores):
  - `DELIVERYSAC_CONNECTION`: cadena de conexión a SQL Server Express.
  - `DELIVERYSAC_ADMIN_PASSWORD`: contraseña para el sembrado de datos base en Development.

## Setup

```bash
# Backend
dotnet restore backend/DeliverySac.sln
dotnet build backend/DeliverySac.sln

# Frontend
cd frontend
npm install
```

## Run

```bash
# Backend (perfil http, http://localhost:5210)
dotnet run --project backend/src/DeliverySac.Api

# Frontend (http://localhost:4200)
cd frontend
ng serve
```

En Development se siembran usuarios (Administrador, Repartidor, Supervisor), unidades (3 motos
lineales y 2 furgonetas) y un cliente base.

## Tests

```bash
# Backend (xUnit) — ciclo rojo-verde-refactor
dotnet test backend/DeliverySac.sln

# Frontend
cd frontend
npm test
```

## Escenarios de validación (end-to-end)

1. **Login por rol (RF-01, AC-01, AC-24)**: iniciar sesión con cada rol y verificar la redirección a
   su pantalla inicial; "Cerrar sesión" vuelve al login y bloquea el retroceso a pantallas
   protegidas.
2. **Cliente y pedido con total (RF-02/03/04, AC-02/03/04)**: registrar un cliente; crear un pedido
   con líneas (2 × 2.50 + 1 × 7.00) y confirmar `total = 12.00`; guardar sin líneas o cantidad 0 debe
   responder HTTP 400.
3. **Asignación y ruta del repartidor (RF-05/06/07, AC-05/07/08)**: asignar un pedido `PENDIENTE`;
   confirmar que aparece solo en la ruta de ese repartidor; acceder por URL/API a un pedido ajeno
   debe responder HTTP 403; verificar el detalle sin scroll horizontal a 320px.
4. **Máquina de estados y trazabilidad (RF-08/09, AC-09/10/11/14/15)**: recorrer
   `ASIGNADO → EN_RUTA → ENTREGADO` registrando hora y usuario; `NO_ENTREGADO`/`REPROGRAMADO` sin
   motivo debe bloquearse; `PENDIENTE → ENTREGADO` y cambios desde `ENTREGADO` deben responder
   HTTP 400; intentar editar la traza debe responder HTTP 405.
5. **Panel del día y supervisor (RF-10/10a, AC-12/13)**: con un pedido del día y uno `REPROGRAMADO`
   de días anteriores, ambos aparecen con estado, hora y motivo; el listado se refresca cada 5 s.
6. **Offline y conectividad (RF-11/13, AC-16/18/19)**: marcar un estado sin conexión, restablecer la
   señal y confirmar la sincronización <= 10 s; al perder conexión aparece el banner "SIN CONEXIÓN"
   y desaparece en < 2 s al recuperarla. Si el servidor rechaza el cambio por conflicto, se descarta
   el local, se notifica y se refresca (RF-022).
7. **Paginación (RF-12, AC-17)**: con > 20 pedidos, enviar `page`/`size` y confirmar el subconjunto
   respetado.
8. **Configuración por entorno (RF-14, AC-20/21)**: construir para producción y verificar que las
   peticiones se resuelven contra la URL del entorno activo sin editar componentes.
9. **Fechas en hora local (RNF-06, AC-23)**: asignar a las 20:00 hora Perú o desde otra zona horaria
   y confirmar que el pedido aparece en la ruta del día correcto.
10. **Sin captura de IDs (RF-UX-07/17, AC-25)**: verificar que los formularios no exponen campos de
    ID y que la selección se hace por nombre (autocompletar/modal con tabla).
11. **Patrones UI y accesibilidad (PRD §7, AC-UX-01/03/05)**: confirmar la matriz de contenedores
    (autocompletar/modal, `p-dialog` 450px para altas cortas, `p-sidebar` para edición extensa,
    `p-confirmDialog` para acciones irreversibles, `p-table` lazy); notificaciones vía Toast; Enter
    guarda y Esc cancela en modales; tokens de tema (sin hex/estilos inline) y etiquetas de estado con
    color. Ver [contracts/ui-ux-patterns.md](./contracts/ui-ux-patterns.md).

## Expected Outcomes

- `dotnet test` y `npm test` en verde.
- Todos los códigos HTTP de validación/autorización (400/403/405) coinciden con los contratos.
- Sin dependencias ni llamadas a servicios externos de terceros.
