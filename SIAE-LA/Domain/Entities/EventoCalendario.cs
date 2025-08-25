using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class EventoCalendario
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Titulo { get; set; } = default!;

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    [MaxLength(40)]
    public string? TipoEvento { get; set; }
}
