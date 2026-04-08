---
description: "Use when creating or modifying API controllers. Covers base controller patterns, versioning, route conventions, and response handling."
applyTo: "src/Api/Controllers/**"
---

# Controller Conventions

## Base Controller
- All controllers must inherit `ApiControllerBase`
- Use `HandleResult<T>` for single-item responses
- Use `HandlePagedResult<T>` for paginated list responses

## Routing & Versioning
- Apply `[ApiVersion("1.0")]` attribute on every controller
- Use `[Route("api/v{version:apiVersion}/[controller]")]`
- Follow REST conventions: GET, POST, PUT, DELETE

## Patterns
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ExampleController(IService<Example> service) : ApiControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
        => HandleResult(await service.GetByIdAsync(id, ct));
}
```

## Rules
- Inject `IService<T>` or specialized service interfaces — never repositories directly
- Return `IActionResult` from all action methods
- Include `CancellationToken` in all async endpoints
- Use `[ProducesResponseType]` for Swagger documentation
