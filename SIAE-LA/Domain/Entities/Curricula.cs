using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities;

public class Curricula
{
    public int Id { get; set; }
    public int DocenteNivelDetalleCursoId { get; set; }
    public DocenteNivelDetalleCurso DocenteNivelDetalleCurso { get; set; } = default!;
    [MaxLength(200)] public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<Calificacion> Calificaciones { get; set; } = new List<Calificacion>();
}
