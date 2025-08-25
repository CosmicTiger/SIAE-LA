using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Docente
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombres { get; set; } = default!;

    [Required, MaxLength(80)]
    public string Apellidos { get; set; } = default!;

    [MaxLength(120), EmailAddress]
    public string? EmailInstitucional { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<DocenteAsignaturaGrupo> Asignaciones { get; set; } = new List<DocenteAsignaturaGrupo>();
}
