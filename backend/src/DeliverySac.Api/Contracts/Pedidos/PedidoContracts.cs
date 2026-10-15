using System.ComponentModel.DataAnnotations;

namespace DeliverySac.Api.Contracts.Pedidos;

public class PedidoLineaRequest
{
    [Required]
    [MaxLength(200)]
    public string Producto { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    [Required]
    [MaxLength(300)]
    public string Descripcion { get; set; } = string.Empty;
}

public class CrearPedidoRequest
{
    public int ClienteId { get; set; }

    public DateOnly FechaReparto { get; set; }

    [MaxLength(500)]
    public string? Comentario { get; set; }

    public List<PedidoLineaRequest> Lineas { get; set; } = new();
}

public class PedidoLineaDto
{
    public int Id { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class PedidoDto
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateOnly FechaReparto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class PedidoDetalleDto : PedidoDto
{
    public string? Comentario { get; set; }
    public List<PedidoLineaDto> Lineas { get; set; } = new();
}
