using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OrderSupervisorApi.Swagger
{
    public class SetVersionInPaths : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var versionSetPaths = new Dictionary<string, OpenApiPathItem>();

            foreach (var path in swaggerDoc.Paths)
            {
                versionSetPaths.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version), path.Value);
            }

            swaggerDoc.Paths.Clear();

            foreach (var path in versionSetPaths)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }
}