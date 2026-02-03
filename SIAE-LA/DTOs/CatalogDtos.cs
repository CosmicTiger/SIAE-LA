using System;
using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    // Nivel DTOs
    public record NivelReadDto(int Id, string DescripcionNivel, string? DescripcionTurno, string? Horario, bool Activo);

    public class NivelCreateDto
    {
        [Required, MaxLength(100)] public string DescripcionNivel { get; set; } = default!;
        [MaxLength(100)] public string? DescripcionTurno { get; set; }
        [MaxLength(50)] public string? Horario { get; set; }
    }

    public class NivelUpdateDto : NivelCreateDto
    {
        public bool Activo { get; set; } = true;
    }

    // GradoSeccionCreate/Update DTOs (read model uses existing GradoSeccionDto in MatriculaDtos)
    public class GradoSeccionCreateDto
    {
        [Required, MaxLength(50)] public string DescripcionGrado { get; set; } = default!;
        [Required, MaxLength(50)] public string DescripcionSeccion { get; set; } = default!;
    }

    public class GradoSeccionUpdateDto : GradoSeccionCreateDto
    {
        public bool Activo { get; set; } = true;
    }

    // Horario DTOs
    public record HorarioReadDto(int Id, int NivelDetalleCursoId, string DiaSemana, TimeSpan HoraInicio, TimeSpan HoraFin, bool Activo, DateTime FechaRegistro);

    public class HorarioCreateDto
    {
        [Required] public int NivelDetalleCursoId { get; set; }
        [Required, MaxLength(20)] public string DiaSemana { get; set; } = default!;
        [Required] public TimeSpan HoraInicio { get; set; }
        [Required] public TimeSpan HoraFin { get; set; }
    }

    public class HorarioUpdateDto
    {
        [Required, MaxLength(20)] public string DiaSemana { get; set; } = default!;
        [Required] public TimeSpan HoraInicio { get; set; }
        [Required] public TimeSpan HoraFin { get; set; }
        public bool Activo { get; set; } = true;
    }

    // Vacante DTO
    public record VacanteDto(int NivelDetalleId, int? TotalVacantes, int? VacantesOcupadas);

    public class VacanteUpdateDto
    {
        public int? TotalVacantes { get; set; }
        public int? VacantesOcupadas { get; set; }
    }

    // DTO usado para actualizar vacantes y estado de un NivelDetalle
    public class NivelDetalleVacantesUpdateDto
    {
        public int? TotalVacantes { get; set; }
        public int? VacantesOcupadas { get; set; }
        public bool? Activo { get; set; }
    }

    public class NivelDetalleVacantesAuditDto
    {
        public int NivelDetalleId { get; set; }
        public int? OldTotalVacantes { get; set; }
        public int? NewTotalVacantes { get; set; }
        public int? OldVacantesOcupadas { get; set; }
        public int? NewVacantesOcupadas { get; set; }
        public bool? OldActivo { get; set; }
        public bool? NewActivo { get; set; }
        public string? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    // NivelDetalleCurso DTOs (asignación de cursos a nivelDetalle)
    public class NivelDetalleCursoCreateDto
    {
        [Required] public int NivelDetalleId { get; set; }
        [Required] public int CursoId { get; set; }
        public bool Activo { get; set; } = true;
    }

    public record NivelDetalleCursoReadDto(int Id, int NivelDetalleId, int CursoId, bool Activo, DateTime FechaRegistro);
}
