# Infrastructure Unit Tests

This project contains unit tests for the Infrastructure layer components.

## Overview

Unit tests for infrastructure components including:
- Extensions (Database, Cache, Logging, etc.)
- Middleware (Exception handling, Validation, etc.)
- Services (Storage, Queue, Notifications, etc.)
- Filters (Validation, etc.)

## Test Framework

- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library
- **Moq** - Mocking framework

## Running Tests

```bash
# Run all unit tests
dotnet test tests/Infrastructure.UnitTests

# Run with coverage
dotnet test tests/Infrastructure.UnitTests /p:CollectCoverage=true

# Run specific test
dotnet test tests/Infrastructure.UnitTests --filter "FullyQualifiedName~DatabaseExtensionTests"
```

## Test Structure

```
tests/Infrastructure.UnitTests/
├── Extensions/
│   └── DatabaseExtensionTests.cs
├── Middleware/
│   └── GlobalExceptionHandlerTests.cs
└── README.md
```

## Writing Unit Tests

### Example Test with Mocking

```csharp
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var exception = new ValidationException("Error");

        // Act
        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(400);
    }
}
```

## Best Practices

1. **Isolate tests** - Mock dependencies
2. **Test one thing** - Single responsibility per test
3. **Arrange-Act-Assert** - Clear test structure
4. **Descriptive names** - MethodName_Scenario_ExpectedResult
5. **Fast execution** - No I/O operations
6. **Independent tests** - No shared state
7. **Test edge cases** - Not just happy paths

## Test Coverage

Unit tests should cover:
- ✅ Configuration validation
- ✅ Exception handling
- ✅ Service registration
- ✅ Middleware pipeline
- ✅ Filter behavior
- ✅ Extension methods
- ✅ Error scenarios
