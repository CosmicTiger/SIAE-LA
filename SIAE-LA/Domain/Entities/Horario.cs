using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Horario
{
    public int Id { get; set; }
    public int NivelDetalleCursoId { get; set; }
    public NivelDetalleCurso NivelDetalleCurso { get; set; } = default!;

    [MaxLength(15)] public string DiaSemana { get; set; } = "Lunes";
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
