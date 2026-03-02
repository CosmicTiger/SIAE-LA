using Microsoft.EntityFrameworkCore;
using SIAE_LA.DTOs;

namespace SIAE_LA.Utils;

public static class AuditHelper
{
    // Devuelve AuditInfo siempre (FechaIngreso obligatoria). Si no se encuentra FechaRegistro
    // intenta leer por reflexión y, en último caso, usa UtcNow.
    public static AuditInfo FromEntry(DbContext db, object entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var entry = db.Entry(entity);
        string? creado = null;
        string? modificado = null;
        DateTime? fechaMod = null;
        DateTime? fechaIngresoOpt = null;

        try { creado = entry.Property("CreadoPor").CurrentValue as string; } catch { }
        try { modificado = entry.Property("ModificadoPor").CurrentValue as string; } catch { }
        try { fechaMod = entry.Property("FechaModificacion").CurrentValue as DateTime?; } catch { }

        // Intentar leer FechaRegistro/FechaIngreso desde propiedades mapeadas por EF
        try
        {
            var propEntry = entry.Property("FechaRegistro");
            var val = propEntry.CurrentValue;
            if (val is DateTime dt) fechaIngresoOpt = dt;
        }
        catch { /* no existe propiedad mapeada "FechaRegistro" */ }

        // Si no encontrado, intentar por reflexión sobre la entidad (p.ej. AnioLectivo)
        if (fechaIngresoOpt is null)
        {
            var pt = entity.GetType().GetProperty("FechaRegistro") ?? entity.GetType().GetProperty("FechaIngreso");
            if (pt is not null)
            {
                try
                {
                    var v = pt.GetValue(entity);
                    if (v is DateTime dt2) fechaIngresoOpt = dt2;
                }
                catch { }
            }
        }

        // último recurso: usar UtcNow para asegurar que FechaIngreso siempre tenga valor
        var fechaIngreso = fechaIngresoOpt ?? DateTime.UtcNow;

        return new AuditInfo(creado, modificado, fechaMod, fechaIngreso);
    }
}
