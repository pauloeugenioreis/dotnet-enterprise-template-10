using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using System.Diagnostics;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// Product management endpoints
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductController : ApiControllerBase
{
    private readonly IService<Product> _service;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IService<Product> service,
        ILogger<ProductController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with performance metrics
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="category">Filter by category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products with execution time</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<dynamic>> GetAllAsync(
        [FromQuery] bool? isActive,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var products = await _service.GetAllAsync(cancellationToken);

        // Apply filters
        var filtered = products.AsQueryable();
        
        if (isActive.HasValue)
            filtered = filtered.Where(p => p.IsActive == isActive.Value);
        
        if (!string.IsNullOrEmpty(category))
            filtered = filtered.Where(p => p.Category.Contains(category, StringComparison.OrdinalIgnoreCase));

        var results = filtered.ToList();

        stopwatch.Stop();

        return Ok(new
        {
            executionTime = $"{stopwatch.ElapsedMilliseconds}ms",
            totalCount = results.Count,
            items = results
        });
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    /// <param name="product">Product data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] Product product,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { error = true, messages = errors });
        }

        var created = await _service.CreateAsync(product, cancellationToken);

        _logger.LogInformation("Product {ProductName} created with ID {ProductId}", 
            created.Name, created.Id);

        var location = Url.Action(
            nameof(GetByIdAsync),
            values: new { id = created.Id }) ?? $"/api/v1/product/{created.Id}";
        
        return Created(location, created);
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="product">Updated product data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        long id,
        [FromBody] Product product,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { error = true, messages = errors });
        }

        if (product.Id == 0 || (id > 0 && product.Id != id))
            product.Id = id;

        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            _logger.LogWarning("Attempted to update non-existent product {ProductId}", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        await _service.UpdateAsync(id, product, cancellationToken);

        _logger.LogInformation("Product {ProductId} updated", id);

        return NoContent();
    }

    /// <summary>
    /// Delete product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            _logger.LogWarning("Attempted to delete non-existent product {ProductId}", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        await _service.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Product {ProductId} deleted", id);

        return NoContent();
    }

    /// <summary>
    /// Export products to Excel file
    /// Uses MiniExcelLibs for high-performance Excel generation
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="category">Filter by category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Excel file</returns>
    [HttpGet("ExportToExcel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> ExportToExcelAsync(
        [FromQuery] bool? isActive,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Excel report for products");

        var stopwatch = Stopwatch.StartNew();

        // Get all products
        var products = await _service.GetAllAsync(cancellationToken);

        // Apply filters
        var filtered = products.AsQueryable();
        
        if (isActive.HasValue)
            filtered = filtered.Where(p => p.IsActive == isActive.Value);
        
        if (!string.IsNullOrEmpty(category))
            filtered = filtered.Where(p => p.Category.Contains(category, StringComparison.OrdinalIgnoreCase));

        var results = filtered.ToList();

        // Configure Excel generation
        var config = new OpenXmlConfiguration
        {
            FastMode = true,           // Enable fast mode for better performance
            EnableAutoWidth = true,    // Auto-adjust column widths
            AutoFilter = true          // Enable auto-filter in Excel
        };

        // Generate Excel file in memory
        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(results, sheetName: "Products", configuration: config)
            .ConfigureAwait(false);
        
        memoryStream.Seek(0, SeekOrigin.Begin);

        stopwatch.Stop();

        _logger.LogInformation(
            "Excel generated with {Count} products in {ElapsedMs}ms",
            results.Count,
            stopwatch.ElapsedMilliseconds);

        // Return file
        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// Activate or deactivate product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="isActive">Active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatusAsync(
        long id,
        [FromQuery] bool isActive,
        CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        product.IsActive = isActive;
        await _service.UpdateAsync(id, product, cancellationToken);

        _logger.LogInformation(
            "Product {ProductId} status changed to {Status}",
            id,
            isActive ? "active" : "inactive");

        return NoContent();
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="quantity">Quantity to add (positive) or remove (negative)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    [HttpPatch("{id}/stock")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStockAsync(
        long id,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        var newStock = product.Stock + quantity;
        
        if (newStock < 0)
        {
            return BadRequest(new
            {
                message = "Insufficient stock",
                currentStock = product.Stock,
                requestedChange = quantity,
                resultingStock = newStock
            });
        }

        product.Stock = newStock;
        await _service.UpdateAsync(id, product, cancellationToken);

        _logger.LogInformation(
            "Product {ProductId} stock updated: {OldStock} -> {NewStock} ({Change})",
            id,
            product.Stock - quantity,
            product.Stock,
            quantity > 0 ? $"+{quantity}" : quantity.ToString());

        return Ok(product);
    }
}
