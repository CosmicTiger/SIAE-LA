using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Calificacion
{
    public int Id { get; set; }

    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = default!;

    public int DocenteAsignaturaGrupoId { get; set; }
    public DocenteAsignaturaGrupo DocenteAsignaturaGrupo { get; set; } = default!;

    [Range(0, 100)]
    public decimal Nota { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
