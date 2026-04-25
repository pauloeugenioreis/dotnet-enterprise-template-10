using System.Net.Http.Json;
using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public class CustomerReviewService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage) 
    : BaseService(httpClientFactory, localStorage, "api/v1/CustomerReview"), ICustomerReviewService
{
    public async Task<PagedResponse<CustomerReviewResponseDto>> GetReviewsAsync(int page = 1, int pageSize = 10, string? productName = null, int? minRating = null, bool? isApproved = null, CancellationToken ct = default)
    {
        var url = $"{ResourcePath}?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(productName)) url += $"&productName={Uri.EscapeDataString(productName)}";
        if (minRating.HasValue) url += $"&minRating={minRating}";
        if (isApproved.HasValue) url += $"&isApproved={isApproved.Value.ToString().ToLower()}";

        return await GetAsync<PagedResponse<CustomerReviewResponseDto>>(url, ct);
    }

    public async Task<CustomerReviewResponseDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await GetAsync<CustomerReviewResponseDto>($"{ResourcePath}/{id}", ct);
    }

    public async Task<CustomerReviewResponseDto> CreateAsync(CreateCustomerReviewRequest review, CancellationToken ct = default)
    {
        return await PostAsync<CreateCustomerReviewRequest, CustomerReviewResponseDto>(ResourcePath!, review, ct);
    }

    public async Task UpdateAsync(string id, UpdateCustomerReviewRequest review, CancellationToken ct = default)
    {
        await PutAsync($"{ResourcePath}/{id}", review, ct);
    }

    public async Task ApproveAsync(string id, bool isApproved, CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await Http.PatchAsJsonAsync($"{ResourcePath}/{id}/approve", new ApproveCustomerReviewRequest { IsApproved = isApproved }, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await DeleteRequestAsync($"{ResourcePath}/{id}", ct);
    }
}
