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

namespace DeliverySac.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize(Roles = "Administrador")]
public class PedidosController : ControllerBase
{
    private readonly DeliverySacDbContext _db;

    public PedidosController(DeliverySacDbContext db)
    {
        _db = db;
    }

    [HttpPost]
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

    [HttpGet("{id:int}")]
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

    private sealed class PedidoRow
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaReparto { get; set; }
        public int Estado { get; set; }
        public decimal Total { get; set; }
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
