using DeliverySac.Api.Application;
using DeliverySac.Api.Contracts.Common;
using DeliverySac.Api.Contracts.Pedidos;
using DeliverySac.Api.Domain;
using DeliverySac.Api.Domain.Enums;
using DeliverySac.Api.Infrastructure;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeliverySac.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly DeliverySacDbContext _db;
    private readonly EstadoPedidoService _estadoPedidoService;

    public PedidosController(DeliverySacDbContext db, EstadoPedidoService estadoPedidoService)
    {
        _db = db;
        _estadoPedidoService = estadoPedidoService;
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<PedidoDetalleDto>> Crear(
        [FromBody] CrearPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var lineas = request.Lineas
            .Select(l => new PedidoLinea
            {
                Producto = l.Producto.Trim(),
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario,
                Descripcion = l.Descripcion.Trim(),
            })
            .ToList();

        if (!PedidoCalculator.SonLineasValidas(lineas))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Pedido inválido",
                Detail = "El pedido requiere al menos una línea válida (cantidad > 0, precio >= 0 y descripción).",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var cliente = await _db.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);

        if (cliente is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Cliente inválido",
                Detail = "El cliente indicado no existe.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var pedido = new Pedido
        {
            ClienteId = request.ClienteId,
            FechaReparto = request.FechaReparto,
            Comentario = request.Comentario?.Trim(),
            Estado = EstadoPedido.Pendiente,
            Total = PedidoCalculator.CalcularTotal(lineas),
            CreatedAt = DateTimeOffset.UtcNow,
            Lineas = lineas,
        };

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = pedido.Id }, ToDetalleDto(pedido, cliente.Nombre));
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<PagedResult<PedidoDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var currentPage = page < 1 ? 1 : page;
        var pageSize = size is < 1 or > 100 ? 10 : size;
        var offset = (currentPage - 1) * pageSize;

        const string sql = """
            SELECT p.Id, c.Nombre AS ClienteNombre, p.FechaReparto, p.Estado, p.Total,
                   COUNT(*) OVER() AS TotalRegistros
            FROM Pedidos p
            INNER JOIN Clientes c ON c.Id = p.ClienteId
            ORDER BY p.FechaReparto DESC, p.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var connection = _db.Database.GetDbConnection();
        var rows = (await connection.QueryAsync<PedidoRow>(
            sql,
            new { Offset = offset, PageSize = pageSize })).ToList();

        var result = new PagedResult<PedidoDto>
        {
            Items = rows.Select(r => new PedidoDto
            {
                Id = r.Id,
                ClienteNombre = r.ClienteNombre,
                FechaReparto = DateOnly.FromDateTime(r.FechaReparto),
                Estado = ((EstadoPedido)r.Estado).ToString(),
                Total = r.Total,
            }).ToList(),
            Total = rows.Count > 0 ? (int)rows[0].TotalRegistros : 0,
        };

        return Ok(result);
    }

    [HttpGet("dia")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<PagedResult<PedidoDiaDto>>> ListarDia(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var currentPage = page < 1 ? 1 : page;
        var pageSize = size is < 1 or > 100 ? 10 : size;
        var offset = (currentPage - 1) * pageSize;

        const string sql = """
            SELECT p.Id, c.Nombre AS ClienteNombre, c.Direccion, p.FechaReparto, p.Estado,
                   ultimo.Motivo AS Motivo, ultimo.FechaHora AS FechaHoraCambio,
                   rep.NombreUsuario AS RepartidorNombre,
                   COUNT(*) OVER() AS TotalRegistros
            FROM Pedidos p
            INNER JOIN Clientes c ON c.Id = p.ClienteId
            OUTER APPLY (
                SELECT TOP 1 ce.Motivo, ce.FechaHora
                FROM CambiosEstado ce
                WHERE ce.PedidoId = p.Id
                ORDER BY ce.FechaHora DESC, ce.Id DESC
            ) ultimo
            OUTER APPLY (
                SELECT TOP 1 u.NombreUsuario
                FROM Asignaciones a
                INNER JOIN Usuarios u ON u.Id = a.RepartidorId
                WHERE a.PedidoId = p.Id AND a.Activa = 1
            ) rep
            WHERE p.FechaReparto = @Hoy OR p.Estado = @Reprogramado
            ORDER BY p.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var connection = _db.Database.GetDbConnection();
        var rows = (await connection.QueryAsync<PedidoDiaRow>(
            sql,
            new
            {
                Offset = offset,
                PageSize = pageSize,
                Hoy = DateOnly.FromDateTime(DateTime.UtcNow).ToDateTime(TimeOnly.MinValue),
                Reprogramado = (int)EstadoPedido.Reprogramado,
            })).ToList();

        var result = new PagedResult<PedidoDiaDto>
        {
            Items = rows.Select(r => new PedidoDiaDto
            {
                Id = r.Id,
                ClienteNombre = r.ClienteNombre,
                Direccion = r.Direccion,
                FechaReparto = DateOnly.FromDateTime(r.FechaReparto),
                Estado = ((EstadoPedido)r.Estado).ToString(),
                Motivo = r.Motivo,
                FechaHoraCambio = r.FechaHoraCambio,
                RepartidorNombre = r.RepartidorNombre,
            }).ToList(),
            Total = rows.Count > 0 ? (int)rows[0].TotalRegistros : 0,
        };

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<PedidoDetalleDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var pedido = await _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Lineas)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (pedido is null)
        {
            return NotFound();
        }

        return Ok(ToDetalleDto(pedido, pedido.Cliente.Nombre));
    }

    [HttpPost("{id:int}/estados")]
    [Authorize(Roles = "Repartidor")]
    public async Task<IActionResult> CambiarEstado(
        int id,
        [FromBody] CambioEstadoRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EstadoPedido>(request.EstadoNuevo, ignoreCase: true, out var estadoNuevo))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Estado inválido",
                Detail = $"El estado '{request.EstadoNuevo}' no es válido.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var operacionId = request.OperacionId ?? Guid.NewGuid();

        var resultado = await _estadoPedidoService.CambiarEstadoAsync(
            id,
            estadoNuevo,
            request.Motivo,
            usuarioId,
            operacionId,
            cancellationToken);

        return resultado switch
        {
            ResultadoCambioEstado.Ok => Ok(),
            ResultadoCambioEstado.PedidoNoEncontrado => NotFound(),
            ResultadoCambioEstado.NoPropietario => Forbid(),
            ResultadoCambioEstado.TransicionInvalida => BadRequest(new ProblemDetails
            {
                Title = "Transición inválida",
                Detail = "El cambio de estado solicitado no está permitido.",
                Status = StatusCodes.Status400BadRequest,
            }),
            ResultadoCambioEstado.MotivoRequerido => BadRequest(new ProblemDetails
            {
                Title = "Motivo requerido",
                Detail = "Debe indicar un motivo para este estado.",
                Status = StatusCodes.Status400BadRequest,
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    [HttpGet("{id:int}/trazabilidad")]
    [Authorize(Roles = "Administrador,Repartidor,Supervisor")]
    public async Task<ActionResult<TrazabilidadDto>> Trazabilidad(int id, CancellationToken cancellationToken)
    {
        var items = await _db.CambiosEstado
            .AsNoTracking()
            .Where(c => c.PedidoId == id)
            .OrderBy(c => c.FechaHora)
            .ThenBy(c => c.Id)
            .Select(c => new TrazabilidadItemDto
            {
                Id = c.Id,
                EstadoAnterior = c.EstadoAnterior == null ? null : c.EstadoAnterior.ToString(),
                EstadoNuevo = c.EstadoNuevo.ToString(),
                FechaHora = c.FechaHora,
                Usuario = c.Usuario.NombreUsuario,
                Motivo = c.Motivo,
            })
            .ToListAsync(cancellationToken);

        return Ok(new TrazabilidadDto { Items = items });
    }

    private sealed class PedidoRow
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaReparto { get; set; }
        public int Estado { get; set; }
        public decimal Total { get; set; }
        public long TotalRegistros { get; set; }
    }

    private sealed class PedidoDiaRow
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaReparto { get; set; }
        public int Estado { get; set; }
        public string? Motivo { get; set; }
        public DateTimeOffset? FechaHoraCambio { get; set; }
        public string? RepartidorNombre { get; set; }
        public long TotalRegistros { get; set; }
    }

    private static PedidoDetalleDto ToDetalleDto(Pedido pedido, string clienteNombre)
    {
        return new PedidoDetalleDto
        {
            Id = pedido.Id,
            ClienteNombre = clienteNombre,
            FechaReparto = pedido.FechaReparto,
            Estado = pedido.Estado.ToString(),
            Total = pedido.Total,
            Comentario = pedido.Comentario,
            Lineas = pedido.Lineas
                .Select(l => new PedidoLineaDto
                {
                    Id = l.Id,
                    Producto = l.Producto,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    Descripcion = l.Descripcion,
                })
                .ToList(),
        };
    }
}
