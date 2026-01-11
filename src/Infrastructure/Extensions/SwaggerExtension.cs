using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ProjectTemplate.Infrastructure.Swagger;
using System.Reflection;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Swagger/OpenAPI configuration extension
/// </summary>
public static class SwaggerExtension
{
    /// <summary>
    /// Adds customized Swagger/OpenAPI documentation
    /// </summary>
    public static IServiceCollection AddCustomizedSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options =>
        {
            // API Information
            var version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "1.0.0";
            var assemblyName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "API";
            
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = $"v{version}",
                Title = $"{assemblyName} API",
                Description = "API built with Clean Architecture template",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@example.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Group operations by controller
            options.OperationFilter<SwaggerGroupByController>();

            // Include XML comments if available
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Add JWT Bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Support for API versioning
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                // Include all endpoints unless they are explicitly marked with [ApiExplorerSettings(IgnoreApi = true)]
                return !apiDesc.ActionDescriptor.EndpointMetadata
                    .OfType<Microsoft.AspNetCore.Mvc.ApiExplorerAttribute>()
                    .Any(x => !x.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase));
            });
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI with custom settings
    /// </summary>
    public static IApplicationBuilder UseCustomizedSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            options.RoutePrefix = "swagger";
            
            // Display request duration
            options.DisplayRequestDuration();
            
            // Enable deep linking
            options.EnableDeepLinking();
            
            // Show extensions
            options.ShowExtensions();
            
            // Enable filter
            options.EnableFilter();
            
            // Enable validator
            options.EnableValidator();
            
            // Set default models expand depth
            options.DefaultModelsExpandDepth(2);
            
            // Set default model rendering to example
            options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
        });

        return app;
    }
}
