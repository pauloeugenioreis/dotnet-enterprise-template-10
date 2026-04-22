# 🔒 Data Annotations para Validação de Configurações

Guia completo sobre validação automática usando Data Annotations nas classes de configuração.

---

## 📋 Índice

- [Visão Geral](#visao-geral)
- [Data Annotations Disponíveis](#data-annotations-disponiveis)
- [Validadores Customizados](#validadores-customizados)
- [Exemplos Práticos](#exemplos-praticos)
- [Habilitando Validação](#habilitando-validacao)
- [Mensagens de Erro](#mensagens-de-erro)

---

<a id="visao-geral"></a>

## 🎯 Visão Geral

O template usa **Data Annotations** para validar configurações automaticamente no startup da aplicação.

### Vantagens

✅ **Validação Automática** - Erros detectados antes da aplicação iniciar
✅ **Mensagens Claras** - Erros descritivos facilitam troubleshooting
✅ **Menos Código** - Validação declarativa vs programática
✅ **Type-Safe** - Validação em tempo de compilação
✅ **Documentação** - Atributos servem como documentação

### Como Funciona

1. **Adicione atributos nas propriedades**

   ```csharp
   public class JwtSettings
   {
       [Required(ErrorMessage = "JWT Secret is required")]
       [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters")]
       public string Secret { get; set; } = string.Empty;
   }
   ```

2. **Habilite validação no registro**

   ```csharp
   services.AddOptions<AppSettings>()
       .Bind(configuration.GetSection("AppSettings"))
       .ValidateDataAnnotations() // ✅ Valida Data Annotations
       .ValidateOnStart(); // ✅ Valida no startup
   ```

---

<a id="data-annotations-disponiveis"></a>

## 🏷️ Data Annotations Disponíveis

### Required

Campo obrigatório - não pode ser null ou vazio.

```csharp
[Required(ErrorMessage = "EnvironmentName is required")]
public string EnvironmentName { get; set; } = string.Empty;
```

### MinLength / MaxLength

Tamanho mínimo/máximo de strings.

```csharp
[MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters")]
public string Secret { get; set; } = string.Empty;

[MaxLength(100, ErrorMessage = "Issuer URL cannot exceed 100 characters")]
public string Issuer { get; set; } = string.Empty;
```

### Range

Valores numéricos dentro de um intervalo.

```csharp
[Range(1, 1440, ErrorMessage = "ExpirationMinutes must be between 1 and 1440")]
public int ExpirationMinutes { get; set; } = 60;

[Range(0.0, 1.0, ErrorMessage = "SamplingRatio must be between 0.0 and 1.0")]
public double SamplingRatio { get; set; } = 1.0;
```

### AllowedValues (.NET 8+)

Valores permitidos de um enum ou lista.

```csharp
[AllowedValues("Development", "Testing", "Staging", "Production",
    ErrorMessage = "EnvironmentName must be Development, Testing, Staging, or Production")]
public string EnvironmentName { get; set; } = string.Empty;

[AllowedValues("Memory", "Redis", "SqlServer",
    ErrorMessage = "Provider must be Memory, Redis, or SqlServer")]
public string Provider { get; set; } = "Memory";
```

### Url

Valida se string é uma URL válida.

```csharp
[Required]
[Url(ErrorMessage = "Issuer must be a valid URL")]
public string Issuer { get; set; } = string.Empty;
```

### EmailAddress

Valida formato de email.

```csharp
[EmailAddress(ErrorMessage = "Invalid email format")]
public string AdminEmail { get; set; } = string.Empty;
```

### RegularExpression

Validação customizada com regex.

```csharp
[RegularExpression(@"^[a-zA-Z0-9-_.]+$",
    ErrorMessage = "Only alphanumeric characters, hyphens, underscores and dots are allowed")]
public string BucketName { get; set; } = string.Empty;
```

---

<a id="validadores-customizados"></a>

## 🔧 Validadores Customizados

<a id="exemplos-praticos"></a>

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
```

### Exemplo 2: Configurações de Banco de Dados

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
```

### Exemplo 3: Configurações de Password Policy

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
```

### Exemplo 4: Configurações de Telemetry

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
```

<a id="habilitando-validacao"></a>

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
            .ValidateDataAnnotations() // ✅ Habilita validação de Data Annotations
            .ValidateOnStart(); // ✅ Valida no startup (falha rápido)

        // Registrar como singleton
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<AppSettings>>().Value);

        return services;
    }
}
```

### Ordem de Validação

1. **Data Annotations** - Validação declarativa (atributos)
2. **IValidateOptions<T>** - Validação programática customizada
3. **ValidateOnStart()** - Executa validações no startup

```csharp
services.AddOptions<AppSettings>()
    .Bind(configuration.GetSection("AppSettings"))
    .ValidateDataAnnotations() // 1º - Valida atributos
    .ValidateOnStart(); // 2º - Executa no startup

// 3º - Validação programática adicional
services.AddSingleton<IValidateOptions<AppSettings>, AuthenticationSettingsValidator>();
```

---

<a id="mensagens-de-erro"></a>

## 💬 Mensagens de Erro

### Mensagens Padrão

```csharp
[Required] // Mensagem: "The {PropertyName} field is required."
public string Secret { get; set; } = string.Empty;

[Range(1, 100)] // Mensagem: "The field {PropertyName} must be between 1 and 100."
public int MaxRetries { get; set; }
```

### Mensagens Customizadas

```csharp
[Required(ErrorMessage = "JWT Secret is required")]
public string Secret { get; set; } = string.Empty;

[Range(1, 100, ErrorMessage = "MaxRetries must be between 1 and 100 attempts")]
public int MaxRetries { get; set; }

[MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters for security")]
public string Secret { get; set; } = string.Empty;
```

### Mensagens com Placeholders

```csharp
[Range(1, 100, ErrorMessage = "{0} must be between {1} and {2}")]
public int MaxRetries { get; set; }
// Resultado: "MaxRetries must be between 1 and 100"

[StringLength(50, MinimumLength = 3,
    ErrorMessage = "{0} must be between {2} and {1} characters")]
public string Name { get; set; } = string.Empty;
// Resultado: "Name must be between 3 and 50 characters"
```

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
```

### Teste Manual

Execute a aplicação com configuração inválida:

#### Windows PowerShell

```powershell
$env:AppSettings__Authentication__Jwt__Secret="short"
dotnet run --project src/Api/Api.csproj
```

**Resultado esperado:**

```text
Unhandled exception. Microsoft.Extensions.Options.OptionsValidationException:
DataAnnotation validation failed for 'AppSettings' members:
'Authentication.Jwt.Secret' with the error: 'JWT Secret must be at least 32 characters for security'.
```

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
```

### ❌ DON'Ts

```csharp
// ❌ Mensagem genérica
[Required(ErrorMessage = "Required")]
public string Secret { get; set; }

// ❌ Range irreal
[Range(1, int.MaxValue)] // Muito amplo
public int Timeout { get; set; }

// ❌ Validação inconsistente
[MinLength(8)]
public string Password { get; set; } = "12345"; // Valor padrão inválido

// ❌ Sem validação em campos críticos
public string Secret { get; set; } = string.Empty; // ❌ Sem [Required] nem [MinLength]
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
