# 🎛️ Guia de Recursos Avançados

Este guia explica como habilitar e configurar os recursos avançados incluídos no template.

---

## Índice

1. [MongoDB](#mongodb)
2. [Quartz.NET (Background Jobs)](#quartznet-background-jobs)
3. [RabbitMQ (Message Queue)](#rabbitmq-message-queue)
4. [Google Cloud Storage](#google-cloud-storage)
5. [JWT Authentication](#jwt-authentication)
6. [API Versioning](#api-versioning)
7. [Global Exception Handler](#global-exception-handler)
8. [Validation Filter](#validation-filter)
9. [Advanced Logging](#advanced-logging)
10. [Swagger/OpenAPI](#swaggeropenapi)
11. [Exception Notification](#exception-notification)
12. [Telemetria e Observabilidade](#telemetria-e-observabilidade)
13. [Rate Limiting](#rate-limiting)
14. [Event Sourcing](#event-sourcing)
15. [CI/CD](#cicd)
16. [Authentication](#authentication)

---

## MongoDB

### O que é?

MongoDB é um banco de dados NoSQL orientado a documentos, ideal para dados não estruturados e escalabilidade horizontal.

### Quando Usar?

- Dados sem schema fixo
- Logs, eventos, dados de auditoria
- Catálogos de produtos com atributos variáveis
- Dados de sessão de usuário

### Como Habilitar

**1. Descomente no Infrastructure.csproj:**

```xml
<PackageReference Include="MongoDB.Driver" Version="3.5.2" />
```

**2. Configure no appsettings.json:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "MongoDB": {
                "ConnectionString": "mongodb://username:password@localhost:27017/projecttemplate"
            }
        }
    }
}
```

**3. Adicione no Program.cs:**

```csharp
// Add MongoDB (OPTIONAL)
builder.Services.AddMongo<Program>();
```

**4. Use no código:**

```csharp
public class MyService
{
    private readonly IMongoDatabase _database;

    public MyService(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<List<MyDocument>> GetAllAsync()
    {
        var collection = _database.GetCollection<MyDocument>("mycollection");
        return await collection.Find(_ => true).ToListAsync();
    }
}
```

---

## Quartz.NET (Background Jobs)

### O que é?

Quartz.NET é um scheduler de jobs para executar tarefas em segundo plano de forma agendada.

### Quando Usar?

- Tarefas agendadas (diárias, semanais, etc.)
- Processamento em lote
- Limpeza de dados antigos
- Sincronização com sistemas externos
- Envio de relatórios

### Como Habilitar

**1. Já está habilitado no Infrastructure.csproj** ✅

**2. Configure no appsettings.json:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Quartz": {
                "MaxConcurrency": 10
            }
        }
    }
}
```

**3. Crie um Job:**

```csharp
// Infrastructure/Jobs/CleanupJob.cs
using Quartz;

public class CleanupJob : IJob
{
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(ILogger<CleanupJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Cleanup job started");

        // Your cleanup logic here
        await Task.Delay(1000);

        _logger.LogInformation("Cleanup job completed");
    }
}
```

```json

**4. Registre no Program.cs:**

```

```csharp
// Add Quartz with Jobs (OPTIONAL)
builder.Services.AddCustomizedQuartz((q, settings) =>
{
    // Daily cleanup at 3 AM
    var cleanupJobKey = new JobKey("cleanup-job");
    q.AddJob<CleanupJob>(opts => opts.WithIdentity(cleanupJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cleanupJobKey)
        .WithIdentity("cleanup-trigger")
        .WithCronSchedule("0 0 3 * * ?"));
});
```

---

## RabbitMQ (Message Queue)

### O que é?

RabbitMQ é um message broker para comunicação assíncrona entre serviços.

### Quando Usar?

- Arquitetura de microserviços
- Processamento assíncrono
- Desacoplamento de serviços
- Event-driven architecture
- Task queues

### Como Habilitar

**1. Já está habilitado no Infrastructure.csproj** ✅

**2. Configure no appsettings.json:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "RabbitMQ": {
                "ConnectionString": "amqp://username:password@localhost:5672/"
            }
        }
    }
}
```

**3. Adicione no Program.cs:**

```csharp
// Add RabbitMQ (OPTIONAL)
builder.Services.AddRabbitMq();
```

```csharp

**4. Use no código:**

```

```csharp
{
    private readonly IQueueService _queueService;

    public OrderService(IQueueService queueService)
    {
        _queueService = queueService;
    }

    public async Task CreateOrderAsync(Order order)
    {
        // Save to database
        await _repository.CreateAsync(order);

        // Publish event to queue
        await _queueService.PublishAsync("orders-queue", new
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Total = order.Total
        });
    }
}
```

---

## Google Cloud Storage

### O que é?

Google Cloud Storage é um serviço de armazenamento de objetos para arquivos e blobs.

### Quando Usar?

- Upload de arquivos/imagens
- Armazenamento de documentos
- Backup de dados
- Distribuição de conteúdo estático

### Como Habilitar

**1. Já está habilitado no Infrastructure.csproj** ✅

**2. Configure no appsettings.json:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Storage": {
                "ServiceAccount": "{\"type\":\"service_account\",\"project_id\":\"your-project\",...}",
                "DefaultBucket": "your-bucket-name"
            }
        }
    }
}
```

**3. Adicione no Program.cs:**

```csharp
// Add Google Cloud Storage (OPTIONAL)
builder.Services.AddStorage<Program>();
```

```csharp

**4. Use no código:**

```

```csharp
public class DocumentController : ApiControllerBase
{
    private readonly IStorageService _storageService;

    public DocumentController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var url = await _storageService.UploadAsync(
            bucketName: "my-bucket",
            objectName: file.FileName,
            contentType: file.ContentType,
            stream: stream);

        return Ok(new { Url = url });
    }
}
```

---

## JWT Authentication

### O que é?

JSON Web Token para autenticação stateless baseada em tokens.

### Quando Usar?

- APIs públicas que precisam autenticação
- Single Page Applications (SPA)
- Mobile apps
- Microserviços

### Como Habilitar

**1. Já está pronto no Infrastructure.csproj** ✅

**2. Configure no appsettings.json:**

```json
{
    "AppSettings": {
        "Authentication": {
            "Jwt": {
                "Secret": "your-super-secret-key-at-least-32-characters-long",
                "Issuer": "https://yourapi.com",
                "Audience": "https://yourapi.com",
                "ExpirationMinutes": 60
            }
        }
    }
}
```

**3. Adicione no Program.cs:**

```csharp
// Add Authentication (OPTIONAL)
builder.Services.AddAuthenticationExtension();

// ...

// Use Authentication
app.UseAuthentication();
app.UseAuthorization();
```

**4. Proteja endpoints:**

```csharp
[Authorize] // Requires authentication
[HttpGet]
public IActionResult GetSecureData()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return Ok(new { UserId = userId });
}
**5. Gere tokens:**
// Services/TokenService.cs
public string GenerateToken(string userId, string username)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_appSettings.Authentication.Jwt.Secret);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username)
        }),
        Expires = DateTime.UtcNow.AddMinutes(_appSettings.Authentication.Jwt.ExpirationMinutes),
        Issuer = _appSettings.Authentication.Jwt.Issuer,
        Audience = _appSettings.Authentication.Jwt.Audience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

---

## API Versioning

### O que é?

Versionamento de API para gerenciar mudanças sem quebrar clientes existentes.

### Quando Usar?

- APIs públicas
- Breaking changes
- Múltiplos clientes com versões diferentes
- Evolução gradual da API

### Como Habilitar

**1. Já está pronto no Infrastructure.csproj** ✅

**2. Adicione no Program.cs:**

```csharp
// Add API Versioning (OPTIONAL)
builder.Services.AddCustomizedApiVersioning();
```

**3. Use em controllers:**

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsV1Controller : ApiControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Version = "1.0", Message = "Old version" });
    }
}

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsV2Controller : ApiControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Version = "2.0", Message = "New version" });
    }
}
**4. Acesse:**
```

```bash
GET /api/v1/products
GET /api/v2/products
GET /api/products?api-version=2.0
GET /api/products (Header: X-Api-Version: 2.0)
```

---

## Global Exception Handler

### O que é?

Middleware que captura todas as exceções não tratadas e retorna respostas consistentes.

### Como Usar

**Já está ativo por padrão!** ✅

**Use exceções customizadas:**
using ProjectTemplate.Domain.Exceptions;

// 404 Not Found
throw new NotFoundException("Product", id);

// 400 Bad Request
throw new ValidationException("Invalid email format");

// 422 Unprocessable Entity
throw new BusinessException("Insufficient stock for order");

// 401 Unauthorized
throw new UnauthorizedAccessException("Invalid credentials");

**Respostas automáticas:**

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    "title": "Not Found",
    "status": 404,
    "detail": "Product with key '123' was not found.",
    "instance": "/api/products/123"
}
```

---

## Validation Filter

### O que é?

Action filter que valida automaticamente argumentos de controllers usando FluentValidation.

### Como Usar

**1. Já está ativo por padrão!** ✅

**2. Crie um Validator:**

```csharp
// Domain/Validators/CreateProductValidator.cs
using FluentValidation;

public class CreateProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock cannot be negative");
    }
}
```

**3. Registre validators no Program.cs:**

```csharp
// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
```

**4. Use no controller:**

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    // ValidationFilter valida automaticamente!
    // Se houver erros, retorna BadRequest antes de chegar aqui

    var product = _mapper.Map<Product>(dto);
    var created = await _service.CreateAsync(product);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

**Resposta de validação:**

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Name": ["Product name is required"],
        "Price": ["Price must be greater than zero"]
    }
}
```

---

## Advanced Logging

### O que é?

Logging estruturado com suporte a Console JSON e Google Cloud Logging.

### Como Usar

**Já está ativo por padrão!** ✅

**Configuração:**

```csharp
// Development: JSON Console
// Production sem GCP: Simple Console
// Production com GCP: Google Cloud Logging
builder.AddCustomLogging();
```

**Use nos serviços:**

```csharp
public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public ProductService(ILogger<ProductService> logger)
    {
        _logger = logger;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _logger.LogInformation("Creating product {ProductName}", product.Name);

        try
        {
            var result = await _repository.CreateAsync(product);
            _logger.LogInformation("Product {ProductId} created successfully", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName}", product.Name);
            throw;
        }
    }

}
```

**Logs estruturados:**

```json
{
    "timestamp": "2026-01-11T10:30:45.123Z",
    "level": "Information",
    "category": "ProjectTemplate.Application.Services.ProductService",
    "message": "Creating product Laptop",
    "scopes": {
        "RequestId": "0HN1234567890",
        "RequestPath": "/api/products"
    }
}
```

---

## Recomendações

### Começando

Use apenas:

- ✅ Global Exception Handler
- ✅ Validation Filter
- ✅ Advanced Logging

### Crescendo

Adicione conforme necessário:

- API Versioning (quando precisar de múltiplas versões)
- JWT Authentication (quando precisar proteger APIs)
- MongoDB (para dados não estruturados)

### Produção

Considere adicionar:

- Quartz.NET (para jobs agendados)
- RabbitMQ (para desacoplamento)
- Cloud Storage (para arquivos)

---

## Próximos Passos

1. Leia a documentação de cada pacote:
    - [MongoDB Driver](https://docs.mongodb.com/drivers/csharp/)
    - [Quartz.NET](https://www.quartz-scheduler.net/documentation/)
    - [RabbitMQ](https://www.rabbitmq.com/getstarted.html)
    - [Google Cloud Storage](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Storage.V1/latest)

2. Teste localmente com Docker Compose:

```yaml
services:
    mongodb:
        image: mongo:latest
        ports:
            - "27017:27017"

    rabbitmq:
        image: rabbitmq:3-management
        ports:
            - "5672:5672"
            - "15672:15672"

    redis:
        image: redis:alpine
        ports:
            - "6379:6379"
```

---

## Swagger/OpenAPI

### O que é?

Swagger (OpenAPI) gera documentação interativa para sua API, permitindo testar endpoints diretamente no navegador.

### Quando Usar?

- Documentação automática e sincronizada
- Validar contratos e schemas antes do deploy
- Compartilhar endpoints com squads/parceiros
- Testar rapidamente sem Postman

### Como Funciona

O template já integra um Swagger customizado com:

- Agrupamento por controller e versão
- Suporte a JWT Bearer token
- Comentários XML (quando habilitados)
- UI com filtros, validador e tempo de resposta

### Configuração

**Program.cs já contém:**

```csharp
// Swagger customizado
builder.Services.AddCustomizedSwagger();

// ...

if (app.Environment.IsDevelopment())
{
    app.UseCustomizedSwagger();
}
```

### Personalizar

Edite `SwaggerExtension.cs` para alterar metadados:

```csharp
options.SwaggerDoc("v1", new OpenApiInfo
{
    Version = "v1.0.0",
    Title = "Minha API Incrível",
    Description = "API do meu projeto",
    Contact = new OpenApiContact
    {
        Name = "Time de Desenvolvimento",
        Email = "dev@meudominio.com",
        Url = new Uri("https://meudominio.com")
    }
});
```

### Habilitar Comentários XML

**1. Atualize Api.csproj:**

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

**2. Documente controllers:**

```csharp
/// <summary>
/// Gerencia operações de produtos
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// Obtém todos os produtos
    /// </summary>
    /// <returns>Lista de produtos</returns>
    /// <response code="200">Sucesso</response>
    /// <response code="500">Erro interno</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        // ...
    }
}
```

### Acessar Swagger

**Development:**

```text
http://localhost:5000/swagger
```

**Testar com Token JWT:**

1. Faça login e copie o token
2. Clique no botão **Authorize**
3. Informe `Bearer {token}`
4. Execute os endpoints normalmente

### Features da UI

- Display Request Duration
- Deep Linking
- Pesquisa/filtro
- Schema Validator
- Try it out com payloads formatados

### Adicionar Novos Filtros

```csharp
public class AddCustomHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Required = true,
            Description = "API Key para autenticação"
        });
    }
}
```

Registrar:

```csharp
options.OperationFilter<AddCustomHeaderParameter>();
```

### Múltiplas Versões

```csharp
options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "API v1" });
options.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "API v2" });

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
});
```

### Desabilitar em Produção

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCustomizedSwagger(); // Apenas em dev/staging
}
```

Para habilitar em produção (não recomendado):

```csharp
app.UseCustomizedSwagger();
```

### Recomendações

- Disponibilize apenas em dev/staging
- Documente endpoints com `[ProducesResponseType]`
- Adicione exemplos com `[SwaggerSchema(Example = "...")]`
- Proteja com autenticação quando exposto publicamente

---

## Exception Notification

### O que é?

Serviço para notificar exceções não tratadas. Por padrão loga tudo, mas pode enviar e-mail, Slack, Teams ou abrir tickets.

### Como Funciona

```csharp
var notificationService = context.RequestServices.GetService<IExceptionNotificationService>();
if (notificationService != null)
{
    await notificationService.NotifyAsync(context, exception);
}
```

### Implementação Padrão

```csharp
public class ExceptionNotificationService : IExceptionNotificationService
{
    private readonly ILogger<ExceptionNotificationService> _logger;

    public ExceptionNotificationService(ILogger<ExceptionNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(HttpContext context, Exception exception)
    {
        var user = context.User.Identity?.Name ?? "Anonymous";
        var path = context.Request.Path;

        _logger.LogError(exception, "Exception for user {User} on {Path}", user, path);
        return Task.CompletedTask;
    }
}
```

### Customizar para Email

```csharp
public class EmailExceptionNotificationService : IExceptionNotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailExceptionNotificationService> _logger;

    public EmailExceptionNotificationService(
        IEmailService emailService,
        ILogger<EmailExceptionNotificationService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task NotifyAsync(HttpContext context, Exception exception)
    {
        try
        {
            var user = context.User.Identity?.Name ?? "Anonymous";
            var path = context.Request.Path;
            var method = context.Request.Method;

            var subject = $"[ERROR] {exception.GetType().Name} em {path}";
            var body = $@"
                <h2>Erro na aplicação</h2>
                <p><strong>Usuário:</strong> {user}</p>
                <p><strong>Endpoint:</strong> {method} {path}</p>
                <p><strong>Mensagem:</strong> {exception.Message}</p>
                <p><strong>Stack Trace:</strong></p>
                <pre>{exception.StackTrace}</pre>";

            await _emailService.SendAsync(
                to: "devops@empresa.com",
                subject: subject,
                body: body);

            _logger.LogInformation("Exception notification sent to devops@empresa.com");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send exception notification");
        }
    }
}
```

Registrar:

```csharp
services.AddScoped<IExceptionNotificationService, EmailExceptionNotificationService>();
```

### Customizar para Slack

```csharp
public class SlackExceptionNotificationService : IExceptionNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public SlackExceptionNotificationService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _webhookUrl = config["Slack:WebhookUrl"];
    }

    public async Task NotifyAsync(HttpContext context, Exception exception)
    {
        var message = new
        {
            text = "🚨 *Erro na API*",
            blocks = new[]
            {
                new
                {
                    type = "section",
                    text = new
                    {
                        type = "mrkdwn",
                        text = $"*Usuário:* {context.User.Identity?.Name ?? "Anonymous"}
" +
                               $"*Endpoint:* `{context.Request.Method} {context.Request.Path}`
" +
                               $"*Erro:* {exception.Message}"
                    }
                }
            }
        };

        await _httpClient.PostAsJsonAsync(_webhookUrl, message);
    }
}
```

### Filtrar Exceções

```csharp
public async Task NotifyAsync(HttpContext context, Exception exception)
{
    if (exception is BusinessException || exception is ValidationException)
    {
        return;
    }

    if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
    {
        await SendNotificationAsync(context, exception);
    }
}
```

### Throttling (evitar spam)

```csharp
private static readonly ConcurrentDictionary<string, DateTime> _lastNotification = new();

public async Task NotifyAsync(HttpContext context, Exception exception)
{
    var key = $"{exception.GetType().Name}:{context.Request.Path}";
    var now = DateTime.UtcNow;

    if (_lastNotification.TryGetValue(key, out var lastTime) &&
        (now - lastTime).TotalMinutes < 5)
    {
        return;
    }

    _lastNotification[key] = now;
    await SendNotificationAsync(context, exception);
}
```

### Recomendações

- Development: apenas logs
- Staging: e-mail para time dev
- Produção: Slack/Teams + ticket
- Use throttling para evitar spam
- Nunca inclua dados sensíveis

---

## Telemetria e Observabilidade

### O que é?

Sistema completo de observabilidade com OpenTelemetry (traces, métricas e logs estruturados).

### Provedores Suportados

Jaeger, Grafana Cloud, Prometheus, Application Insights, Datadog, Dynatrace e Console.

### Como Habilitar

**1. appsettings.json**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Telemetry": {
                "Enabled": true,
                "Providers": ["jaeger", "prometheus"],
                "SamplingRatio": 1.0,
                "Jaeger": {
                    "Host": "localhost",
                    "Port": 6831
                }
            }
        }
    }
}
```

**2. Inicie a stack**

```bash
docker-compose up -d jaeger prometheus grafana
```

**3. Acesse**

- Jaeger: http://localhost:16686
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000

### Métricas Automáticas

HTTP request duration, active requests, SQL/EF Core, GC, memória e thread pool.

### Métrica customizada

```csharp
public class ProductService : Service<Product>
{
    private readonly Counter<long> _productCreatedCounter;

    public ProductService(IRepository<Product> repository, IMeterFactory meterFactory)
        : base(repository)
    {
        var meter = meterFactory.Create("ProjectTemplate.Api");
        _productCreatedCounter = meter.CreateCounter<long>("products.created");
    }

    public override async Task<Product> AddAsync(Product entity, CancellationToken ct = default)
    {
        var result = await base.AddAsync(entity, ct);
        _productCreatedCounter.Add(1, new KeyValuePair<string, object>("category", entity.Category));
        return result;
    }
}
```

### Configuração de Produção

```json
{
    "Telemetry": {
        "Enabled": true,
        "Providers": ["applicationinsights", "prometheus"],
        "SamplingRatio": 0.1,
        "ApplicationInsights": {
            "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=https://..."
        }
    }
}
```

### Mais Informações

Consulte [docs/TELEMETRY.md](TELEMETRY.md).

---

## Rate Limiting

### O que é?

Controla a taxa de requisições para proteger a API contra abusos e garantir fair usage.

### Estratégias Disponíveis

Fixed Window, Sliding Window, Token Bucket e Concurrency.

### Como Habilitar

```json
{
    "AppSettings": {
        "Infrastructure": {
            "RateLimiting": {
                "Enabled": true,
                "EnableWhitelist": true,
                "WhitelistedIps": ["192.168.1.100", "10.0.0.0/24"],
                "Policies": {
                    "FixedWindow": {
                        "Enabled": true,
                        "PermitLimit": 100,
                        "WindowSeconds": 60,
                        "QueueLimit": 10
                    },
                    "SlidingWindow": {
                        "Enabled": true,
                        "PermitLimit": 200,
                        "WindowSeconds": 60,
                        "SegmentsPerWindow": 6
                    },
                    "TokenBucket": {
                        "Enabled": true,
                        "TokenLimit": 50,
                        "ReplenishmentPeriodSeconds": 10,
                        "TokensPerPeriod": 10
                    },
                    "Concurrency": {
                        "Enabled": true,
                        "PermitLimit": 10,
                        "QueueLimit": 20
                    }
                }
            }
        }
    }
}
```

Aplicando nos endpoints:

```csharp
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    [EnableRateLimiting("sliding")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => await Task.FromResult(Ok());

    [EnableRateLimiting("fixed")]
    [HttpPost]
    public async Task<IActionResult> Create(Product product) => await Task.FromResult(Ok());

    [EnableRateLimiting("concurrent")]
    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel() => await Task.FromResult(Ok());

    [DisableRateLimiting]
    [HttpGet("health")]
    public IActionResult Health() => Ok();
}
```

### Resposta 429

**Headers:**

```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1705330260
Retry-After: 45
```

**Body:**

```json
{
    "error": "Rate limit exceeded",
    "message": "Too many requests. Limit: 100 per window.",
    "clientIp": "192.168.1.100",
    "retryAfter": 45,
    "resetAt": "2024-01-15T10:51:00Z"
}
```

### Limites Recomendados

| Tipo de API     | Fixed Window  | Token Bucket          | Concurrency     |
| --------------- | ------------- | --------------------- | --------------- |
| API Pública     | 100 req/min   | 50 tokens, 10/10s     | 5 simultâneas   |
| API Autenticada | 1000 req/min  | 500 tokens, 100/10s   | 20 simultâneas  |
| API Interna     | 5000 req/min  | 2000 tokens, 500/10s  | 50 simultâneas  |
| API Premium     | 10000 req/min | 5000 tokens, 1000/10s | 100 simultâneas |

### Whitelist de IPs

```json
{
    "RateLimiting": {
        "EnableWhitelist": true,
        "WhitelistedIps": ["192.168.1.100", "10.0.0.0/24", "172.16.0.0/16"]
    }
}
```

### Testando

**PowerShell:**

```powershell
1..105 | ForEach-Object {
  $response = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/Product" -SkipHttpErrorCheck
  Write-Host "Request $_: $($response.StatusCode)"
}
```

**curl:**

```bash
for i in {1..105}; do
  curl -i http://localhost:5000/api/v1/Product
done
```

Verificar headers:

```bash
curl -i http://localhost:5000/api/v1/Product | grep -i "x-ratelimit"
```

### Logs

```text
✅  Rate Limiting enabled: 4 policies configured
📊  Fixed Window: 100 req/60s
📊  Sliding Window: 200 req/60s (6 segments)
📊  Token Bucket: 50 tokens, refill 10/10s
📊  Concurrency: 10 simultaneous requests
```

### Mais Informações

Consulte [docs/RATE-LIMITING.md](RATE-LIMITING.md).

---

## CI/CD

### O que é?

CI/CD (Continuous Integration/Continuous Deployment) automatiza build, testes, análises e deploy.

### Plataformas Suportadas

Pipelines prontos para GitHub Actions, Azure DevOps e GitLab CI.

### Features Incluídas

| Feature           | GitHub Actions | Azure DevOps | GitLab CI   |
| ----------------- | -------------- | ------------ | ----------- |
| Build             | ✅             | ✅           | ✅          |
| Unit Tests        | ✅             | ✅           | ✅          |
| Integration Tests | ✅             | ✅           | ✅          |
| Code Coverage     | ✅ Codecov     | ✅ Built-in  | ✅ Built-in |
| Security Scan     | ✅             | ✅           | ✅          |
| Docker Build      | ✅             | ✅           | ✅          |
| Deploy            | ✅ Manual      | ✅ Approval  | ✅ Manual   |
| Cache             | ✅             | ✅           | ✅          |

### Quick Start - GitHub Actions

1. Configure secrets:

```bash
DOCKER_USERNAME=seu-usuario
DOCKER_PASSWORD=seu-token
```

1. Faça push para `main` ou `develop`.
2. Acompanhe na aba **Actions**.

### Quick Start - Azure DevOps

1. Pipelines → New pipeline → use `azure-pipelines.yml`.
2. Crie a service connection `DockerHubConnection`.
3. Push para `main`/`develop`.

### Quick Start - GitLab CI

1. `.gitlab-ci.yml` já está pronto.
2. Habilite o Container Registry.
3. Push → pipeline executa automaticamente.

### Pipeline Stages

```text
1. 🏗️ Build    → restore, build, publish
2. 🧪 Test     → unit + integration + coverage
3. 📊 Quality  → code analysis + security scan
4. 🐳 Docker   → build/tag/push
5. 🚀 Deploy   → manual ou com aprovação
```

### Badges

- GitHub Actions: ![CI/CD](https://github.com/usuario/repo/actions/workflows/ci.yml/badge.svg)
- Azure DevOps: [![Build Status](https://dev.azure.com/org/project/_apis/build/status/pipeline)](https://dev.azure.com/org/project/_build)
- GitLab CI:
  - ![pipeline status](https://gitlab.com/usuario/repo/badges/main/pipeline.svg)
  - ![coverage report](https://gitlab.com/usuario/repo/badges/main/coverage.svg)

### Personalização

**Alterar versão do .NET**

```yaml
# GitHub Actions
env:
    DOTNET_VERSION: "10.0.x"
```

```yaml
# Azure DevOps
variables:
    dotnetVersion: "10.0.x"
```

```yaml
# GitLab CI
image: mcr.microsoft.com/dotnet/sdk:10.0
```

**Deploy automático**
Remova condicionais (`if`, `condition`, `when: manual`) para liberar deploy contínuo.

### Logs e Resultados

Todos os pipelines geram testes (TRX/JUnit), cobertura, security scan, artifacts e imagens Docker versionadas.

### Troubleshooting

**Build falhou**

```bash
dotnet restore
dotnet build --configuration Release
dotnet test
```

**Docker build falhou**

- Verifique Dockerfile na raiz
- Certifique-se de que o Docker está rodando
- GitLab: habilite `docker:dind`

**Secrets não funcionam**

- Confirme nomes e se estão marcados como Protected (GitLab)

### Mais Informações

Consulte [docs/CICD.md](CICD.md).

---

## Event Sourcing

### O que é?

Armazena toda a sequência de eventos imutáveis e reconstrói o estado a partir deles, oferecendo auditoria completa e capacidade de time-travel.

### Quando Usar

| Cenário                          | Recomendação             |
| -------------------------------- | ------------------------ |
| Sistemas financeiros             | ✅ Altamente recomendado |
| E-commerce (pedidos, pagamentos) | ✅ Recomendado           |
| Healthcare (prontuários)         | ✅ Recomendado           |

### Quick Start

```json
{
    "Infrastructure": {
        "EventSourcing": {
            "Enabled": true,
            "Mode": "Hybrid",
            "AuditEntities": ["Order"]
        }
    }
}
```

Inicie o banco de eventos:

```bash
docker-compose up -d postgres-events
```

Endpoints de auditoria:

- `GET /api/audit/Order/123`
- `GET /api/audit/Order/123/at/{timestamp}`

Mais detalhes em [docs/EVENT-SOURCING.md](EVENT-SOURCING.md).

---

## Authentication

### O que é?

Sistema completo de autenticação/autorização com JWT, refresh tokens e OAuth2 (Google, Microsoft, GitHub).

### Quando Usar

| Cenário                       | Recomendação   |
| ----------------------------- | -------------- |
| APIs públicas protegidas      | ✅ Essencial   |
| Aplicações multi-usuário      | ✅ Essencial   |
| Sistemas com múltiplos perfis | ✅ Recomendado |
| Login social                  | ✅ Recomendado |

### Recursos

- JWT Authentication
- Refresh tokens com rotação
- OAuth2 providers
- Políticas de senha configuráveis
- Role-based authorization
- Revogação de tokens e auditoria

### Quick Start

**1. Configure appsettings.json**

```json
{
    "Authentication": {
        "Enabled": true,
        "JwtSettings": {
            "Secret": "your-256-bit-secret-key-change-this",
            "Issuer": "ProjectTemplate",
            "Audience": "ProjectTemplate",
            "ExpirationMinutes": 60
        }
    }
}
```

**2. Criar migration**

```bash
dotnet ef migrations add AddAuthentication --project src/Data --startup-project src/Api
dotnet ef database update --project src/Data --startup-project src/Api
```

**3. Executar**

```bash
dotnet run --project src/Api
```

Use o Swagger para registrar, fazer login e autorizar requisições.

### Endpoints

| Método | Endpoint                    | Descrição                     |
| ------ | --------------------------- | ----------------------------- |
| POST   | `/api/auth/register`        | Registrar novo usuário        |
| POST   | `/api/auth/login`           | Login com usuário/senha       |
| POST   | `/api/auth/refresh-token`   | Renovar access token          |
| POST   | `/api/auth/revoke-token`    | Revogar refresh token         |
| GET    | `/api/auth/me`              | Dados do usuário autenticado  |
| POST   | `/api/auth/change-password` | Alterar senha                 |
| PUT    | `/api/auth/profile`         | Atualizar perfil              |
| POST   | `/api/auth/oauth2/login`    | Login com provedores externos |

### Exemplo de Uso

```csharp
var registerDto = new RegisterDto
{
    Username = "john.doe",
    Email = "john@example.com",
    Password = "P@ssw0rd123",
    FirstName = "John",
    LastName = "Doe"
};

var registerResponse = await authService.RegisterAsync(registerDto);

var loginDto = new LoginDto
{
    UsernameOrEmail = "john.doe",
    Password = "P@ssw0rd123"
};

var authResponse = await authService.LoginAsync(loginDto, "127.0.0.1");

httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
```

### OAuth2 Providers

```json
{
    "OAuth2Settings": {
        "GoogleOAuthSettings": {
            "Enabled": true,
            "ClientId": "your-google-client-id",
            "ClientSecret": "your-google-client-secret"
        },
        "MicrosoftOAuthSettings": {
            "Enabled": true,
            "ClientId": "your-microsoft-client-id",
            "ClientSecret": "your-microsoft-client-secret",
            "TenantId": "common"
        },
        "GitHubOAuthSettings": {
            "Enabled": true,
            "ClientId": "your-github-client-id",
            "ClientSecret": "your-github-client-secret"
        }
    }
}
```

### Password Policy

```json
{
    "PasswordPolicySettings": {
        "MinimumLength": 8,
        "RequireDigit": true,
        "RequireLowercase": true,
        "RequireUppercase": true,
        "RequireNonAlphanumeric": true,
        "MaxFailedAccessAttempts": 5,
        "LockoutMinutes": 15
    }
}
```

### Security Best Practices

- Use HTTPS (`RequireHttpsMetadata = true`)
- Armazene o segredo JWT em variáveis seguras/Key Vault
- Tokens curtos (15–60 min) + refresh rotation
- Refresh tokens em cookies HttpOnly
- Rate limiting nos endpoints de auth
- Log de eventos para auditoria
- Produção: prefira BCrypt ou Argon2 para hashing de senhas

---

**Navegação:**

- [⬆️ Voltar ao README](../README.md)
- [📖 Ver Índice](../INDEX.md)
- [🚀 Quick Start](../QUICK-START.md)

---

_Última atualização: Janeiro 2026 | Versão: 1.0.0_
