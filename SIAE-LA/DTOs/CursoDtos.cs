using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public record CursoReadDto(
    int Id,
    string Descripcion,
    string? Codigo,
    bool Activo,
    string? CreadoPor = null,
    string? ModificadoPor = null,
    DateTime? FechaModificacion = null,
    DateTime? FechaIngreso = null
    );

    // Incluye FechaIngreso (fecha_registro) además de las propiedades de auditoría
    public sealed record AuditInfo(string? CreadoPor, string? ModificadoPor, DateTime? FechaModificacion, DateTime FechaIngreso);


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
