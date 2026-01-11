using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Service for notifying about exceptions
/// </summary>
public interface IExceptionNotificationService
{
    /// <summary>
    /// Notifies about an exception that occurred during request processing
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="exception">The exception that occurred</param>
    Task NotifyAsync(HttpContext context, Exception exception);
}
