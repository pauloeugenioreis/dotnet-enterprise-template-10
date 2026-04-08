# Guia de Troca de ORM

Este documento fornece instruções detalhadas sobre como trocar o ORM padrão (Entity Framework Core) por outras alternativas.

## 📋 Índice

- [ORMs Suportados](#orms-suportados)
- [Como Funciona](#como-funciona)
- [Entity Framework Core (Padrão)](#entity-framework-core-padrao)
- [Dapper (Alta Performance)](#dapper-alta-performance)
- [ADO.NET](#ado-net)
- [NHibernate](#nhibernate)
- [Linq2Db](#linq2db)
- [Resumo Rápido](#resumo-rapido)
- [Comparação de ORMs](#comparacao-de-orms)
- [Combinando ORMs](#combinando-orms)
- [Dicas](#dicas)
- [Testes](#testes)
- [Como Adicionar um Novo Repositório ORM](#como-adicionar-um-novo-repositorio-orm)

---

<a id="orms-suportados"></a>

## 📋 ORMs Suportados

1. **Entity Framework Core** (✅ Habilitado por Padrão - InMemory/SQL Server)
2. **Dapper** (✅ Habilitado por Padrão - Alta Performance)
3. **ADO.NET** (✅ Habilitado por Padrão - Controle Total)
4. **NHibernate** (✅ Implementado - Pacotes Comentados)
5. **Linq2Db** (✅ Implementado - Pacotes Comentados)

> **🎉 NOVIDADE:** Entity Framework Core, Dapper e ADO.NET estão **todos habilitados simultaneamente**!
> Você pode usar os três no mesmo serviço, escolhendo o melhor ORM para cada operação.

---

<a id="como-funciona"></a>

## 🎯 Como Funciona

### 🚀 **MÚLTIPLOS ORMs HABILITADOS SIMULTANEAMENTE!**

**Entity Framework Core, Dapper e ADO.NET** estão **todos habilitados por padrão** no projeto!

Cada ORM usa sua **interface específica** para evitar conflitos:

- **`IRepository<Product>`** → Entity Framework Core (padrão, InMemory para testes)
- **`IProductDapperRepository`** → Dapper (alta performance, SQL Server)
- **`IProductAdoRepository`** → ADO.NET (controle total, SQL Server)

### 🎨 Como Usar Múltiplos ORMs

```csharp
public class ProductService
{
    private readonly IRepository<Product> _repository;            // EF Core (padrão)
    private readonly IProductDapperRepository _dapperRepository;  // Dapper (alta performance)
    private readonly IProductAdoRepository _adoRepository;        // ADO.NET (controle total)

    public ProductService(
        IRepository<Product> repository,
        IProductDapperRepository dapperRepository,
        IProductAdoRepository adoRepository)
    {
        _repository = repository;
        _dapperRepository = dapperRepository;
        _adoRepository = adoRepository;
    }

    // Use EF Core para CRUD normal com change tracking
    public async Task<Product> CreateAsync(Product product)
    {
        return await _repository.AddAsync(product);
    }

    // Use Dapper para queries de leitura complexas (melhor performance)
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dapperRepository.GetAllAsync();
    }

    // Use ADO.NET para operações em lote ou controle total
    public async Task BulkUpdatePricesAsync(Dictionary<long, decimal> prices)
    {
        return await _adoRepository.BulkUpdatePricesAsync(prices);
    }
}
```

### 📝 Escolha o ORM Certo para Cada Situação

| Operação                      | ORM Recomendado      | Motivo                                |
| ----------------------------- | -------------------- | ------------------------------------- |
| CRUD simples                  | **EF Core**          | Change tracking, validação automática |
| Relatórios complexos          | **Dapper**           | Alta performance, queries otimizadas  |
| Bulk operations               | **ADO.NET**          | Controle total, máxima performance    |
| Testes unitários              | **EF Core InMemory** | Rápido, sem banco real                |
| Projetos enterprise complexos | **NHibernate**       | Maturidade, features avançadas        |
| Performance com LINQ completo | **Linq2Db**          | LINQ support, alta performance        |

### 🚀 **Registro Automático com Scrutor**

Todos os repositórios são **registrados automaticamente** usando **Scrutor** com `.AsMatchingInterface()`!

**Não é necessário configuração manual!** Os repositórios são detectados e registrados automaticamente.

#### Como o registro automático funciona?

// src/Infrastructure/Extensions/DependencyInjectionExtensions.cs
services.Scan(scan => scan
.FromAssembliesOf(typeof(Repository<>))
.AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
.AsMatchingInterface() // ← Mágica aqui!
.WithScopedLifetime()
);
**O que `.AsMatchingInterface()` faz:**

- `Repository<Product>` → registrado como `IRepository<Product>` ✅
- `ProductDapperRepository` → registrado como `IProductDapperRepository` ✅
- `ProductAdoRepository` → registrado como `IProductAdoRepository` ✅
- **NÃO sobrescreve** `IRepository<Product>` (evita conflitos!) ✅

**Não há configuração de ORM no appsettings.json!** Isso simplifica o uso e evita erros de configuração.

## 📁 Estrutura dos Repositórios

Todo o código de referência de cada ORM vive em `src/Data/Repository` e está pronto para ser habilitado quando necessário:

```text
src/Data/Repository/
├── Repository.cs                 # EF Core (genérico)
├── OrderRepository.cs            # EF Core específico
├── Dapper/
│   ├── ProductDapperRepository.cs
│   └── OrderDapperRepository.cs
├── NHibernate/
│   ├── ProductNHibernateRepository.cs
│   └── OrderNHibernateRepository.cs
├── Linq2Db/
│   ├── ProductLinq2DbRepository.cs
│   └── OrderLinq2DbRepository.cs
└── Ado/
    ├── ProductAdoRepository.cs
    └── OrderAdoRepository.cs
```

Use essa estrutura como catálogo: basta habilitar o ORM desejado e as implementações ficam imediatamente disponíveis para injeção de dependências.

## ⚡ Comparação Rápida por ORM

| ORM | Performance | Controle | Verbosidade | Curva de Aprendizado | Uso Ideal |
| --- | ----------- | -------- | ----------- | -------------------- | --------- |
| **Entity Framework** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ | Fácil | CRUD geral, RAD |
| **Dapper** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | Fácil | Queries complexas |
| **ADO.NET** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐ | Médio | Performance crítica |
| **NHibernate** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | Difícil | Aplicações enterprise |
| **Linq2Db** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Médio | LINQ com alta performance |

Essa tabela resume o trade-off de cada opção antes de mergulhar nas seções detalhadas abaixo.

---

<a id="entity-framework-core-padrao"></a>

## 🔄 Entity Framework Core (Padrão)

### Status: ✅ **Habilitado por Padrão**

Entity Framework Core está sempre habilitado como **ORM primário** do projeto.

- **`IRepository<Product>`** sempre resolve para **EF Core**
- **InMemory** para testes (rápido, sem banco real)
- **SQL Server** para produção (configurável via `DatabaseType`)

### Quando Usar

- ✅ **CRUD** operações normais
- ✅ **Change tracking** automático
- ✅ **Validações** com Data Annotations
- ✅ **Migrations** automáticas
- ✅ **Relacionamentos** complexos entre entidades
- ✅ **Testes unitários** com InMemory

### Localização no Código

**Arquivo**: `src/Infrastructure/Extensions/DatabaseExtension.cs`
**Linha**: ~73 (procure por "PRIMARY: Entity Framework Core")

// PRIMARY: Entity Framework Core (Change Tracking, Migrations, CRUD)
services.AddEntityFramework(connectionString, dbSettings);

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

<a id="dapper-alta-performance"></a>

## ⚡ Dapper (Alta Performance)

### Status: ✅ **Habilitado por Padrão**

Dapper está habilitado simultaneamente com EF Core! Use **`IProductDapperRepository`** para acessar.

### Quando Usar

- ✅ **Queries complexas** de leitura
- ✅ **Relatórios** com muitos dados
- ✅ **Performance crítica** em consultas
- ✅ **Queries otimizadas** manualmente
- ✅ **Stored procedures**

### Como Usar

```csharp
public class ProductService
{
    private readonly IProductDapperRepository _dapperRepo;

    public ProductService(IProductDapperRepository dapperRepo)
    {
        _dapperRepo = dapperRepo;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        // Dapper: alta performance para leitura
        return await _dapperRepo.GetAllAsync();
    }
}
```

### Localização no Código

**Arquivo**: `src/Infrastructure/Extensions/DatabaseExtension.cs`
**Linha**: ~76 (procure por "ENABLED: Dapper")

// ENABLED: Dapper (High Performance Queries, Complex Reports)
services.AddDapper(connectionString);

### Interfaces Disponíveis

- **`IProductDapperRepository`** - Repositório de produtos com Dapper
- **`IOrderDapperRepository`** - Repositório de pedidos com Dapper

### Implementações

- ✅ `src/Data/Repository/Dapper/ProductDapperRepository.cs`
- ✅ `src/Data/Repository/Dapper/OrderDapperRepository.cs`

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

// ✅ Use Microsoft.Data.SqlClient (moderno, mantido)
using Microsoft.Data.SqlClient;

// ❌ NÃO use System.Data.SqlClient (obsoleto)
// using System.Data.SqlClient;
**Compatibilidade**: O `Microsoft.Data.SqlClient` implementa `IDbConnection`, funcionando perfeitamente com **Dapper** e **ADO.NET**!

**Pacotes Necessários**: Dapper já está incluído no `src/Data/Data.csproj` ✅

---

<a id="ado-net"></a>

## ⚡ ADO.NET (Controle Total)

### Status: ✅ **Habilitado por Padrão**

ADO.NET está habilitado simultaneamente com EF Core e Dapper! Use **`IProductAdoRepository`** para acessar.

### Quando Usar

- ✅ **Controle total** sobre comandos SQL
- ✅ **Performance crítica** com otimizações manuais
- ✅ **Bulk operations** personalizadas
- ✅ **Queries extremamente complexas**
- ✅ **Integração com sistemas legados**

### Como Usar

```csharp
public class ProductService
{
    private readonly IProductAdoRepository _adoRepo;

    public ProductService(IProductAdoRepository adoRepo)
    {
        _adoRepo = adoRepo;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        // ADO.NET: controle total sobre execução
        return await _adoRepo.GetAllAsync();
    }
}
```

### Localização no Código

**Arquivo**: `src/Infrastructure/Extensions/DatabaseExtension.cs`
**Linha**: ~79 (procure por "ENABLED: ADO.NET")

// ENABLED: ADO.NET (Maximum Control, Legacy Integration)
services.AddAdo(connectionString);

### Interfaces Disponíveis

- **`IProductAdoRepository`** - Repositório de produtos com ADO.NET
- **`IOrderAdoRepository`** - Repositório de pedidos com ADO.NET

### Implementações

- ✅ `src/Data/Repository/Ado/ProductAdoRepository.cs`
- ✅ `src/Data/Repository/Ado/OrderAdoRepository.cs`

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

// ✅ Use Microsoft.Data.SqlClient (moderno, mantido)
using Microsoft.Data.SqlClient;

// ❌ NÃO use System.Data.SqlClient (obsoleto)
// using System.Data.SqlClient;
**Pacotes Necessários**: `Microsoft.Data.SqlClient >= 6.1.1` já está incluído no projeto ✅

**Benefícios**:

- ✅ **Suporte ativo** da Microsoft
- ✅ **Novas features** do SQL Server
- ✅ **Melhor segurança** e correções de bugs
- ✅ **Compatível com .NET 10** e versões futuras

---

<a id="nhibernate"></a>

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

// src/Data/Mappings/ProductMap.cs

```csharp
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

<a id="linq2db"></a>

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

// src/Data/Context/ApplicationDataConnection.cs

```csharp
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

<a id="resumo-rapido"></a>

## 📍 Resumo Rápido

### ✅ O que está habilitado por padrão?

1. **Entity Framework Core** - CRUD normal, InMemory em testes
2. **Dapper** - Queries de alta performance
3. **ADO.NET** - Controle total sobre SQL

### 🎯 Como usar cada ORM?

// Entity Framework Core (padrão)
private readonly IRepository<Product> \_repository;

// Dapper (alta performance)
private readonly IProductDapperRepository \_dapperRepo;

// ADO.NET (controle total)
private readonly IProductAdoRepository \_adoRepo;

### 📂 Onde estão configurados?

**Arquivo**: `src/Infrastructure/Extensions/DatabaseExtension.cs`

// Linha ~73: Entity Framework Core
services.AddEntityFramework(connectionString, dbSettings);

// Linha ~76: Dapper
services.AddDapper(connectionString);

// Linha ~79: ADO.NET
services.AddAdo(connectionString);

// Linhas ~82-89: NHibernate e Linq2Db (opcional, comentados)

### Vantagens de Usar Múltiplos ORMs

- ✅ **Não precisa escolher um único ORM** - use os 3 simultaneamente
- ✅ **Testes rápidos** - EF Core InMemory (sem SQL Server)
- ✅ **Performance quando precisar** - Dapper e ADO.NET disponíveis
- ✅ **Sem conflitos** - cada ORM usa interfaces específicas
- ✅ **Fácil de manter** - Scrutor registra tudo automaticamente

### 🚀 Adicionar ORMs Opcionais (NHibernate, Linq2Db)

Se quiser habilitar NHibernate ou Linq2Db:

1. **Abra**: `src/Infrastructure/Extensions/DatabaseExtension.cs`
2. **Localize**: Linhas ~82-89 (métodos comentados)
3. **Descomente**: A linha do ORM desejado
4. **Implemente**: Repositórios e configurações necessárias (veja seções abaixo)

---

<a id="comparacao-de-orms"></a>

## 📊 Comparação de ORMs

| ORM            | Status        | Performance | Facilidade | Maturidade | Change Tracking | LINQ Support | Migrations | Vantagens                              |
| -------------- | ------------- | ----------- | ---------- | ---------- | --------------- | ------------ | ---------- | -------------------------------------- |
| **EF Core**    | ✅ Habilitado | Boa         | Fácil      | Alta       | Sim             | Completo     | Sim        | Change tracking, migrations            |
| **Dapper**     | ✅ Habilitado | Excelente   | Moderado   | Alta       | Não             | Limitado     | Não        | Alta performance, queries customizadas |
| **ADO.NET**    | ✅ Habilitado | Excelente   | Complexo   | Muito Alta | Não             | Não          | Não        | Controle total, máxima performance     |
| **NHibernate** | 💤 Opcional   | Boa         | Complexo   | Muito Alta | Sim             | Bom          | Limitado   | Maturidade, features enterprise        |
| **Linq2Db**    | 💤 Opcional   | Excelente   | Moderado   | Alta       | Opcional        | Completo     | Limitado   | LINQ completo, performance             |

### Quando usar cada um?

| ORM                       | Use quando...                                                                        |
| ------------------------- | ------------------------------------------------------------------------------------ |
| **Entity Framework Core** | Operações CRUD normais, change tracking necessário, migrations automáticas           |
| **Dapper**                | Queries complexas de leitura, relatórios, performance crítica em leitura             |
| **ADO.NET**               | Controle total sobre SQL, bulk operations personalizadas, integração legada          |
| **NHibernate**            | Projetos enterprise complexos (requer implementação adicional)                       |
| **Linq2Db**               | Performance próxima ao Dapper com full LINQ support (requer implementação adicional) |

### Métricas de Performance Práticas

| ORM | Read Speed | Write Speed | Suporte a LINQ | Complexidade |
| --- | ---------- | ----------- | -------------- | ------------ |
| **Dapper** | ⚡⚡⚡⚡⚡ | ⚡⚡⚡⚡⚡ | ❌ | Média |
| **Linq2Db** | ⚡⚡⚡⚡⚡ | ⚡⚡⚡⚡ | ✅ | Média |
| **EF Core** | ⚡⚡⚡ | ⚡⚡⚡ | ✅ | Baixa |
| **NHibernate** | ⚡⚡⚡ | ⚡⚡⚡ | ✅ | Alta |
| **ADO.NET** | ⚡⚡⚡⚡⚡ | ⚡⚡⚡⚡⚡ | Manual | Alta |

Use essa visão para decidir rapidamente qual ORM priorizar quando o foco é velocidade de leitura, escrita ou simplicidade.

---

<a id="combinando-orms"></a>

## 🔀 Combinando ORMs (Padrão no Template!)

> **✨ NOVIDADE**: Múltiplos ORMs já estão habilitados simultaneamente! Você não precisa escolher apenas um.

### Status Atual

Por padrão, o template já está configurado com:

- ✅ **Entity Framework Core** (operações normais, InMemory para testes)
- ✅ **Dapper** (queries de alta performance)
- ✅ **ADO.NET** (controle total)

### Como Usar Múltiplos ORMs

```csharp
public class ProductService
{
    // Injete múltiplos repositórios simultaneamente!
    private readonly IRepository<Product> _repository; // EF Core (padrão)
    private readonly IProductDapperRepository _dapperRepository; // Dapper
    private readonly IProductAdoRepository _adoRepository; // ADO.NET

    public ProductService(
        IRepository<Product> repository,
        IProductDapperRepository dapperRepository,
        IProductAdoRepository adoRepository)
    {
        _repository = repository;
        _dapperRepository = dapperRepository;
        _adoRepository = adoRepository;
    }

    // Use EF Core para operações normais de CRUD
    public async Task<Product> CreateAsync(Product product)
    {
        return await _repository.AddAsync(product);
    }

    // Use Dapper para relatórios e queries complexas
    public async Task<IEnumerable<Product>> GetProductReportAsync()
    {
        return await _dapperRepository.GetAllAsync();
    }

    // Use ADO.NET para operações que precisam de controle total
    public async Task<Product?> GetProductWithMaxControlAsync(long id)
    {
        return await _adoRepository.GetByIdAsync(id);
    }
}
```

### Vantagens de Usar Múltiplos ORMs

| Cenário              | ORM Recomendado   | Por quê?                         |
| -------------------- | ----------------- | -------------------------------- |
| CRUD simples         | EF Core           | Change tracking automático       |
| Leitura em massa     | Dapper            | Menos overhead, mais performance |
| Relatórios complexos | Dapper            | SQL otimizado manualmente        |
| Bulk operations      | ADO.NET           | Controle total sobre comandos    |
| Migrations           | EF Core           | Ferramentas integradas           |
| Stored procedures    | ADO.NET ou Dapper | Controle sobre parâmetros        |

### Exemplo Real de Uso Híbrido

```csharp
public class OrderService
{
    private readonly IRepository<Order> _repository; // EF Core
    private readonly IOrderDapperRepository _dapperRepo; // Dapper

    public OrderService(
        IRepository<Order> repository,
        IOrderDapperRepository dapperRepo)
    {
        _repository = repository;
        _dapperRepo = dapperRepo;
    }

    // Use EF Core para criar pedidos (usa change tracking)
    public async Task<Order> CreateOrderAsync(Order order)
    {
        return await _repository.AddAsync(order);
    }

    // Use Dapper para relatórios de vendas (queries otimizadas)
    public async Task<IEnumerable<Order>> GetSalesReportAsync(DateTime start, DateTime end)
    {
        return await _dapperRepo.GetOrdersByDateRangeAsync(start, end);
    }
}
```

### 🔑 Interfaces Específicas Previnem Conflitos

Cada ORM usa interfaces específicas para evitar conflitos de injeção de dependências:

```csharp
// Entity Framework Core
IRepository<Product> repository;  // Usa InMemory em testes, SQL Server em produção

// Dapper
IProductDapperRepository dapperRepo;  // Sempre usa SQL Server

// ADO.NET
IProductAdoRepository adoRepo;  // Sempre usa SQL Server
```

> **💡 IMPORTANTE**: Se você tentar injetar `IRepository<Product>`, sempre receberá a implementação do EF Core (InMemory em testes). Para usar Dapper ou ADO.NET, injete a interface específica!

---

<a id="dicas"></a>

## 💡 Dicas

1. **Performance**: Use Dapper ou ADO.NET para queries de alta performance
2. **Produtividade**: Use EF Core para desenvolvimento rápido com change tracking
3. **Híbrido**: Combine os 3 ORMs habilitados para extrair o melhor de cada um
4. **Migrations**: Use EF Core para migrations automáticas (já configurado)
5. **Testes**: Mantenha os testes usando interfaces - eles funcionam com qualquer ORM

### 🎯 Guia Rápido de Decisão

CRUD simples, tracking necessário?
→ Use IRepository<T> (EF Core)

Query complexa, relatório, performance?
→ Use IProductDapperRepository (Dapper)

Controle total, bulk operations, stored procedures?
→ Use IProductAdoRepository (ADO.NET)

Precisa de múltiplos ao mesmo tempo?
→ Injete todas as interfaces que precisar!

### 🔁 Alternando rapidamente entre ORMs

1. Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`.
2. Comente a chamada atual (`services.AddEntityFramework(...)`).
3. Descomente o método correspondente ao ORM desejado (`AddDapper`, `AddNHibernate`, `AddLinq2Db`, `AddAdo`).
4. Salve, execute `dotnet build`/`dotnet run --project src/Api` e valide seus endpoints.

Como todas as implementações seguem a interface `IRepository<T>` ou versões específicas (`IProductDapperRepository`, `IOrderAdoRepository`, etc.), **não é preciso alterar controllers ou services** para trocar de ORM: a injeção de dependência redireciona automaticamente para a implementação ativa.

---

<a id="testes"></a>

## 🧪 Testes

### Executar a API com o ORM desejado

Depois de habilitar um ORM alternativo, rode a API normalmente e valide os mesmos endpoints usados no dia a dia:

```bash
dotnet run --project src/Api
```

### Swagger UI

```text
http://localhost:5000/swagger
```

### Testar Products via REST

```bash
# GET - Listar produtos
curl http://localhost:5000/api/v1/product

# GET - Buscar produto por ID
curl http://localhost:5000/api/v1/product/1

# POST - Criar produto
curl -X POST http://localhost:5000/api/v1/product \
    -H "Content-Type: application/json" \
    -d '{"name":"Test Product","price":99.99,"stock":10,"category":"Test"}'
```

### Testar Orders via REST

```bash
# GET - Listar pedidos
curl http://localhost:5000/api/v1/order

# GET - Buscar pedido por ID (com itens)
curl http://localhost:5000/api/v1/order/1
```

### Testes Automatizados

Os testes usam **EF Core InMemory** por padrão, independentemente de quantos ORMs estão habilitados:

```csharp
public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _mockRepo;  // EF Core (InMemory)

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(new Product { Id = 1, Name = "Test" });

        // Act & Assert
        // Funciona com qualquer implementação!
    }
}
```

### Por que os testes funcionam?

- ✅ `IRepository<Product>` sempre resolve para EF Core InMemory em testes
- ✅ Dapper e ADO.NET só são usados quando você injeta as interfaces específicas
- ✅ Nenhuma conexão com SQL Server é necessária para rodar os testes
- ✅ Todos os 33 testes passam (7 unit + 26 integration)

---

## 🆘 Troubleshooting

### "Type not found" ao usar NHibernate

Instale os pacotes `NHibernate` e `FluentNHibernate` no `src/Data/Data.csproj` e garanta que o método `AddNHibernate` esteja descomentado em `DatabaseExtension`.

### "Table doesn't exist" ao usar Dapper ou Linq2Db

Execute as migrations do EF Core para criar as tabelas antes de rodar os repositórios alternativos:

```bash
dotnet ef database update --project src/Data --startup-project src/Api
```

### Erro de compilação com Linq2Db

Confirme que os pacotes `linq2db`, `linq2db.EntityFrameworkCore` e `linq2db.AspNet` estão referenciados e que as exclusões de `ItemGroup` foram comentadas.

### Oracle demora para subir

Esse container pode levar até 2 minutos na primeira execução; aguarde os logs mostrarem `DATABASE IS READY TO USE!` antes de testar.

---

## 📚 Recursos Adicionais

- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [NHibernate Documentation](https://nhibernate.info/)
- [Linq2Db Documentation](https://linq2db.github.io/)
- [ADO.NET Documentation](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/)

---

<a id="como-adicionar-um-novo-repositorio-orm"></a>

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
