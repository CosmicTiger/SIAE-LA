using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public record CursoReadDto(
    int Id,
    string Descripcion,
    string? Codigo,
    bool Activo
    );


    public class CursoCreateDto
    {
        [Required, MaxLength(120)] public string Descripcion { get; set; } = default!;
        [MaxLength(30)] public string? Codigo { get; set; }
    }


    public class CursoUpdateDto : CursoCreateDto
    {
        public bool Activo { get; set; } = true;
    }
}
