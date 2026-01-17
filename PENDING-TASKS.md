# 📋 Tarefas Pendentes - Clean Architecture Template

## ✅ CONCLUÍDO (9/14 - 64%)

### Infraestrutura e Configuração
- [x] **.editorconfig** - Regras de estilo e análise de código
- [x] **Directory.Build.props** - Configuração centralizada de build
- [x] **.dockerignore** - Otimização de build Docker

### Segurança e Performance
- [x] **HTTP Security Headers** - HSTS, XContentTypeOptions, ReferrerPolicy, XXssProtection, Xfo
- [x] **Output Caching** - 3 políticas configuradas (10s, 300s, 600s)
- [x] **Random → RandomNumberGenerator** - 33 erros CA5394 eliminados

### Exception Handling (Completo - 16 generic catch blocks eliminados)
- [x] **InfrastructureExceptions.cs** - Exceções customizadas criadas (StorageException, TokenValidationException, EventStoreException)
- [x] **Service.cs** - Try-catch redundante removido, validações de parâmetros adicionadas
- [x] **StorageService.cs** - Transformação de exceções do Google Cloud para exceções de domínio
- [x] **JwtTokenService.cs** - Exceções de token transformadas em TokenValidationException
- [x] **MartenEventStore.cs** - Logging adequado adicionado, catch silencioso corrigido
- [x] **GlobalExceptionHandler** - Mapeamento aprimorado com novas exceções de infraestrutura

### ConfigureAwait (Completo - 178 awaits em 14 arquivos)
- [x] **Service.cs** - 6 awaits
- [x] **OrderService.cs** - 9 awaits
- [x] **Repository.cs** - 8 awaits
- [x] **AuthService.cs** - 24 awaits
- [x] **MartenEventStore.cs** - 10 awaits
- [x] **UserRepository.cs** - 21 awaits
- [x] **OrderRepository.cs** - 6 awaits
- [x] **HybridRepository.cs** - 17 awaits
- [x] **ProductDapperRepository.cs** - 11 awaits
- [x] **OrderDapperRepository.cs** - 17 awaits
- [x] **ProductAdoRepository.cs** - 19 awaits
- [x] **OrderAdoRepository.cs** - 24 awaits
- [x] **DbSeeder.cs** - 17 awaits
- [x] **JwtTokenService.cs** - 2 awaits
- [x] **StorageService.cs** - 3 awaits
- [x] **GlobalExceptionHandler.cs** - 5 awaits
- [x] **ValidationFilter.cs** - 2 awaits

---

## ⏳ PENDENTES (4/14 - 29%)

### 1. 🔍 Resolver TODOs no Codebase (6 ocorrências)

| Arquivo | Linha | TODO | Prioridade |
|---------|-------|------|------------|
| `SwaggerExtension.cs` | 41 | Adicionar autenticação JWT ao Swagger | ALTA |
| `TelemetryExtension.cs` | 184 | Configurar Application Insights | MÉDIA |
| `TelemetryExtension.cs` | 266 | Configurar métricas customizadas | MÉDIA |
| `AuthService.cs` | 117 | Implementar refresh token rotation | ALTA |
| `ExceptionNotificationService.cs` | 43 | Adicionar integração com serviço de notificação | BAIXA |
| Outros | - | Revisar código comentado | BAIXA |

**Tempo estimado**: 2-3 horas

---

### 2. ✅ **Melhorar Exception Handling (CONCLUÍDO)** ~~(16 generic catch blocks)~~

**Implementado com sucesso!** ✅

#### ✅ **Mudanças realizadas:**

1. **Criado `InfrastructureExceptions.cs`** com exceções customizadas:
   - `StorageException` - Erros em operações de storage (GCS, Azure Blob, S3)
   - `TokenValidationException` - Erros de validação de JWT/tokens
   - `EventStoreException` - Erros no event store (Marten)

2. **Service.cs** - Abordagem híbrida:
   - ❌ Removido try-catch redundante (16 blocos eliminados)
   - ✅ Adicionado validações de parâmetros (`ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException`)
   - ✅ GlobalExceptionHandler captura todas as exceções

3. **StorageService.cs** - Boundary Pattern:
   - ✅ Mantido try-catch para transformar exceções técnicas do Google Cloud em exceções de domínio
   - ✅ Tratamento específico por tipo de erro HTTP (404, 403, etc.)
   - ✅ Delete idempotente (não falha se arquivo já foi deletado)

4. **JwtTokenService.cs** - Token handling:
   - ✅ Transformação de `SecurityTokenException` em `TokenValidationException`
   - ✅ Tratamento específico para token expirado, assinatura inválida, etc.

5. **MartenEventStore.cs** - Event Store:
   - ✅ Adicionado logger ao construtor
   - ✅ Catch silencioso substituído por logging apropriado em `ConvertToTypedEvent`

6. **GlobalExceptionHandler** - Mapeamento aprimorado:
   - ✅ Adicionado handling para `StorageException` (500)
   - ✅ Adicionado handling para `TokenValidationException` (401)
   - ✅ Adicionado handling para `EventStoreException` (500)
   - ✅ Adicionado handling para `OperationCanceledException` (499)
   - ✅ Adicionado handling para `TimeoutException` (504)

#### 📊 **Resultados:**
- ✅ Build: **Sucesso** (0 erros, 296 warnings - não bloqueantes)
- ✅ Tests: **33/33 passando** (100% success rate)
- ✅ Código mais limpo e manutenível
- ✅ Exceções autodocumentadas
- ✅ Melhor observabilidade e debugging

**Tempo gasto**: ~3 horas

---

### 3. 🔄 Implementar Polly para Resiliência

