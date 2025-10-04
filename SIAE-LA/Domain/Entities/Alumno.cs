namespace SIAE_LA.Domain.Entities;

public class Alumno
{
    public int Id { get; set; }

    public int PersonaId { get; set; }
    public Persona Persona { get; set; } = default!;

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
}
