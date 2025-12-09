namespace SIAE_LA.Domain.Entities;

public class Matricula
{
    public int Id { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(30)] public string? ValorCodigo { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(30)] public string? Codigo { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(40)] public string? Situacion { get; set; } // Regular/Repite

    public int AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = default!;

    public int NivelDetalleId { get; set; }
    public NivelDetalle NivelDetalle { get; set; } = default!;

    public int? ApoderadoId { get; set; }
    public Apoderado? Apoderado { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(120)] public string? InstitucionProcedencia { get; set; }
    public bool? EsRepitente { get; set; }

    public int PeriodoId { get; set; }
    public Periodo Periodo { get; set; } = default!;

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
