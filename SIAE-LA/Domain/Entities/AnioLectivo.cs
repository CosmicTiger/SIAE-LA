#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class AnioLectivo
{
    public int Id { get; set; }
    [Required] public int Anio { get; set; }
    [MaxLength(100)] public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<Periodo> Periodos { get; set; } = new List<Periodo>();
    public ICollection<Matricula>? Matriculas { get; set; }
}
