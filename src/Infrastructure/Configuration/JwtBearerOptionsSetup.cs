using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Configuration;

/// <summary>
/// Configures JWT Bearer authentication options from AppSettings
/// </summary>
public class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AppSettings _appSettings;

    public JwtBearerOptionsSetup(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(JwtBearerDefaults.AuthenticationScheme, options);
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        var authSettings = _appSettings.Authentication;
        var key = Encoding.ASCII.GetBytes(authSettings.Jwt.Secret);

        options.RequireHttpsMetadata = false; // Set to true in production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = authSettings.Jwt.ValidateIssuer,
            ValidateAudience = authSettings.Jwt.ValidateAudience,
            ValidateLifetime = authSettings.Jwt.ValidateLifetime,
            ValidateIssuerSigningKey = authSettings.Jwt.ValidateIssuerSigningKey,
            ValidIssuer = authSettings.Jwt.Issuer,
            ValidAudience = authSettings.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers["Token-Expired"] = "true";
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = "Unauthorized",
                    message = "You are not authorized to access this resource"
                });
                return context.Response.WriteAsync(result, context.HttpContext.RequestAborted);
            }
        };
    }
}
