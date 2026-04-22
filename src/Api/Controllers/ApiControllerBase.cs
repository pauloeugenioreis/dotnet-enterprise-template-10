using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.SharedModels;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// Base controller with common functionality.
/// All controllers require authorization by default.
/// Use [AllowAnonymous] on specific endpoints to opt out.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(T? result)
    {
        if (object.Equals(result, default(T)))
        {
            return NotFound();
        }

        return Ok(result);
    }

    protected IActionResult HandlePagedResult<T>(IEnumerable<T> items, long total, int page, int pageSize)
    {
        long safeTotal = total < 0 ? 0 : total;
        int safePageSize = pageSize <= 0 ? 10 : pageSize;
        int safeTotalPages = (int)Math.Ceiling(safeTotal / (double)safePageSize);

        var response = new PagedResponse<T>
        {
            Items = items,
            TotalCount = safeTotal,
            PageNumber = page,
            PageSize = safePageSize,
            TotalPages = safeTotalPages
        };

        return Ok(response);
    }
}
