using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Periodo
{
    public int Id { get; set; }
    [Required, MaxLength(60)] public string Descripcion { get; set; } = default!;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Nueva relación: Periodo pertenece a un Año lectivo (AnioLectivo)
    public int? AnioLectivoId { get; set; }
    public AnioLectivo? AnioLectivo { get; set; }
    
    // Orden dentro del año lectivo (1..N)
    public int Orden { get; set; } = 0;
}
