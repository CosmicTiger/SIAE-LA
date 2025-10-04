namespace SIAE_LA.Domain.Entities;

public class NivelDetalleCurso
{
    public int Id { get; set; }
    public int NivelDetalleId { get; set; }
    public NivelDetalle NivelDetalle { get; set; } = default!;

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = default!;

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<DocenteNivelDetalleCurso> Docentes { get; set; } = new List<DocenteNivelDetalleCurso>();
    public ICollection<Horario> Horarios { get; set; } = new List<Horario>();
    public ICollection<Curricula> Curriculas { get; set; } = new List<Curricula>();
}
