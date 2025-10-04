namespace SIAE_LA.Domain.Entities;

public class NivelDetalle
{
    public int Id { get; set; }
    public int NivelId { get; set; }
    public Nivel Nivel { get; set; } = default!;

    public int GradoSeccionId { get; set; }
    public GradoSeccion GradoSeccion { get; set; } = default!;

    public int? TotalVacantes { get; set; }
    public int? VacantesOcupadas { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<NivelDetalleCurso> Cursos { get; set; } = new List<NivelDetalleCurso>();
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
