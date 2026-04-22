# 🏗️ Arquitetura do Template

Visão geral da arquitetura Clean Architecture implementada neste template.

---

## 📐 Diagrama de Camadas

```text
┌─────────────────────────────── API LAYER ──────────────────────────────┐
│ • Controllers, Program.cs, Middleware, Swagger                         │
│ • Endpoints: GET / POST / PUT / DELETE                                 │
│ • Authentication & Authorization (JWT, OAuth2)                         │
└────────────────────────────────────────────────────────────────────────┘
                          │ HTTP Requests │
                          ▼
┌──────────────────────── INFRASTRUCTURE LAYER ───────────────────────────┐
│ • Extensions, Middleware, Filters, Notifications                        │
│ • Health Checks, CORS                                                     │
│ • Compression, Rate Limiting, OpenTelemetry                             │
└─────────────────────────────────────────────────────────────────────────┘
                          │ Cross-cutting │
                          ▼
┌────────────────────────── APPLICATION LAYER ────────────────────────────┐
│ • Services (Business Logic), Orchestration, Workflows                   │
│ • DTOs & AutoMapper Profiles                                            │
│ • Validators (FluentValidation)                                         │
└─────────────────────────────────────────────────────────────────────────┘
                          │ Business Ops │
                          ▼
┌────────────────────────────── DATA LAYER ───────────────────────────────┐
│ • Repositories (IRepository<T>)                                         │
│ • DbContext (EF Core) / ADO (Dapper / SQL)                              │
│ • EF Core Configurations (Mappings)                                     │
└─────────────────────────────────────────────────────────────────────────┘
                          │ Data Access │
                          ▼
┌────────────────────────────── DOMAIN LAYER ─────────────────────────────┐
│ • Entities, Enums, Value Objects                                        │
│ • Interfaces (IRepository, IService)                                    │
│ • Domain Exceptions, Validators, AppSettings                            │
└─────────────────────────────────────────────────────────────────────────┘
                          │ Core Business │
                          ▼
┌────────────────────────────── DATABASE ─────────────────────────────────┐
│ SQL Server │ PostgreSQL │ MySQL │ Oracle                                │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxo de Requisição

```text
┌────────────┐
│   Client   │
└─────┬──────┘
      │ HTTP Request (GET /api/products)
      ▼
┌───────────────────────┐
│  API Controller       │ ◄── ApiControllerBase
│  ProductController    │     - Validation
└──────┬────────────────┘     - Error handling
       │                       - Response formatting
       │ Call service
       ▼
┌───────────────────────┐
│ Application Service   │ ◄── Service<T>
│  ProductService       │     - Business logic
└──────┬────────────────┘     - Logging
       │                       - Error handling
       │ Call repository
       ▼
┌───────────────────────┐
│ Data Repository       │ ◄── Repository<T>
│  Repository<Product>  │     - CRUD operations
└──────┬────────────────┘     - Queries
       │                       - Async operations
       │ Query database
       ▼
┌───────────────────────┐
│  Database Context     │ ◄── ApplicationDbContext
│  ApplicationDbContext │     - EF Core DbContext
└──────┬────────────────┘     - Mappings
       │                       - Connection
       │ Execute SQL
       ▼
┌───────────────────────┐
│  Database             │
│  SQL Server / Others  │
└───────────────────────┘
```

---

## 🎯 Princípios de Arquitetura

### 1. Separation of Concerns (SoC)

Cada camada tem uma responsabilidade específica e bem definida.

### 2. Dependency Inversion (DIP)

Camadas superiores não dependem diretamente de camadas inferiores.
Todas dependem de abstrações (interfaces).

### 3. Single Responsibility (SRP)

Cada classe/módulo tem uma única razão para mudar.

### 4. Open/Closed (OCP)

Aberto para extensão, fechado para modificação.

### 5. Interface Segregation (ISP)

Interfaces específicas em vez de uma interface geral.

---

## 📦 Dependências Entre Camadas

```text
Api
 ├── → Infrastructure (DI, Extensions)
 ├── → Application (Services)
 ├── → Data (Context, Repositories)
 └── → Domain (Entities, Interfaces)

Infrastructure
 ├── → Application
 ├── → Data
 └── → Domain

