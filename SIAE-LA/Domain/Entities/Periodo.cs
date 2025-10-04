using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Periodo
{
    public int Id { get; set; }
    [Required, MaxLength(60)] public string Descripcion { get; set; } = default!;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
