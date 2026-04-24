---
name: generate-excel-report
description: "Scaffold a new endpoint to generate and download a high-performance Excel report using MiniExcel."
argument-hint: "Entity name (e.g., Customer)"
---

# Generate Excel Report Scaffold

Creates a high-performance Excel export endpoint for a given entity using the `MiniExcel` library.

## When to Use
- When the user requests an endpoint to download an Excel sheet with the records of an entity.
- To export filtered data in an optimized way without allocating large strings in memory.

## Guidelines

- **No Pagination**: Export endpoints must return ALL records that match the given filters, ignoring any pagination parameters (`page`, `pageSize`).
- **DTO Projection**: ALWAYS export DTOs (e.g., `ProductResponseDto`), NEVER raw Domain Entities, to ensure proper column names and avoid exposing sensitive internal state.
- **FastMode**: `MiniExcel` should be configured with `FastMode = true` and `EnableAutoWidth = true` via `OpenXmlConfiguration`.
- **MemoryStream**: Data must be written directly to a `MemoryStream` and returned via the `File()` helper.

## Procedure

### 1. API Controller
1. Open the target controller (e.g., `src/Server/Api/Controllers/{Name}Controller.cs`).
2. Add a new `[HttpGet("ExportToExcel")]` endpoint.
3. Accept the same filtering query parameters (`[FromQuery]`) that the `GetAll` endpoint uses, but DO NOT accept `page` or `pageSize`.
4. Accept a `CancellationToken`.
5. Call the service to retrieve all filtered items without pagination.
6. Initialize the `OpenXmlConfiguration`.
7. Write the items to a `MemoryStream` using `await memoryStream.SaveAsAsync(items, sheetName: "{Name}s", configuration: config);`.
8. Reset the stream position (`memoryStream.Seek(0, SeekOrigin.Begin);`).
9. Return the file using `return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "{Name}s_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");`.

## Reference Template

```csharp
[HttpGet("ExportToExcel")]
public async Task<ActionResult> ExportToExcelAsync(
  [FromQuery] string? someFilter,
  CancellationToken cancellationToken)
{
  // 1. Fetch data without pagination
  var (items, _) = await _service.GetAllAsync(someFilter, page: null, pageSize: null, cancellationToken);
  
  // 2. Configure MiniExcel
  var config = new OpenXmlConfiguration
  {
    FastMode = true,
    EnableAutoWidth = true,
    AutoFilter = true
  };

  // 3. Write to memory stream
  var memoryStream = new MemoryStream();
  await memoryStream.SaveAsAsync(items.ToList(), sheetName: "Report", configuration: config);
  memoryStream.Seek(0, SeekOrigin.Begin);

  // 4. Return as file
  return File(
    memoryStream,
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
}
```
