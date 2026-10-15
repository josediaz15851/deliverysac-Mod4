namespace DeliverySac.Api.Domain;

public class Asignacion
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int RepartidorId { get; set; }
    public int UnidadId { get; set; }
    public DateOnly Fecha { get; set; }
    public bool Activa { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Usuario Repartidor { get; set; } = null!;
    public Unidad Unidad { get; set; } = null!;
}
