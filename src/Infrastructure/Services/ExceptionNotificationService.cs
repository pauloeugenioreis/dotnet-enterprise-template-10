using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProjectTemplate.Domain.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectTemplate.Infrastructure.Services;

/// <summary>
/// Default implementation that logs exceptions to console
/// Override this service to implement custom notification logic (email, Slack, etc.)
/// </summary>
public class ExceptionNotificationService : IExceptionNotificationService
{
    private readonly ILogger<ExceptionNotificationService> _logger;

    public ExceptionNotificationService(ILogger<ExceptionNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(HttpContext context, Exception exception)
    {
        try
        {
            var user = context.User.Identity?.Name ?? "Anonymous";
            var path = context.Request.Path;
            var method = context.Request.Method;

            // Log exception details
            _logger.LogError(exception,
                "Exception occurred for user {User} on {Method} {Path}",
                user, method, path);

            // Add user claims if available
            if (context.User.Identity is ClaimsIdentity claimsIdentity)
            {
                var claims = claimsIdentity.Claims;
                _logger.LogDebug("User claims: {Claims}",
                    string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));
            }

            // TODO: Implement custom notification logic here
            // Examples:
            // - Send email to admin
            // - Post to Slack channel
            // - Create issue in issue tracker
            // - Store in error database
        }
        catch (Exception ex)
        {
            // Don't throw exceptions in error handler
            _logger.LogError(ex, "Failed to send exception notification");
        }

        return Task.CompletedTask;
    }
}
