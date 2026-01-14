using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace ProjectTemplate.Infrastructure.Middleware;

/// <summary>
/// Global exception handler middleware
/// Catches unhandled exceptions and returns consistent ProblemDetails responses
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        // Notify about exception (can be overridden to send emails, Slack, etc.)
        try
        {
            var notificationService = context.RequestServices.GetService<IExceptionNotificationService>();
            if (notificationService != null)
            {
                // Create exception context DTO (decoupled from HttpContext)
                var exceptionContext = new Domain.Dtos.ExceptionContext
                {
                    User = context.User.Identity?.Name ?? "Anonymous",
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    Timestamp = DateTime.UtcNow,
                    AdditionalInfo = new Dictionary<string, string>
                    {
                        ["TraceId"] = context.TraceIdentifier,
                        ["ContentType"] = context.Request.ContentType ?? "N/A"
                    }
                };

                await notificationService.NotifyAsync(exceptionContext, exception);
            }
        }
        catch (Exception notificationEx)
        {
            _logger.LogError(notificationEx, "Failed to send exception notification");
        }

        try
        {
            var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
            var hostEnvironment = context.RequestServices.GetService<IHostEnvironment>();

            // Determine status code and details based on exception type
            var (status, title, detail) = exception switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    "Validation Error",
                    validationEx.Message),
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "You are not authorized to access this resource."),
                NotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    notFoundEx.Message),
                BusinessException businessEx => (
                    StatusCodes.Status422UnprocessableEntity,
                    "Business Rule Violation",
                    businessEx.Message),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    hostEnvironment?.IsDevelopment() == true ? exception.Message : "An error occurred processing your request.")
            };

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            // Add stack trace in development
            if (hostEnvironment?.IsDevelopment() == true && exception is not BusinessException)
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            }

            context.Response.StatusCode = status;
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails
            });
        }
        catch (Exception ex)
        {
            // Fallback if ProblemDetails service fails
            _logger.LogError(ex, "Error writing ProblemDetails for exception: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }
}
