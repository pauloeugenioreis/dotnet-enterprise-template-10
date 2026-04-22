# 📝 Configuration Guide

Guia completo sobre como trabalhar com configurações no template.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [AppSettings Structure](#-appsettings-structure)
- [Como Usar Configurações](#como-usar-configuracoes)
- [IOptions Pattern](#-ioptions-pattern)
- [Best Practices](#best-practices)
- [Exemplos Práticos](#-exemplos-práticos)
- [Validação de Configurações](#-validação-de-configurações)
- [Sobrescrita de Configurações](#-sobrescrita-de-configurações)

---

## 🎯 Visão Geral

Este template usa o **IOptions Pattern** do ASP.NET Core para gerenciar configurações de forma tipada, validada e testável.

### Princípios Fundamentais

1. ✅ **AppSettingsExtension** é o ÚNICO lugar que lê `IConfiguration`
2. ✅ Todas as configurações estão em `AppSettings.cs` (Domain layer)
3. ✅ Use **IOptions<AppSettings>** para injetar configurações
4. ✅ Validação automática no startup via `ValidateOnStart()`
5. ✅ Configurações são imutáveis depois do startup

---

## 📂 AppSettings Structure

```csharp
// Domain/AppSettings.cs
public class AppSettings
{
    public string EnvironmentName { get; set; }
    public InfrastructureSettings Infrastructure { get; set; }
    public AuthenticationSettings Authentication { get; set; }

    public bool IsDevelopment() => EnvironmentName == "Development";
    public bool IsProduction() => EnvironmentName == "Production";
}
```

### Estrutura no appsettings.json

```json
{
  "AppSettings": {
    "EnvironmentName": "Development",
    "Authentication": {
      "Jwt": {
        "Secret": "my-secret-key",
        "Issuer": "my-issuer"
      }
    },
    "Infrastructure": {
      "Database": {
        "DatabaseType": "InMemory",
        "ConnectionString": ""
      }
    }
  }
}
```

---

<a id="como-usar-configuracoes"></a>

## ⚙️ Como Usar Configurações

Sempre injete `IOptions<AppSettings>` nas camadas da aplicação para obter acesso tipado às configurações.

### Controllers

```csharp
public class ProductController : ApiControllerBase
{
  private readonly AppSettings _settings;

  public ProductController(IOptions<AppSettings> settings)
  {
    _settings = settings.Value;
  }
}
```

### Services

```csharp
public class EmailService : IEmailService
{
  private readonly AppSettings _settings;

  public EmailService(IOptions<AppSettings> settings)
  {
    _settings = settings.Value;
  }

  public async Task SendEmail(string to, string subject)
  {
    var smtpServer = _settings.Infrastructure.Email.SmtpServer;
  }
}
```

### Repositories

```csharp
public class ProductRepository : Repository<Product>
{
  private readonly AppSettings _settings;

  public ProductRepository(
    DbContext context,
    IOptions<AppSettings> settings) : base(context)
  {
    _settings = settings.Value;
  }
}
```

### Background Services

```csharp
public class CleanupBackgroundService : BackgroundService
{
  private readonly AppSettings _settings;

  public CleanupBackgroundService(IOptions<AppSettings> settings)
  {
    _settings = settings.Value;
  }
}
```

---

## 🔧 IOptions Pattern

Escolha a implementação adequada conforme o ciclo de vida da dependência.

### 1. IOptions<AppSettings>

```csharp
public class MyService
{
    private readonly AppSettings _settings;

    public MyService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }
}
```

**Vantagens:**

- Simples e performático
- Ideal para serviços Singleton ou Scoped

### 2. IOptionsSnapshot<AppSettings>

```csharp
public class MyController : ControllerBase
{
    private readonly AppSettings _settings;

    public MyController(IOptionsSnapshot<AppSettings> options)
    {
        _settings = options.Value;
    }
}
```

**Vantagens:**

- Recarrega configurações em cada request (quando habilitado)
- Ideal para serviços Scoped

### 3. AppSettings Diretamente (Singleton)

```csharp
public class MySingletonService
{
    private readonly AppSettings _settings;

    public MySingletonService(AppSettings settings)
    {
        _settings = settings;
    }
}
```

**Vantagens:**

- Mais simples
- Registrado como Singleton em `AppSettingsExtension`

---

<a id="best-practices"></a>

## 📌 Best Practices

### ✅ DO's

```csharp
// ✅ Injete IOptions<AppSettings> em Controllers
public class ProductController : ApiControllerBase
{
    private readonly AppSettings _settings;

    public ProductController(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }
}

// ✅ Injete IOptions<AppSettings> em Services
public class EmailService : IEmailService
{
    private readonly AppSettings _settings;

    public EmailService(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmail(string to, string subject)
    {
        var smtpServer = _settings.Infrastructure.Email.SmtpServer;
        // ...
    }
}

// ✅ Injete IOptions<AppSettings> em Repositories
public class ProductRepository : Repository<Product>
{
    private readonly AppSettings _settings;

    public ProductRepository(
        DbContext context,
        IOptions<AppSettings> settings) : base(context)
    {
        _settings = settings.Value;
    }
}

// ✅ Injete IOptions<AppSettings> em Background Services
public class CleanupBackgroundService : BackgroundService
{
    private readonly AppSettings _settings;

    public CleanupBackgroundService(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }
}
```

### ❌ DON'Ts

```csharp
// ❌ NUNCA use IConfiguration diretamente
public class MyService
{
    public MyService(IConfiguration configuration) { } // ❌
}

// ❌ NUNCA leia manualmente do JSON
public class MyService
{
    public MyService(IConfiguration config)
    {
        var value = config.GetSection("AppSettings:Infrastructure:Telemetry:Enabled"); // ❌
    }
}

// ❌ NUNCA use BuildServiceProvider em services
public class MyService
{
    public MyService(IServiceCollection services)
    {
        var provider = services.BuildServiceProvider(); // ❌ MEMORY LEAK!
    }
}
```

---

## 🎓 Exemplos Práticos

### Exemplo 1: Controller com Configurações

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ApiControllerBase
{
  private readonly IProductService _productService;
  private readonly AppSettings _settings;
  private readonly ILogger<ProductController> _logger;

  public ProductController(
    IProductService productService,
    IOptions<AppSettings> settings,
    ILogger<ProductController> logger)
  {
    _productService = productService;
    _settings = settings.Value;
    _logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {


    var products = await _productService.GetAllAsync();
    return Ok(products);
  }
}
```

### Exemplo 2: Service com Configurações

```csharp
public class ProductService : IProductService
{
  private readonly IRepository<Product> _repository;
  private readonly AppSettings _settings;
  private readonly ILogger<ProductService> _logger;

  public ProductService(
    IRepository<Product> repository,
    IOptions<AppSettings> settings,
    ILogger<ProductService> logger)
  {
    _repository = repository;
    _settings = settings.Value;
    _logger = logger;
  }

  public async Task<Product> CreateAsync(Product product)
  {
    if (_settings.Infrastructure.EventSourcing.Enabled)
    {
      await SaveProductCreatedEvent(product);
    }

    return await _repository.AddAsync(product);
  }

  private async Task SaveProductCreatedEvent(Product product)
  {
    var eventProvider = _settings.Infrastructure.EventSourcing.Provider;
    _logger.LogInformation("Saving event to {Provider}", eventProvider);
  }
}
```

### Exemplo 3: Repository Customizado

```csharp
public class ProductDapperRepository : IRepository<Product>
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly AppSettings _settings;
  private readonly ILogger<ProductDapperRepository> _logger;

  public ProductDapperRepository(
    IDbConnectionFactory connectionFactory,
    IOptions<AppSettings> settings,
    ILogger<ProductDapperRepository> logger)
  {
    _connectionFactory = connectionFactory;
    _settings = settings.Value;
    _logger = logger;
  }

  public async Task<IEnumerable<Product>> GetAllAsync()
  {
    using var connection = _connectionFactory.CreateConnection();
    var commandTimeout = _settings.Infrastructure.Database.CommandTimeoutSeconds;

    return await connection.QueryAsync<Product>(
      "SELECT * FROM Products",
      commandTimeout: commandTimeout);
  }
}
```

### Exemplo 4: Background Service

```csharp
public class DataCleanupService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly AppSettings _settings;
  private readonly ILogger<DataCleanupService> _logger;

  public DataCleanupService(
    IServiceProvider serviceProvider,
    IOptions<AppSettings> settings,
    ILogger<DataCleanupService> logger)
  {
    _serviceProvider = serviceProvider;
    _settings = settings.Value;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        _logger.LogInformation("Running cleanup job...");

        if (_settings.IsProduction())
        {
          using var scope = _serviceProvider.CreateScope();
          var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();

          await CleanupOldData(repository);
        }

        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during cleanup");
      }
    }
  }
}
```

### Exemplo 5: Middleware Customizado

```csharp
public class CustomHeaderMiddleware
{
  private readonly RequestDelegate _next;
  private readonly AppSettings _settings;
  private readonly ILogger<CustomHeaderMiddleware> _logger;

  public CustomHeaderMiddleware(
    RequestDelegate next,
    IOptions<AppSettings> settings,
    ILogger<CustomHeaderMiddleware> logger)
  {
    _next = next;
    _settings = settings.Value;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    if (_settings.IsDevelopment())
    {
      context.Response.Headers["X-Environment"] = "Development";
      context.Response.Headers["X-Debug-Mode"] = "true";
    }

    await _next(context);
  }
}
```

---

## ✅ Validação de Configurações

### Validação Automática no Startup

O template usa `IValidateOptions<T>` para validar configurações no startup:

```csharp
// Infrastructure/Configuration/AuthenticationSettingsValidator.cs
public class AuthenticationSettingsValidator : IValidateOptions<AppSettings>
{
  public ValidateOptionsResult Validate(string? name, AppSettings options)
  {
    var authSettings = options.Authentication;

    if (!authSettings.Enabled)
      return ValidateOptionsResult.Success;

    if (string.IsNullOrWhiteSpace(authSettings.Jwt.Secret))
    {
      return ValidateOptionsResult.Fail(
        "JWT Secret is required");
    }

    if (authSettings.Jwt.Secret.Length < 32)
    {
      return ValidateOptionsResult.Fail(
        "JWT Secret must be at least 32 characters");
    }

    return ValidateOptionsResult.Success;
  }
}
```

### Registro do Validador

```csharp
// No seu extension method
services.AddSingleton<IValidateOptions<AppSettings>, AuthenticationSettingsValidator>();
```

---

## 🌍 Sobrescrita de Configurações

O ASP.NET Core permite sobrescrever configurações do `appsettings.json` usando **variáveis de ambiente**. Isso é essencial para deployment em diferentes ambientes (Docker, Kubernetes, Cloud).

### Padrão de Nomenclatura

Use **dois underscores** (`__`) para navegar na hierarquia do JSON:

```text
AppSettings__Propriedade__SubPropriedade
```

### Estrutura de Configuração

```json
{
  "AppSettings": {
    "EnvironmentName": "Development",
    "Authentication": {
      "Jwt": {
        "Secret": "my-secret-key",
        "Issuer": "my-issuer"
      }
    },
    "Infrastructure": {
      "Database": {
        "DatabaseType": "InMemory",
        "ConnectionString": ""
      },
      "MongoDB": {
        "ConnectionString": ""
      },
      "RabbitMQ": {
        "ConnectionString": ""
      },
      "Storage": {
        "Provider": "Google",
        "DefaultBucket": "",
        "Google": {
          "ServiceAccount": "",
          "ProjectId": ""
        },
        "Azure": {
          "ConnectionString": "",
          "BlobServiceUri": "",
          "ManagedIdentityClientId": ""
        },
        "Aws": {
          "AccessKeyId": "",
          "SecretAccessKey": "",
          "Region": "us-east-1",
          "Profile": "",
          "ServiceUrl": ""
        }
      }
    }
  }
}
```

### Exemplos de Variáveis de Ambiente

```bash
# Ambiente
AppSettings__EnvironmentName=Production

# Database Connection
AppSettings__Infrastructure__Database__DatabaseType=SqlServer
AppSettings__Infrastructure__Database__ConnectionString="Server=prod-server;Database=MyDb;..."

# JWT Settings
AppSettings__Authentication__Jwt__Secret=super-secret-production-key-with-at-least-32-chars
AppSettings__Authentication__Jwt__Issuer=https://api.production.com
AppSettings__Authentication__Jwt__Audience=https://app.production.com
AppSettings__Authentication__Jwt__ExpirationMinutes=60


# Database Settings
AppSettings__Infrastructure__Database__Provider=SqlServer
AppSettings__Infrastructure__Database__CommandTimeoutSeconds=60

# Telemetry
AppSettings__Infrastructure__Telemetry__Enabled=true
AppSettings__Infrastructure__Telemetry__Provider=ApplicationInsights
AppSettings__Infrastructure__Telemetry__ApplicationInsights__ConnectionString="InstrumentationKey=xxx"

# Storage Provider (choose one)
AppSettings__Infrastructure__Storage__Provider=Google
AppSettings__Infrastructure__Storage__Google__ServiceAccount="{\"type\":\"service_account\",...}"

# Azure example
# AppSettings__Infrastructure__Storage__Provider=Azure
# AppSettings__Infrastructure__Storage__Azure__ConnectionString="DefaultEndpointsProtocol=https;AccountName=..."

# AWS example
# AppSettings__Infrastructure__Storage__Provider=Aws
# AppSettings__Infrastructure__Storage__Aws__Region=us-east-1
# AppSettings__Infrastructure__Storage__Aws__AccessKeyId=AKIAXXXXX
# AppSettings__Infrastructure__Storage__Aws__SecretAccessKey=YOURSECRET
```

---

## 🐳 Docker Compose

### docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    image: myapi:latest
    ports:
      - "5000:8080"
    environment:
      - AppSettings__EnvironmentName=Production
      - AppSettings__Infrastructure__Database__DatabaseType=SqlServer
      - AppSettings__Infrastructure__Database__ConnectionString=Server=sqlserver;Database=MyDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true
      - AppSettings__Authentication__Jwt__Secret=my-super-secret-key-for-production-with-64-characters-minimum
      - AppSettings__Authentication__Jwt__Issuer=https://api.mycompany.com
      - AppSettings__Authentication__Jwt__Audience=https://app.mycompany.com
      - AppSettings__Authentication__Jwt__ExpirationMinutes=120
      - AppSettings__Infrastructure__Telemetry__Enabled=true
      - AppSettings__Infrastructure__Telemetry__Provider=Console
    depends_on:
      - sqlserver
      - redis

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
```

### Usando arquivo .env

Crie um arquivo `.env` na raiz do projeto:

```env
# .env
ENVIRONMENT_NAME=Production
JWT_SECRET=my-super-secret-key-for-production-with-64-characters-minimum
JWT_ISSUER=https://api.mycompany.com
SQL_CONNECTION=Server=sqlserver;Database=MyDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true
```

No `docker-compose.yml`:

```yaml
version: '3.8'

services:
  api:
    image: myapi:latest
    env_file:
      - .env
    environment:
      - AppSettings__EnvironmentName=${ENVIRONMENT_NAME}
      - AppSettings__Authentication__Jwt__Secret=${JWT_SECRET}
      - AppSettings__Authentication__Jwt__Issuer=${JWT_ISSUER}

      - AppSettings__Infrastructure__Database__ConnectionString=${SQL_CONNECTION}
```

---

## ☸️ Kubernetes

### ConfigMap para configurações não-sensíveis

```yaml
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: api-config
  namespace: default
data:
  AppSettings__EnvironmentName: "Production"
  AppSettings__Authentication__Jwt__Issuer: "https://api.mycompany.com"
  AppSettings__Authentication__Jwt__Audience: "https://app.mycompany.com"
  AppSettings__Authentication__Jwt__ExpirationMinutes: "120"
  AppSettings__Infrastructure__Database__Provider: "SqlServer"
  AppSettings__Infrastructure__Database__CommandTimeoutSeconds: "60"
  AppSettings__Infrastructure__Telemetry__Enabled: "true"
  AppSettings__Infrastructure__Telemetry__Provider: "ApplicationInsights"
```

### Secret para configurações sensíveis

```yaml
# secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: api-secrets
  namespace: default
type: Opaque
stringData:
  AppSettings__Authentication__Jwt__Secret: "my-super-secret-key-for-production-with-64-characters-minimum"
  AppSettings__Infrastructure__Database__ConnectionString: "Server=prod-sql.database.windows.net;Database=MyDb;User Id=admin;Password=SecurePass123!"

  AppSettings__Infrastructure__Telemetry__ApplicationInsights__ConnectionString: "InstrumentationKey=xxxxx-xxxx-xxxx-xxxx-xxxxx"
  AppSettings__Infrastructure__MongoDB__ConnectionString: "mongodb://username:password@prod-mongodb:27017/mydb"
  AppSettings__Infrastructure__RabbitMQ__ConnectionString: "amqp://username:password@prod-rabbitmq:5672/"
  AppSettings__Infrastructure__Storage__Google__ServiceAccount: "base64-encoded-service-account-json"
  AppSettings__Infrastructure__Storage__Azure__ConnectionString: "DefaultEndpointsProtocol=https;AccountName=..."
  AppSettings__Infrastructure__Storage__Aws__AccessKeyId: "AKIAXXXXX"
  AppSettings__Infrastructure__Storage__Aws__SecretAccessKey: "YOURSECRET"
```

### Deployment usando ConfigMap e Secret

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-deployment
  namespace: default
spec:
  replicas: 3
  selector:
    matchLabels:
      app: api
  template:
    metadata:
      labels:
        app: api
    spec:
      containers:
      - name: api
        image: myregistry.azurecr.io/myapi:latest
        ports:
        - containerPort: 8080
        envFrom:
        - configMapRef:
            name: api-config
        - secretRef:
            name: api-secrets
        env:
        - name: AppSettings__EnvironmentName
          valueFrom:
            configMapKeyRef:
              name: api-config
              key: AppSettings__EnvironmentName
        - name: AppSettings__Authentication__Jwt__Secret
          valueFrom:
            secretKeyRef:
              name: api-secrets
              key: AppSettings__Authentication__Jwt__Secret
        - name: AppSettings__Infrastructure__Database__ConnectionString
          valueFrom:
            secretKeyRef:
              name: api-secrets
              key: AppSettings__Infrastructure__Database__ConnectionString
```

### Aplicar no Kubernetes

```bash
# Criar ConfigMap
kubectl apply -f configmap.yaml

# Criar Secret
kubectl apply -f secret.yaml

# Criar Deployment
kubectl apply -f deployment.yaml

# Verificar
kubectl get pods
kubectl logs <pod-name>
```

---

## ☁️ Azure App Service

### Configurar no Portal Azure

1. **Portal Azure** → **App Service** → Sua aplicação
2. **Configuration** → **Application settings**
3. **+ New application setting**

```text
Name: AppSettings__Authentication__Jwt__Secret
Value: my-super-secret-key-for-production-with-at-least-32-characters-minimum
```

### Configurar via Azure CLI

```bash
# Login
az login

# Definir variáveis
az webapp config appsettings set \
  --resource-group MyResourceGroup \
  --name MyApiApp \
  --settings \
    "AppSettings__EnvironmentName=Production" \
    "AppSettings__Authentication__Jwt__Secret=my-super-secret-key" \
    "AppSettings__Infrastructure__Database__ConnectionString=Server=xxx"

# Listar configurações
az webapp config appsettings list \
  --resource-group MyResourceGroup \
  --name MyApiApp
```

### Key Vault Integration

```bash
# Referenciar secret do Key Vault
az webapp config appsettings set \
  --resource-group MyResourceGroup \
  --name MyApiApp \
  --settings \
    "AppSettings__Authentication__Jwt__Secret=@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/JwtSecret/)"
```

---

## ☁️ AWS Elastic Beanstalk

### .ebextensions/environment.config

```yaml
option_settings:
  aws:elasticbeanstalk:application:environment:
    AppSettings__EnvironmentName: "Production"
    AppSettings__Authentication__Jwt__Secret: "my-super-secret-key-for-production"
    AppSettings__Authentication__Jwt__Issuer: "https://api.mycompany.com"

```

### AWS CLI

```bash
# Definir variáveis
aws elasticbeanstalk update-environment \
  --environment-name my-api-env \
  --option-settings \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=AppSettings__EnvironmentName,Value=Production \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=AppSettings__Authentication__Jwt__Secret,Value=my-secret
```

---

## 🔐 Ordem de Precedência

O ASP.NET Core carrega configurações na seguinte ordem (última sobrescreve primeira):

1. **appsettings.json**
2. **appsettings.{Environment}.json** (ex: `appsettings.Production.json`)
3. **User Secrets** (apenas Development)
4. **Variáveis de Ambiente**
5. **Command Line Arguments**

### Exemplo Prático

```json
{
  "AppSettings": {
    "Authentication": {
      "Jwt": {
        "Secret": "development-secret"
      }
    }
  }
}
```

```bash
AppSettings__Authentication__Jwt__Secret=production-secret
```

**Resultado:** Aplicação usa `"production-secret"` 🎯

---

## 🧪 Testando Localmente

### Windows PowerShell

```powershell
# Definir variável temporária
$env:AppSettings__Authentication__Jwt__Secret="test-secret-key-with-at-least-32-characters"
$env:AppSettings__EnvironmentName="Development"

# Executar aplicação
dotnet run --project src/Api/Api.csproj

# Limpar após teste
Remove-Item Env:AppSettings__Authentication__Jwt__Secret
```

### Linux / macOS

```bash
# Definir variável temporária
export AppSettings__Authentication__Jwt__Secret="test-secret-key-with-at-least-32-characters"
export AppSettings__EnvironmentName="Development"

# Executar aplicação
dotnet run --project src/Api/Api.csproj

# Limpar após teste
unset AppSettings__Authentication__Jwt__Secret
```

### Visual Studio / VS Code

Crie `launchSettings.json`:

```json
{
  "profiles": {
    "Api": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AppSettings__Authentication__Jwt__Secret": "test-secret-key-with-at-least-32-characters",

      }
    }
  }
}
```

---

## 📋 Checklist de Deployment

Antes de fazer deploy, verifique:

- ✅ **Secrets** não estão no código fonte
- ✅ **Connection Strings** corretas para o ambiente
- ✅ **JWT Secret** tem pelo menos 32 caracteres
- ✅ **Issuer/Audience** apontam para URLs corretas

- ✅ **Telemetry** habilitado para produção
- ✅ **EnvironmentName** correto (Production/Staging)
- ✅ **Variáveis de ambiente** testadas localmente
- ✅ **Logs** configurados corretamente
- ✅ **Health Checks** funcionando

---

## 🔍 Troubleshooting

### Problema: Configuração retorna null

**Causa:** Nome da propriedade no JSON não corresponde à classe C#

```text
// ❌ Errado
{
  "AppSettings": {
    "Infrastructure": { ... }
  }
}

// ✅ Correto
{
  "AppSettings": {
    "Infrastructure": { ... }
  }
}
```

### Problema: Erro "Cannot resolve IOptions<AppSettings>"

**Causa:** Esqueceu de chamar `AddAppSettingsConfiguration`

```csharp
// Program.cs ou InfrastructureExtensions.cs
services.AddAppSettingsConfiguration(configuration, environment); // ✅ Necessário
```

### Problema: Validação não acontece

**Causa:** Faltou registrar o validador ou chamar `.ValidateOnStart()`

```csharp
services.AddOptions<AppSettings>()
    .Bind(configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart(); // ✅ Necessário para validação no startup
```

---

## 📚 Referências

- [Options Pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Options validation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options#options-validation)

---

## 🎯 Resumo

### Regra de Ouro

> **Sempre que precisar de configurações em qualquer lugar do código (Controller, Service, Repository, Middleware, Background Service), injete `IOptions<AppSettings>`**

```csharp
// ✅ Sempre faça assim
public MyClass(IOptions<AppSettings> settings)
{
  var config = settings.Value;
}

// ❌ Nunca faça assim
public MyClass(IConfiguration configuration)
{
  var config = configuration.GetSection("AppSettings");
}
```

### Checklist

- ✅ Usar `IOptions<AppSettings>` em todos os lugares
- ✅ Nunca usar `IConfiguration` diretamente
- ✅ Criar validadores com `IValidateOptions<T>`
- ✅ Registrar validadores no DI
- ✅ Testar configurações no startup
- ✅ Usar nomes consistentes no JSON e C#
