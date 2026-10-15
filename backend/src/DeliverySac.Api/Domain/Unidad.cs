using DeliverySac.Api.Domain.Enums;

namespace DeliverySac.Api.Domain;

public class Unidad
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public TipoUnidad Tipo { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}
