# 📝 Configuration Guide

Guia completo sobre como trabalhar com configurações no template.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [AppSettings Structure](#-appsettings-structure)
- [Como Usar Configurações](#-como-usar-configurações)
- [IOptions Pattern](#-ioptions-pattern)
- [Best Practices](#-best-practices)
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

// Domain/AppSettings.cs
public class AppSettings
{
    public string EnvironmentName { get; set; }
    public InfrastructureSettings Infrastructure { get; set; }
    public AuthenticationSettings Authentication { get; set; }
    
    // Helper methods
    public bool IsDevelopment() => EnvironmentName == "Development";
    public bool IsProduction() => EnvironmentName == "Production";
}
### Estrutura no appsettings.json

{
  "AppSettings": {
    "EnvironmentName": "Development",
    "Infrastructure": {
      "Cache": { ... },
      "Database": {
        "DatabaseType": "SqlServer",
        "ConnectionString": "Server=localhost;...",
        "ReadOnlyConnectionString": ""
      },
      "MongoDB": { "ConnectionString": "" },
      "RabbitMQ": { "ConnectionString": "" },
      "Storage": { "ServiceAccount": "", "DefaultBucket": "" },
      "Telemetry": { ... },
      "RateLimiting": { ... },
      "EventSourcing": { ... }
    },
    "Authentication": {
      "Enabled": true,
      "Jwt": { ... }
    }
  }
}
---

## 💡 Como Usar Configurações

### ⚠️ IMPORTANTE: Use IOptions<AppSettings>

**❌ NUNCA faça isso:**

public class ProductService
{
    private readonly IConfiguration _configuration;
    
    public ProductService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // ❌ MAU: Leitura manual de configuração
        var cacheEnabled = _configuration.GetValue<bool>("AppSettings:Infrastructure:Cache:Enabled");
    }
}
**✅ SEMPRE faça isso:**

public class ProductService
{
    private readonly AppSettings _appSettings;
    
    public ProductService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
        
        // ✅ BOM: Acesso tipado e validado
        var cacheEnabled = _appSettings.Infrastructure.Cache.Enabled;
    }
}
---

## 🔧 IOptions Pattern

### Três formas de injetar configurações

#### 1. IOptions<T> (Recomendado)

public class MyService
{
    private readonly AppSettings _settings;
    
    public MyService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }
}
✅ **Vantagens:**
- Lazy loading (só carrega quando acessar `.Value`)
- Ideal para Singleton services
- Melhor performance

#### 2. IOptionsSnapshot<T> (Para Scoped Services)

public class MyController : ControllerBase
{
    private readonly AppSettings _settings;
    
    public MyController(IOptionsSnapshot<AppSettings> options)
    {
        _settings = options.Value;
    }
}
✅ **Vantagens:**
- Recarrega configurações a cada request (se configurado)
- Ideal para Scoped services
- Suporta reload de configuração em runtime

#### 3. AppSettings Diretamente (Singleton)

public class MyService
{
    private readonly AppSettings _settings;
    
    public MyService(AppSettings settings)
    {
        _settings = settings;
    }
}
✅ **Vantagens:**
- Mais simples
- Registrado como Singleton em `AppSettingsExtension`

---

## 📌 Best Practices

### ✅ DO's

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
### ❌ DON'Ts

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
        var value = config.GetSection("AppSettings:Infrastructure:Cache:Enabled"); // ❌
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
---

## 🎓 Exemplos Práticos

### Exemplo 1: Controller com Configurações

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
        // Usar configuração para decisões de negócio
        if (_settings.Infrastructure.Cache.Enabled)
        {
            _logger.LogInformation("Cache is enabled, checking cache first");
            // Lógica com cache
        }

        var products = await _productService.GetAllAsync();
        return Ok(products);
    }
}
### Exemplo 2: Service com Configurações

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
        // Usar configuração de Event Sourcing
        if (_settings.Infrastructure.EventSourcing.Enabled)
        {
            // Salvar evento
            await SaveProductCreatedEvent(product);
        }

        return await _repository.AddAsync(product);
    }

    private async Task SaveProductCreatedEvent(Product product)
    {
        var eventProvider = _settings.Infrastructure.EventSourcing.Provider;
        _logger.LogInformation("Saving event to {Provider}", eventProvider);
        
        // Lógica de event sourcing
    }
}
### Exemplo 3: Repository Customizado

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
        
        // Usar timeout da configuração
        var commandTimeout = _settings.Infrastructure.Database.CommandTimeoutSeconds;
        
        return await connection.QueryAsync<Product>(
            "SELECT * FROM Products",
            commandTimeout: commandTimeout);
    }
}
### Exemplo 4: Background Service

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
                
                // Verificar se está em produção
                if (_settings.IsProduction())
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
                    
                    // Limpeza de dados
                    await CleanupOldData(repository);
                }
                
                // Aguardar intervalo configurado
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }
        }
    }
}
### Exemplo 5: Middleware Customizado

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
        // Adicionar header customizado se em development
        if (_settings.IsDevelopment())
        {
            context.Response.Headers["X-Environment"] = "Development";
            context.Response.Headers["X-Debug-Mode"] = "true";
        }

        await _next(context);
    }
}
---