#### Patterns a implementar:

1. **Retry Policy** (tentativas automáticas)
   - HTTP calls
   - Database operations
   - External services

2. **Circuit Breaker** (proteção contra falhas)
   - Storage service
   - External APIs
   - Event sourcing

3. **Timeout Policy** (limites de tempo)
   - Long-running operations
   - Database queries

4. **Fallback Policy** (comportamento alternativo)
   - Cache fallback
   - Default responses

**Arquivos a modificar:**
```text
src/Infrastructure/
├── Extensions/PollyExtension.cs  [CRIAR]
├── Services/AuthService.cs       [ATUALIZAR]
├── Services/StorageService.cs    [ATUALIZAR]
└── Services/MartenEventStore.cs  [ATUALIZAR]
```

**Pacotes necessários:**
- Polly 8.x
- Polly.Extensions.Http
- Microsoft.Extensions.Http.Polly

**Tempo estimado**: 4-5 horas

---

### 4. 📊 Warnings do Analyzer (413 total)

#### Breakdown por categoria:

| Código | Quantidade | Descrição | Prioridade |
|--------|-----------|-----------|------------|
| **CA2007** | 0 | ConfigureAwait missing | ✅ RESOLVIDO |
| **CA1062** | 100+ | Validate parameters for null | MÉDIA |
| **CA1303** | 50+ | Hardcoded strings (i18n) | BAIXA |
| **IDE0011** | 50+ | Add braces to if statements | BAIXA |
| **CA1707** | 40+ | Underscores in test names | BAIXA |
| **S1135** | 6 | TODOs in code | MÉDIA |
| **CA1305** | 20+ | Culture-specific operations | MÉDIA |
| **CA1849** | 15+ | Sync over async calls | ALTA |
| **Outros** | 132+ | Diversos | VARIADA |

**Ações sugeridas:**
1. ✅ CA2007 - CONCLUÍDO (178 ConfigureAwait adicionados)
2. CA1062 - Adicionar guards com `ArgumentNullException.ThrowIfNull()`
3. CA1849 - Substituir por versões assíncronas (BeginTransaction → BeginTransactionAsync)
4. Restantes - Avaliar caso a caso

**Tempo estimado**: 6-8 horas

---

### 5. 📄 Markdown Linting Issues (366 total)

#### Arquivos afetados:
```text
docs/
├── ADR-RATE-LIMITING.md
├── ARCHITECTURE.md
├── AUTHENTICATION.md
├── CICD.md
├── EVENT-SOURCING.md
├── FEATURES.md
├── KUBERNETES.md
├── ORM-GUIDE.md
├── RATE-LIMITING.md
├── SECURITY.md
└── TELEMETRY.md

Root:
├── README.md
├── QUICK-START.md
└── INDEX.md
```

**Principais problemas:**
- Heading levels inconsistentes
- Links quebrados
- Code blocks sem language tag
- Trailing spaces
- Missing blank lines

**Ferramenta sugerida**: markdownlint
**Tempo estimado**: 2-3 horas

---

## 📈 Progresso Geral

```text
[████████████████████████░] 71% Concluído

✅ Concluído:     10 tarefas
⏳ Pendente:      4 tarefas
⏱️  Tempo estimado: 14-22 horas
```

---

## 🎯 Próximos Passos Recomendados

### ✅ Sprint 1 - Performance & Segurança - **CONCLUÍDO**
1. ✅ ConfigureAwait completo → **CONCLUÍDO** 🎉
2. ✅ Melhorar exception handling → **CONCLUÍDO** 🎉
3. ✅ Abordagem híbrida implementada → **CONCLUÍDO** 🎉

### 🔄 Sprint 2 - Resiliência & Qualidade (Próximo - 4-5 horas)
**Prioridade ALTA:**
1. 🔄 Implementar Polly para Resiliência (Task #3)
   - Retry Policy para HTTP calls e database operations
   - Circuit Breaker para external APIs
   - Timeout Policy para long-running operations
2. 📝 Resolver TODOs críticos (Task #1) - 6 ocorrências
   - JWT no Swagger (ALTA)
   - Refresh token rotation (ALTA)
   - Application Insights (MÉDIA)

### 📊 Sprint 3 - Code Quality & Standards (6-8 horas)
**Prioridade MÉDIA:**
3. 🔧 Resolver CA1849 (sync over async) - 15+ ocorrências
4. 🛡️ Adicionar validação de parâmetros CA1062 - 100+ ocorrências
   - Usar `ArgumentNullException.ThrowIfNull()`
   - Priorizar controllers e services públicos
5. 🌍 Culture-specific operations CA1305/CA1311 - 20+ ocorrências

### 📝 Sprint 4 - Documentação (2-3 horas)
**Prioridade BAIXA:**
6. 📄 Fix markdown linting (366 issues)
7. 📚 Atualizar docs com mudanças recentes
8. 💡 Adicionar exemplos de uso de exception handling

---

## 📝 Notas

- **Build Status**: ✅ 0 erros, 296 warnings (não-bloqueantes)
- **Test Status**: ✅ 33/33 testes passando (100% success rate)
- **ConfigureAwait**: ✅ 178 awaits otimizados em 14 arquivos
- **Exception Handling**: ✅ Abordagem híbrida implementada (16 catch blocks refatorados)
- **Test Coverage**: Não medido (considerar adicionar coverlet)
- **Performance Baseline**: Não estabelecido (considerar BenchmarkDotNet)
- **Security Scan**: Pendente (considerar integrar Snyk/SonarQube)

---

**Última atualização**: 2026-01-15
**Versão**: 1.1.0
**Responsável**: Paulo Eugênio Reis
