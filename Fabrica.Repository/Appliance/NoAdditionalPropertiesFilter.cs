using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fabrica.Repository.Appliance;

public class NoAdditionalPropertiesFilter: ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Type == "object")
            schema.AdditionalPropertiesAllowed = false;
    }
}