Application
 ├── → Data
 └── → Domain

Data
 └── → Domain

Domain
 └── (No dependencies - Pure business logic)
```

---

## 🏛️ Detalhamento das Camadas

### 1️⃣ Domain Layer (Camada de Domínio)

**Responsabilidade:** Regras de negócio e entidades core

**Contém:**

- `Entities/` - Objetos de negócio (Product, Order, etc.)
- `Interfaces/` - Contratos (IRepository, IService)
- `Enums/` - Enumerações de domínio
- `Exceptions/` - Exceções de domínio
- `Validators/` - Regras de validação
- `AppSettings.cs` - Modelo de configuração

**Dependências:** Nenhuma ❌

**Exemplo:**

```csharp
// Domain/Entities/Product.cs
public class Product : EntityBase
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Domain/Interfaces/IRepository.cs
public interface IRepository<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(long id, CancellationToken ct);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
}
```

---

### 2️⃣ Data Layer (Camada de Dados)

**Responsabilidade:** Acesso a dados e persistência

**Contém:**

- `Context/` - DbContext do EF Core
- `Repository/` - Implementação de repositórios
- `Mappings/` - Configurações de EF Core
- `ADO/` - Queries com Dapper

**Dependências:** Domain ✅

**Exemplo:**

```csharp
// Data/Repository/Repository.cs
public class Repository<T> : IRepository<T> where T : EntityBase
{
    private readonly ApplicationDbContext _context;

    public async Task<T?> GetByIdAsync(long id, CancellationToken ct)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, ct);
    }
}
```

---

### 3️⃣ Application Layer (Camada de Aplicação)

**Responsabilidade:** Lógica de negócio e orquestração

**Contém:**

- `Services/` - Services de aplicação
- `Mappings/` - AutoMapper profiles
- `Builders/` - Object builders
- `Helpers/` - Funções auxiliares

**Dependências:** Domain, Data ✅

**Exemplo:**

```csharp
// Application/Services/ProductService.cs
public class ProductService : Service<Product>
{
    public ProductService(IRepository<Product> repo, ILogger<ProductService> logger)
        : base(repo, logger)
    {
    }

