using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public record PeriodoReadDto(int Id, string Descripcion, bool Activo, string? CreadoPor = null, string? ModificadoPor = null, DateTime? FechaModificacion = null, DateTime? FechaIngreso = null);


    public class PeriodoCreateDto
    {
        [Required, MaxLength(60)] public string Descripcion { get; set; } = default!;
        // Opcional: permitir especificar orden al crear
        public int? Orden { get; set; }
    }


    public class PeriodoUpdateDto : PeriodoCreateDto
    { public bool Activo { get; set; } = true; }

    // DTO para reordenar periodos en masa
    public sealed class PeriodoReorderDto
    {
        // Lista de ids de periodos en el orden deseado
        public int[] PeriodoIds { get; set; } = Array.Empty<int>();
    }
}
