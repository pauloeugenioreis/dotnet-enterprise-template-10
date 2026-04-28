using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// Order management endpoints with custom service and repository
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrderController : ApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]

    [ProducesResponseType(typeof(PagedResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] string? status,
        [FromQuery] string? searchTerm,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _orderService.GetAllOrderDetailsAsync(status, searchTerm, startDate, endDate, page, pageSize, cancellationToken);

        if (page.HasValue && pageSize.HasValue)
            return HandlePagedResult(items, total, page.Value, pageSize.Value);

        return Ok(items);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]

    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderDetailsAsync(id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Get orders by customer email
    /// </summary>
    [HttpGet("customer/{email}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomerAsync(string email, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersByCustomerAsync(email, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Create new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateOrderRequest dto,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.CreateOrderAsync(dto, cancellationToken);

        _logger.LogInformation("Order {OrderNumber} created", order.OrderNumber);

        var location = Url.Action(
            nameof(GetByIdAsync),
            values: new { id = order.Id }) ?? $"/api/v1/order/{order.Id}";

        return Created(location, order);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatusAsync(
        long id,
        [FromBody] UpdateOrderStatusDto dto,
        CancellationToken cancellationToken)
    {
        await _orderService.UpdateOrderStatusAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update existing order
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        long id,
        [FromBody] UpdateOrderRequest dto,
        CancellationToken cancellationToken)
    {
        // For simplicity, we'll map the DTO to the entity here if the service doesn't have a direct DTO method
        // But since we want to follow Clean Architecture, we should check if IOrderService has UpdateAsync
        // It inherits from IService<Order>, so we can fetch and update
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order == null) return NotFound();

        order.CustomerName = dto.CustomerName;
        order.Status = dto.Status;
        if (dto.ShippingAddress != null) order.ShippingAddress = dto.ShippingAddress;
        if (dto.Notes != null) order.Notes = dto.Notes;

        await _orderService.UpdateAsync(id, order, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrderAsync(
        long id,
        [FromBody] string? reason,
        CancellationToken cancellationToken)
    {
        var dto = new UpdateOrderStatusDto("Cancelled", reason ?? "Cancelled by user");
        var order = await _orderService.UpdateOrderStatusAsync(id, dto, cancellationToken);

        _logger.LogInformation("Order {OrderNumber} cancelled", order.OrderNumber);

        return Ok(order);
    }

    /// <summary>
    /// Export orders to Excel
    /// </summary>
    [HttpGet("ExportToExcel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> ExportToExcelAsync(
        [FromQuery] string? status,
        [FromQuery] string? searchTerm,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Excel report for orders");

        var stopwatch = Stopwatch.StartNew();

        var (orders, _) = await _orderService.GetAllOrderDetailsAsync(status, searchTerm, startDate, endDate, cancellationToken: cancellationToken);

        var results = orders.ToList();

        // Flatten data for Excel
        var excelData = results.SelectMany(o => o.Items.Select(i => new
        {
            OrderId = o.Id,
            o.OrderNumber,
            o.CustomerName,
            o.CustomerEmail,
            o.Status,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            ItemSubtotal = i.Subtotal,
            OrderSubtotal = o.Subtotal,
            Tax = o.Tax,
            ShippingCost = o.ShippingCost,
            OrderTotal = o.Total,
            OrderDate = o.CreatedAt
        })).ToList();

        var config = new OpenXmlConfiguration
        {
            FastMode = true,
            EnableAutoWidth = true,
            AutoFilter = true
        };

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(excelData, sheetName: "Orders", configuration: config);

        memoryStream.Seek(0, SeekOrigin.Begin);

        stopwatch.Stop();

        _logger.LogInformation(
            "Excel generated with {Count} orders in {ElapsedMs}ms",
            results.Count,
            stopwatch.ElapsedMilliseconds);

        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// Get order statistics (computed at the database level)
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(OrderStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var stats = await _orderService.GetStatisticsAsync(cancellationToken);
        return Ok(stats);
    }
}