    // Custom business logic here
}
```

---

### 4️⃣ Infrastructure Layer (Camada de Infraestrutura)

**Responsabilidade:** Cross-cutting concerns e integrações

**Contém:**

- `Extensions/` - Extension methods (DI, DB)
- `Middleware/` - Middlewares customizados
- `Filters/` - Action filters
- `Notifications/` - Sistema de notificações
- `Services/` - Serviços externos (Email, SMS, etc.)

**Dependências:** Todas as camadas ✅

**Exemplo:**

```csharp
// Infrastructure/Extensions/DatabaseExtension.cs
public static IServiceCollection AddDatabase(this IServiceCollection services)
{
    var provider = appSettings.Infrastructure.Database.Provider;
    switch (provider)
    {
        case "EntityFrameworkCore":
            services.AddDbContext<ApplicationDbContext>();
            break;
        case "Dapper":
            services.AddSingleton<IDbConnection>(...);
            break;
    }
}
```

---

### 5️⃣ API Layer (Camada de Apresentação)

**Responsabilidade:** Endpoints HTTP e configuração da API

**Contém:**

- `Controllers/` - API controllers
- `Program.cs` - Configuração da aplicação
- `appsettings.json` - Configurações por ambiente

**Dependências:** Todas as camadas ✅

**Exemplo:**

```csharp
// Api/Controllers/ProductController.cs
[Route("api/[controller]")]
public class ProductController : ApiControllerBase
{
    private readonly IService<Product> _service;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var product = await _service.GetByIdAsync(id, ct);
        return HandleResult(product);
    }
}
```

---

## 🔌 Padrões Implementados

### 1. Repository Pattern

- Interface genérica `IRepository<T>`
- Implementação genérica `Repository<T>`
- Abstração do acesso a dados

### 2. Service Pattern

- Classe base `Service<T>`
- Lógica de negócio centralizada
- Logging e error handling

### 3. Dependency Injection

- Constructor injection
- **Scrutor para registro automático inteligente**
  - `.AsMatchingInterface()` - Registra apenas interface correspondente ao nome
  - `.AsImplementedInterfaces()` - Registra todas as interfaces implementadas
  - **Zero configuração manual** para novos repositórios/serviços
- Lifetime management (Scoped, Singleton, Transient)

#### Como funciona o registro automático

```csharp
// Registra TODOS os repositórios automaticamente
services.Scan(scan => scan
    .FromAssembliesOf(typeof(Repository<>))
    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
    .AsMatchingInterface()  // ← Evita conflitos de DI!
    .WithScopedLifetime()
);
```

**Exemplo de mapeamento automático:**

| Classe | Interface Registrada |
| -------- | --------------------- |
| `Repository<Product>` | `IRepository<Product>` |
| `ProductDapperRepository` | `IProductDapperRepository` |
| `ProductAdoRepository` | `IProductAdoRepository` |
| `OrderService` | `IOrderService` |

**Benefícios:**

- ✅ Adicione um novo repositório → Registrado automaticamente
- ✅ Múltiplos ORMs sem conflito
- ✅ Testes isolados (usam `IRepository<T>` com InMemory)
- ✅ Produção escolhe o ORM específico via injeção

### 4. Options Pattern

- `AppSettings.cs` fortemente tipado
- `IOptions<T>` injection
- Validação em startup

### 5. Factory Pattern

- Database provider factory

- Extensible para novos providers

---

## 🎛️ Configuração Modular

```text
Program.cs
    │
    ├── AddAppSettings()          // Configurações validadas
    │
    ├── AddDatabase()             // Multi-ORM support
    │   ├── EntityFrameworkCore
    │   ├── Dapper
    │   ├── NHibernate
    │   └── Linq2Db
    │

    │   ├── Memory
    │   ├── Redis
    │   └── SqlServer
    │
    ├── AddHealthChecks()         // Health monitoring
    │   ├── Basic
    │   ├── Ready
    │   └── Database
    │
    ├── AddDependencyInjection()  // Auto service registration
    │
    └── AddInfrastructure()       // All cross-cutting
        ├── CORS
        ├── Compression
        ├── Rate Limiting
        ├── OpenTelemetry
        └── Swagger
```

---

## 🚀 Deploy Architecture

```text
┌─────────────────────────────────────────────────────┐
│                  Kubernetes Cluster                 │
│                                                     │
│  ┌────────────────────────────────────────────┐     │
│  │           Ingress Controller               │     │
│  │  (Nginx - TLS/SSL, Load Balancing)         │     │
│  └──────────────┬─────────────────────────────┘     │
│                 │                                   │
│  ┌──────────────▼─────────────────────────────┐     │
│  │            Service (ClusterIP)             │     │
│  │         projecttemplate-api:80             │     │
│  └──────────────┬─────────────────────────────┘     │
│                 │                                   │
│  ┌──────────────▼─────────────────────────────┐     │
│  │          Deployment (Pods)                 │     │
│  │  ┌──────────────┐  ┌──────────────┐        │     │
│  │  │   Pod 1      │  │   Pod 2      │        │     │
│  │  │  API:8080    │  │  API:8080    │        │     │
│  │  │  256Mi-512Mi │  │  256Mi-512Mi │        │     │
│  │  └──────────────┘  └──────────────┘        │     │
│  └────────────────────────────────────────────┘     │
│                                                     │
│  ┌────────────────────────────────────────────┐     │
│  │           ConfigMap / Secrets              │     │
│  │  Environment Variables, Connection Strings │     │
│  └────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│              External Services                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────┐   │
│  │   Database   │  │    Redis     │  │  Others  │   │
│  │  SQL Server  │  │   APIs   │   │
│  └──────────────┘  └──────────────┘  └──────────┘   │
└─────────────────────────────────────────────────────┘
```

---

## 📊 Fluxo de Dados

```text
User Request
     ↓
[Controller] → Validate input
     ↓
[Service] → Business logic + Logging
     ↓
[Repository] → Data access abstraction
     ↓
[DbContext] → EF Core / Dapper
     ↓
[Database] → SQL queries
     ↓
[DbContext] ← Results
     ↓
[Repository] ← Entities
     ↓