## ✅ Validação de Configurações

### Validação Automática no Startup

O template usa `IValidateOptions<T>` para validar configurações no startup:

// Infrastructure/Configuration/AuthenticationSettingsValidator.cs
public class AuthenticationSettingsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        var authSettings = options.Authentication;

        if (!authSettings.Enabled)
            return ValidateOptionsResult.Success;

        // Validar JWT Secret
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
### Registro do Validador

// No seu extension method
services.AddSingleton<IValidateOptions<AppSettings>, AuthenticationSettingsValidator>();
### Criando Seu Próprio Validador

public class CacheSettingsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        var cacheSettings = options.Infrastructure.Cache;

        if (!cacheSettings.Enabled)
            return ValidateOptionsResult.Success;

        // Validar Redis
        if (cacheSettings.Provider == "Redis" && 
            string.IsNullOrWhiteSpace(cacheSettings.Redis?.ConnectionString))
        {
            return ValidateOptionsResult.Fail(
                "Redis connection string is required when Redis provider is selected");
        }

        return ValidateOptionsResult.Success;
    }
}
---

## 🌍 Sobrescrita de Configurações

O ASP.NET Core permite sobrescrever configurações do `appsettings.json` usando **variáveis de ambiente**. Isso é essencial para deployment em diferentes ambientes (Docker, Kubernetes, Cloud).

### Padrão de Nomenclatura

Use **dois underscores** (`__`) para navegar na hierarquia do JSON:

