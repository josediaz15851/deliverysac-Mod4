using System.ComponentModel.DataAnnotations;

namespace DeliverySac.Api.Contracts.Clientes;

public class ClienteRequest
{
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Direccion { get; set; } = string.Empty;
}

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}
