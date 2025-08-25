using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Periodo
{
    public int Id { get; set; }
    public int Anio { get; set; }

    [Required, MaxLength(40)]
    public string NombreCorte { get; set; } = default!;
}
