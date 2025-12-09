// Infrastructure/Persistence/Naming/SnakeCaseExtensions.cs
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SIAE_LA.Infrastructure.Persistence.Naming;

public static class SnakeCaseExtensions
{
    static readonly Regex Snake1 = new(@"([a-z0-9])([A-Z])", RegexOptions.Compiled);
    static readonly Regex Snake2 = new(@"([A-Z])([A-Z][a-z])", RegexOptions.Compiled);

    public static string ToSnakeCase(this string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name;
        var s = Snake2.Replace(Snake1.Replace(name, "$1_$2"), "$1_$2");
        return s.Replace("__", "_").ToLowerInvariant();
    }

    /// <summary>
    /// Convierte tablas, columnas, índices, PK/FK/constraints a snake_case.
    /// Evita tocar tablas de ASP.NET Identity (AspNet*).
    /// </summary>
    public static void UseSnakeCaseNames(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Evitar las tablas de Identity si no quieres renombrarlas
            var tableName = entity.GetTableName() ?? "";
            if (tableName.StartsWith("AspNet", StringComparison.OrdinalIgnoreCase))
                continue;

            entity.SetTableName(tableName.ToSnakeCase());

            // Columnas
            foreach (var property in entity.GetProperties())
            {
                var current = property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema()));
                property.SetColumnName(current!.ToSnakeCase());
            }

            // Claves
            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()!.ToSnakeCase());

            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(fk.GetConstraintName()!.ToSnakeCase());

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()!.ToSnakeCase());
        }
    }
}
