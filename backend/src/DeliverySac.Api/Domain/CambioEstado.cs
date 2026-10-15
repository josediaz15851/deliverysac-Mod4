using DeliverySac.Api.Domain.Enums;

namespace DeliverySac.Api.Domain;

public class CambioEstado
{
    public long Id { get; set; }
    public int PedidoId { get; set; }
    public EstadoPedido? EstadoAnterior { get; set; }
    public EstadoPedido EstadoNuevo { get; set; }
    public DateTimeOffset FechaHora { get; set; }
    public int UsuarioId { get; set; }
    public string? Motivo { get; set; }
    public Guid OperacionId { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
