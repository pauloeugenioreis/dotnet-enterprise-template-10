# 🎛️ Guia de Recursos Avançados

Este guia explica como habilitar e configurar os recursos avançados incluídos no template.

---

## 📋 Índice

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
    "ConnectionStrings": {
      "MongoDB": "mongodb://username:password@localhost:27017/projecttemplate"
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
using MongoDB.Driver;

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

**4. Registre no Program.cs:**
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
```json
{
  "AppSettings": {
    "ConnectionStrings": {
      "RabbitMQ": "amqp://username:password@localhost:5672/"
    }
  }
}
```

**3. Adicione no Program.cs:**
```csharp
// Add RabbitMQ (OPTIONAL)
builder.Services.AddRabbitMq();
```

**4. Use no código:**
```csharp
public class OrderService
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
```json
{
  "AppSettings": {
    "ConnectionStrings": {
      "ServiceAccount": "{\"type\":\"service_account\",\"project_id\":\"your-project\",...}"
    },
    "Infrastructure": {
      "Storage": {
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

**4. Use no código:**
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
```

**5. Gere tokens:**
```csharp
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
```

**4. Acesse:**
```
GET /api/v1/products
GET /api/v2/products
GET /api/products?api-version=2.0
GET /api/products (Header: X-Api-Version: 2.0)
```

---

## 🚨 Global Exception Handler

### O que é?
Middleware que captura todas as exceções não tratadas e retorna respostas consistentes.

### Como Usar

**Já está ativo por padrão!** ✅

**Use exceções customizadas:**
```csharp
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
```

---

## ✅ Validation Filter

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

## 📊 Advanced Logging

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

**Para customizar, edite `SwaggerExtension.cs`:**

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

**1. Adicione no Api.csproj:**
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

**2. Documente seus controllers:**
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
```
http://localhost:5000/swagger
```

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
```csharp
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
```

**Registrar:**
```csharp
options.OperationFilter<AddCustomHeaderParameter>();
```

### Múltiplas Versões

**Para suportar v1 e v2:**
```csharp
options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "API v1" });
options.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "API v2" });

// UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
});
```

### Desabilitar em Produção

**Por segurança, Swagger já está desabilitado em produção:**
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCustomizedSwagger(); // Apenas em dev
}
```

**Para habilitar em produção (não recomendado):**
```csharp
// Sem verificação de ambiente
app.UseCustomizedSwagger();
```

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
```csharp
// Em GlobalExceptionHandler.cs
var notificationService = context.RequestServices.GetService<IExceptionNotificationService>();
if (notificationService != null)
{
    await notificationService.NotifyAsync(context, exception);
}
```

### Implementação Padrão

**O template inclui implementação básica que loga no console:**
```csharp
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
```

### Customizar para Email

**Criar implementação customizada:**
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
```

**Registrar no DependencyInjectionExtension.cs:**
```csharp
// Substituir implementação padrão
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
```

### Filtrar Exceções

**Notificar apenas erros críticos:**
```csharp
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
```

### Throttling (Limitar Notificações)

**Evitar spam com muitas notificações:**
```csharp
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
```

### Recomendações

- ✅ **Development**: Apenas logs no console
- ✅ **Staging**: Email para equipe de dev
- ✅ **Production**: Slack/Teams + sistema de tickets
- ✅ Use throttling para evitar spam
- ✅ Filtre exceções não críticas (400, 404, ValidationException)
- ❌ Nunca envie informações sensíveis nas notificações

---

**Navegação:**
- [⬆️ Voltar ao README](../README.md)
- [📖 Ver Índice](../INDEX.md)
- [🚀 Quick Start](../QUICK-START.md)

---

*Última atualização: Janeiro 2026 | Versão: 1.0.0*
