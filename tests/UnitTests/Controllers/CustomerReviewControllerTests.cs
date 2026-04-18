using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

namespace ProjectTemplate.UnitTests.Controllers;

/// <summary>
/// Unit tests for CustomerReviewController (MongoDB).
/// Tests controller logic with mocked ICustomerReviewService.
/// </summary>
public class CustomerReviewControllerTests
{
    private readonly Mock<ICustomerReviewService> _mockService;
    private readonly Mock<ILogger<CustomerReviewController>> _mockLogger;
    private readonly CustomerReviewController _controller;

    public CustomerReviewControllerTests()
    {
        _mockService = new Mock<ICustomerReviewService>();
        _mockLogger = new Mock<ILogger<CustomerReviewController>>();
        _controller = new CustomerReviewController(_mockService.Object, _mockLogger.Object);

        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/api/v1/CustomerReview/507f1f77bcf86cd799439011");
        _controller.Url = mockUrlHelper.Object;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkResult_WhenReviewsExist()
    {
        // Arrange
        var reviews = new List<CustomerReviewResponseDto>
        {
            new() { Id = "507f1f77bcf86cd799439011", ProductName = "Laptop", Rating = 5, Title = "Great!", CustomerName = "John" },
            new() { Id = "507f1f77bcf86cd799439012", ProductName = "Mouse", Rating = 4, Title = "Good", CustomerName = "Jane" }
        };
        _mockService
            .Setup(s => s.GetAllReviewsAsync(null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reviews, (long)reviews.Count));

        // Act
        var result = await _controller.GetAllAsync(null, null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult_WhenPaginationProvided()
    {
        // Arrange
        var reviews = new List<CustomerReviewResponseDto>
        {
            new() { Id = "507f1f77bcf86cd799439011", ProductName = "Laptop", Rating = 5, Title = "Great!", CustomerName = "John" }
        };
        _mockService
            .Setup(s => s.GetAllReviewsAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reviews, 15L));

        // Act
        var result = await _controller.GetAllAsync(null, null, null, 1, 10, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<PagedResponse<CustomerReviewResponseDto>>();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var reviewId = "507f1f77bcf86cd799439011";
        var review = new CustomerReviewResponseDto
        {
            Id = reviewId,
            ProductName = "Laptop",
            CustomerName = "John",
            Rating = 5,
            Title = "Excellent product"
        };
        _mockService
            .Setup(s => s.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _controller.GetByIdAsync(reviewId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var reviewId = "507f1f77bcf86cd799439099";
        _mockService
            .Setup(s => s.GetReviewByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerReviewResponseDto?)null);

        // Act
        var result = await _controller.GetByIdAsync(reviewId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateCustomerReviewRequest
        {
            ProductName = "Laptop",
            CustomerName = "John",
            Rating = 5,
            Title = "Excellent product",
            Comment = "Highly recommend!",
            Tags = ["quality", "recommended"],
            IsVerifiedPurchase = true
        };
        var created = new CustomerReviewResponseDto
        {
            Id = "507f1f77bcf86cd799439011",
            ProductName = request.ProductName,
            CustomerName = request.CustomerName,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment,
            Tags = request.Tags,
            IsVerifiedPurchase = request.IsVerifiedPurchase
        };
        _mockService
            .Setup(s => s.CreateReviewAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.CreateAsync(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/v1/CustomerReview/{created.Id}");
    }

    [Fact]
    public async Task UpdateAsync_WithValidDto_ReturnsNoContent()
    {
        // Arrange
        var reviewId = "507f1f77bcf86cd799439011";
        var request = new UpdateCustomerReviewRequest
        {
            ProductName = "Laptop Pro",
            CustomerName = "John",
            Rating = 4,
            Title = "Updated review"
        };
        _mockService
            .Setup(s => s.UpdateReviewAsync(reviewId, request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(reviewId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = "507f1f77bcf86cd799439099";
        var request = new UpdateCustomerReviewRequest
        {
            ProductName = "Laptop",
            CustomerName = "John",
            Rating = 5,
            Title = "Test"
        };
        _mockService
            .Setup(s => s.UpdateReviewAsync(reviewId, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"CustomerReview with ID {reviewId} not found"));

        // Act
        var act = () => _controller.UpdateAsync(reviewId, request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ApproveAsync_WithValidPayload_ReturnsNoContent()
    {
        // Arrange
        var reviewId = "507f1f77bcf86cd799439011";
        var dto = new ApproveCustomerReviewRequest { IsApproved = true };
        _mockService
            .Setup(s => s.ApproveReviewAsync(reviewId, dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ApproveAsync(reviewId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var reviewId = "507f1f77bcf86cd799439011";
        _mockService
            .Setup(s => s.DeleteReviewAsync(reviewId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(reviewId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = "507f1f77bcf86cd799439099";
        _mockService
            .Setup(s => s.DeleteReviewAsync(reviewId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"CustomerReview with ID {reviewId} not found"));

        // Act
        var act = () => _controller.DeleteAsync(reviewId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
