using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Calificacion
{
    public int Id { get; set; }
    public int CurriculaId { get; set; }
    public Curricula Curricula { get; set; } = default!;

    public int AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = default!;

    [Range(0, 100)] public decimal Nota { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
