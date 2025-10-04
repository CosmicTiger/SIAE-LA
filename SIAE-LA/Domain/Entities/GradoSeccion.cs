using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class GradoSeccion
{
    public int Id { get; set; }
    [Required, MaxLength(40)] public string DescripcionGrado { get; set; } = default!;
    [Required, MaxLength(40)] public string DescripcionSeccion { get; set; } = default!;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<NivelDetalle> NivelDetalles { get; set; } = new List<NivelDetalle>();
}
