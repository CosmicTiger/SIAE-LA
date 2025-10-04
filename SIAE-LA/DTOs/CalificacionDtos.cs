using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public record CalificacionReadDto(
    int Id,
    int CurriculaId,
    int AlumnoId,
    decimal Nota,
    DateTime FechaRegistro,
    bool Activo
    );


    public class CalificacionCreateDto
    {
        [Required] public int CurriculaId { get; set; }
        [Required] public int AlumnoId { get; set; }
        [Range(0, 100)] public decimal Nota { get; set; }
    }


    public class CalificacionUpdateDto
    {
        [Range(0, 100)] public decimal Nota { get; set; }
        public bool Activo { get; set; } = true;
    }
}
