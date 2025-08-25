using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Estudiante
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string CodigoUnico { get; set; } = default!;

    [Required, MaxLength(80)]
    public string Nombres { get; set; } = default!;

    [Required, MaxLength(80)]
    public string Apellidos { get; set; } = default!;

    public DateTime? FechaNacimiento { get; set; }

    [MaxLength(15)]
    public string? Genero { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
}
