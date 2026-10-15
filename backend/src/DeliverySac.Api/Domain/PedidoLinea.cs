namespace DeliverySac.Api.Domain;

public class PedidoLinea
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    public Pedido Pedido { get; set; } = null!;
}
