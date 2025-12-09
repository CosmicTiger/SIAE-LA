#nullable enable
namespace SIAE_LA.DTOs
{
    public sealed record TutorAssignmentDto(
        int Id,
        int ApoderadoId,
        int PersonaId,
        string ApoderadoNombre,
        string ApoderadoApellidos,
        string? DocumentoIdentidad,
        bool EsResponsableLegal,
        DateTime FechaInicio,
        DateTime? FechaFin,
        bool Activo
    );

    public sealed class TutorAssignCreateDto
    {
        public int ApoderadoId { get; set; }
        public bool EsResponsableLegal { get; set; } = false;
        public DateTime? FechaInicio { get; set; } // si null -> UtcNow
    }
}