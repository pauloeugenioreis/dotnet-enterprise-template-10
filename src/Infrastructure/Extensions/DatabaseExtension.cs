using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Infrastructure.Services;
// using LinqToDB.AspNet; // Uncomment when using Linq2Db

namespace ProjectTemplate.Infrastructure.Extensions;

/// <summary>
/// Database configuration extension supporting multiple ORMs
/// 
/// IMPORTANT: Entity Framework Core is the DEFAULT ORM and is enabled by default.
/// 
/// ==================================================================================
/// QUICK START - How to switch ORMs:
/// ==================================================================================
/// 1. DAPPER (Ready to use immediately):
///    - Comment line 41: services.AddEntityFramework(...)
///    - Uncomment line 47: services.AddDapper(...)
///    - Run the project!
/// 
/// 2. NHIBERNATE (Requires package installation):
///    - Uncomment NHibernate packages in src/Data/Data.csproj (lines ~31-32)
///    - Remove <Compile Remove> for NHibernate in Data.csproj (~line 46)
///    - Uncomment implementation in AddNHibernate method below
///    - Comment line 41, uncomment line 52
///    - Run: dotnet restore && dotnet run --project src/Api
/// 
/// 3. LINQ2DB (Requires package installation):
///    - Uncomment linq2db packages in Data.csproj and Infrastructure.csproj
///    - Uncomment using LinqToDB.AspNet at top of this file
///    - Remove <Compile Remove> for Linq2Db in Data.csproj (~line 51)
///    - Uncomment implementation in AddLinq2Db method below
///    - Comment line 41, uncomment line 57
///    - Run: dotnet restore && dotnet run --project src/Api
/// 
/// ==================================================================================
/// ALL ORMs HAVE COMPLETE IMPLEMENTATIONS FOR PRODUCT AND ORDER!
/// See: src/Data/Repository/README.md for detailed instructions
/// ==================================================================================
/// 
/// NO CONFIGURATION IN appsettings.json IS NEEDED!
/// This is intentional to keep configuration simple and avoid errors.
/// </summary>
public static class DatabaseExtension
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        var dbSettings = appSettings?.Infrastructure?.Database ?? new DatabaseSettings();
        var connectionString = appSettings?.ConnectionStrings?.DefaultConnection 
            ?? throw new InvalidOperationException("DefaultConnection string is required");

        // ==============================================================================
        // ORM CONFIGURATION - Entity Framework Core is used by default
        // ==============================================================================
        // To use a different ORM, uncomment the desired section below and comment out
        // the Entity Framework section. See docs/ORM-GUIDE.md for detailed instructions.
        // ==============================================================================

        // DEFAULT: Entity Framework Core
        services.AddEntityFramework(connectionString, dbSettings);

        // ALTERNATIVE 1: Dapper (High Performance)
        // Uncomment the line below to use Dapper instead of Entity Framework
        // See docs/ORM-GUIDE.md - "Dapper Configuration" section
        // services.AddDapper(connectionString);

        // ALTERNATIVE 2: NHibernate (Enterprise Features)
        // Uncomment the line below to use NHibernate instead of Entity Framework
        // See docs/ORM-GUIDE.md - "NHibernate Configuration" section
        // services.AddNHibernate(connectionString, dbSettings);

        // ALTERNATIVE 3: Linq2Db (LINQ + Performance)
        // Uncomment the line below to use Linq2Db instead of Entity Framework
        // See docs/ORM-GUIDE.md - "Linq2Db Configuration" section
        // services.AddLinq2Db(connectionString, dbSettings);

        // ALTERNATIVE 4: ADO.NET (Maximum Control & Performance)
        // Uncomment the line below to use raw ADO.NET instead of Entity Framework
        // See docs/ORM-GUIDE.md - "ADO.NET Configuration" section
        // services.AddAdo(connectionString);

        return services;
    }

    private static IServiceCollection AddEntityFramework(
        this IServiceCollection services,
        string connectionString,
        DatabaseSettings settings)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Configure database provider based on DatabaseType
            // For production, install and uncomment the appropriate provider:
            // - Microsoft.EntityFrameworkCore.SqlServer for SQL Server
            // - Oracle.EntityFrameworkCore for Oracle
            // - Npgsql.EntityFrameworkCore.PostgreSQL for PostgreSQL
            // - Pomelo.EntityFrameworkCore.MySql for MySQL
            
            switch (settings.DatabaseType.ToLower())
            {
                case "memory":
                case "inmemory":
                    options.UseInMemoryDatabase("ProjectTemplateDb");
                    break;

                // case "sqlserver":
                //     options.UseSqlServer(connectionString, sqlOptions =>
                //     {
                //         sqlOptions.CommandTimeout(settings.CommandTimeoutSeconds);
                //         sqlOptions.EnableRetryOnFailure(3);
                //     });
                //     break;

                default:
                    // Default to in-memory for testing/development
                    options.UseInMemoryDatabase("ProjectTemplateDb");
                    break;
            }

            // Common EF Core configurations
            if (settings.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            options.EnableDetailedErrors();
        });

        // Register DbContext as generic DbContext for Repository<T> to resolve
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register IDbConnectionFactory for Dapper/ADO.NET repositories (if they are auto-registered by Scrutor)
        services.AddSingleton<IDbConnectionFactory>(sp => 
            new SqlConnectionFactory(connectionString));

        return services;
    }

    private static IServiceCollection AddDapper(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbConnectionFactory for dependency injection
        // BEST PRACTICE: Instead of creating connections inside repositories,
        // we inject a factory that creates connections. This provides:
        // - Better testability (can mock IDbConnectionFactory)
        // - Proper dependency injection
        // - Centralized connection configuration
        // - Easier to switch database providers
        services.AddScoped<IDbConnectionFactory>(provider => 
            new Infrastructure.Services.SqlConnectionFactory(connectionString));
        
        // Register Dapper repositories
        // Dapper repositories implement IRepositoryDapper<T> (which inherits from IRepository<T>)
        // This prevents auto-registration by Scrutor and allows explicit ORM selection
        services.AddScoped<IRepository<Product>, Data.Repository.Dapper.ProductDapperRepository>();
        services.AddScoped<IRepository<Order>, Data.Repository.Dapper.OrderDapperRepository>();
        
        return services;
    }

    private static IServiceCollection AddNHibernate(
        this IServiceCollection services,
        string connectionString,
        DatabaseSettings settings)
    {
        // =============================================================================
        // TO USE NHIBERNATE:
        // 1. Uncomment NHibernate packages in src/Data/Data.csproj (lines ~31-32)
        // 2. Uncomment the line below that removes NHibernate files from compilation
        // 3. Run: dotnet restore
        // 4. Uncomment the code below
        // =============================================================================
        
        throw new NotImplementedException(
            "NHibernate is not enabled. " +
            "To enable: Uncomment NHibernate packages in src/Data/Data.csproj, " +
            "remove the <Compile Remove> for NHibernate in Data.csproj, " +
            "then uncomment the implementation below. " +
            "See src/Data/Repository/README.md for details.");
        
        /*
        // Configure NHibernate SessionFactory
        var sessionFactory = FluentNHibernate.Cfg.Fluently.Configure()
            .Database(FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012
                .ConnectionString(connectionString)
                .ShowSql())
            .Mappings(m => m.FluentMappings
                .AddFromAssemblyOf<Data.Mappings.NHibernate.ProductMap>())
            .BuildSessionFactory();

        services.AddSingleton(sessionFactory);
        services.AddScoped(factory => sessionFactory.OpenSession());
        
        // Register NHibernate repositories
        services.AddScoped<IRepository<Product>, Data.Repository.NHibernate.ProductNHibernateRepository>();
        services.AddScoped<IRepository<Order>, Data.Repository.NHibernate.OrderNHibernateRepository>();

        return services;
        */
    }

    private static IServiceCollection AddLinq2Db(
        this IServiceCollection services,
        string connectionString,
        DatabaseSettings settings)
    {
        // =============================================================================
        // TO USE LINQ2DB:
        // 1. Uncomment linq2db packages in src/Data/Data.csproj (lines ~35-36)
        // 2. Uncomment linq2db.AspNet in src/Infrastructure/Infrastructure.csproj (line ~44)
        // 3. Uncomment the using statement at the top of this file (line 8)
        // 4. Uncomment the line below that removes Linq2Db files from compilation
        // 5. Run: dotnet restore
        // 6. Uncomment the code below
        // =============================================================================
        
        throw new NotImplementedException(
            "Linq2Db is not enabled. " +
            "To enable: Uncomment linq2db packages in src/Data/Data.csproj and src/Infrastructure/Infrastructure.csproj, " +
            "remove the <Compile Remove> for Linq2Db in Data.csproj, " +
            "then uncomment the implementation below. " +
            "See src/Data/Repository/README.md for details.");
        
        /*
        // Configure Linq2Db DataConnection
        services.AddLinqToDbContext<ApplicationDataConnection>((provider, options) =>
        {
            options.UseSqlServer(connectionString);
        });
        
        // Register Linq2Db repositories
        services.AddScoped<IRepository<Product>, Data.Repository.Linq2Db.ProductLinq2DbRepository>();
        services.AddScoped<IRepository<Order>, Data.Repository.Linq2Db.OrderLinq2DbRepository>();

        return services;
        */
    }

    /// <summary>
    /// Configures ADO.NET as the data access layer
    /// Provides maximum control and performance with raw database operations
    /// </summary>
    private static IServiceCollection AddAdo(
        this IServiceCollection services,
        string connectionString)
    {
        // Register connection factory (if not already registered)
        services.AddSingleton<IDbConnectionFactory>(sp => 
            new SqlConnectionFactory(connectionString));
        
        // Register ADO.NET repositories
        // ADO.NET repositories implement IRepositoryAdo<T> (which inherits from IRepository<T>)
        // This prevents auto-registration by Scrutor and allows explicit ORM selection
        services.AddScoped<IRepository<Product>, Data.Repository.Ado.ProductAdoRepository>();
        services.AddScoped<IRepository<Order>, Data.Repository.Ado.OrderAdoRepository>();

        return services;
    }
}
