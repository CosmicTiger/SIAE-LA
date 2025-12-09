#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.Domain.Entities
{
    /// <summary>
    /// Asignación histórica/activa de un Apoderado (Tutor) a un Alumno.
    /// Mantiene vigencias para poder cambiar tutor sin perder el historial.
    /// </summary>
    public class AlumnoApoderado
    {
        public int Id { get; set; }

        [Required]
        public int AlumnoId { get; set; }
        public Alumno Alumno { get; set; } = default!;

        [Required]
        public int ApoderadoId { get; set; }
        public Apoderado Apoderado { get; set; } = default!;

        // Vigencia de la asignación
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        public DateTime? FechaFin { get; set; }

        // Indica si es el responsable legal del alumno en esta asignación
        public bool EsResponsableLegal { get; set; } = false;
    }
}