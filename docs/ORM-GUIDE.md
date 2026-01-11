# Guia de Troca de ORM

Este documento fornece instruções detalhadas sobre como trocar o ORM padrão (Entity Framework Core) por outras alternativas.

## 📋 ORMs Suportados

1. **Entity Framework Core** (Padrão)
2. **Dapper** (Configuração incluída)
3. **NHibernate** (Preparado para implementação)
4. **Linq2Db** (Preparado para implementação)

---

## 🔄 Entity Framework Core (Padrão)

### Configuração

No `appsettings.json`:

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Database": {
        "Provider": "EntityFramework",
        "DatabaseType": "SqlServer"
      }
    }
  }
}
```

### Implementação de Repositório

```csharp
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .ToListAsync();
    }
}
```

---

## ⚡ Dapper (Alta Performance)

### 1. Configuração

No `appsettings.json`:

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Database": {
        "Provider": "Dapper",
        "DatabaseType": "SqlServer"
      }
    }
  }
}
```

### 2. Instalação de Pacotes

Já incluído no template. Verifique `src/Data/Data.csproj`:

```xml
<PackageReference Include="Dapper" Version="2.1.66" />
```

### 3. Implementação de Repositório com Dapper

```csharp
using Dapper;
using System.Data;
using System.Data.SqlClient;

public class ProductDapperRepository : IRepository<Product>
{
    private readonly string _connectionString;

    public ProductDapperRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id", 
            new { Id = id });
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<Product>("SELECT * FROM Products");
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"
            INSERT INTO Products (Name, Price, Description, CreatedAt, IsActive) 
            VALUES (@Name, @Price, @Description, @CreatedAt, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as bigint)";
        
        var id = await connection.QuerySingleAsync<long>(sql, entity);
        entity.Id = id;
        return entity;
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"
            UPDATE Products 
            SET Name = @Name, 
                Price = @Price, 
                Description = @Description,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", new { entity.Id });
    }

    // Implemente os demais métodos da interface...
}
```

### 4. Registro no DI

Em `src/Infrastructure/Extensions/DatabaseExtension.cs`:

```csharp
private static IServiceCollection AddDapper(
    this IServiceCollection services,
    string connectionString)
{
    // Registrar connection string como singleton
    services.AddSingleton(connectionString);
    
    // Registrar repositórios manualmente
    services.AddScoped<IRepository<Product>, ProductDapperRepository>();
    
    return services;
}
```

---

## 🔧 NHibernate

### 1. Instalação de Pacotes

Adicione ao `src/Data/Data.csproj`:

```xml
<PackageReference Include="NHibernate" Version="5.5.2" />
<PackageReference Include="FluentNHibernate" Version="3.4.0" />
```

### 2. Configuração

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Database": {
        "Provider": "NHibernate",
        "DatabaseType": "SqlServer"
      }
    }
  }
}
```

### 3. Criar Mappings

```csharp
// src/Data/Mappings/ProductMap.cs
using FluentNHibernate.Mapping;

public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Table("Products");
        Id(x => x.Id).GeneratedBy.Identity();
        Map(x => x.Name).Not.Nullable();
        Map(x => x.Price).Not.Nullable();
        Map(x => x.Description).Nullable();
        Map(x => x.CreatedAt).Not.Nullable();
        Map(x => x.UpdatedAt).Nullable();
        Map(x => x.IsActive).Not.Nullable();
    }
}
```

### 4. Configurar SessionFactory

```csharp
// src/Infrastructure/Extensions/DatabaseExtension.cs
private static IServiceCollection AddNHibernate(
    this IServiceCollection services,
    string connectionString,
    DatabaseSettings settings)
{
    var sessionFactory = Fluently.Configure()
        .Database(MsSqlConfiguration.MsSql2012
            .ConnectionString(connectionString)
            .ShowSql())
        .Mappings(m => m.FluentMappings
            .AddFromAssemblyOf<ProductMap>())
        .BuildSessionFactory();

    services.AddSingleton(sessionFactory);
    services.AddScoped(factory => sessionFactory.OpenSession());

    return services;
}
```

### 5. Implementar Repositório

```csharp
public class ProductNHibernateRepository : IRepository<Product>
{
    private readonly ISession _session;