[Service] ← Processing + Error handling
     ↓
[Controller] ← Format response
     ↓
User Response (JSON)
```

---

## 🔐 Security Layers

```text
┌─────────────────────────────────────────────┐
│  1. Network Security                        │
│     - HTTPS/TLS                             │
│     - Network Policies (K8s)                │
│     - Firewall rules                        │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│  2. Application Security                    │
│     - CORS policies                         │
│     - Rate limiting                         │
│     - Input validation                      │
│     - Authentication (Future: JWT/OAuth)    │
│     - Authorization (Future: Roles)         │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│  3. Data Security                           │
│     - Encrypted connections                 │
│     - SQL injection prevention              │
│     - Parameterized queries                 │
│     - Secrets management (K8s Secrets)      │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│  4. Container Security                      │
│     - Non-root user                         │
│     - Read-only filesystem                  │
│     - Security context                      │
│     - Resource limits                       │
└─────────────────────────────────────────────┘
```

---

## 📈 Escalabilidade

### Horizontal Scaling

```text
Load Balancer
      │
      ├─── Pod 1 (API Instance)
      ├─── Pod 2 (API Instance)
      ├─── Pod 3 (API Instance)
      └─── Pod N (Auto-scaled)
```

**Features:**

- Horizontal Pod Autoscaler (HPA)
- Resource-based scaling (CPU/Memory)
- Metrics-based scaling (custom metrics)

### Vertical Scaling

```text
Initial: 256Mi memory, 100m CPU
    ↓
Scaled: 512Mi memory, 500m CPU
    ↓
Max:    1Gi memory, 2000m CPU
```

**Features:**

- Resource requests and limits
- Vertical Pod Autoscaler (VPA) - Future

---

## 🛡️ Testes de Arquitetura (ArchUnitNET)

Para garantir que a integridade da Clean Architecture seja mantida ao longo do tempo, implementamos testes automatizados de arquitetura no projeto `tests/ArchitectureTests`.

### Regras Validadas

1. Isolamento do Domínio: A camada de Domínio não pode ter dependências de nenhuma outra camada do projeto.
2. Isolamento da Aplicação: A camada de Aplicação não pode depender da Infraestrutura ou dos Dados.
3. Independência de Dados/Infra: A camada de Dados e Infraestrutura não podem depender da API.
4. Padrões de Nomes: DTOs devem terminar com o sufixo "Dto" ou "Request/Response".
5. Padrões de Herança: Entidades devem herdar de `EntityBase` e Controllers de `ApiControllerBase`.

```csharp
// Exemplo de teste de arquitetura
var result = Types().InAssembly(domainAssembly)
    .ShouldNot().HaveDependencyOnAny(infraAssembly, dataAssembly, apiAssembly)
    .GetResult();
```

---

## 🎓 Melhores Práticas Implementadas

- ✅ **Clean Architecture** - Separação clara de responsabilidades
- ✅ **SOLID Principles** - Código maintainable e extensível
- ✅ **Dependency Injection** - Loose coupling, testability
- ✅ **Repository Pattern** - Abstração de dados
- ✅ **Service Pattern** - Lógica de negócio centralizada
- ✅ **Options Pattern** - Configuração fortemente tipada
- ✅ **Async/Await** - Operações assíncronas
- ✅ **CancellationToken** - Cancelamento de operações
- ✅ **Logging** - Structured logging
- ✅ **Health Checks** - Monitoring e readiness
- ✅ **Multi-ORM** - Flexibilidade de escolha
- ✅ **Docker** - Containerização
- ✅ **Kubernetes** - Orquestração

---

## 📚 Próximos Passos

Para entender melhor a arquitetura:

1. **[README.md](../README.md)** - Visão geral e uso
2. **[QUICK-START.md](../QUICK-START.md)** - Hands-on prático
3. **[docs/ORM-GUIDE.md](ORM-GUIDE.md)** - Camada de dados
4. **[docs/KUBERNETES.md](KUBERNETES.md)** - Deployment

---

**Navegação:**

- [⬆️ Voltar ao README](../README.md)
- [📖 Ver Índice](../INDEX.md)
- [🚀 Quick Start](../QUICK-START.md)
