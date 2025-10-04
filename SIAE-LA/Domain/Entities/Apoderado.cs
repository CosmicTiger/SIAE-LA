namespace SIAE_LA.Domain.Entities;

public class Apoderado
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public Persona Persona { get; set; } = default!;
    public string? TipoParentesco { get; set; } // madre, padre, tutor legal…
    public string? EstadoCivil { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
