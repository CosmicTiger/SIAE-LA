using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public record CalificacionReadDto(
    int Id,
    int CurriculaId,
    int AlumnoId,
    decimal Nota,
    DateTime FechaRegistro,
    bool Activo,
    string? CreadoPor = null,
    string? ModificadoPor = null,
    DateTime? FechaModificacion = null,
    DateTime? FechaIngreso = null
    );


    public class CalificacionCreateDto
    {
        [Required] public int CurriculaId { get; set; }
        [Required] public int AlumnoId { get; set; }
        [Range(0, 100)] public decimal Nota { get; set; }
        // Nuevo: periodo al que corresponde la nota
        [Required] public int PeriodoId { get; set; }
    }


    public class CalificacionUpdateDto
    {
        [Range(0, 100)] public decimal Nota { get; set; }
        public bool Activo { get; set; } = true;
    }
}
