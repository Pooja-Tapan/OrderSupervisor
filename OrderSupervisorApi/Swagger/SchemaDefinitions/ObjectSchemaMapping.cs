using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OrderSupervisorApi.Swagger.SchemaDefinitions
{
    public static class ObjectSchemaMapping
    {
        public static void MapObject(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.MapType<object>(() => new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema {Type = "string"},
                    new OpenApiSchema {Type = "object"},
                    new OpenApiSchema {Type = "array"}
                }
            });
        }
    }
}
