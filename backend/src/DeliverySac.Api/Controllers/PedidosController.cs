using DeliverySac.Api.Application;
using DeliverySac.Api.Contracts.Pedidos;
using DeliverySac.Api.Domain;
using DeliverySac.Api.Domain.Enums;
using DeliverySac.Api.Infrastructure;
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
