using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using ProjectTemplate.SharedModels;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// Product management endpoints
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductController(
    IProductService productService,
    ILogger<ProductController> logger) : ApiControllerBase
{
    /// <summary>
    /// Get all products with performance metrics
    /// </summary>
    [HttpGet]

    [ProducesResponseType(typeof(PagedResponse<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] bool? isActive,
        [FromQuery] string? category,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (items, total) = await productService.GetAllProductsAsync(isActive, category, page, pageSize, cancellationToken);

        if (page.HasValue && pageSize.HasValue)
            return HandlePagedResult(items, total, page.Value, pageSize.Value);

        return Ok(items);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]

    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var product = await productService.GetProductByIdAsync(id, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} not found", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest dto,
        CancellationToken cancellationToken)
    {
        var created = await productService.CreateProductAsync(dto, cancellationToken);

        var location = Url.Action(
            nameof(GetByIdAsync),
            values: new { id = created.Id }) ?? $"/api/v1/product/{created.Id}";

        return Created(location, created);
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        long id,
        [FromBody] UpdateProductRequest dto,
        CancellationToken cancellationToken)
    {
        await productService.UpdateProductAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Export products to Excel file
    /// </summary>
    [HttpGet("ExportToExcel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> ExportToExcelAsync(
        [FromQuery] bool? isActive,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Excel report for products");

        var stopwatch = Stopwatch.StartNew();

        var (products, _) = await productService.GetAllProductsAsync(isActive, category, cancellationToken: cancellationToken);
        var productList = products.ToList();

        var config = new OpenXmlConfiguration
        {
            FastMode = true,
            EnableAutoWidth = true,
            AutoFilter = true
        };

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(productList, sheetName: "Products", configuration: config);
        memoryStream.Seek(0, SeekOrigin.Begin);

        stopwatch.Stop();

        logger.LogInformation("Excel generated with {Count} products in {ElapsedMs}ms",
            productList.Count, stopwatch.ElapsedMilliseconds);

        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// Activate or deactivate product
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatusAsync(
        long id,
        [FromBody] UpdateProductStatusRequest dto,
        CancellationToken cancellationToken)
    {
        await productService.UpdateProductStatusAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    [HttpPatch("{id}/stock")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStockAsync(
        long id,
        [FromBody] UpdateProductStockRequest dto,
        CancellationToken cancellationToken)
    {
        var updated = await productService.UpdateProductStockAsync(id, dto, cancellationToken);
        return Ok(updated);
    }
}
