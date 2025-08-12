using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.AspNetCore.Http;

public class SwaggerFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) ||
                        (p.ParameterType.IsClass && p.ParameterType.GetProperties()
                          .Any(prop => prop.PropertyType == typeof(IFormFile))))
            .ToList();

        if (!fileUploadParams.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = GenerateSchema(context)
                }
            }
        };

        operation.Parameters.Clear(); 
    }

    private OpenApiSchema GenerateSchema(OperationFilterContext context)
    {
        var dtoType = context.MethodInfo.GetParameters()
            .Select(p => p.ParameterType)
            .FirstOrDefault(t => t.IsClass && t.GetProperties().Any(prop => prop.PropertyType == typeof(IFormFile)));

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new System.Collections.Generic.Dictionary<string, OpenApiSchema>()
        };

        if (dtoType == null)
            return schema;

        foreach (var prop in dtoType.GetProperties())
        {
            if (prop.PropertyType == typeof(IFormFile))
            {
                schema.Properties.Add(prop.Name, new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                });
            }
            else
            {
                schema.Properties.Add(prop.Name, new OpenApiSchema
                {
                    Type = "string"
                });
            }
        }

        return schema;
    }
}