```
AppSettings__Propriedade__SubPropriedade
### Estrutura de Configuração

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
        "ServiceAccount": "",
        "DefaultBucket": ""
      },
      "Cache": {
        "Enabled": true,
        "Provider": "Memory"
      }
    }
  }
}
### Exemplos de Variáveis de Ambiente

# Ambiente
AppSettings__EnvironmentName=Production

# Database Connection
AppSettings__Infrastructure__Database__DatabaseType=SqlServer
AppSettings__Infrastructure__Database__ConnectionString=Server=prod-server;Database=MyDb;...

# JWT Settings
AppSettings__Authentication__Jwt__Secret=super-secret-production-key-with-at-least-32-chars
AppSettings__Authentication__Jwt__Issuer=https://api.production.com
AppSettings__Authentication__Jwt__Audience=https://app.production.com
AppSettings__Authentication__Jwt__ExpirationMinutes=60

# Cache Settings
AppSettings__Infrastructure__Cache__Enabled=true
AppSettings__Infrastructure__Cache__Provider=Redis
AppSettings__Infrastructure__Cache__Redis__ConnectionString=redis-prod.cache.windows.net:6380,ssl=true

# Database Settings
AppSettings__Infrastructure__Database__Provider=SqlServer
AppSettings__Infrastructure__Database__CommandTimeoutSeconds=60

# Telemetry
AppSettings__Infrastructure__Telemetry__Enabled=true
AppSettings__Infrastructure__Telemetry__Provider=ApplicationInsights
AppSettings__Infrastructure__Telemetry__ApplicationInsights__ConnectionString=InstrumentationKey=xxx
---

## 🐳 Docker Compose

### docker-compose.yml

version: '3.8'

services:
  api:
    image: myapi:latest
    ports:
      - "5000:8080"
    environment:
      # Ambiente
      - AppSettings__EnvironmentName=Production
      
      # Database
      - AppSettings__Infrastructure__Database__DatabaseType=SqlServer
      - AppSettings__Infrastructure__Database__ConnectionString=Server=sqlserver;Database=MyDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true
      
      # JWT
      - AppSettings__Authentication__Jwt__Secret=my-super-secret-key-for-production-with-64-characters-minimum
      - AppSettings__Authentication__Jwt__Issuer=https://api.mycompany.com
      - AppSettings__Authentication__Jwt__Audience=https://app.mycompany.com
      - AppSettings__Authentication__Jwt__ExpirationMinutes=120
      
      # Cache Redis
      - AppSettings__Infrastructure__Cache__Enabled=true
      - AppSettings__Infrastructure__Cache__Provider=Redis
      - AppSettings__Infrastructure__Cache__Redis__ConnectionString=redis:6379
      
      # Telemetry
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
### Usando arquivo .env

Crie um arquivo `.env` na raiz do projeto:

# .env
ENVIRONMENT_NAME=Production
JWT_SECRET=my-super-secret-key-for-production-with-64-characters-minimum
JWT_ISSUER=https://api.mycompany.com
REDIS_CONNECTION=redis:6379
SQL_CONNECTION=Server=sqlserver;Database=MyDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true
No `docker-compose.yml`:

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
      - AppSettings__Infrastructure__Cache__Redis__ConnectionString=${REDIS_CONNECTION}
      - AppSettings__Infrastructure__Database__ConnectionString=${SQL_CONNECTION}
---

## ☸️ Kubernetes

### ConfigMap para configurações não-sensíveis

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
  AppSettings__Infrastructure__Cache__Enabled: "true"
  AppSettings__Infrastructure__Cache__Provider: "Redis"
  AppSettings__Infrastructure__Database__Provider: "SqlServer"
  AppSettings__Infrastructure__Database__CommandTimeoutSeconds: "60"
  AppSettings__Infrastructure__Telemetry__Enabled: "true"
  AppSettings__Infrastructure__Telemetry__Provider: "ApplicationInsights"
### Secret para configurações sensíveis

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
  AppSettings__Infrastructure__Cache__Redis__ConnectionString: "prod-redis.redis.cache.windows.net:6380,password=xxx,ssl=True"
  AppSettings__Infrastructure__Telemetry__ApplicationInsights__ConnectionString: "InstrumentationKey=xxxxx-xxxx-xxxx-xxxx-xxxxx"
  AppSettings__Infrastructure__MongoDB__ConnectionString: "mongodb://username:password@prod-mongodb:27017/mydb"
  AppSettings__Infrastructure__RabbitMQ__ConnectionString: "amqp://username:password@prod-rabbitmq:5672/"
  AppSettings__Infrastructure__Storage__ServiceAccount: "base64-encoded-service-account-json"
### Deployment usando ConfigMap e Secret

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
        
        # Injetar variáveis do ConfigMap
        envFrom:
        - configMapRef:
            name: api-config
        - secretRef:
            name: api-secrets
        
        # OU variáveis individuais
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
### Aplicar no Kubernetes

# Criar ConfigMap
kubectl apply -f configmap.yaml

# Criar Secret
kubectl apply -f secret.yaml

# Criar Deployment
kubectl apply -f deployment.yaml

# Verificar
kubectl get pods
kubectl logs <pod-name>
---

## ☁️ Azure App Service

### Configurar no Portal Azure

1. **Portal Azure** → **App Service** → Sua aplicação
2. **Configuration** → **Application settings**
3. **+ New application setting**

```
Name: AppSettings__Authentication__Jwt__Secret
Value: my-super-secret-key-for-production-with-64-characters-minimum
### Configurar via Azure CLI

# Login
az login

# Definir variáveis
az webapp config appsettings set \
  --resource-group MyResourceGroup \
  --name MyApiApp \
  --settings \
    "AppSettings__EnvironmentName=Production" \
    "AppSettings__Authentication__Jwt__Secret=my-super-secret-key" \
    "AppSettings__Infrastructure__Database__ConnectionString=Server=xxx" \
    "AppSettings__Infrastructure__Cache__Provider=Redis"

# Listar configurações
az webapp config appsettings list \
  --resource-group MyResourceGroup \
  --name MyApiApp
### Key Vault Integration

# Referenciar secret do Key Vault
az webapp config appsettings set \
  --resource-group MyResourceGroup \
  --name MyApiApp \
  --settings \
    "AppSettings__Authentication__Jwt__Secret=@Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/JwtSecret/)"
---

## ☁️ AWS Elastic Beanstalk

