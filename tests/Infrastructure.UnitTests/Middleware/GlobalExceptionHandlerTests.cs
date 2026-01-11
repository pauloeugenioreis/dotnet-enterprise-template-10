using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Infrastructure.Middleware;
using System.Net;
using Xunit;

namespace ProjectTemplate.Infrastructure.UnitTests.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionHandler
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _nextMock = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var exception = new ValidationException("Validation error");
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        var handler = new GlobalExceptionHandler(_nextMock.Object, _loggerMock.Object);

        // Act
        await handler.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var exception = new NotFoundException("Resource not found");
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        var handler = new GlobalExceptionHandler(_nextMock.Object, _loggerMock.Object);

        // Act
        await handler.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_BusinessException_Returns422()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var exception = new BusinessException("Business rule violation");
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        var handler = new GlobalExceptionHandler(_nextMock.Object, _loggerMock.Object);

        // Act
        await handler.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var exception = new Exception("Unexpected error");
        
        _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        var handler = new GlobalExceptionHandler(_nextMock.Object, _loggerMock.Object);

        // Act
        await handler.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
}
