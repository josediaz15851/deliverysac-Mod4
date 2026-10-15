using DeliverySac.Api.Domain.Enums;

namespace DeliverySac.Api.Application;

public class EstadoPedidoService
{
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
}