### .ebextensions/environment.config

option_settings:
  aws:elasticbeanstalk:application:environment:
    AppSettings__EnvironmentName: "Production"
    AppSettings__Authentication__Jwt__Secret: "my-super-secret-key-for-production"
    AppSettings__Authentication__Jwt__Issuer: "https://api.mycompany.com"
    AppSettings__Infrastructure__Cache__Provider: "Redis"
    AppSettings__Infrastructure__Cache__Redis__ConnectionString: "prod-redis.cache.amazonaws.com:6379"
### AWS CLI

# Definir variáveis
aws elasticbeanstalk update-environment \
  --environment-name my-api-env \
  --option-settings \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=AppSettings__EnvironmentName,Value=Production \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=AppSettings__Authentication__Jwt__Secret,Value=my-secret
---

## 🔐 Ordem de Precedência

O ASP.NET Core carrega configurações na seguinte ordem (última sobrescreve primeira):

1. **appsettings.json**
2. **appsettings.{Environment}.json** (ex: `appsettings.Production.json`)
3. **User Secrets** (apenas Development)
4. **Variáveis de Ambiente**
5. **Command Line Arguments**

### Exemplo Prático

// appsettings.json
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

# Variável de Ambiente sobrescreve
AppSettings__Authentication__Jwt__Secret=production-secret
**Resultado:** Aplicação usa `"production-secret"` 🎯

---

## 🧪 Testando Localmente

### Windows PowerShell

# Definir variável temporária
$env:AppSettings__Authentication__Jwt__Secret="test-secret-key-with-at-least-32-characters"
$env:AppSettings__EnvironmentName="Development"

# Executar aplicação
dotnet run --project src/Api/Api.csproj

# Limpar após teste
Remove-Item Env:AppSettings__Authentication__Jwt__Secret
### Linux / macOS

# Definir variável temporária
export AppSettings__Authentication__Jwt__Secret="test-secret-key-with-at-least-32-characters"
export AppSettings__EnvironmentName="Development"

# Executar aplicação
dotnet run --project src/Api/Api.csproj

# Limpar após teste
unset AppSettings__Authentication__Jwt__Secret
### Visual Studio / VS Code

Crie `launchSettings.json`:

{
  "profiles": {
    "Api": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AppSettings__Authentication__Jwt__Secret": "test-secret-key-with-at-least-32-characters",
        "AppSettings__Infrastructure__Cache__Provider": "Memory"
      }
    }
  }
}
---

## 📋 Checklist de Deployment

Antes de fazer deploy, verifique:

- ✅ **Secrets** não estão no código fonte
- ✅ **Connection Strings** corretas para o ambiente
- ✅ **JWT Secret** tem pelo menos 32 caracteres
- ✅ **Issuer/Audience** apontam para URLs corretas
- ✅ **Cache Provider** configurado (Memory/Redis)
- ✅ **Telemetry** habilitado para produção
- ✅ **EnvironmentName** correto (Production/Staging)
- ✅ **Variáveis de ambiente** testadas localmente
- ✅ **Logs** configurados corretamente
- ✅ **Health Checks** funcionando

---

## 🔍 Troubleshooting

### Problema: Configuração retorna null

**Causa:** Nome da propriedade no JSON não corresponde à classe C#

// ❌ Errado
{
  "AppSettings": {
    "Infraestructure": { ... }  // ❌ Typo (Infraestructure ao invés de Infrastructure)
  }
}

// ✅ Correto
{
  "AppSettings": {
    "Infrastructure": { ... }  // ✅ Nome correto
  }
}
### Problema: Erro "Cannot resolve IOptions<AppSettings>"

**Causa:** Esqueceu de chamar `AddAppSettingsConfiguration`

// Program.cs ou InfrastructureExtensions.cs
services.AddAppSettingsConfiguration(configuration, environment); // ✅ Necessário
### Problema: Validação não acontece

**Causa:** Faltou registrar o validador ou chamar `.ValidateOnStart()`

services.AddOptions<AppSettings>()
    .Bind(configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart(); // ✅ Necessário para validação no startup
---

## 📚 Referências

- [Options Pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Options validation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options#options-validation)

---

## 🎯 Resumo

### Regra de Ouro

> **Sempre que precisar de configurações em qualquer lugar do código (Controller, Service, Repository, Middleware, Background Service), injete `IOptions<AppSettings>`**

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
