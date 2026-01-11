using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(T? result)
    {
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    protected IActionResult HandlePagedResult<T>(IEnumerable<T> items, int total, int page, int pageSize)
    {
        var response = new PagedResponse<T>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };

        return Ok(response);
    }
}

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
