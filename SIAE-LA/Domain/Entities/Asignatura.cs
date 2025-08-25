using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Asignatura
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Nombre { get; set; } = default!;

    [Required, MaxLength(30)]
    public string Codigo { get; set; } = default!;

    public int? Creditos { get; set; }

    public ICollection<DocenteAsignaturaGrupo> Asignaciones { get; set; } = new List<DocenteAsignaturaGrupo>();
}
