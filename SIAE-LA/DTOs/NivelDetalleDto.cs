namespace SIAE_LA.DTOs
{
    public sealed class NivelDetalleDto
    {
        public int NivelDetalleId { get; set; }
        public int NivelId { get; set; }
        public string NivelDescripcion { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public int GradoSeccionId { get; set; }
        public string GradoDescripcion { get; set; } = string.Empty;
        public string SeccionDescripcion { get; set; } = string.Empty;
        public int? TotalVacantes { get; set; } = 0;
        public int? VacantesOcupadas { get; set; } = 0;
        public DateTime FechaRegistro { get; set; }
        public bool activo { get; set; }
    }

    public sealed class NivelDetalleCursoDto
    {
        public int Id { get; set; }
        public int NivelDetalleId { get; set; }
        public int CursoId { get; set; }
        public string NivelDescripcion { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public string GradoSeccion { get; set; } = string.Empty;
        public string CursoDescripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public sealed class NivelDetalleCreateDto
    {
        public int NivelId { get; set; }
        public int GradoSeccionId { get; set; }

        public int? TotalVacantes { get; set; }
    }
}
