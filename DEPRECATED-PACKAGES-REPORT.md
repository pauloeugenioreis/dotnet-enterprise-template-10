# 📋 Relatório de Pacotes Deprecated e Problemas Identificados

**Data da Análise:** 14 de Janeiro de 2026  
**Projeto:** .NET 10 Clean Architecture Template  
**Versão:** .NET 10.0

---

## 🚨 Pacotes DEPRECATED Identificados

### 1. **OpenTelemetry.Exporter.Jaeger** ❌ DEPRECATED

**Projeto:** `Infrastructure.csproj`  
**Versão Atual:** `1.5.1`  
**Status:** ⚠️ **LEGACY/DEPRECATED**

**Problema:**
- O pacote `OpenTelemetry.Exporter.Jaeger` foi marcado como **Legacy** pela OpenTelemetry Foundation
- Jaeger agora suporta nativamente o protocolo OTLP (OpenTelemetry Protocol)

**Solução Recomendada:**
```xml
<!-- ❌ REMOVER -->
<PackageReference Include="OpenTelemetry.Exporter.Jaeger" Version="1.5.1" />

<!-- ✅ USAR -->
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.14.0" />
```

**Configuração Atualizada:**
```csharp
// Ao invés de UseJaegerExporter()
services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317"); // Jaeger OTLP gRPC
            // ou
            options.Endpoint = new Uri("http://localhost:4318/v1/traces"); // Jaeger OTLP HTTP
        })
    );
```

**Impacto:**
- 🔴 **ALTO** - Funcionalidade crítica de telemetria
- ⏰ **URGENTE** - O pacote pode ser removido em versões futuras

