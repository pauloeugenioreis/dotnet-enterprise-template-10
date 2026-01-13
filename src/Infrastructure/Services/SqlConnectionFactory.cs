using ProjectTemplate.Domain.Interfaces;
using System.Data.Common;
using System.Data.SqlClient;

namespace ProjectTemplate.Infrastructure.Services;

/// <summary>
/// SQL Server connection factory implementation
/// Registered in DI and injected into Dapper and ADO.NET repositories
/// Returns DbConnection to support async operations
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
            
        _connectionString = connectionString;
    }

    public DbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
