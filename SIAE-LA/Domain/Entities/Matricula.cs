namespace SIAE_LA.Domain.Entities;

public class Matricula
{
    public int Id { get; set; }

    public int EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; } = default!;

    public int GrupoId { get; set; }
    public Grupo Grupo { get; set; } = default!

;
    public int PeriodoId { get; set; }
    public Periodo Periodo { get; set; } = default!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
