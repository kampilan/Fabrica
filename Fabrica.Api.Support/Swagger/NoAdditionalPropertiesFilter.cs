
// ReSharper disable UnusedMember.Global

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace Fabrica.Api.Support.Swagger;

public class NoAdditionalPropertiesFilter: ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Type == "object")
            schema.AdditionalPropertiesAllowed = false;
    }
}