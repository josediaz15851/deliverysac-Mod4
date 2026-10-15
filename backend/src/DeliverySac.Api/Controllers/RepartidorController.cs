using System.Security.Claims;
using DeliverySac.Api.Application;
using DeliverySac.Api.Contracts.Repartidor;
using DeliverySac.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliverySac.Api.Controllers;

[ApiController]
[Route("api/repartidor")]
[Authorize(Roles = "Repartidor")]
public class RepartidorController : ControllerBase
{
    private readonly DeliverySacDbContext _db;

    public RepartidorController(DeliverySacDbContext db)
    {
        _db = db;
    }

    [HttpGet("pedidos")]
    public async Task<ActionResult<IEnumerable<PedidoRutaDto>>> Pedidos(CancellationToken cancellationToken)
    {
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var hoy = FechaOperativa.Hoy();

        var pedidos = await _db.Pedidos
            .AsNoTracking()
            .Where(p => p.FechaReparto == hoy
                && p.Asignaciones.Any(a => a.Activa && a.RepartidorId == usuarioId))
            .OrderBy(p => p.Id)
            .Select(p => new PedidoRutaDto
            {
                Id = p.Id,
                ClienteNombre = p.Cliente.Nombre,
                Direccion = p.Cliente.Direccion,
                Estado = p.Estado.ToString(),
                FechaReparto = p.FechaReparto,
                Total = p.Total,
            })
            .ToListAsync(cancellationToken);

        return Ok(pedidos);
    }
}
