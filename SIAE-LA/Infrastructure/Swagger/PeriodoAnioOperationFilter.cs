using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SIAE_LA.Infrastructure.Swagger;

// Adds descriptions for query parameters periodoId and anioLectivoId and documents precedence
public class PeriodoAnioOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) operation.Parameters = new System.Collections.Generic.List<OpenApiParameter>();

        void EnsureParam(string name, string description)
        {
            if (operation.Parameters.Any(p => p.Name == name)) return;
            var param = new OpenApiParameter
            {
                Name = name,
                In = ParameterLocation.Query,
                Description = description,
                Required = false,
                Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
            };

            // Add a small example value to help users in Swagger UI
            if (string.Equals(name, "periodoId", System.StringComparison.OrdinalIgnoreCase))
            {
                param.Example = new OpenApiInteger(1);
            }
            else if (string.Equals(name, "anioLectivoId", System.StringComparison.OrdinalIgnoreCase))
            {
                param.Example = new OpenApiInteger(2024);
            }

            operation.Parameters.Add(param);
        }

        EnsureParam("periodoId", "Optional: filter by Periodo.id. If provided and 'anioLectivoId' is not provided, the API will resolve the corresponding AnioLectivo and filter by that year.");
        EnsureParam("anioLectivoId", "Optional: filter by AnioLectivo id directly. If provided it takes precedence over 'periodoId'.");
    }
}
