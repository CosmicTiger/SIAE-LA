namespace SIAE_LA.DTOs
{
    public class TutorCreateDto : PersonaInputDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? TipoParentesco { get; set; }      // Madre, Padre, Tutor legal…
    }
}
