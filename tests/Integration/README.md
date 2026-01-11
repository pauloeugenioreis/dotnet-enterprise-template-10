# Integration Tests

This project contains integration tests for the ProjectTemplate API.

## Overview

Integration tests verify that all layers of the application work together correctly, including:
- API Controllers
- Services
- Repositories
- Database interactions
- Middleware
- Authentication/Authorization

## Test Framework

- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library
- **WebApplicationFactory** - In-memory test server
- **InMemory Database** - EF Core in-memory provider for testing

## Running Tests

```bash
# Run all integration tests
dotnet test tests/Integration

# Run with coverage
dotnet test tests/Integration /p:CollectCoverage=true

# Run specific test
dotnet test tests/Integration --filter "FullyQualifiedName~ProductControllerTests.GetAll_ReturnsSuccessStatusCode"
```

## Test Structure

```
tests/Integration/
├── Controllers/
│   ├── ProductControllerTests.cs
│   └── OrderControllerTests.cs
├── WebApplicationFactoryFixture.cs
└── README.md
```

## Writing Integration Tests

### Example Test

```csharp
public class ProductControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public ProductControllerTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Best Practices

1. **Use WebApplicationFactory** - Creates in-memory test server
2. **Use InMemory Database** - Fast and isolated tests
3. **Seed test data** - Ensure consistent test state
4. **Test full workflows** - Not just individual components
5. **Use FluentAssertions** - More readable assertions
6. **Clean up after tests** - Reset database between tests
7. **Test error scenarios** - Not just happy paths

## Test Coverage

Integration tests should cover:
- ✅ CRUD operations
- ✅ Business logic validation
- ✅ Error handling
- ✅ Authentication/Authorization
- ✅ Data validation
- ✅ API response formats
- ✅ Status codes
