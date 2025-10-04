using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Persona
{
    public int Id { get; set; }
    [MaxLength(30)] public string? ValorCodigo { get; set; }
    [MaxLength(30)] public string? Codigo { get; set; }

    [Required, MaxLength(80)] public string Nombres { get; set; } = default!;
    [Required, MaxLength(80)] public string Apellidos { get; set; } = default!;

    [MaxLength(40)] public string? DocumentoIdentidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    [MaxLength(10)] public string? Sexo { get; set; }

    [MaxLength(80)] public string? Ciudad { get; set; }
    [MaxLength(140)] public string? Direccion { get; set; }
    [MaxLength(120), EmailAddress] public string? Email { get; set; }
    [MaxLength(30)] public string? NumeroTelefono { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navegaciones opcionales hacia roles funcionales
    public Alumno? Alumno { get; set; }
    public Docente? Docente { get; set; }
    public Apoderado? Apoderado { get; set; }
}
