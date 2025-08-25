using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Nivel
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string Nombre { get; set; } = default!;

    public ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
}
