namespace SIAE_LA.Domain.Entities;

public class DocenteNivelDetalleCurso
{
    public int Id { get; set; }
    public int NivelDetalleCursoId { get; set; }
    public NivelDetalleCurso NivelDetalleCurso { get; set; } = default!;

    public int DocenteId { get; set; }
    public Docente Docente { get; set; } = default!;

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<Curricula> Curriculas { get; set; } = new List<Curricula>();
}
