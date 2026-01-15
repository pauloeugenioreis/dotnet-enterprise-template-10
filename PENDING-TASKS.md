# 📋 Tarefas Pendentes - Clean Architecture Template

## ✅ CONCLUÍDO (8/14 - 57%)

### Infraestrutura e Configuração
- [x] **.editorconfig** - Regras de estilo e análise de código
- [x] **Directory.Build.props** - Configuração centralizada de build
- [x] **.dockerignore** - Otimização de build Docker

### Segurança e Performance
- [x] **HTTP Security Headers** - HSTS, XContentTypeOptions, ReferrerPolicy, XXssProtection, Xfo
- [x] **Output Caching** - 3 políticas configuradas (10s, 300s, 600s)
- [x] **Random → RandomNumberGenerator** - 33 erros CA5394 eliminados

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

## ⏳ PENDENTES (5/14 - 36%)

### 1.  Resolver TODOs no Codebase (6 ocorrências)

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

### 2. ⚠️ Melhorar Exception Handling (16 generic catch blocks)

#### Locais com `catch (Exception ex)` genérico:

**Application Layer:**
```csharp
src/Application/Services/Service.cs
├── GetByIdAsync()          - linha 27
├── GetAllAsync()           - linha 40
├── CreateAsync()           - linha 55
├── UpdateAsync()           - linha 75
├── DeleteAsync()           - linha 95
└── GetPagedAsync()         - linha 111
```

**Infrastructure Layer:**
```csharp
src/Infrastructure/Services/
├── StorageService.cs       - 3 métodos (linhas 42, 62, 79)
├── JwtTokenService.cs      - ValidateAccessTokenAsync() (linha 110)

src/Infrastructure/Middleware/
└── GlobalExceptionHandler.cs - 3 handlers (linhas 37, 73, 130)

src/Infrastructure/Extensions/
└── StorageExtension.cs     - CreateStorageClient() (linha 45)
```

**Outros:**
```csharp
src/Infrastructure/Services/
├── ExceptionNotificationService.cs  - linha 50
├── MartenEventStore.cs             - linha 279
```

**Ação requerida**: Criar exceções específicas e tratamentos apropriados
**Tempo estimado**: 3-4 horas

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
```
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
```
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
├── CHANGELOG.md
├── CONTRIBUTING.md
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

```
[████████████████████░░░░] 64% Concluído

✅ Concluído:     9 tarefas
⏳ Pendente:      5 tarefas
⏱️  Tempo estimado: 18-26 horas
```

---

## 🎯 Próximos Passos Recomendados

### Sprint 1 - Performance & Segurança (Semana 1)
1. ✅ ConfigureAwait completo → **CONCLUÍDO** 🎉
2. Resolver CA1849 (sync over async)
3. Implementar Polly básico (Retry + Circuit Breaker)

### Sprint 2 - Code Quality (Semana 2)
4. Melhorar exception handling
5. Resolver TODOs críticos
6. Adicionar validação de parâmetros (CA1062)

### Sprint 3 - Documentação (Semana 3)
7. Fix markdown linting
8. Atualizar documentação com mudanças
9. Adicionar exemplos de uso

---

## 📝 Notas

- **Build Status**: ✅ 0 erros, 500 warnings (não-bloqueantes)
- **Test Status**: ✅ 33/33 testes passando
- **ConfigureAwait**: ✅ 178 awaits otimizados em 14 arquivos
- **Test Coverage**: Não medido (considerar adicionar coverlet)
- **Performance Baseline**: Não estabelecido (considerar BenchmarkDotNet)
- **Security Scan**: Pendente (considerar integrar Snyk/SonarQube)

---

**Última atualização**: 2026-01-14
**Versão**: 1.0.0
**Responsável**: Paulo Eugênio Reis
