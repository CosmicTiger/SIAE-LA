using SIAE_LA.Domain.Entities;

namespace SIAE_LA.DTOs
{
    public record DocenteReadDto(
    int Id,
    string Nombres,
    string Apellidos,
    string? Codigo,
    string? DocumentoIdentidad,
    string? Ciudad,
    string? Direccion,
    bool Activo
    );

    public sealed class DocenteCreateWithAccountsDto
    {
        // Persona del Docente
        public PersonaInputDto DocentePersona { get; set; } = default!;
        
        // Usuario del docente
        public string DocenteEmail { get; set; } = default!;

        public string DocentePassword { get; set; } = default!;
        
    }

    public sealed class DocenteCreateResultDto
    {
        public int DocenteId { get; set; }
        
        public int DocentePersonaId { get; set; }
        
        public string? DocenteEmail { get; set; }
    }

    public sealed class DocenteUpdateDto : PersonaInputDto
    {
        public bool Activo { get; set; } = true;
    }
}