# Guia de Troca de ORM

Este documento fornece instruções detalhadas sobre como trocar o ORM padrão (Entity Framework Core) por outras alternativas.

## 📋 Índice

- [ORMs Suportados](#-orms-suportados)
- [Como Funciona](#-como-funciona)
- [Entity Framework Core (Padrão)](#-entity-framework-core-padrão)
- [Dapper (Alta Performance)](#-dapper-alta-performance)
- [ADO.NET](#-adonet)
- [NHibernate](#-nhibernate)
- [Linq2Db](#-linq2db)
- [Resumo Rápido](#-resumo-rápido)
- [Comparação de ORMs](#-comparação-de-orms)
- [Combinando ORMs](#-combinando-orms)
- [Dicas](#-dicas)
- [Testes](#-testes)
- [Como Adicionar um Novo Repositório ORM](#-como-adicionar-um-novo-repositório-orm)

---

## 📋 ORMs Suportados

1. **Entity Framework Core** (Padrão - Habilitado)
2. **Dapper** (✅ Implementado - Registro Automático)
3. **ADO.NET** (✅ Implementado - Registro Automático)
4. **NHibernate** (Preparado - TODO)
5. **Linq2Db** (Preparado - TODO)

---

## 🎯 Como Funciona

**Entity Framework Core é o ORM padrão** e está habilitado por padrão no código.

### 🚀 **NOVO: Registro Automático com Scrutor**

Todos os repositórios são **registrados automaticamente** usando **Scrutor** com `.AsMatchingInterface()`!

**Não é necessário configuração manual!** Os repositórios de ORMs alternativos (Dapper, ADO.NET, NHibernate, Linq2Db) são detectados e registrados automaticamente quando você os implementa.

#### Como o registro automático funciona?

```csharp
// src/Infrastructure/Extensions/DependencyInjectionExtensions.cs
services.Scan(scan => scan
    .FromAssembliesOf(typeof(Repository<>))
    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
    .AsMatchingInterface()  // ← Mágica aqui!
    .WithScopedLifetime()
);
```

**O que `.AsMatchingInterface()` faz:**
- `Repository<Product>` → registrado como `IRepository<Product>`
- `ProductDapperRepository` → registrado como `IProductDapperRepository` ✅
- `ProductAdoRepository` → registrado como `IProductAdoRepository` ✅
- **NÃO** sobrescreve `IRepository<Product>` (evita conflitos!) ✅

### 🎨 Para usar múltiplos ORMs:

```csharp
public class ProductService
{
    private readonly IRepository<Product> _efRepository;          // EF Core (padrão)
    private readonly IProductDapperRepository _dapperRepository;  // Dapper (alta performance)
    private readonly IProductAdoRepository _adoRepository;        // ADO.NET (controle total)
    
    // Escolha o repositório adequado para cada operação!
}
```

**Não há configuração de ORM no appsettings.json!** Isso simplifica o uso e evita erros de configuração.

---

## 🔄 Entity Framework Core (Padrão)

### Status: ✅ **Habilitado por Padrão**

### Localização no Código

**Arquivo**: `src/Infrastructure/Extensions/DatabaseExtension.cs`  
**Linha**: ~26 (procure por "DEFAULT: Entity Framework Core")

```csharp
// DEFAULT: Entity Framework Core
services.AddEntityFramework(connectionString, dbSettings);
```

### Configuração no appsettings.json

Apenas configure o tipo de banco de dados:

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Database": {
        "DatabaseType": "SqlServer",
        "CommandTimeout": 30
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

### Status: 💤 **Comentado - Pronto para Uso**

### Como Habilitar

**Passo 1**: Abra o arquivo `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 2**: Comente a linha do Entity Framework (~linha 26):
```csharp
// DEFAULT: Entity Framework Core
// services.AddEntityFramework(connectionString, dbSettings);
```

**Passo 3**: Descomente a linha do Dapper (~linha 29):
```csharp
// ALTERNATIVE 1: Dapper (High Performance)
services.AddDapper(connectionString);
```

### Implementação de Repositório com Dapper

```csharp
using Dapper;
using System.Data;
// ✅ Use Microsoft.Data.SqlClient (moderno, mantido)
using Microsoft.Data.SqlClient;

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

**Passo 4**: Registre seus repositórios Dapper no método `AddDapper`:

Edite `src/Infrastructure/Extensions/DatabaseExtension.cs`:
```csharp
private static IServiceCollection AddDapper(
    this IServiceCollection services,
    string connectionString)
{
    services.AddSingleton(connectionString);
    
    // Registre seus repositórios Dapper aqui
    services.AddScoped<IRepository<Product>, ProductDapperRepository>();
    
    return services;
}
```

### ⚠️ Importante: Microsoft.Data.SqlClient

**Desde .NET 10**, o projeto utiliza **Microsoft.Data.SqlClient** (versão moderna e ativa) ao invés do obsoleto `System.Data.SqlClient`.

```csharp
// ✅ Use Microsoft.Data.SqlClient (moderno, mantido)
using Microsoft.Data.SqlClient;

// ❌ NÃO use System.Data.SqlClient (obsoleto)
// using System.Data.SqlClient;
```

**Compatibilidade**: O `Microsoft.Data.SqlClient` implementa `IDbConnection`, funcionando perfeitamente com **Dapper** e **ADO.NET**!

**Pacotes Necessários**: Dapper já está incluído no `src/Data/Data.csproj` ✅

---

## ⚡ ADO.NET

### Status: ✅ **Pronto para uso - Máxima Performance**

### Como Habilitar

**Passo 1**: Abra o arquivo `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 2**: Comente a linha do Entity Framework (~linha 67):
```csharp
// DEFAULT: Entity Framework Core
// services.AddEntityFramework(connectionString, dbSettings);
```

**Passo 3**: Descomente a linha do ADO.NET (~linha 81):
```csharp
// ALTERNATIVE 4: ADO.NET (Maximum Control & Performance)
services.AddAdo(connectionString);
```

**Passo 4**: Execute o projeto:
```bash
dotnet run --project src/Api
```

### Características

- **✅ Implementações completas** de `ProductAdoRepository` e `OrderAdoRepository`
- **✅ Performance máxima** - sem overhead de ORM
- **✅ Controle total** sobre SQL, parâmetros e transações
- **✅ IDbConnectionFactory** - gerenciamento adequado de conexões via DI
- **✅ SqlCommand e SqlDataReader** - APIs nativas do ADO.NET
- **✅ Transações explícitas** para operações complexas
- **✅ Mapping manual** de DataReader para objetos

### Exemplo de Implementação

```csharp
public class ProductAdoRepository : IRepository<Product>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductAdoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Products WHERE Id = @Id";
        
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new Product
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2)
                // ... mapping manual de todas as propriedades
            };
        }

        return null;
    }
}
```

### Quando Usar ADO.NET

- ✅ Queries com requisitos de **performance extrema**
- ✅ **ETL e processamento em lote** com milhares de registros
- ✅ Situações que exigem **controle total** sobre SQL e transações
- ✅ Stored procedures complexas
- ✅ **Cenários educacionais** para entender como ORMs funcionam por baixo dos panos

### ⚠️ Importante: Microsoft.Data.SqlClient

**Desde .NET 10**, o projeto utiliza **Microsoft.Data.SqlClient** (versão moderna e ativa) ao invés do obsoleto `System.Data.SqlClient`.

```csharp
// ✅ Use Microsoft.Data.SqlClient (moderno, mantido)
using Microsoft.Data.SqlClient;

// ❌ NÃO use System.Data.SqlClient (obsoleto)
// using System.Data.SqlClient;
```

**Pacotes Necessários**: `Microsoft.Data.SqlClient >= 6.1.1` já está incluído no projeto ✅

**Benefícios**:
- ✅ **Suporte ativo** da Microsoft
- ✅ **Novas features** do SQL Server
- ✅ **Melhor segurança** e correções de bugs
- ✅ **Compatível com .NET 10** e versões futuras

---

## 🔧 NHibernate

### Status: 💤 **Comentado - Preparado**

### Como Habilitar

**Passo 1**: Abra o arquivo `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 2**: Comente a linha do Entity Framework (~linha 26):
```csharp
// DEFAULT: Entity Framework Core
// services.AddEntityFramework(connectionString, dbSettings);
```

**Passo 3**: Descomente a linha do NHibernate (~linha 34):
```csharp
// ALTERNATIVE 2: NHibernate (Enterprise Features)
services.AddNHibernate(connectionString, dbSettings);
```

### Implementação Completa

**Passo 4**: Instale os pacotes NuGet necessários:

Adicione ao `src/Data/Data.csproj`:

```xml
<PackageReference Include="NHibernate" Version="5.5.2" />
<PackageReference Include="FluentNHibernate" Version="3.4.0" />
```

**Passo 5**: Configure o SessionFactory no método `AddNHibernate`:

Edite o método em `src/Infrastructure/Extensions/DatabaseExtension.cs`:

```csharp
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

**Passo 6**: Crie os mappings e repositórios conforme mostrado abaixo.

### Criar Mappings

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

### Implementar Repositório

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

### Status: 💤 **Comentado - Preparado**

### Como Habilitar

**Passo 1**: Abra o arquivo `src/Infrastructure/Extensions/DatabaseExtension.cs`

**Passo 2**: Comente a linha do Entity Framework (~linha 26):
```csharp
// DEFAULT: Entity Framework Core
// services.AddEntityFramework(connectionString, dbSettings);
```

**Passo 3**: Descomente a linha do Linq2Db (~linha 39):
```csharp
// ALTERNATIVE 3: Linq2Db (LINQ + Performance)
services.AddLinq2Db(connectionString, dbSettings);
```

### Implementação Completa

**Passo 4**: Instale os pacotes NuGet necessários:

Adicione ao `src/Data/Data.csproj`:

```xml
<PackageReference Include="linq2db" Version="5.4.1" />
<PackageReference Include="linq2db.EntityFrameworkCore" Version="8.1.0" />
```

**Passo 5**: Configure o DataConnection no método `AddLinq2Db`:

Edite o método em `src/Infrastructure/Extensions/DatabaseExtension.cs`:

```csharp
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

**Passo 6**: Crie o DataConnection e repositórios conforme mostrado abaixo.

### Criar DataConnection

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

### Implementar Repositório

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

## 📍 Resumo Rápido

### Para trocar de ORM:

1. **Abra**: `src/Infrastructure/Extensions/DatabaseExtension.cs`
2. **Comente**: A linha do EF Core (linha ~26)
3. **Descomente**: A linha do ORM desejado (linhas ~29, ~34 ou ~39)
4. **Implemente**: Os métodos necessários conforme documentação acima
5. **Pronto**: Não precisa alterar appsettings.json!

### Localização das Linhas:

| ORM | Linha Aproximada | Método |
|-----|------------------|---------|
| **Entity Framework** | ~26 | `AddEntityFramework()` |
| **Dapper** | ~29 | `AddDapper()` |
| **NHibernate** | ~34 | `AddNHibernate()` |
| **Linq2Db** | ~39 | `AddLinq2Db()` |

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

## 🔧 Como Adicionar um Novo Repositório ORM

Graças ao **Scrutor com `.AsMatchingInterface()`**, adicionar um novo repositório é **extremamente simples**:

### Passo 1: Criar a Interface Específica

```csharp
// src/Domain/Interfaces/IProductDapperRepository.cs
public interface IProductDapperRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsWithHighPerformanceAsync();
}
```

### Passo 2: Implementar o Repositório

```csharp
// src/Data/Repository/Dapper/ProductDapperRepository.cs
public class ProductDapperRepository : IProductDapperRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public ProductDapperRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Product?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id", 
            new { Id = id }
        );
    }
    
    // Implemente os demais métodos...
}
```

### Passo 3: **Pronto! Não precisa fazer mais nada!** 🎉

O Scrutor detectará automaticamente sua classe e registrará:
- ✅ `ProductDapperRepository` → `IProductDapperRepository`
- ❌ **NÃO** registrará como `IRepository<Product>` (sem conflito!)

### Injetando em um Serviço

```csharp
public class ProductService
{
    private readonly IRepository<Product> _repository;              // EF Core
    private readonly IProductDapperRepository _dapperRepository;    // Dapper
    
    public ProductService(
        IRepository<Product> repository,
        IProductDapperRepository dapperRepository)
    {
        _repository = repository;
        _dapperRepository = dapperRepository;
    }
    
    public async Task<IEnumerable<Product>> GetProductsForReportAsync()
    {
        // Use Dapper para queries de leitura complexas (melhor performance)
        return await _dapperRepository.GetProductsWithHighPerformanceAsync();
    }
    
    public async Task<Product> CreateAsync(Product product)
    {
        // Use EF Core para operações CRUD (change tracking, validação)
        return await _repository.AddAsync(product);
    }
}
```

### 📝 Convenções de Nomenclatura

Para o registro automático funcionar corretamente:

1. **Interface** deve ter o prefixo `I` e nome da classe:
   - Classe: `ProductDapperRepository`
   - Interface: `IProductDapperRepository` ✅

2. **Interface** deve herdar de `IRepository<T>`:
   ```csharp
   public interface IProductDapperRepository : IRepository<Product> { }
   ```

3. **Classe** deve estar no namespace `*.Repository.*`:
   ```csharp
   namespace ProjectTemplate.Data.Repository.Dapper { }
   ```

---

Para mais informações sobre cada ORM, consulte suas documentações oficiais.
