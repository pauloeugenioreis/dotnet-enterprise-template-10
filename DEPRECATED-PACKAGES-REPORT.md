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

### 2. **Microsoft.AspNetCore.Http.Abstractions** ✅ RESOLVIDO

**Projeto:** `Domain.csproj`  
**Versão Antiga:** `2.3.9` → **Status:** ✅ **REMOVIDO**

**Problema Original:**
- Versão 2.3.9 do .NET Core 2.x violava Clean Architecture
- Domain layer tinha dependência HTTP inadequada

**Solução Implementada:**
1. ✅ **Criado DTO de contexto** - `src/Domain/Dtos/ExceptionContext.cs`
2. ✅ **Refatorado IExceptionNotificationService** - Usa ExceptionContext ao invés de HttpContext
3. ✅ **Removida dependência HTTP** - Domain.csproj agora limpo
4. ✅ **Criado IExecutionContextService** - Nova interface para contexto de execução
5. ✅ **Implementado ExecutionContextService** - Infrastructure fornece contexto HTTP
6. ✅ **Refatorado HybridRepository** - Usa IExecutionContextService ao invés de IHttpContextAccessor

**Arquivos Modificados:**
- ✅ `src/Domain/Dtos/ExceptionContext.cs` - Novo DTO para contexto de exceção
- ✅ `src/Domain/Interfaces/IExceptionNotificationService.cs` - Refatorado
- ✅ `src/Domain/Interfaces/IExecutionContextService.cs` - Nova interface
- ✅ `src/Domain/Domain.csproj` - Removida dependência HTTP
- ✅ `src/Infrastructure/Services/ExceptionNotificationService.cs` - Implementação atualizada
- ✅ `src/Infrastructure/Services/ExecutionContextService.cs` - Nova implementação
- ✅ `src/Infrastructure/Middleware/GlobalExceptionHandler.cs` - Cria ExceptionContext
- ✅ `src/Data/Repository/HybridRepository.cs` - Usa IExecutionContextService
- ✅ `src/Infrastructure/Extensions/DependencyInjectionExtensions.cs` - Registra novo serviço

**Benefícios:**
- ✅ **Clean Architecture respeitada** - Domain sem dependências de infraestrutura
- ✅ **Testabilidade melhorada** - Interfaces podem ser mockadas facilmente
- ✅ **Desacoplamento total** - Domain não conhece HTTP/ASP.NET Core
- ✅ **Flexibilidade** - Pode usar contexto de outras fontes (gRPC, mensageria, etc)

**Impacto:**
- 🟡 **MÉDIO** - Refatoração significativa mas isolada
- ⏰ **MODERADO** - Concluído no Próximo Sprint

---

### 3. **System.Data.SqlClient** ✅ RESOLVIDO

**Projeto:** `Data.csproj`  
**Versão Antiga:** `4.9.0` → **Nova:** `Microsoft.Data.SqlClient 6.1.1`  
**Status:** ✅ **MIGRADO**

**Solução Implementada:**
```xml
<!-- ❌ REMOVIDO -->
<!-- <PackageReference Include="System.Data.SqlClient" Version="4.9.0" /> -->

<!-- ✅ IMPLEMENTADO -->
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.1" />
```

**Código Atualizado:**
```csharp
// ✅ Atualizado em todos os repositórios
using Microsoft.Data.SqlClient;
```

**Arquivos Modificados:**
- ✅ `src/Data/Data.csproj` - Atualizado pacote para v6.1.1 (compatível com EF Core 10.0.2)
- ✅ `src/Infrastructure/Services/SqlConnectionFactory.cs` - using atualizado
- ✅ `src/Data/Repository/Dapper/*` - Usam IDbConnectionFactory (via Microsoft.Data.SqlClient)
- ✅ `src/Data/Repository/Ado/*` - Usam IDbConnectionFactory (via Microsoft.Data.SqlClient)
- ✅ `docs/ORM-GUIDE.md` - Documentação atualizada com exemplos

**Benefícios:**
- ✅ **Suporte ativo** da Microsoft
- ✅ **Compatível com .NET 10** e EF Core 10.0.2
- ✅ **Melhor segurança** e correções de bugs
- ✅ **Novas features** do SQL Server

