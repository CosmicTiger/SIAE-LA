#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public record MatriculaReadDto(
        int Id,
        int AlumnoId,
        int NivelDetalleId,
        int PeriodoId,
        int? ApoderadoId,
        string? Situacion,
        string? InstitucionProcedencia,
        bool? EsRepitente,
        DateTime FechaRegistro
    );

    // Nueva estructura: incluir detalle del NivelDetalle con Nivel y GradoSeccion
    public record GradoSeccionDto(int Id, string DescripcionGrado, string DescripcionSeccion);

    public record NivelDto(int Id, string DescripcionNivel, string? DescripcionTurno, string? Horario);

    public record NivelDetalleDto(
        int Id,
        int NivelId,
        NivelDto Nivel,
        int GradoSeccionId,
        GradoSeccionDto GradoSeccion,
        int? TotalVacantes,
        int? VacantesOcupadas
    );

    public sealed record NivelResumenDto(
            int NivelDetalleId,
            int NivelId,
            string NivelDescripcion,
            string? NivelTurno,
            int GradoSeccionId,
            string GradoDescripcion,
            string SeccionDescripcion
        );

    public record MatriculaWithDetalleDto(
        int Id,
        AlumnoReadDto alumno,
        NivelDetalleDto NivelDetalle,
        PeriodoReadDto Periodo,
        TutorDto? Apoderado,
        string? Situacion,
        string? InstitucionProcedencia,
        bool? EsRepetente,
        bool Activo,
        DateTime FechaRegistro
    );

    public sealed record MatriculaResumenDto(
            int MatriculaId,
            NivelResumenDto? Nivel,
            int PeriodoId,
            string? Situacion,
            bool? EsRepetente,
            int? ApoderadoId,
            DateTime FechaRegistro
        );

    public class MatriculaCreateDto
    {
        [Required] public int AlumnoId { get; set; }
        [Required] public int NivelDetalleId { get; set; }
        [Required] public int PeriodoId { get; set; }
        public int? ApoderadoId { get; set; }
        [MaxLength(40)] public string? Situacion { get; set; }
        [MaxLength(120)] public string? InstitucionProcedencia { get; set; }
        public bool? EsRepitente { get; set; }
    }
}
