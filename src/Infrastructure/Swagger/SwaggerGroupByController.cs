using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectTemplate.Infrastructure.Swagger;

/// <summary>
/// Groups Swagger operations by controller name for better organization
/// </summary>
public class SwaggerGroupByController : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
        if (!string.IsNullOrEmpty(controllerName))
        {
            operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = controllerName } };
        }
    }
}
