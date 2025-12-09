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
    string ? Sexo,
    bool Activo
    );

    // General DTO para usos varios (similar a DocenteReadDto con campo opcional GradoEstudio)
    public record DocenteDto(
        int Id,
        string Nombres,
        string Apellidos,
        string? Codigo,
        string? DocumentoIdentidad,
        string? Ciudad,
        string? Direccion,
        bool Activo,
        string? GradoEstudio
    );

    // DTO para crear Docente (sin cuenta de usuario)
    public sealed class DocenteCreateDto : PersonaInputDto
    {
        public string? GradoEstudio { get; set; }
    }

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

    // DocenteCursoDto: representa una asignación/curso asignado a un docente
    public sealed class DocenteCursoDto
    {
        public int Id { get; set; }
        public int DocenteId { get; set; }
        public int NivelDetalleCursoId { get; set; }
        public int NivelId { get; set; }
        public string NivelDescripcion { get; set; } = default!;
        public int GradoSeccionId { get; set; }
        public string GradoDescripcion { get; set; } = default!;
        public int CursoId { get; set; }
        public string CursoDescripcion { get; set; } = default!;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    // Curricula DTO
    public sealed class CurriculaDto
    {
        public int Id { get; set; }
        public int DocenteNivelDetalleCursoId { get; set; }
        public string Titulo { get; set; } = default!;
        public string Descripcion { get; set; } = default!;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    // DTO para crear Curricula
    public sealed class CurriculaCreateDto
    {
        public int DocenteNivelDetalleCursoId { get; set; }
        public string? Descripcion { get; set; }
    }

    // DTO para actualizar Curricula
    public sealed class CurriculaUpdateDto
    {
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
    }

    // DTO para crear/actualizar asignaciones de docente a nivel_detalle_curso
    public sealed class DocenteAsignacionDto
    {
        public int DocenteId { get; set; }
        public int NivelDetalleCursoId { get; set; }
        public bool Activo { get; set; } = true;
    }
}