    public ProductNHibernateRepository(ISession session)
    {
        _session = session;
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _session.GetAsync<Product>(id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _session.Query<Product>().ToListAsync(cancellationToken);
    }

    // Implemente os demais métodos...
}
```

---

## 🚀 Linq2Db

### 1. Instalação de Pacotes

Adicione ao `src/Data/Data.csproj`:

```xml
<PackageReference Include="linq2db" Version="5.4.1" />
<PackageReference Include="linq2db.EntityFrameworkCore" Version="8.1.0" />
```

### 2. Configuração

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Database": {
        "Provider": "Linq2Db",
        "DatabaseType": "SqlServer"
      }
    }
  }
}
```

### 3. Criar DataConnection

```csharp
// src/Data/Context/ApplicationDataConnection.cs
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;

public class ApplicationDataConnection : DataConnection
{
    public ApplicationDataConnection(LinqToDbConnectionOptions<ApplicationDataConnection> options)
        : base(options)
    {
    }

    public ITable<Product> Products => this.GetTable<Product>();
}
```

### 4. Configurar no DI

```csharp
// src/Infrastructure/Extensions/DatabaseExtension.cs
private static IServiceCollection AddLinq2Db(
    this IServiceCollection services,
    string connectionString,
    DatabaseSettings settings)
{
    services.AddLinqToDbContext<ApplicationDataConnection>((provider, options) =>
    {
        options.UseSqlServer(connectionString);
    });

    return services;
}
```

### 5. Implementar Repositório

```csharp
public class ProductLinq2DbRepository : IRepository<Product>
{
    private readonly ApplicationDataConnection _db;

    public ProductLinq2DbRepository(ApplicationDataConnection db)
    {
        _db = db;
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Products.ToListAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        entity.Id = await _db.InsertWithInt64IdentityAsync(entity, token: cancellationToken);
        return entity;
    }

    // Implemente os demais métodos...
}
```

---

## 📊 Comparação de ORMs

| Característica | EF Core | Dapper | NHibernate | Linq2Db |
|---------------|---------|---------|------------|---------|
| **Performance** | Boa | Excelente | Boa | Excelente |
| **Facilidade** | Fácil | Moderado | Complexo | Moderado |
| **Maturidade** | Alta | Alta | Muito Alta | Alta |
| **Change Tracking** | Sim | Não | Sim | Opcional |
| **LINQ Support** | Completo | Limitado | Bom | Completo |
| **Migrations** | Sim | Não | Limitado | Limitado |

### Quando usar cada um?

- **Entity Framework Core**: Para a maioria dos projetos. Bom equilíbrio entre produtividade e performance.
- **Dapper**: Quando precisa de máxima performance em queries complexas ou bulk operations.
- **NHibernate**: Para projetos enterprise complexos que precisam de recursos avançados de ORM.
- **Linq2Db**: Quando precisa de performance próxima ao Dapper mas com full LINQ support.

---

## 🔀 Combinando ORMs

Você pode usar múltiplos ORMs no mesmo projeto:

```csharp
// Use EF Core para a maioria das operações
public class ProductService
{
    private readonly IRepository<Product> _repository; // EF Core
    private readonly ProductDapperRepository _dapperRepo; // Dapper para relatórios

    public async Task<IEnumerable<ProductReport>> GetComplexReportAsync()
    {
        // Use Dapper para queries complexas de leitura
        return await _dapperRepo.GetComplexReportAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        // Use EF Core para operações CRUD normais
        return await _repository.AddAsync(product);
    }
}
```

---

## 💡 Dicas

1. **Performance**: Se performance é crítica, considere Dapper ou Linq2Db
2. **Produtividade**: Para desenvolvimento rápido, use EF Core
3. **Complexidade**: Para domínios complexos, NHibernate pode ser útil
4. **Migrations**: Se precisa de migrations automáticas, use EF Core
5. **Híbrido**: Não tenha medo de combinar ORMs conforme a necessidade

---

## 🧪 Testes

Independente do ORM escolhido, mantenha seus testes focados nas interfaces:

```csharp
public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _mockRepo;
    
    [Fact]
    public async Task GetByIdAsync_ReturnsProduct()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(new Product { Id = 1, Name = "Test" });
        
        // Act & Assert
        // O teste não sabe qual ORM está sendo usado!
    }
}
```

---

Para mais informações sobre cada ORM, consulte suas documentações oficiais.
