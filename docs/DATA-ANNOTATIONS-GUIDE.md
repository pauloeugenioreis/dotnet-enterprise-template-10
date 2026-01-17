# 🔒 Data Annotations para Validação de Configurações

Guia completo sobre validação automática usando Data Annotations nas classes de configuração.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Data Annotations Disponíveis](#-data-annotations-disponíveis)
- [Validadores Customizados](#-validadores-customizados)
- [Exemplos Práticos](#-exemplos-práticos)
- [Habilitando Validação](#-habilitando-validação)
- [Mensagens de Erro](#-mensagens-de-erro)

---

## 🎯 Visão Geral

O template usa **Data Annotations** para validar configurações automaticamente no startup da aplicação.

### Vantagens

✅ **Validação Automática** - Erros detectados antes da aplicação iniciar
✅ **Mensagens Claras** - Erros descritivos facilitam troubleshooting
✅ **Menos Código** - Validação declarativa vs programática
✅ **Type-Safe** - Validação em tempo de compilação
✅ **Documentação** - Atributos servem como documentação

### Como Funciona

```csharp
// 1. Adicione atributos nas propriedades
public class JwtSettings
{
    [Required(ErrorMessage = "JWT Secret is required")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters")]
    public string Secret { get; set; } = string.Empty;
}

// 2. Habilite validação no registro
services.AddOptions<AppSettings>()
    .Bind(configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations()  // ✅ Valida Data Annotations
    .ValidateOnStart();         // ✅ Valida no startup
```markdown
---

## 🏷️ Data Annotations Disponíveis

### Required

Campo obrigatório - não pode ser null ou vazio.

```csharp
[Required(ErrorMessage = "EnvironmentName is required")]
public string EnvironmentName { get; set; } = string.Empty;
```markdown
### MinLength / MaxLength

Tamanho mínimo/máximo de strings.

```csharp
[MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters")]
public string Secret { get; set; } = string.Empty;

[MaxLength(100, ErrorMessage = "Issuer URL cannot exceed 100 characters")]
public string Issuer { get; set; } = string.Empty;
```markdown
### Range

Valores numéricos dentro de um intervalo.

```csharp
[Range(1, 1440, ErrorMessage = "ExpirationMinutes must be between 1 and 1440")]
public int ExpirationMinutes { get; set; } = 60;

[Range(0.0, 1.0, ErrorMessage = "SamplingRatio must be between 0.0 and 1.0")]
public double SamplingRatio { get; set; } = 1.0;
```markdown
### AllowedValues (.NET 8+)

Valores permitidos de um enum ou lista.

```csharp
[AllowedValues("Development", "Testing", "Staging", "Production",
    ErrorMessage = "EnvironmentName must be Development, Testing, Staging, or Production")]
public string EnvironmentName { get; set; } = string.Empty;

[AllowedValues("Memory", "Redis", "SqlServer",
    ErrorMessage = "Provider must be Memory, Redis, or SqlServer")]
public string Provider { get; set; } = "Memory";
```markdown
### Url

Valida se string é uma URL válida.

```csharp
[Required]
[Url(ErrorMessage = "Issuer must be a valid URL")]
public string Issuer { get; set; } = string.Empty;
```markdown
### EmailAddress

Valida formato de email.

```csharp
[EmailAddress(ErrorMessage = "Invalid email format")]
public string AdminEmail { get; set; } = string.Empty;
```markdown
### RegularExpression

Validação customizada com regex.

```csharp
[RegularExpression(@"^[a-zA-Z0-9-_.]+$", 
    ErrorMessage = "Only alphanumeric characters, hyphens, underscores and dots are allowed")]
public string BucketName { get; set; } = string.Empty;
```markdown
---

## 🔧 Validadores Customizados

### RequiredIf - Requer campo quando condição é verdadeira

```csharp
// Validators/RequiredIfAttribute.cs
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object _propertyValue;

    public RequiredIfAttribute(string propertyName, object propertyValue)
    {
        _propertyName = propertyName;
        _propertyValue = propertyValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var propertyInfo = instance.GetType().GetProperty(_propertyName);

        if (propertyInfo == null)
            return new ValidationResult($"Property {_propertyName} not found");

        var propertyValue = propertyInfo.GetValue(instance);

        // Se a condição for verdadeira, o campo é obrigatório
        if (Equals(propertyValue, _propertyValue))
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return new ValidationResult(
                    ErrorMessage ?? 
                    $"{validationContext.DisplayName} is required when {_propertyName} is {_propertyValue}");
            }
        }

        return ValidationResult.Success;
    }
}
```text
**Uso:**

```csharp
public class CacheSettings
{
    public string Provider { get; set; } = "Memory";
    public RedisSettings? Redis { get; set; }
}

public class RedisSettings
{
    [RequiredIf(nameof(CacheSettings.Provider), "Redis",
        ErrorMessage = "Redis ConnectionString is required when Provider is Redis")]
    public string ConnectionString { get; set; } = string.Empty;
}
```markdown
### RedisConnectionString - Valida formato de connection string Redis

```csharp
// Validators/RedisConnectionStringAttribute.cs
public class RedisConnectionStringAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string connectionString)
            return ValidationResult.Success;

        if (string.IsNullOrWhiteSpace(connectionString))
            return new ValidationResult("Redis connection string cannot be empty");

        // Validar formato básico: host:port
        if (!connectionString.Contains(':'))
        {
            return new ValidationResult(
                "Redis connection string must contain host and port (e.g., 'localhost:6379')");
        }

        return ValidationResult.Success;
    }
}
```text
**Uso:**

```csharp
public class RedisSettings
{
    [RedisConnectionString(ErrorMessage = "Invalid Redis connection string format")]
    public string ConnectionString { get; set; } = string.Empty;
}
```markdown
---

## 🎓 Exemplos Práticos

### Exemplo 1: Configurações de Autenticação

```csharp
public class AuthenticationSettings
{
    public bool Enabled { get; set; } = true;
    
    [Required]
    public JwtSettings Jwt { get; set; } = new();
}

public class JwtSettings
{
    [Required(ErrorMessage = "JWT Secret is required")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters for security")]
    public string Secret { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "JWT Issuer is required")]
    [Url(ErrorMessage = "Issuer must be a valid URL")]
    public string Issuer { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "JWT Audience is required")]
    [Url(ErrorMessage = "Audience must be a valid URL")]
    public string Audience { get; set; } = string.Empty;
    
    [Range(1, 1440, ErrorMessage = "ExpirationMinutes must be between 1 and 1440 (24 hours)")]
    public int ExpirationMinutes { get; set; } = 60;
    
    [Range(1, 90, ErrorMessage = "RefreshTokenExpirationDays must be between 1 and 90 days")]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
```markdown
### Exemplo 2: Configurações de Cache

```csharp
public class CacheSettings
{
    public bool Enabled { get; set; } = true;
    
    [Required(ErrorMessage = "Cache Provider is required")]
    [AllowedValues("Memory", "Redis", "SqlServer",
        ErrorMessage = "Provider must be Memory, Redis, or SqlServer")]
    public string Provider { get; set; } = "Memory";
    
    public RedisSettings? Redis { get; set; }
    
    [Range(1, 1440, ErrorMessage = "DefaultExpirationMinutes must be between 1 and 1440")]
    public int DefaultExpirationMinutes { get; set; } = 60;
}

public class RedisSettings
{
    [RequiredIf(nameof(CacheSettings.Provider), "Redis",
        ErrorMessage = "Redis ConnectionString is required when Provider is Redis")]
    [RedisConnectionString(ErrorMessage = "Invalid Redis connection string format")]
    public string ConnectionString { get; set; } = string.Empty;
}
```markdown
### Exemplo 3: Configurações de Banco de Dados

```csharp
public class DatabaseSettings
{
    [Required(ErrorMessage = "DatabaseType is required")]
    [AllowedValues("InMemory", "SqlServer", "Oracle", "PostgreSQL", "MySQL",
        ErrorMessage = "DatabaseType must be InMemory, SqlServer, Oracle, PostgreSQL, or MySQL")]
    public string DatabaseType { get; set; } = "InMemory";
    
    [Range(1, 300, ErrorMessage = "CommandTimeoutSeconds must be between 1 and 300 seconds")]
    public int CommandTimeoutSeconds { get; set; } = 30;
    
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
}
```markdown
### Exemplo 4: Configurações de Password Policy

```csharp
public class PasswordPolicySettings
{
    [Range(6, 128, ErrorMessage = "MinimumLength must be between 6 and 128 characters")]
    public int MinimumLength { get; set; } = 8;
    
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    
    [Range(1, 100, ErrorMessage = "MaxFailedAccessAttempts must be between 1 and 100")]
    public int MaxFailedAccessAttempts { get; set; } = 5;
    
    [Range(1, 1440, ErrorMessage = "LockoutMinutes must be between 1 and 1440 (24 hours)")]
    public int LockoutMinutes { get; set; } = 15;
}
```markdown
### Exemplo 5: Configurações de Telemetry

```csharp
public class TelemetrySettings
{
    public bool Enabled { get; set; } = false;
    
    public string[] Providers { get; set; } = Array.Empty<string>();
    
    [Range(0.0, 1.0, ErrorMessage = "SamplingRatio must be between 0.0 and 1.0")]
    public double SamplingRatio { get; set; } = 1.0;
    
    public bool EnableSqlInstrumentation { get; set; } = true;
    public bool EnableHttpInstrumentation { get; set; } = true;
}
```markdown
---

## ⚙️ Habilitando Validação

### No AppSettingsExtension

```csharp
// Infrastructure/Extensions/AppSettingsExtension.cs
public static class AppSettingsExtension
{
    public static IServiceCollection AddAppSettingsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Bind e validar settings
        services.AddOptions<AppSettings>()
            .Bind(configuration.GetSection("AppSettings"))
            .ValidateDataAnnotations()  // ✅ Habilita validação de Data Annotations
            .ValidateOnStart();         // ✅ Valida no startup (falha rápido)

        // Registrar como singleton
        services.AddSingleton(sp => 
            sp.GetRequiredService<IOptions<AppSettings>>().Value);

        return services;
    }
}
```markdown
### Ordem de Validação

1. **Data Annotations** - Validação declarativa (atributos)
2. **IValidateOptions<T>** - Validação programática customizada
3. **ValidateOnStart()** - Executa validações no startup

```csharp
services.AddOptions<AppSettings>()
    .Bind(configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations()                          // 1º - Valida atributos
    .ValidateOnStart();                                  // 2º - Executa no startup

// 3º - Validação programática adicional
services.AddSingleton<IValidateOptions<AppSettings>, 
    AuthenticationSettingsValidator>();
```markdown
---

## 💬 Mensagens de Erro

### Mensagens Padrão

```csharp
[Required]  // Mensagem: "The {PropertyName} field is required."
public string Secret { get; set; } = string.Empty;

[Range(1, 100)]  // Mensagem: "The field {PropertyName} must be between 1 and 100."
public int MaxRetries { get; set; }
```markdown
### Mensagens Customizadas

```csharp
[Required(ErrorMessage = "JWT Secret is required")]
public string Secret { get; set; } = string.Empty;

[Range(1, 100, ErrorMessage = "MaxRetries must be between 1 and 100 attempts")]
public int MaxRetries { get; set; }

[MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters for security")]
public string Secret { get; set; } = string.Empty;
```markdown
### Mensagens com Placeholders

```csharp
[Range(1, 100, ErrorMessage = "{0} must be between {1} and {2}")]
public int MaxRetries { get; set; }
// Resultado: "MaxRetries must be between 1 and 100"

[StringLength(50, MinimumLength = 3, 
    ErrorMessage = "{0} must be between {2} and {1} characters")]
public string Name { get; set; } = string.Empty;
// Resultado: "Name must be between 3 and 50 characters"
```markdown
---

## 🔍 Testando Validação

### Teste de Startup

```csharp
[Fact]
public void AppSettings_Should_Fail_With_Invalid_JWT_Secret()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["AppSettings:Authentication:Jwt:Secret"] = "short" // ❌ < 32 chars
        })
        .Build();

    var services = new ServiceCollection();
    services.AddOptions<AppSettings>()
        .Bind(configuration.GetSection("AppSettings"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // Act & Assert
    var serviceProvider = services.BuildServiceProvider();
    
    var exception = Assert.Throws<OptionsValidationException>(() =>
        serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value);
    
    Assert.Contains("JWT Secret must be at least 32 characters", exception.Message);
}
```markdown
### Teste Manual

Execute a aplicação com configuração inválida:

```bash
# Windows PowerShell
$env:AppSettings__Authentication__Jwt__Secret="short"
dotnet run --project src/Api/Api.csproj
```text
**Resultado esperado:**

```
Unhandled exception. Microsoft.Extensions.Options.OptionsValidationException: 
DataAnnotation validation failed for 'AppSettings' members: 
'Authentication.Jwt.Secret' with the error: 'JWT Secret must be at least 32 characters for security'.
```markdown
---

## 🎯 Boas Práticas

### ✅ DO's

```csharp
// ✅ Use ErrorMessage descritivo
[Required(ErrorMessage = "JWT Secret is required for authentication")]
public string Secret { get; set; } = string.Empty;

// ✅ Valide ranges realistas
[Range(1, 1440, ErrorMessage = "ExpirationMinutes must be between 1 and 1440 (24h)")]
public int ExpirationMinutes { get; set; }

// ✅ Combine múltiplas validações
[Required]
[MinLength(32)]
[MaxLength(256)]
public string Secret { get; set; } = string.Empty;

// ✅ Use AllowedValues para enums
[AllowedValues("Memory", "Redis", "SqlServer")]
public string Provider { get; set; } = "Memory";
```markdown
### ❌ DON'Ts

```csharp
// ❌ Mensagem genérica
[Required(ErrorMessage = "Required")]
public string Secret { get; set; }

// ❌ Range irreal
[Range(1, int.MaxValue)]  // Muito amplo
public int Timeout { get; set; }

// ❌ Validação inconsistente
[MinLength(8)]
public string Password { get; set; } = "12345";  // Valor padrão inválido

// ❌ Sem validação em campos críticos
public string Secret { get; set; } = string.Empty;  // ❌ Sem [Required] nem [MinLength]
```

---

## 📚 Referências

- [Data Annotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [Options Validation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options#options-validation)
- [Custom Validation Attributes](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validationattribute)

---

## ✅ Checklist

Antes de fazer deployment:

- ✅ Todos os campos obrigatórios têm `[Required]`
- ✅ Strings têm `[MinLength]` / `[MaxLength]` apropriados
- ✅ Números têm `[Range]` realista
- ✅ URLs têm `[Url]`
- ✅ Enums têm `[AllowedValues]`
- ✅ Mensagens de erro são descritivas
- ✅ `ValidateDataAnnotations()` está habilitado
- ✅ `ValidateOnStart()` está habilitado
- ✅ Testes cobrem validações críticas

---

**Próximos Passos:**
- [CONFIGURATION-GUIDE.md](CONFIGURATION-GUIDE.md) - Guia completo de configuração
- [ARCHITECTURE.md](ARCHITECTURE.md) - Arquitetura do template
