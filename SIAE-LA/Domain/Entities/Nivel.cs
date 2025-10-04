using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Nivel
{
    public int Id { get; set; }
    [Required, MaxLength(60)] public string DescripcionNivel { get; set; } = default!;
    [MaxLength(40)] public string? DescripcionTurno { get; set; } // Mañana/Tarde
    [MaxLength(40)] public string? Horario { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<NivelDetalle> Detalles { get; set; } = new List<NivelDetalle>();
}
