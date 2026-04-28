using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Swagger extension methods with JWT authentication support
/// </summary>
public static class SwaggerExtension
{
    /// <summary>
    /// Add Swagger with JWT Bearer authentication
    /// </summary>
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ProjectTemplate API",
                Version = "v1",
                Description = "Enterprise .NET Template API with JWT Authentication",
                Contact = new OpenApiContact
                {
                    Name = "Paulo Eugênio",
                    Email = "pauloeugenioreis@msn.com",
                    Url = new Uri("https://www.linkedin.com/in/pauloeugenioreis")
                }
            });

            // Add JWT Bearer authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // DocumentFilter: tem acesso ao OpenApiDocument e cria referências corretamente no OpenApi 2.x
            c.DocumentFilter<AuthorizeDocumentFilter>();

            // Include XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerWithAuth(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectTemplate API v1");
            c.RoutePrefix = string.Empty;
        });

        return app;
    }
}

internal sealed class AuthorizeDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var path in swaggerDoc.Paths)
        {
            if (path.Value?.Operations is null) continue;
            foreach (var (operationType, operation) in path.Value.Operations)
            {
                var httpMethod = operationType.ToString();
                var relativePath = path.Key.TrimStart('/');

                var apiDesc = context.ApiDescriptions.FirstOrDefault(d =>
                    string.Equals(d.HttpMethod, httpMethod, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(d.RelativePath?.TrimEnd('/') ?? string.Empty, relativePath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));

                if (apiDesc is null) continue;

                var metadata = apiDesc.ActionDescriptor.EndpointMetadata;

                if (metadata.OfType<IAllowAnonymous>().Any()) continue;
                if (!metadata.OfType<IAuthorizeData>().Any()) continue;

                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", swaggerDoc),
                        []
                    }
                });
            }
        }
    }
}