**Documentação:**
- [Microsoft.Data.SqlClient Introduction](https://devblogs.microsoft.com/dotnet/introducing-the-new-microsoftdatasqlclient/)

---

### 4. **Microsoft.Extensions.Caching.Memory** ✅ RESOLVIDO

**Projeto:** `Infrastructure.csproj`  
**Versão Antiga:** `10.0.2` → **Status:** ✅ **REMOVIDO**

**Problema Original:**
- Pacote redundante, já incluído no framework ASP.NET Core
- Warning NU1510 indicava que deveria ser removido

**Solução Implementada:**
```xml
<!-- ❌ REMOVIDO -->
<!-- <PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.2" /> -->

<!-- ✅ Framework já inclui -->
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

**Arquivo Modificado:**
- ✅ `src/Infrastructure/Infrastructure.csproj` - Removida dependência redundante

**Benefícios:**
- ✅ **Projeto mais limpo** - Sem dependências desnecessárias
- ✅ **Sem warnings NU1510** - Build mais limpo
- ✅ **Melhor manutenibilidade** - Menos referências para gerenciar

**Impacto:**
- 🟢 **BAIXO** - Apenas limpeza, sem mudanças de comportamento
- ⏰ **CONCLUÍDO** - Sprint Backlog

---

### 5. **OpenTelemetry.Exporter.Prometheus.AspNetCore** ✅ RESOLVIDO

**Projeto:** `Infrastructure.csproj`  
**Versão Antiga:** `1.14.0-alpha.1` → **Nova:** `1.14.0-beta.1`  
**Status:** ✅ **ATUALIZADO**

**Problema Original:**
- Versão alpha não existia mais no NuGet
- Warning NU1603 indicava resolução automática para beta

**Solução Implementada:**
```xml
<!-- ❌ VERSÃO INEXISTENTE -->
<!-- <PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.14.0-alpha.1" /> -->

<!-- ✅ VERSÃO MAIS RECENTE DISPONÍVEL -->
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.14.0-beta.1" />
```

**Arquivo Modificado:**
- ✅ `src/Infrastructure/Infrastructure.csproj` - Atualizado para versão beta disponível

**Nota sobre versão stable:**
- ⚠️ **Não há versão RC ou stable ainda** (verificado em 2026-01-14)
- ✅ **Versão beta é a mais recente** disponível no NuGet (27 versões encontradas)
- 📅 **Monitorar** lançamento de versão stable no futuro

**Benefícios:**
- ✅ **Sem warnings NU1603** - Build limpo
- ✅ **Versão explícita** - Sem ambiguidade na resolução
- ✅ **Compatível** - Funciona perfeitamente com OpenTelemetry 1.14.0

**Impacto:**
- 🟡 **MÉDIO** - Beta é aceitável para observabilidade (não crítico)
- ⏰ **CONCLUÍDO** - Sprint Backlog

---

### 6. **AspNetCoreRateLimit** ✅ DOCUMENTADO (Decisão Adiada)

**Projeto:** `Infrastructure.csproj`  
**Versão Atual:** `5.0.0`  
**Status:** ✅ **MANTIDO** (com ADR criado)

**Análise:**
- O pacote `AspNetCoreRateLimit` tem baixa atividade de manutenção
- .NET 7+ oferece Rate Limiting nativo como alternativa
- Pacote atual funciona perfeitamente e tem features avançadas

**Decisão (ADR):**
**MANTER** versão atual, migração planejada para o futuro se necessário.

**Rationale:**
- ✅ **Implementação madura** - 5+ anos em produção, battle-tested
- ✅ **Features avançadas** - Whitelist, blacklist, custom messages, distributed cache
- ✅ **Zero breaking changes** - Funciona perfeitamente no .NET 10
- ✅ **Configuração JSON** - Mais simples que código
- ✅ **4 estratégias** já implementadas e documentadas
- ⚠️ **Migração futura** - Considerar quando .NET native tiver feature parity

**Documentação Criada:**
- ✅ `docs/ADR-RATE-LIMITING.md` - Architecture Decision Record completo
  - Análise comparativa AspNetCoreRateLimit vs .NET Native
  - Matriz de decisão (6-3 para AspNetCoreRateLimit)
  - Roadmap de migração (Q2 2026 review)
  - Estimativa de esforço (4.5-5.5 dias)

**Próximos Passos:**
- 📅 **Q2 2026** - Revisar decisão
- 🔄 **Monitorar** atividade do repositório GitHub
- 🎯 **Migrar** apenas se houver motivo técnico forte

**Impacto:**
- 🟢 **BAIXO** - Risk level: LOW-MEDIUM
- ⏰ **DOCUMENTADO** - Sprint Backlog

**Referências:**
- [ADR-RATE-LIMITING.md](docs/ADR-RATE-LIMITING.md)
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
