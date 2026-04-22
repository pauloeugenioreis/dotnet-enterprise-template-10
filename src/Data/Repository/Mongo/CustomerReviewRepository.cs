using ProjectTemplate.SharedModels;
using MongoDB.Driver;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Data.Repository.Mongo;

/// <summary>
/// MongoDB repository for CustomerReview documents.
/// Uses FilterDefinitionBuilder for composable, type-safe queries.
/// </summary>
public class CustomerReviewRepository : MongoRepository<CustomerReview>, ICustomerReviewRepository
{
    public CustomerReviewRepository(IMongoDatabase database) : base(database)
    {
    }

    public async Task<(IEnumerable<CustomerReview> Items, long Total)> GetByFilterAsync(
        string? productName,
        int? minRating,
        bool? isApproved,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<CustomerReview>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(productName))
        {
            filter &= builder.Regex(r => r.ProductName, new MongoDB.Bson.BsonRegularExpression(productName, "i"));
        }

        if (minRating.HasValue)
        {
            filter &= builder.Gte(r => r.Rating, minRating.Value);
        }

        if (isApproved.HasValue)
        {
            filter &= builder.Eq(r => r.IsApproved, isApproved.Value);
        }

        var total = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var query = Collection.Find(filter).SortByDescending(r => r.CreatedAt);

        if (page.HasValue && pageSize.HasValue)
        {
            var items = await query
                .Skip((page.Value - 1) * pageSize.Value)
                .Limit(pageSize.Value)
                .ToListAsync(cancellationToken);
            return (items, total);
        }

        var allItems = await query.ToListAsync(cancellationToken);
        return (allItems, total);
    }
}
