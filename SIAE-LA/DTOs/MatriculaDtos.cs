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
    bool? EsRepetente,
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
        public bool? EsRepetente { get; set; }
    }
}
