using ProjectTemplate.Shared.Models;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

namespace ProjectTemplate.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for CustomerReviewController.
/// Uses an in-memory implementation of ICustomerReviewService to keep tests deterministic in CI.
/// </summary>
[Collection("Integration Tests")]
public class CustomerReviewControllerTests
{
    private readonly HttpClient _client;

    public CustomerReviewControllerTests(WebApplicationFactoryFixture factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICustomerReviewService>();
                services.AddSingleton<ICustomerReviewService, InMemoryCustomerReviewService>();
            });
        });

        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WithoutPagination_ReturnsArray()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/CustomerReview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<List<CustomerReviewResponseDto>>();
        items.Should().NotBeNull();
        items.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsPagedResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/CustomerReview?page=1&pageSize=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<CustomerReviewResponseDto>>();
        payload.Should().NotBeNull();
        payload!.Items.Should().NotBeNull();
        payload.Items.Should().HaveCount(1);
        payload.PageNumber.Should().Be(1);
        payload.PageSize.Should().Be(1);
        payload.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetById_ExistingReview_ReturnsOk()
    {
        // Arrange
        var createRequest = CreateValidRequest(title: "Created for GetById");
        var createResponse = await _client.PostAsJsonAsync("/api/v1/CustomerReview", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/CustomerReview/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var review = await response.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();
        review.Should().NotBeNull();
        review!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetById_NonExistentReview_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/CustomerReview/non-existent-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ValidReview_ReturnsCreated()
    {
        // Arrange
        var request = CreateValidRequest(title: "Brand new review");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/CustomerReview", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/v1/CustomerReview/");

        var payload = await response.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();
        payload.Should().NotBeNull();
        payload!.Title.Should().Be("Brand new review");
    }

    [Fact]
    public async Task Update_ExistingReview_ReturnsNoContent()
    {
        // Arrange
        var createRequest = CreateValidRequest(title: "Before update");
        var createResponse = await _client.PostAsJsonAsync("/api/v1/CustomerReview", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();

        var updateRequest = new UpdateCustomerReviewRequest
        {
            ProductName = "Laptop",
            CustomerName = "Maria",
            CustomerEmail = "maria@example.com",
            Rating = 4,
            Title = "After update",
            Comment = "Updated comment",
            Tags = ["updated"],
            Metadata = new Dictionary<string, string> { ["source"] = "integration-test" },
            IsVerifiedPurchase = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/CustomerReview/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/CustomerReview/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();
        updated!.Title.Should().Be("After update");
    }

    [Fact]
    public async Task Approve_ExistingReview_ReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/CustomerReview", CreateValidRequest(title: "Approval test"));
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();

        // Act
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/CustomerReview/{created!.Id}/approve",
            new ApproveCustomerReviewRequest { IsApproved = true });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/CustomerReview/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();
        updated!.IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ExistingReview_ReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/CustomerReview", CreateValidRequest(title: "Delete test"));
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerReviewResponseDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/CustomerReview/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/CustomerReview/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static CreateCustomerReviewRequest CreateValidRequest(string title) => new()
    {
        ProductName = "Laptop",
        CustomerName = "Maria",
        CustomerEmail = "maria@example.com",
        Rating = 5,
        Title = title,
        Comment = "Great product",
        Tags = ["quality", "recommend"],
        Metadata = new Dictionary<string, string> { ["source"] = "integration-test" },
        IsVerifiedPurchase = true
    };

    private sealed record PagedResult<T>
    {
        public List<T> Items { get; init; } = [];
        public long TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }

    private sealed class InMemoryCustomerReviewService : ICustomerReviewService
    {
        private readonly List<CustomerReviewResponseDto> _items =
        [
            new CustomerReviewResponseDto
            {
                Id = "review-seed-1",
                ProductName = "Laptop",
                CustomerName = "Seed User",
                CustomerEmail = "seed@example.com",
                Rating = 5,
                Title = "Seed review",
                Comment = "Seed comment",
                Tags = ["seed"],
                Metadata = new Dictionary<string, string>(),
                IsVerifiedPurchase = true,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            }
        ];

        private readonly object _sync = new();

        public Task<CustomerReviewResponseDto?> GetReviewByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                return Task.FromResult(_items.FirstOrDefault(r => r.Id == id));
            }
        }

        public Task<(IEnumerable<CustomerReviewResponseDto> Items, long Total)> GetAllReviewsAsync(
            string? productName,
            int? minRating,
            bool? isApproved,
            int? page = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                IEnumerable<CustomerReviewResponseDto> query = _items;

                if (!string.IsNullOrWhiteSpace(productName))
                {
                    query = query.Where(x => x.ProductName.Contains(productName, StringComparison.OrdinalIgnoreCase));
                }

                if (minRating.HasValue)
                {
                    query = query.Where(x => x.Rating >= minRating.Value);
                }

                if (isApproved.HasValue)
                {
                    query = query.Where(x => x.IsApproved == isApproved.Value);
                }

                var total = query.LongCount();

                if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
                {
                    query = query
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value);
                }

                return Task.FromResult((query.ToList().AsEnumerable(), total));
            }
        }

        public Task<CustomerReviewResponseDto> CreateReviewAsync(CreateCustomerReviewRequest dto, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var created = new CustomerReviewResponseDto
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ProductName = dto.ProductName,
                    CustomerName = dto.CustomerName,
                    CustomerEmail = dto.CustomerEmail,
                    Rating = dto.Rating,
                    Title = dto.Title,
                    Comment = dto.Comment,
                    Tags = dto.Tags,
                    Metadata = dto.Metadata,
                    IsVerifiedPurchase = dto.IsVerifiedPurchase,
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                };

                _items.Add(created);
                return Task.FromResult(created);
            }
        }

        public Task UpdateReviewAsync(string id, UpdateCustomerReviewRequest dto, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var current = _items.FirstOrDefault(r => r.Id == id)
                    ?? throw new NotFoundException($"CustomerReview with ID {id} not found");

                var updated = current with
                {
                    ProductName = dto.ProductName,
                    CustomerName = dto.CustomerName,
                    CustomerEmail = dto.CustomerEmail,
                    Rating = dto.Rating,
                    Title = dto.Title,
                    Comment = dto.Comment,
                    Tags = dto.Tags,
                    Metadata = dto.Metadata,
                    IsVerifiedPurchase = dto.IsVerifiedPurchase,
                    UpdatedAt = DateTime.UtcNow
                };

                var index = _items.FindIndex(r => r.Id == id);
                _items[index] = updated;

                return Task.CompletedTask;
            }
        }

        public Task ApproveReviewAsync(string id, ApproveCustomerReviewRequest dto, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var current = _items.FirstOrDefault(r => r.Id == id)
                    ?? throw new NotFoundException($"CustomerReview with ID {id} not found");

                var updated = current with
                {
                    IsApproved = dto.IsApproved,
                    UpdatedAt = DateTime.UtcNow
                };

                var index = _items.FindIndex(r => r.Id == id);
                _items[index] = updated;

                return Task.CompletedTask;
            }
        }

        public Task DeleteReviewAsync(string id, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var removed = _items.RemoveAll(r => r.Id == id) > 0;
                if (!removed)
                {
                    throw new NotFoundException($"CustomerReview with ID {id} not found");
                }

                return Task.CompletedTask;
            }
        }
    }
}
