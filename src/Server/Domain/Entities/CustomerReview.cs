namespace ProjectTemplate.Domain.Entities;

/// <summary>
/// Customer review entity stored in MongoDB.
/// Demonstrates NoSQL advantages: flexible tags, variable metadata, nested data.
/// </summary>
public class CustomerReview : MongoEntityBase
{
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }

    /// <summary>
    /// Flexible tag collection — showcases NoSQL schema-less design.
    /// Example: ["excellent-quality", "fast-shipping", "recommended"]
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Variable attributes — showcases NoSQL document flexibility.
    /// Example: { "platform": "mobile", "browser": "chrome", "os": "android" }
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
}
