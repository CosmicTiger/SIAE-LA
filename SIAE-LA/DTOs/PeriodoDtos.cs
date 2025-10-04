using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public record PeriodoReadDto(int Id, string Descripcion, bool Activo);


    public class PeriodoCreateDto
    {
        [Required, MaxLength(60)] public string Descripcion { get; set; } = default!;
    }


    public class PeriodoUpdateDto : PeriodoCreateDto
    { public bool Activo { get; set; } = true; }
}
