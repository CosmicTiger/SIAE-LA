namespace SIAE_LA.DTOs
{
    public record AlumnoReadDto(
    int Id,
    string Nombres,
    string Apellidos,
    string? Codigo,
    string? DocumentoIdentidad,
    string? Ciudad,
    string? Direccion,
    bool Activo
    );


    public sealed record AlumnoDetailDto(
            int AlumnoId,
            PersonaDto Persona,
            MatriculaResumenDto? MatriculaActual,
            TutorDto? Tutor,
            bool Activo
        );

    public sealed class AlumnoCreateWithAccountsDto
    {
        // Persona del alumno (DEBE tener FechaNacimiento)
        public PersonaInputDto AlumnoPersona { get; set; } = default!;

        // Usuario del alumno
        public string AlumnoEmail { get; set; } = default!;
        public string AlumnoPassword { get; set; } = default!;

        // Tutor: requerido si el alumno es menor de 18
        public TutorCreateDto? Tutor { get; set; }
    }

    // Result para el POST avanzado
    public sealed class AlumnoCreateResultDto
    {
        public int AlumnoId { get; set; }
        public int AlumnoPersonaId { get; set; }
        public string AlumnoEmail { get; set; } = default!;
        public bool EsMenorDeEdad { get; set; }
        public int? TutorPersonaId { get; set; }
        public int? ApoderadoId { get; set; }
        public string? TutorEmail { get; set; }
    }

    public sealed class AlumnoUpdateDto : PersonaInputDto
    {
        // Estado del Alumno (no de Persona)
        public bool Activo { get; set; } = true;
    }

    public sealed class AlumnoStatusUpdateDto
    {
        // Estado del Alumno (no de Persona)
        public bool Activo { get; set; } = true;
    }

    // DTO para devolver horarios por alumno
    public sealed record StudentHorarioDto(int AlumnoId, IEnumerable<HorarioDto> Horarios);
}
