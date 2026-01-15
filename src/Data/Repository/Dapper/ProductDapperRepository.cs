using Dapper;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using System.Data;

namespace ProjectTemplate.Data.Repository.Dapper;

/// <summary>
/// Dapper implementation of Product repository
/// High-performance data access using Dapper micro-ORM
/// Uses IDbConnectionFactory injected via DI for proper connection management
/// </summary>
public class ProductDapperRepository : IProductDapperRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductDapperRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, Name, Description, Price, Stock, Category, IsActive, CreatedAt, UpdatedAt
            FROM Products
            WHERE Id = @Id";

        return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, Name, Description, Price, Stock, Category, IsActive, CreatedAt, UpdatedAt
            FROM Products";

        return await connection.QueryAsync<Product>(sql).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Product>> FindAsync(Func<Product, bool> predicate, CancellationToken cancellationToken = default)
    {
        // Dapper doesn't support LINQ predicates directly
        // For complex queries, implement specific methods or use GetAllAsync and filter in memory
        var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        return all.Where(predicate);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Products (Name, Description, Price, Stock, Category, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Description, @Price, @Stock, @Category, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT CAST(SCOPE_IDENTITY() as bigint)";

        entity.CreatedAt = DateTime.UtcNow;
        var id = await connection.QuerySingleAsync<long>(sql, entity).ConfigureAwait(false);
        entity.Id = id;

        return entity;
    }

    public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Products (Name, Description, Price, Stock, Category, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Description, @Price, @Stock, @Category, @IsActive, @CreatedAt, @UpdatedAt)";

        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }

        await connection.ExecuteAsync(sql, entityList).ConfigureAwait(false);
        return entityList;
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Products
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                Stock = @Stock,
                Category = @Category,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        entity.UpdatedAt = DateTime.UtcNow;
        await connection.ExecuteAsync(sql, entity).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Products WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { entity.Id }).ConfigureAwait(false);
    }

    public async Task DeleteRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Products WHERE Id = @Id";
        await connection.ExecuteAsync(sql, entities.Select(e => new { e.Id })).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string countSql = "SELECT COUNT(*) FROM Products";
        var total = await connection.ExecuteScalarAsync<int>(countSql).ConfigureAwait(false);

        const string dataSql = @"
            SELECT Id, Name, Description, Price, Stock, Category, IsActive, CreatedAt, UpdatedAt
            FROM Products
            ORDER BY Id
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var items = await connection.QueryAsync<Product>(dataSql, new
        {
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        }).ConfigureAwait(false);

        return (items, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dapper doesn't have a unit of work pattern by default
        // Changes are immediately persisted
        return Task.FromResult(0);
    }
}
