using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

namespace ProjectTemplate.UnitTests.Services;

/// <summary>
/// Unit tests for CustomerReviewService (MongoDB).
/// Tests business logic with mocked ICustomerReviewRepository.
/// </summary>
public class CustomerReviewServiceTests
{
    private readonly Mock<ICustomerReviewRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerReviewService>> _mockLogger;
    private readonly CustomerReviewService _service;

    public CustomerReviewServiceTests()
    {
        _mockRepository = new Mock<ICustomerReviewRepository>();
        _mockLogger = new Mock<ILogger<CustomerReviewService>>();
        _service = new CustomerReviewService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetReviewByIdAsync_WithExistingId_ReturnsDto()
    {
        // Arrange
        var review = CreateSampleReview("507f1f77bcf86cd799439011");
        _mockRepository
            .Setup(r => r.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var result = await _service.GetReviewByIdAsync("507f1f77bcf86cd799439011");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("507f1f77bcf86cd799439011");
        result.ProductName.Should().Be("Laptop");
        result.Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetReviewByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync("507f1f77bcf86cd799439099", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerReview?)null);

        // Act
        var result = await _service.GetReviewByIdAsync("507f1f77bcf86cd799439099");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllReviewsAsync_ReturnsMappedDtos()
    {
        // Arrange
        var reviews = new List<CustomerReview>
        {
            CreateSampleReview("507f1f77bcf86cd799439011"),
            CreateSampleReview("507f1f77bcf86cd799439012", productName: "Mouse", rating: 4)
        };
        _mockRepository
            .Setup(r => r.GetByFilterAsync(null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reviews.AsEnumerable(), (long)reviews.Count));

        // Act
        var (items, total) = await _service.GetAllReviewsAsync(null, null, null);

        // Assert
        items.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetAllReviewsAsync_WithFilters_PassesFiltersToRepository()
    {
        // Arrange
        var reviews = new List<CustomerReview> { CreateSampleReview("507f1f77bcf86cd799439011") };
        _mockRepository
            .Setup(r => r.GetByFilterAsync("Laptop", 4, true, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((reviews.AsEnumerable(), 1L));

        // Act
        var (items, total) = await _service.GetAllReviewsAsync("Laptop", 4, true, 1, 10);

        // Assert
        items.Should().HaveCount(1);
        _mockRepository.Verify(r => r.GetByFilterAsync("Laptop", 4, true, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReviewAsync_CreatesAndReturnsMappedDto()
    {
        // Arrange
        var request = new CreateCustomerReviewRequest
        {
            ProductName = "Laptop",
            CustomerName = "John",
            Rating = 5,
            Title = "Excellent!",
            Comment = "Highly recommended",
            Tags = ["quality", "value"],
            Metadata = new Dictionary<string, string> { ["platform"] = "web" },
            IsVerifiedPurchase = true
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<CustomerReview>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerReview entity, CancellationToken _) =>
            {
                entity.Id = "507f1f77bcf86cd799439011";
                return entity;
            });

        // Act
        var result = await _service.CreateReviewAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        result.ProductName.Should().Be("Laptop");
        result.Rating.Should().Be(5);
        result.Tags.Should().Contain("quality");
        result.Metadata.Should().ContainKey("platform");
        result.IsApproved.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateReviewAsync_WithExistingId_UpdatesSuccessfully()
    {
        // Arrange
        var review = CreateSampleReview("507f1f77bcf86cd799439011");
        _mockRepository
            .Setup(r => r.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<CustomerReview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateCustomerReviewRequest
        {
            ProductName = "Laptop Pro",
            CustomerName = "John",
            Rating = 4,
            Title = "Updated review"
        };

        // Act
        await _service.UpdateReviewAsync("507f1f77bcf86cd799439011", request);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(
            It.Is<CustomerReview>(c => c.ProductName == "Laptop Pro" && c.Rating == 4),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReviewAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync("507f1f77bcf86cd799439099", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerReview?)null);

        var request = new UpdateCustomerReviewRequest
        {
            ProductName = "Laptop",
            CustomerName = "John",
            Rating = 5,
            Title = "Test"
        };

        // Act
        var act = () => _service.UpdateReviewAsync("507f1f77bcf86cd799439099", request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*507f1f77bcf86cd799439099*");
    }

    [Fact]
    public async Task ApproveReviewAsync_WithExistingId_SetsApprovalStatus()
    {
        // Arrange
        var review = CreateSampleReview("507f1f77bcf86cd799439011");
        review.IsApproved = false;
        _mockRepository
            .Setup(r => r.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<CustomerReview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ApproveReviewAsync("507f1f77bcf86cd799439011", new ApproveCustomerReviewRequest { IsApproved = true });

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(
            It.Is<CustomerReview>(c => c.IsApproved),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_WithExistingId_DeletesSuccessfully()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.DeleteReviewAsync("507f1f77bcf86cd799439011");

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync("507f1f77bcf86cd799439099", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = () => _service.DeleteReviewAsync("507f1f77bcf86cd799439099");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*507f1f77bcf86cd799439099*");
    }

    private static CustomerReview CreateSampleReview(
        string id,
        string productName = "Laptop",
        int rating = 5)
    {
        return new CustomerReview
        {
            Id = id,
            ProductName = productName,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Rating = rating,
            Title = "Great product!",
            Comment = "Really enjoyed using it.",
            Tags = ["quality", "recommended"],
            Metadata = new Dictionary<string, string> { ["platform"] = "web" },
            IsVerifiedPurchase = true,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}
