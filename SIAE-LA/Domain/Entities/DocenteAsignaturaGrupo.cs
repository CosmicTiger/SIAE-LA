namespace SIAE_LA.Domain.Entities;

public class DocenteAsignaturaGrupo
{
    public int Id { get; set; }

    public int DocenteId { get; set; }
    public Docente Docente { get; set; } = default!;

    public int AsignaturaId { get; set; }
    public Asignatura Asignatura { get; set; } = default!;

    public int GrupoId { get; set; }
    public Grupo Grupo { get; set; } = default!;

    public int PeriodoId { get; set; }
    public Periodo Periodo { get; set; } = default!;

    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
}
