using DeliverySac.Api.Domain.Enums;

namespace DeliverySac.Api.Domain;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<CambioEstado> CambiosEstado { get; set; } = new List<CambioEstado>();
    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}
