using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Curso
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Descripcion { get; set; } = default!;
    [MaxLength(30)] public string? Codigo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<NivelDetalleCurso> Niveles { get; set; } = new List<NivelDetalleCurso>();
}
