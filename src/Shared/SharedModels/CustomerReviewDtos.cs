namespace ProjectTemplate.SharedModels;

/// <summary>
/// Request payload for creating a customer review.
/// </summary>
public record CreateCustomerReviewRequest
{
    public required string ProductName { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public int Rating { get; init; }
    public required string Title { get; init; }
    public string? Comment { get; init; }
    public List<string> Tags { get; init; } = [];
    public Dictionary<string, string> Metadata { get; init; } = [];
    public bool IsVerifiedPurchase { get; init; }
}

/// <summary>
/// Request payload for updating a customer review.
/// </summary>
public record UpdateCustomerReviewRequest
{
    public required string ProductName { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public int Rating { get; init; }
    public required string Title { get; init; }
    public string? Comment { get; init; }
    public List<string> Tags { get; init; } = [];
    public Dictionary<string, string> Metadata { get; init; } = [];
    public bool IsVerifiedPurchase { get; init; }
}

/// <summary>
/// DTO returned by the API when interacting with customer reviews.
/// </summary>
public record CustomerReviewResponseDto
{
    public string Id { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerEmail { get; init; }
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Comment { get; init; }
    public List<string> Tags { get; init; } = [];
    public Dictionary<string, string> Metadata { get; init; } = [];
    public bool IsVerifiedPurchase { get; init; }
    public bool IsApproved { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Request payload for approving or rejecting a customer review.
/// </summary>
public record ApproveCustomerReviewRequest
{
    public bool IsApproved { get; init; }
}
