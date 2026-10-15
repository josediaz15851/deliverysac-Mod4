namespace DeliverySac.Api.Contracts.Pedidos;

public class CambioEstadoRequest
{
    public string EstadoNuevo { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public Guid? OperacionId { get; set; }
}

public class TrazabilidadItemDto
{
    public long Id { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateTimeOffset FechaHora { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}

public class TrazabilidadDto
{
    public List<TrazabilidadItemDto> Items { get; set; } = new();
}
