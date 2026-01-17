# 🎛️ Guia de Recursos Avançados

Este guia explica como habilitar e configurar os recursos avançados incluídos no template.

---

## 📋 Índice

1. [MongoDB](#-mongodb)
2. [Quartz.NET (Background Jobs)](#-quartznet-background-jobs)
3. [RabbitMQ (Message Queue)](#-rabbitmq-message-queue)
4. [Google Cloud Storage](#-google-cloud-storage)
5. [JWT Authentication](#-jwt-authentication)
6. [API Versioning](#-api-versioning)
7. [Global Exception Handler](#-global-exception-handler)
8. [Validation Filter](#-validation-filter)
9. [Advanced Logging](#-advanced-logging)
10. [Swagger/OpenAPI](#-swaggeropenapi)
11. [Exception Notification](#-exception-notification)
12. [Telemetria e Observabilidade](#-telemetria-e-observabilidade)
13. [Rate Limiting](#-rate-limiting)
14. [Event Sourcing](#-event-sourcing)
15. [CI/CD](#-cicd)

---

## 🍃 MongoDB

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
---

## ⏰ Quartz.NET (Background Jobs)

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

```
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
---

## 🐰 RabbitMQ (Message Queue)

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

```
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
---

## ☁️ Google Cloud Storage

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

```
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
---

## 🔐 JWT Authentication

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

```
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
---

## 📌 API Versioning

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

```
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
---

## 🚨 Global Exception Handler

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
```

**Respostas automáticas:**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Product with key '123' was not found.",
  "instance": "/api/products/123"
}
---

## ✅ Validation Filter

### O que é?
Action filter que valida automaticamente argumentos de controllers usando FluentValidation.

### Como Usar

**1. Já está ativo por padrão!** ✅

**2. Crie um Validator:**

```
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
---

## 📊 Advanced Logging

### O que é?
Logging estruturado com suporte a Console JSON e Google Cloud Logging.

### Como Usar

**Já está ativo por padrão!** ✅

**Configuração:**
// Development: JSON Console
// Production sem GCP: Simple Console
// Production com GCP: Google Cloud Logging
builder.AddCustomLogging();
**Use nos serviços:**
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
**Logs estruturados:**
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
---

## 🎯 Recomendações

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

## 📚 Próximos Passos

1. Leia a documentação de cada pacote:
   - [MongoDB Driver](https://docs.mongodb.com/drivers/csharp/)
   - [Quartz.NET](https://www.quartz-scheduler.net/documentation/)
   - [RabbitMQ](https://www.rabbitmq.com/getstarted.html)
   - [Google Cloud Storage](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Storage.V1/latest)

2. Teste localmente com Docker Compose:
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
---

## 📚 Swagger/OpenAPI

### O que é?
Swagger (OpenAPI) gera documentação interativa para sua API, permitindo testar endpoints diretamente no navegador.

### Quando Usar?
- **Sempre!** Essencial para:
  - Documentação automática da API
  - Testar endpoints sem Postman
  - Facilitar integração para outros desenvolvedores
  - Validar contratos de API

### Como Funciona

O template já vem com Swagger customizado que inclui:
- ✅ Agrupamento automático por controller
- ✅ Suporte a JWT Bearer token
- ✅ Versionamento de API
- ✅ Comentários XML (se habilitados)
- ✅ UI customizada com filtros e validação

### Configuração

**Já está configurado no Program.cs:**
// Swagger customizado
builder.Services.AddCustomizedSwagger();

// ...

if (app.Environment.IsDevelopment())
{
    app.UseCustomizedSwagger();
}
### Personalizar

**Para customizar, edite `SwaggerExtension.cs`:**

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
### Habilitar Comentários XML

**1. Adicione no Api.csproj:**
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
**2. Documente seus controllers:**
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
### Acessar Swagger

**Development:**
```
```bash
http://localhost:5000/swagger
**Testar com Token JWT:**
1. Faça login e copie o token
2. Clique no botão "Authorize" 🔒
3. Cole o token (sem "Bearer")
4. Todos os requests usarão o token

### Features da UI

- **Display Request Duration**: Mostra tempo de resposta
- **Deep Linking**: URLs diretas para endpoints
- **Filter**: Buscar endpoints por nome
- **Validator**: Valida respostas contra schema
- **Try it out**: Testar endpoints ao vivo

### Adicionar Novos Filtros

**Criar filtro customizado:**
public class AddCustomHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Required = true,
            Description = "API Key para autenticação"
        });
    }
}
**Registrar:**
options.OperationFilter<AddCustomHeaderParameter>();
### Múltiplas Versões

**Para suportar v1 e v2:**
options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "API v1" });
options.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "API v2" });

// UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
});
### Desabilitar em Produção

**Por segurança, Swagger já está desabilitado em produção:**
if (app.Environment.IsDevelopment())
{
    app.UseCustomizedSwagger(); // Apenas em dev
}
**Para habilitar em produção (não recomendado):**
// Sem verificação de ambiente
app.UseCustomizedSwagger();
### Recomendações

- ✅ Use Swagger em **Development e Staging**
- ❌ **Desabilite em produção** por segurança
- ✅ Documente todos os endpoints com XML comments
- ✅ Use `[ProducesResponseType]` para docs precisos
- ✅ Adicione exemplos com `[SwaggerSchema(Example = "...")]`

---

## 📧 Exception Notification

### O que é?
Serviço para notificar sobre exceções não tratadas. Por padrão, registra no console/logs, mas pode ser estendido para enviar emails, Slack, etc.

### Quando Usar?
- Alertas em tempo real sobre erros críticos
- Notificações para equipe de DevOps
- Integração com sistemas de ticketing
- Envio de relatórios de erro

### Como Funciona

**Já está integrado no GlobalExceptionHandler:**
// Em GlobalExceptionHandler.cs
var notificationService = context.RequestServices.GetService<IExceptionNotificationService>();
if (notificationService != null)
{
    await notificationService.NotifyAsync(context, exception);
}
### Implementação Padrão

**O template inclui implementação básica que loga no console:**
public class ExceptionNotificationService : IExceptionNotificationService
{
    public async Task NotifyAsync(HttpContext context, Exception exception)
    {
        var user = context.User.Identity?.Name ?? "Anonymous";
        var path = context.Request.Path;

        _logger.LogError(exception,
            "Exception for user {User} on {Path}",
            user, path);
    }
}
### Customizar para Email

**Criar implementação customizada:**
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
                <pre>{exception.StackTrace}</pre>
            ";

            await _emailService.SendAsync(
                to: "devops@empresa.com",
                subject: subject,
                body: body
            );

            _logger.LogInformation("Exception notification sent to devops@empresa.com");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send exception notification");
        }
    }
}
**Registrar no DependencyInjectionExtension.cs:**
// Substituir implementação padrão
services.AddScoped<IExceptionNotificationService, EmailExceptionNotificationService>();
### Customizar para Slack

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
            text = $"🚨 *Erro na API*",
            blocks = new[]
            {
                new
                {
                    type = "section",
                    text = new
                    {
                        type = "mrkdwn",
                        text = $"*Usuário:* {context.User.Identity?.Name ?? "Anonymous"}\n" +
                               $"*Endpoint:* `{context.Request.Method} {context.Request.Path}`\n" +
                               $"*Erro:* {exception.Message}"
                    }
                }
            }
        };

        await _httpClient.PostAsJsonAsync(_webhookUrl, message);
    }
}
### Filtrar Exceções

**Notificar apenas erros críticos:**
public async Task NotifyAsync(HttpContext context, Exception exception)
{
    // Ignorar exceções de negócio
    if (exception is BusinessException || exception is ValidationException)
    {
        return;
    }

    // Notificar apenas 500+
    if (context.Response.StatusCode >= 500)
    {
        await SendNotificationAsync(context, exception);
    }
}
### Throttling (Limitar Notificações)

**Evitar spam com muitas notificações:**
private static readonly ConcurrentDictionary<string, DateTime> _lastNotification = new();

public async Task NotifyAsync(HttpContext context, Exception exception)
{
    var key = $"{exception.GetType().Name}:{context.Request.Path}";
    var now = DateTime.UtcNow;

    // Notificar apenas 1x a cada 5 minutos para mesma exceção/endpoint
    if (_lastNotification.TryGetValue(key, out var lastTime))
    {
        if ((now - lastTime).TotalMinutes < 5)
        {
            return; // Skip notification
        }
    }

    _lastNotification[key] = now;
    await SendNotificationAsync(context, exception);
}
### Recomendações

- ✅ **Development**: Apenas logs no console
- ✅ **Staging**: Email para equipe de dev
- ✅ **Production**: Slack/Teams + sistema de tickets
- ✅ Use throttling para evitar spam
- ✅ Filtre exceções não críticas (400, 404, ValidationException)
- ❌ Nunca envie informações sensíveis nas notificações

---

## 📊 Telemetria e Observabilidade

### O que é?

Sistema completo de **observabilidade** com **OpenTelemetry** incluindo:
- **Traces**: Rastreamento distribuído de requests
- **Metrics**: Métricas de performance e negócio
- **Logs**: Logging estruturado (já implementado)

### Provedores Suportados

✅ **Jaeger** - Distributed tracing (open source)
✅ **Grafana Cloud** - Stack completa gerenciada
✅ **Prometheus** - Metrics collection (open source)
✅ **Application Insights** - APM Azure
✅ **Datadog** - APM enterprise completo
✅ **Dynatrace** - APM enterprise avançado
✅ **Console** - Debug local

### Como Habilitar

**1. Configurar appsettings.json:**

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
**2. Iniciar Stack de Observabilidade:**

# Jaeger + Prometheus + Grafana
docker-compose up -d
**3. Acessar UIs:**

- **Jaeger**: http://localhost:16686 (traces)
- **Prometheus**: http://localhost:9090 (metrics)
- **Grafana**: http://localhost:3000 (dashboards)

### Métricas Automáticas

✅ HTTP request duration
✅ HTTP active requests
✅ SQL query duration
✅ Entity Framework operations
✅ GC collections
✅ Memory usage
✅ Thread pool

### Exemplo: Métrica Customizada

public class ProductService : Service<Product>
{
    private readonly Counter<long> _productCreatedCounter;

    public ProductService(IRepository<Product> repository, IMeterFactory meterFactory)
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
### Configurações de Produção

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
### Mais Informações

📖 **Documentação completa**: [docs/TELEMETRY.md](TELEMETRY.md)

**Configurar provedores específicos:**
- Application Insights (Azure)
- Datadog
- Dynatrace
- Grafana Cloud
- Custom OTLP endpoints

---

## 🚦 Rate Limiting

### O que é?

Rate Limiting controla a taxa de requisições que clientes podem fazer à API, protegendo contra abusos, DDoS e garantindo disponibilidade para todos os usuários.

### Quando Usar?

- ✅ APIs públicas expostas à internet
- ✅ Proteger contra ataques DDoS
- ✅ Garantir fair usage entre clientes
- ✅ Controlar custos de infraestrutura
- ✅ Limitar operações pesadas (exports, relatórios)

### Estratégias Disponíveis

#### 1. **Fixed Window** (Janela Fixa)
- Limite fixo por período (ex: 100 req/min)
- Simples e previsível
- Ideal para APIs públicas

#### 2. **Sliding Window** (Janela Deslizante)
- Janela "desliza" suavemente
- Evita picos no reset da janela
- Melhor para alto tráfego

#### 3. **Token Bucket** (Balde de Tokens)
- Permite bursts ocasionais
- Taxa sustentada configurável
- Mais flexível e realista

#### 4. **Concurrency** (Concorrência)
- Limita requisições **simultâneas**
- Protege recursos limitados (DB, threads)
- Ideal para operações pesadas

### Como Habilitar

**1. Configure no appsettings.json:**

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
**2. Aplique nos endpoints:**

using Microsoft.AspNetCore.RateLimiting;

[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    // Leitura: Sliding Window (suave)
    [EnableRateLimiting("sliding")]
    [HttpGet]
    public async Task<IActionResult> GetAll() { ... }

    // Escrita: Fixed Window (previsível)
    [EnableRateLimiting("fixed")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product) { ... }

    // Operação pesada: Concurrency
    [EnableRateLimiting("concurrent")]
    [HttpGet("ExportToExcel")]
    public async Task<IActionResult> ExportToExcel() { ... }

    // Sem limite (público)
    [DisableRateLimiting]
    [HttpGet("health")]
    public IActionResult Health() => Ok();
}
### Resposta de Limite Excedido (429)

Quando o limite é excedido:

**Headers:**
```
```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1705330260
Retry-After: 45
**Body:**
{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Limit: 100 per window.",
  "clientIp": "192.168.1.100",
  "retryAfter": 45,
  "resetAt": "2024-01-15T10:51:00Z"
}
### Limites Recomendados

| Tipo de API | Fixed Window | Token Bucket | Concurrency |
|---|---|---|---|
| **API Pública** | 100 req/min | 50 tokens, 10/10s | 5 simultâneas |
| **API Autenticada** | 1000 req/min | 500 tokens, 100/10s | 20 simultâneas |
| **API Interna** | 5000 req/min | 2000 tokens, 500/10s | 50 simultâneas |
| **API Premium** | 10000 req/min | 5000 tokens, 1000/10s | 100 simultâneas |

### Whitelist de IPs

IPs whitelistados não sofrem limitação:

{
  "RateLimiting": {
    "EnableWhitelist": true,
    "WhitelistedIps": [
      "192.168.1.100",    // IP único
      "10.0.0.0/24",      // Rede CIDR
      "172.16.0.0/16"     // Rede privada
    ]
  }
}
**Usa casos:**
- Servidores internos (CI/CD, monitoramento)
- IPs de parceiros
- Load balancers e proxies

### Testando Rate Limiting

**PowerShell:**
1..105 | ForEach-Object {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/Product" -SkipHttpErrorCheck
    Write-Host "Request $_: $($response.StatusCode)"
}
**curl:**
for i in {1..105}; do
  curl -i http://localhost:5000/api/v1/Product
done
**Verificar headers:**
curl -i http://localhost:5000/api/v1/Product | grep -i "x-ratelimit"
### Logs

```
```text
✅  Rate Limiting enabled: 4 policies configured
📊  Fixed Window: 100 req/60s
📊  Sliding Window: 200 req/60s (6 segments)
📊  Token Bucket: 50 tokens, refill 10/10s
📊  Concurrency: 10 simultaneous requests
### Mais Informações

📖 **Documentação completa**: [docs/RATE-LIMITING.md](RATE-LIMITING.md)

**Tópicos detalhados:**
- Comparação de estratégias
- Configuração por ambiente
- Whitelist de IPs
- Testes e troubleshooting
- Melhores práticas

---

## 🔄 CI/CD

### O que é?

CI/CD (Continuous Integration/Continuous Deployment) automatiza build, testes e deploy da aplicação.

### Quando Usar?

- ✅ **Sempre!** Todo projeto moderno precisa de CI/CD
- ✅ Garantir que código compila antes de merge
- ✅ Executar testes automaticamente em cada commit
- ✅ Deploy automático para ambientes de staging/produção
- ✅ Manter qualidade de código com análises automáticas

### Plataformas Suportadas

O template inclui pipelines prontos para:

#### 1. **GitHub Actions**
- Pipeline completo em `.github/workflows/ci.yml`
- Build, testes, coverage, Docker, deploy
- Integração nativa com GitHub
- Cache de NuGet packages
- Artifacts com retenção de 7 dias

#### 2. **Azure DevOps**
- Pipeline multi-stage em `azure-pipelines.yml`
- Suporte a environments (staging, production)
- Integração com Azure Container Registry
- Approvals para deploy em produção
- Relatórios de testes e cobertura

#### 3. **GitLab CI/CD**
- Pipeline de 5 stages em `.gitlab-ci.yml`
- GitLab Container Registry integrado
- Environments automáticos
- JUnit test reports
- Manual deploy com rollback

### Features Incluídas

| Feature | GitHub Actions | Azure DevOps | GitLab CI |
|---------|----------------|--------------|-----------|
| **Build** | ✅ | ✅ | ✅ |
| **Unit Tests** | ✅ | ✅ | ✅ |
| **Integration Tests** | ✅ | ✅ | ✅ |
| **Code Coverage** | ✅ Codecov | ✅ Built-in | ✅ Built-in |
| **Security Scan** | ✅ | ✅ | ✅ |
| **Docker Build** | ✅ | ✅ | ✅ |
| **Auto Deploy** | ✅ Manual | ✅ Approval | ✅ Manual |
| **Cache** | ✅ | ✅ | ✅ |

### Quick Start - GitHub Actions

**1. Nenhuma configuração necessária!** O arquivo já está pronto.

**2. Configure secrets** (Settings → Secrets):
```
```bash
DOCKER_USERNAME=seu-usuario
DOCKER_PASSWORD=seu-token
**3. Push para `main` ou `develop`** - Pipeline executa automaticamente!

**4. Ver resultados** na aba **Actions**.

### Quick Start - Azure DevOps

**1. Criar pipeline:**
- Pipelines → New pipeline
- Selecione seu repositório
- Use existing YAML: `azure-pipelines.yml`

**2. Criar service connection** para Docker Hub:
- Project Settings → Service connections
- New → Docker Registry
- Nome: `DockerHubConnection`

**3. Push para `main` ou `develop`** - Pipeline executa!

### Quick Start - GitLab CI

**1. Nenhuma configuração necessária!** O arquivo `.gitlab-ci.yml` já está pronto.

**2. Pipeline executa automaticamente** em todo push.

**3. Habilite Container Registry**:
- Settings → General → Container Registry → Enable

**4. Ver resultados** em CI/CD → Pipelines.

### Pipeline Stages

Todos os pipelines seguem este fluxo:

```
```bash
1. 🏗️  Build
   ├── Restore dependencies
   ├── Build solution
   └── Publish artifacts

2. 🧪 Test
   ├── Run unit tests
   ├── Run integration tests
   └── Generate coverage report

3. 📊 Quality
   ├── Code coverage analysis
   ├── Security vulnerability scan
   └── Check outdated packages

4. 🐳 Docker
   ├── Build Docker image
   ├── Tag with version/branch
   └── Push to registry

5. 🚀 Deploy (Manual)
   ├── Deploy to Staging
   └── Deploy to Production (with approval)
### Badges

Adicione ao seu README.md:

**GitHub Actions:**
![CI/CD](https://github.com/usuario/repo/actions/workflows/ci.yml/badge.svg)
**Azure DevOps:**
[![Build Status](https://dev.azure.com/org/project/_apis/build/status/pipeline)](https://dev.azure.com/org/project/_build)
**GitLab CI:**
[![pipeline status](https://gitlab.com/usuario/repo/badges/main/pipeline.svg)](https://gitlab.com/usuario/repo/-/commits/main)
[![coverage report](https://gitlab.com/usuario/repo/badges/main/coverage.svg)](https://gitlab.com/usuario/repo/-/commits/main)
### Personalização

**Alterar versão do .NET:**

# GitHub Actions
env:
  DOTNET_VERSION: '10.0.x'

# Azure DevOps
variables:
  dotnetVersion: '10.0.x'

# GitLab CI
image: mcr.microsoft.com/dotnet/sdk:10.0
**Deploy automático** (remover aprovação manual):

# GitHub Actions - remover condição
if: github.ref == 'refs/heads/main'

# Azure DevOps - remover condition
# condition: manual

# GitLab CI - remover when
# when: manual
### Logs e Resultados

Todos os pipelines geram:

- ✅ **Test Results**: TRX/JUnit format
- ✅ **Coverage Report**: Cobertura de código
- ✅ **Security Scan**: Vulnerabilidades encontradas
- ✅ **Build Artifacts**: DLLs e executáveis
- ✅ **Docker Images**: Imagens versionadas

### Troubleshooting

**Build falha:**
# Testar localmente primeiro
dotnet restore
dotnet build --configuration Release
dotnet test
**Docker build falha:**
- Verifique se Dockerfile está na raiz
- Verifique se serviços Docker estão ativos
- Para GitLab CI, use `docker:dind` service

**Secrets não funcionam:**
- Verifique se estão configurados corretamente
- Case-sensitive (diferenciam maiúsculas/minúsculas)
- GitLab: marque como "Protected" para branches protegidas

### Mais Informações

📖 **Documentação completa**: [docs/CICD.md](CICD.md)

**Tópicos detalhados:**
- Configuração step-by-step para cada plataforma
- Service connections e secrets
- Environments e approvals
- Personalização avançada
- Testes locais de pipelines
- Troubleshooting completo

---

## 📜 Event Sourcing

### O que é?

**Event Sourcing** é um padrão arquitetural onde o estado da aplicação é determinado por uma sequência de eventos imutáveis, ao invés de armazenar apenas o estado atual. Fornece auditoria completa, rastreabilidade e capacidade de "time travel".

### Quando Usar

| Cenário | Recomendação |
|---------|--------------|
| Sistemas financeiros | ✅ Altamente recomendado |
| E-commerce (pedidos, pagamentos) | ✅ Recomendado |
| Healthcare (prontuários) | ✅ Recomendado |

### Quick Start

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
```bash

docker-compose up -d postgres-events
**API de Auditoria:**
- `GET /api/audit/Order/123` - Histórico completo
- `GET /api/audit/Order/123/at/2026-01-11T12:00:00Z` - Time travel

### Mais Informações

📖 **[Documentação completa de Event Sourcing](EVENT-SOURCING.md)**

---

## 🔐 Authentication

### O que é?

Sistema completo de **autenticação e autorização** com JWT (JSON Web Tokens) e OAuth2. Fornece registro de usuários, login, refresh tokens, gerenciamento de perfil e integração com provedores externos (Google, Microsoft, GitHub).

### Quando Usar

| Cenário | Recomendação |
|---------|--------------|
| APIs públicas que precisam de autenticação | ✅ Essencial |
| Aplicações multi-usuário | ✅ Essencial |
| Sistemas com diferentes níveis de acesso | ✅ Recomendado |
| Integração com login social | ✅ Recomendado |

### Recursos

- **JWT Authentication** - Token-based authentication
- **Refresh Tokens** - Long-lived tokens for token renewal
- **OAuth2 Providers** - Google, Microsoft, GitHub
- **Password Policy** - Configurable requirements
- **Role-Based Authorization** - User roles and permissions
- **Token Revocation** - Logout and invalidate tokens
- **IP Tracking** - Security auditing

### Quick Start

**1. Habilitar no appsettings.json:**
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
**2. Criar migration:**
dotnet ef migrations add AddAuthentication --project src/Data --startup-project src/Api
dotnet ef database update --project src/Data --startup-project src/Api
**3. Testar no Swagger:**
dotnet run --project src/Api
# Acesse http://localhost:5000
# POST /api/auth/register - Registrar usuário
# POST /api/auth/login - Fazer login
# Use o botão "Authorize" no Swagger com: Bearer {token}
### Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/register` | Registrar novo usuário |
| POST | `/api/auth/login` | Login com username/email e senha |
| POST | `/api/auth/refresh-token` | Renovar access token |
| POST | `/api/auth/revoke-token` | Revogar refresh token (logout) |
| GET | `/api/auth/me` | Obter informações do usuário logado |
| POST | `/api/auth/change-password` | Alterar senha |
| PUT | `/api/auth/profile` | Atualizar perfil do usuário |
| POST | `/api/auth/oauth2/login` | Login com OAuth2 providers |

### Exemplo de Uso

// Register
var registerDto = new RegisterDto
{
    Username = "john.doe",
    Email = "john@example.com",
    Password = "P@ssw0rd123",
    FirstName = "John",
    LastName = "Doe"
};

var response = await authService.RegisterAsync(registerDto);
// response.AccessToken
// response.RefreshToken

// Login
var loginDto = new LoginDto
{
    UsernameOrEmail = "john.doe",
    Password = "P@ssw0rd123"
};

var authResponse = await authService.LoginAsync(loginDto, "127.0.0.1");

// Use token in API calls
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
### OAuth2 Providers

**Google:**
{
  "OAuth2Settings": {
    "GoogleOAuthSettings": {
      "Enabled": true,
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
**Microsoft:**
{
  "MicrosoftOAuthSettings": {
    "Enabled": true,
    "ClientId": "your-microsoft-client-id",
    "ClientSecret": "your-microsoft-client-secret",
    "TenantId": "common"
  }
}
**GitHub:**
{
  "GitHubOAuthSettings": {
    "Enabled": true,
    "ClientId": "your-github-client-id",
    "ClientSecret": "your-github-client-secret"
  }
}
### Password Policy

Configure requisitos de senha:
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

- ✅ Use HTTPS in production (`RequireHttpsMetadata = true`)
- ✅ Store JWT secret in environment variables or Key Vault
- ✅ Keep access tokens short-lived (15-60 minutes)
- ✅ Use refresh token rotation
- ✅ Store refresh tokens in HttpOnly cookies
- ✅ Implement rate limiting on auth endpoints
- ✅ Log authentication events for auditing
- ⚠️ **Production:** Replace SHA256 with BCrypt or Argon2

### Mais Informações

📖 **[Documentação completa de Authentication](AUTHENTICATION.md)**

---

**Navegação:**
- [⬆️ Voltar ao README](../README.md)
- [📖 Ver Índice](../INDEX.md)
- [🚀 Quick Start](../QUICK-START.md)

---

*Última atualização: Janeiro 2026 | Versão: 1.0.0*
