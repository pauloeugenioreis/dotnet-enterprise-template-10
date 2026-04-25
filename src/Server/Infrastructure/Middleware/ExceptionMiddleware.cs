using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProjectTemplate.Domain.Exceptions;

namespace ProjectTemplate.Infrastructure.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            BusinessException be => new { StatusCode = (int)HttpStatusCode.BadRequest, Message = be.Message },
            ValidationException ve => new { StatusCode = (int)HttpStatusCode.BadRequest, Message = ve.Message },
            NotFoundException nfe => new { StatusCode = (int)HttpStatusCode.NotFound, Message = nfe.Message },
            _ => new { StatusCode = (int)HttpStatusCode.InternalServerError, Message = "An unexpected error occurred." }
        };

        context.Response.StatusCode = response.StatusCode;

        if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Business exception occurred: {Message}", exception.Message);
        }

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
