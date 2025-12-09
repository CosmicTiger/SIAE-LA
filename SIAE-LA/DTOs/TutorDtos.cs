namespace SIAE_LA.DTOs
{
    public class TutorCreateDto : PersonaInputDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? TipoParentesco { get; set; }      // Madre, Padre, Tutor legal…
    }

    public sealed record TutorDto(
            int ApoderadoId,
            int PersonaId,
            string Nombres,
            string Apellidos,
            string? DocumentoIdentidad,
            string? Email,
            string? NumeroTelefono
        );
}
