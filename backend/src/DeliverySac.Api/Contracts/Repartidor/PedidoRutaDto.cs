namespace DeliverySac.Api.Contracts.Repartidor;

public class PedidoRutaDto
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateOnly FechaReparto { get; set; }
    public decimal Total { get; set; }
}
