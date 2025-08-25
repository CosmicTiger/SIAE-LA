using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Grupo
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string Nombre { get; set; } = default!;

    public int NivelId { get; set; }
    public Nivel Nivel { get; set; } = default!;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<DocenteAsignaturaGrupo> Asignaciones { get; set; } = new List<DocenteAsignaturaGrupo>();
}
