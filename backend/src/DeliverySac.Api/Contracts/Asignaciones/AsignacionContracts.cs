namespace DeliverySac.Api.Contracts.Asignaciones;

public class CrearAsignacionRequest
{
    public int RepartidorId { get; set; }
    public int UnidadId { get; set; }
}

public class RepartidorDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
}

public class UnidadDto
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