**Documentação:**
- [OpenTelemetry Exporter Migration Guide](https://opentelemetry.io/docs/instrumentation/net/exporters/)
- [Jaeger OTLP Documentation](https://www.jaegertracing.io/docs/1.46/apis/#opentelemetry-protocol-otlp)

---

## ⚠️ Pacotes com Avisos

### 2. **Microsoft.AspNetCore.Http.Abstractions** ⚠️ POTENCIALMENTE DESNECESSÁRIO

**Projeto:** `Domain.csproj`  
**Versão Atual:** `2.3.9` (MUITO DESATUALIZADA)  
**Status:** ⚠️ **VERSÃO ANTIGA**

**Problema:**
- Versão 2.3.9 é do .NET Core 2.x (lançada em 2018)
- Em um projeto .NET 10, esta versão é extremamente antiga
- Pode causar conflitos de dependências

**Análise:**
- O Domain layer NÃO deveria ter dependência de abstrações HTTP (viola Clean Architecture)
- Esta dependência provavelmente vem de alguma classe que não deveria estar no Domain

**Solução Recomendada:**
1. **MELHOR:** Remover completamente esta dependência do Domain
2. **ALTERNATIVA:** Se realmente necessário, atualizar para versão do .NET 10:
   ```xml
   <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="10.0.2" />
   ```

**Impacto:**
- 🟡 **MÉDIO** - Pode causar problemas de compatibilidade
- ⏰ **MODERADO** - Deve ser corrigido antes de produção

---

### 3. **System.Data.SqlClient** ⚠️ OBSOLETO (mas funcional)

**Projeto:** `Data.csproj`  
**Versão Atual:** `4.9.0`  
**Status:** ⚠️ **SUBSTITUÍDO**

**Problema:**
- `System.Data.SqlClient` foi **substituído** por `Microsoft.Data.SqlClient`
- Embora ainda funcione, não recebe mais atualizações ativas
- Microsoft recomenda migração para `Microsoft.Data.SqlClient`

**Solução Recomendada:**
```xml
<!-- ❌ SUBSTITUIR -->
<PackageReference Include="System.Data.SqlClient" Version="4.9.0" />

<!-- ✅ USAR -->
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.0.0" />
```

**Código a Atualizar:**
```csharp
// ❌ Antigo
using System.Data.SqlClient;

// ✅ Novo
using Microsoft.Data.SqlClient;
```

**Impacto:**
- 🟡 **MÉDIO** - Funciona mas não é recomendado
- ⏰ **MODERADO** - Planejar migração em breve

**Documentação:**
- [Microsoft.Data.SqlClient Introduction](https://devblogs.microsoft.com/dotnet/introducing-the-new-microsoftdatasqlclient/)

---

### 4. **Microsoft.Extensions.Caching.Memory** ⚠️ REDUNDANTE

**Projeto:** `Infrastructure.csproj`  
**Versão Atual:** `10.0.2`  
**Status:** ⚠️ **DESNECESSÁRIO**

**Warning do NuGet:**
```
NU1510: PackageReference Microsoft.Extensions.Caching.Memory will not be pruned. 
Consider removing this package from your dependencies, as it is likely unnecessary.
```

**Problema:**
- Este pacote já está incluído no framework do ASP.NET Core
- Referência explícita é redundante

**Solução Recomendada:**
```xml
<!-- ❌ REMOVER completamente -->
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.2" />

<!-- ✅ Já incluído em -->
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

**Impacto:**
- 🟢 **BAIXO** - Apenas limpeza
- ⏰ **BAIXO** - Pode ser feito a qualquer momento

---

### 5. **OpenTelemetry.Exporter.Prometheus.AspNetCore** ⚠️ VERSÃO NÃO ENCONTRADA

**Projeto:** `Infrastructure.csproj`  
**Versão Solicitada:** `1.14.0-alpha.1`  
**Versão Resolvida:** `1.14.0-beta.1`  
**Status:** ⚠️ **VERSÃO INCORRETA**

**Warning do NuGet:**
```
NU1603: Infrastructure depends on OpenTelemetry.Exporter.Prometheus.AspNetCore 
(>= 1.14.0-alpha.1) but OpenTelemetry.Exporter.Prometheus.AspNetCore 1.14.0-alpha.1 
was not found. OpenTelemetry.Exporter.Prometheus.AspNetCore 1.14.0-beta.1 was resolved instead.
```

**Problema:**
- A versão alpha especificada não existe mais
- NuGet está resolvendo para uma versão beta mais recente

**Solução Recomendada:**
```xml
<!-- ❌ ATUALIZAR -->
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.14.0-alpha.1" />

<!-- ✅ USAR versão stable ou RC -->
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.14.0-rc.1" />
```

**Impacto:**
- 🟡 **MÉDIO** - Usando versão beta em produção não é ideal
- ⏰ **MODERADO** - Atualizar quando versão stable estiver disponível

---

### 6. **AspNetCoreRateLimit** ⚠️ PACKAGE SEM MANUTENÇÃO ATIVA

**Projeto:** `Infrastructure.csproj`  
**Versão Atual:** `5.0.0`  
**Status:** ⚠️ **BAIXA ATIVIDADE DE MANUTENÇÃO**

**Problema:**
- O pacote `AspNetCoreRateLimit` tem baixa atividade de manutenção
- .NET 7+ introduziu Rate Limiting nativo via `Microsoft.AspNetCore.RateLimiting`

**Solução Recomendada (Longo Prazo):**
```xml
<!-- ❌ CONSIDERAR SUBSTITUIR -->
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />

<!-- ✅ USAR (Rate Limiting nativo do .NET) -->
<!-- Já incluído no framework, não precisa de pacote -->
```

**Código Atualizado:**
```csharp
// .NET 7+ Native Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

app.UseRateLimiter();
```

**Impacto:**
- 🟡 **MÉDIO** - Funciona mas pode ser modernizado
- ⏰ **BAIXO** - Migração pode ser planejada para o futuro

**Documentação:**
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)

---

## 📊 Resumo de Ações Recomendadas

### 🔴 URGENTE (Deprecated)

| Pacote | Projeto | Ação | Prioridade |
|--------|---------|------|------------|
| **OpenTelemetry.Exporter.Jaeger** | Infrastructure | ❌ Remover<br>✅ Usar `OpenTelemetry.Exporter.OpenTelemetryProtocol` | 🔴 ALTA |

### 🟡 IMPORTANTE (Obsoleto/Antigo)

| Pacote | Projeto | Ação | Prioridade |
|--------|---------|------|------------|
| **Microsoft.AspNetCore.Http.Abstractions** | Domain | ❌ Remover ou atualizar para 10.0.2 | 🟡 MÉDIA |
| **System.Data.SqlClient** | Data | 🔄 Migrar para `Microsoft.Data.SqlClient` | 🟡 MÉDIA |
| **OpenTelemetry.Exporter.Prometheus.AspNetCore** | Infrastructure | 🔄 Atualizar para versão RC/Stable | 🟡 MÉDIA |

### 🟢 LIMPEZA (Redundante/Melhorias)

| Pacote | Projeto | Ação | Prioridade |
|--------|---------|------|------------|
| **Microsoft.Extensions.Caching.Memory** | Infrastructure | ❌ Remover (redundante) | 🟢 BAIXA |
| **AspNetCoreRateLimit** | Infrastructure | 🔄 Considerar migração para .NET nativo | 🟢 BAIXA |

---

## 🛠️ Plano de Ação Sugerido

### **Fase 1 - Crítico (Sprint Atual)**

1. ✅ **Substituir OpenTelemetry.Exporter.Jaeger**
   - Remover pacote deprecated
   - Implementar OTLP exporter
   - Testar integração com Jaeger
   - Atualizar documentação

### **Fase 2 - Importante (Próximo Sprint)**

2. ✅ **Corrigir Microsoft.AspNetCore.Http.Abstractions no Domain**
   - Identificar uso no Domain layer
   - Remover ou mover para camada apropriada
   - Manter Clean Architecture principles

3. ✅ **Migrar System.Data.SqlClient para Microsoft.Data.SqlClient**
   - Atualizar referências de using
   - Testar repositórios Dapper e ADO.NET
   - Atualizar documentação ORM-GUIDE.md

### **Fase 3 - Melhorias (Backlog)**

4. ✅ **Remover Microsoft.Extensions.Caching.Memory redundante**
   - Simples remoção do .csproj
   - Sem impacto no código

5. ✅ **Atualizar OpenTelemetry.Exporter.Prometheus.AspNetCore**
   - Aguardar versão stable
   - Ou usar versão RC mais recente

6. ✅ **Avaliar migração de AspNetCoreRateLimit**
   - Documentar prós/contras
   - Planejar migração para .NET native (futuro)

---

## 📝 Checklist de Execução

### Sprint Atual ✅ **CONCLUÍDO** (14/01/2026)
- [x] Remover `OpenTelemetry.Exporter.Jaeger` do Infrastructure.csproj
- [x] Pacote `OpenTelemetry.Exporter.OpenTelemetryProtocol` já estava presente
- [x] Atualizar `TelemetryExtension.cs` para usar OTLP
- [x] Atualizar `AppSettings.cs` (JaegerSettings com OTLP ports)
- [x] Atualizar `appsettings.json` (configuração padrão)
- [x] Atualizar `docker-compose.yml` (Jaeger OTLP endpoints 4317/4318)
- [x] Atualizar `TELEMETRY.md` documentation
- [ ] Testar telemetria end-to-end (próximo passo)

### Próximo Sprint
- [ ] Analisar uso de `Microsoft.AspNetCore.Http.Abstractions` no Domain
- [ ] Refatorar código para remover dependência HTTP do Domain
- [ ] Substituir `System.Data.SqlClient` por `Microsoft.Data.SqlClient`
- [ ] Atualizar imports em `SqlConnectionFactory.cs`
- [ ] Atualizar imports em repositórios ADO.NET
- [ ] Testar todos os repositórios Dapper e ADO.NET
- [ ] Atualizar `ORM-GUIDE.md`

### Backlog
- [ ] Remover `Microsoft.Extensions.Caching.Memory` do Infrastructure.csproj
- [ ] Atualizar `OpenTelemetry.Exporter.Prometheus.AspNetCore` para RC
- [ ] Documentar migração futura de `AspNetCoreRateLimit`
- [ ] Criar ADR (Architecture Decision Record) para mudanças

---

## 🔍 Comandos de Verificação

```bash
# Verificar pacotes deprecated
dotnet list package --deprecated

# Verificar pacotes vulneráveis
dotnet list package --vulnerable

# Verificar pacotes desatualizados
dotnet list package --outdated

# Verificar todos os pacotes
dotnet list package --include-transitive

# Restaurar e verificar warnings
dotnet restore --verbosity detailed
```

---

## 📚 Referências

- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [Microsoft.Data.SqlClient Migration Guide](https://docs.microsoft.com/en-us/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace)
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [NuGet Package Deprecation](https://docs.microsoft.com/en-us/nuget/nuget-org/deprecate-packages)
- [.NET 10 Breaking Changes](https://docs.microsoft.com/en-us/dotnet/core/compatibility/)

---

## 📧 Contato

Para dúvidas ou sugestões sobre este relatório:
- **Repository:** pauloeugenioreis/dotnet-enterprise-template-10
- **Date:** January 14, 2026

---

**Nota:** Este relatório foi gerado automaticamente através de análise do projeto e consulta ao NuGet.org. Recomenda-se revisar periodicamente (mensalmente) para identificar novos pacotes deprecated ou vulnerabilidades.
