# Validación del quickstart — DeliverySac

Resultado de la validación de los escenarios end-to-end del
[quickstart](../specs/001-reparto-trazabilidad-pedidos/quickstart.md) (feature
`001-reparto-trazabilidad-pedidos`).

| # | Escenario | Cobertura | Estado |
|---|---|---|---|
| 1 | Login por rol | `AuthEndpointsTests` + login spec | OK |
| 2 | Cliente y pedido con total | `ClientesEndpointsTests`, `PedidosEndpointsTests` | OK |
| 3 | Asignación y ruta del repartidor | `AsignacionesEndpointsTests`, `EstadosEndpointsTests` | OK |
| 4 | Máquina de estados y trazabilidad | `EstadoPedidoTests`, `EstadosEndpointsTests` | OK |
| 5 | Panel del día y supervisor | `ListadoDiaTests`, `listado-dia` spec | OK |
| 6 | Offline y conectividad | `OfflineService` spec, `OfflineSyncTests`, `IdempotenciaTests` | OK |
| 7 | Paginación | `PedidosEndpointsTests` | OK |
| 8 | Configuración por entorno | build de producción (apiUrl por entorno) | OK |
| 9 | Fechas en hora local | `FechasTests` | OK |
| 10 | Sin captura de IDs | formularios por selección (cliente por modal) | OK |
| 11 | Patrones UI y accesibilidad | `theme.spec`, `a11y.spec`, `responsive.spec` | OK |

## Evidencia

- Backend: `dotnet test` en verde (unit + integración, incluida idempotencia y rendimiento).
- Frontend: `npm run test:ci` en verde y `ng build` sin errores.
- CORS libre para la demo (`AllowAnyOrigin`); TLS 1.2+ y HSTS en producción.
- Sin dependencias ni llamadas a servicios externos de terceros.
