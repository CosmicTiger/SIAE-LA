using System;

namespace SIAE_LA.DTOs
{
    public record HorarioDto(
        int Id,
        int NivelDetalleCursoId,
        string DiaSemana,
        TimeSpan HoraInicio,
        TimeSpan HoraFin,
        bool Activo,
        DateTime FechaRegistro,
        string? CreadoPor = null,
        string? ModificadoPor = null,
        DateTime? FechaModificacion = null,
        DateTime? FechaIngreso = null
    );

    public class HorarioCreateInputDto
    {
        public int NivelDetalleCursoId { get; set; }
        public string DiaSemana { get; set; } = "Lunes";
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }

    public class HorarioUpdateInputDto : HorarioCreateInputDto
    {
        public bool Activo { get; set; } = true;
    }
}
