namespace SIAE_LA.DTOs
{
    public sealed class DocenteInputDto
    {
        public string? GradoEstudio { get; set; }
    }

    public sealed class RegisterDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string[] Roles { get; set; } = Array.Empty<string>();

        // Para todos los roles salvo el único Admin
        public PersonaInputDto? Persona { get; set; }

        // Marca si la persona (Direccion/Subdireccion) es docente de profesión
        public bool EsDocente { get; set; } = false;

        // Requerido si por reglas hay que crear fila en Docente
        public DocenteInputDto? Docente { get; set; }
    }

    public sealed class LoginDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public sealed record AuthResponse(string AccessToken, string Email, string? FullName, IReadOnlyList<string> Roles);

}
