using DeliverySac.Api.Domain;
using DeliverySac.Api.Domain.Enums;
using DeliverySac.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliverySac.Api.Application;

public enum ResultadoCambioEstado
{
    Ok,
    PedidoNoEncontrado,
    NoPropietario,
    TransicionInvalida,
    MotivoRequerido,
}

public class EstadoPedidoService
{
    private readonly DeliverySacDbContext _db;

    public EstadoPedidoService(DeliverySacDbContext db)
    {
        _db = db;
    }

    public static bool PuedeTransicionar(EstadoPedido desde, EstadoPedido hacia)
    {
        return desde switch
        {
            EstadoPedido.Pendiente => hacia == EstadoPedido.Asignado,
            EstadoPedido.Asignado => hacia == EstadoPedido.EnRuta,
            EstadoPedido.EnRuta => hacia is EstadoPedido.Entregado or EstadoPedido.NoEntregado or EstadoPedido.Reprogramado,
            EstadoPedido.Reprogramado => hacia == EstadoPedido.Asignado,
            _ => false,
        };
    }

    public static bool RequiereMotivo(EstadoPedido estado)
    {
        return estado is EstadoPedido.NoEntregado or EstadoPedido.Reprogramado;
    }

    public static bool EsEstadoTerminal(EstadoPedido estado)
    {
        return estado == EstadoPedido.Entregado;
    }

    public async Task<ResultadoCambioEstado> CambiarEstadoAsync(
        int pedidoId,
        EstadoPedido estadoNuevo,
        string? motivo,
        int usuarioId,
        Guid operacionId,
        CancellationToken cancellationToken)
    {
        var operacionExistente = await _db.CambiosEstado
            .AnyAsync(c => c.OperacionId == operacionId, cancellationToken);

        if (operacionExistente)
        {
            return ResultadoCambioEstado.Ok;
        }

        var pedido = await _db.Pedidos
            .Include(p => p.Asignaciones)
            .FirstOrDefaultAsync(p => p.Id == pedidoId, cancellationToken);

        if (pedido is null)
        {
            return ResultadoCambioEstado.PedidoNoEncontrado;
        }

        var asignacionActiva = pedido.Asignaciones.FirstOrDefault(a => a.Activa);
        if (asignacionActiva is null || asignacionActiva.RepartidorId != usuarioId)
        {
            return ResultadoCambioEstado.NoPropietario;
        }

        if (!PuedeTransicionar(pedido.Estado, estadoNuevo))
        {
            return ResultadoCambioEstado.TransicionInvalida;
        }

        if (RequiereMotivo(estadoNuevo) && string.IsNullOrWhiteSpace(motivo))
        {
            return ResultadoCambioEstado.MotivoRequerido;
        }

        var ahora = DateTimeOffset.UtcNow;

        _db.CambiosEstado.Add(new CambioEstado
        {
            PedidoId = pedidoId,
            EstadoAnterior = pedido.Estado,
            EstadoNuevo = estadoNuevo,
            FechaHora = ahora,
            UsuarioId = usuarioId,
            Motivo = RequiereMotivo(estadoNuevo) ? motivo!.Trim() : null,
            OperacionId = operacionId,
        });

        pedido.Estado = estadoNuevo;

        await _db.SaveChangesAsync(cancellationToken);

        return ResultadoCambioEstado.Ok;
    }
}
