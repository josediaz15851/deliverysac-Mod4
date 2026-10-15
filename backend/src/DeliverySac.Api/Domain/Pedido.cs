using DeliverySac.Api.Domain.Enums;

namespace DeliverySac.Api.Domain;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateOnly FechaReparto { get; set; }
    public string? Comentario { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public ICollection<PedidoLinea> Lineas { get; set; } = new List<PedidoLinea>();
    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    public ICollection<CambioEstado> CambiosEstado { get; set; } = new List<CambioEstado>();
}
