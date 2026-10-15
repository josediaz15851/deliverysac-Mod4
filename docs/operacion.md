# DeliverySac — Operación y demo

Sistema web de reparto y trazabilidad de pedidos. Documentación complementaria al
[PRD](../PRD.md) y al [quickstart de la feature](../specs/001-reparto-trazabilidad-pedidos/quickstart.md).

## Stack

- **Backend**: .NET 8 (LTS) + EF Core (comandos) + Dapper (consultas) + SQL Server Express.
- **Frontend**: Angular 19 + PrimeNG 19 (tema institucional, primario `#1976D2`) + PWA (Sakai).
- **Tests**: xUnit (backend) y Karma/Jasmine (frontend).

## Levantar el entorno

```bash
# Backend (perfil http, http://localhost:5210)
dotnet run --project backend/src/DeliverySac.Api --environment Development

# Frontend (http://localhost:4200)
cd frontend
npm install
ng serve
```

> La instancia de SQL Server Express de demo es `.\MSSQLSERVER01` (Windows auth). La cadena de
> conexión se define por `DELIVERYSAC_CONNECTION` o `backend/src/DeliverySac.Api/appsettings.Development.local.json`
> (no versionado).

## Usuarios de demo

Sembrados en Development con la clave `DELIVERYSAC_ADMIN_PASSWORD` (demo: `Admin.123`).

| Usuario | Rol |
|---|---|
| `admin` | Administrador |
| `repartidor` | Repartidor |
| `supervisor` | Supervisor |

## Tests

```bash
# Backend
dotnet test backend/

# Frontend
cd frontend
npm run test:ci
npx ng build
```

## API principal

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | público | Inicio de sesión (JWT) |
| GET/POST | `/api/clientes` | Administrador | CRUD de clientes |
| GET/POST | `/api/pedidos` | Administrador | Listado/alta de pedidos (total calculado) |
| GET | `/api/pedidos/{id}` | Administrador | Detalle |
| POST | `/api/pedidos/{id}/asignaciones` | Administrador | Asignar repartidor/unidad |
| POST | `/api/pedidos/{id}/estados` | Repartidor | Cambio de estado (idempotente por `operacionId`) |
| GET | `/api/pedidos/{id}/trazabilidad` | Admin/Repartidor/Supervisor | Traza inmutable |
| GET | `/api/repartidor/pedidos` | Repartidor | Ruta del día (pedidos propios) |
| GET | `/api/pedidos/dia` | Admin/Supervisor | Panel del día (hoy + reprogramados previos) |

## Operación offline (repartidor)

- Los cambios de estado se guardan en una cola persistida en **IndexedDB**.
- Al recuperar la conexión (evento `online`) se envían automáticamente, con reintentos y backoff.
- Cada cambio lleva un `operacionId` único; reenviar la misma operación **no duplica** la traza
  (idempotencia por índice único en el servidor).
- Si el servidor responde conflicto (400/409), el cambio local se descarta y se notifica por Toast.
