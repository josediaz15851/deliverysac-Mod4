using System.Security.Claims;
using Dapper;
using DeliverySac.Api.Contracts.Asignaciones;
using DeliverySac.Api.Contracts.Common;
using DeliverySac.Api.Contracts.Pedidos;
using DeliverySac.Api.Domain;
using DeliverySac.Api.Domain.Enums;
using DeliverySac.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliverySac.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "Administrador")]
public class AsignacionesController : ControllerBase
{
    private readonly DeliverySacDbContext _db;

    public AsignacionesController(DeliverySacDbContext db)
    {
        _db = db;
    }

    [HttpPost("pedidos/{pedidoId:int}/asignaciones")]
    public async Task<IActionResult> Asignar(
        int pedidoId,
        [FromBody] CrearAsignacionRequest request,
        CancellationToken cancellationToken)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.Asignaciones)
            .FirstOrDefaultAsync(p => p.Id == pedidoId, cancellationToken);

        if (pedido is null)
        {
            return NotFound();
        }

        if (pedido.Estado is not (EstadoPedido.Pendiente or EstadoPedido.Reprogramado))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Asignación no permitida",
                Detail = "Solo se pueden asignar pedidos en estado Pendiente o Reprogramado.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var repartidor = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == request.RepartidorId && u.Rol == RolUsuario.Repartidor && u.Activo,
                cancellationToken);

        var unidad = await _db.Unidades
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UnidadId && u.Activo, cancellationToken);

        if (repartidor is null || unidad is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Datos de asignación inválidos",
                Detail = "El repartidor o la unidad indicados no existen o están inactivos.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        foreach (var anterior in pedido.Asignaciones.Where(a => a.Activa))
        {
            anterior.Activa = false;
        }

        var estadoAnterior = pedido.Estado;
        var ahora = DateTimeOffset.UtcNow;
        var fecha = DateOnly.FromDateTime(ahora.UtcDateTime);

        _db.Asignaciones.Add(new Asignacion
        {
            PedidoId = pedidoId,
            RepartidorId = request.RepartidorId,
            UnidadId = request.UnidadId,
            Fecha = fecha,
            Activa = true,
            CreatedAt = ahora,
        });

        pedido.Estado = EstadoPedido.Asignado;

        _db.CambiosEstado.Add(new CambioEstado
        {
            PedidoId = pedidoId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = EstadoPedido.Asignado,
            FechaHora = ahora,
            UsuarioId = ObtenerUsuarioId(),
            OperacionId = Guid.NewGuid(),
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    [HttpGet("pedidos/candidatos")]
    public async Task<ActionResult<PagedResult<PedidoDto>>> Candidatos(
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
            WHERE p.Estado IN @Estados
            ORDER BY p.FechaReparto DESC, p.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var connection = _db.Database.GetDbConnection();
        var rows = (await connection.QueryAsync<PedidoRow>(
            sql,
            new
            {
                Estados = new[] { (int)EstadoPedido.Pendiente, (int)EstadoPedido.Reprogramado },
                Offset = offset,
                PageSize = pageSize,
            })).ToList();

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

    [HttpGet("repartidores")]
    public async Task<ActionResult<IEnumerable<RepartidorDto>>> Repartidores(CancellationToken cancellationToken)
    {
        var repartidores = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == RolUsuario.Repartidor && u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new RepartidorDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                NombreUsuario = u.NombreUsuario,
            })
            .ToListAsync(cancellationToken);

        return Ok(repartidores);
    }

    [HttpGet("unidades")]
    public async Task<ActionResult<IEnumerable<UnidadDto>>> Unidades(CancellationToken cancellationToken)
    {
        var unidades = await _db.Unidades
            .AsNoTracking()
            .Where(u => u.Activo)
            .OrderBy(u => u.Identificador)
            .Select(u => new UnidadDto
            {
                Id = u.Id,
                Identificador = u.Identificador,
                Tipo = u.Tipo.ToString(),
            })
            .ToListAsync(cancellationToken);

        return Ok(unidades);
    }

    private int ObtenerUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : 0;
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
}
