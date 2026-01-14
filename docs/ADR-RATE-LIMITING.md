# ADR: Rate Limiting Strategy - AspNetCoreRateLimit vs .NET Native

**Status:** DEFERRED  
**Date:** January 14, 2026  
**Decision Makers:** Architecture Team  
**Next Review:** Q2 2026

---

## Context

O projeto atualmente utiliza `AspNetCoreRateLimit` v5.0.0 para gerenciamento de rate limiting. O .NET 7+ introduziu rate limiting nativo através de `Microsoft.AspNetCore.RateLimiting`, oferecendo uma alternativa oficial da Microsoft.

### Current Implementation

```csharp
// AspNetCoreRateLimit - Current
services.AddMemoryCache();
services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
services.AddInMemoryRateLimiting();
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

app.UseIpRateLimiting();
```

**Features in Use:**
- ✅ IP-based rate limiting
- ✅ Client ID rate limiting
- ✅ Endpoint-specific rules
- ✅ Memory cache storage
- ✅ Distributed cache support (Redis)
- ✅ Whitelist/blacklist
- ✅ Custom response messages

### Native .NET Alternative

```csharp
// .NET Native Rate Limiting
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
    });
});

app.UseRateLimiter();
```

---

## Decision

**MANTER** `AspNetCoreRateLimit` v5.0.0 por enquanto, com migração planejada para .NET native no futuro.

### Rationale

#### Pros - AspNetCoreRateLimit
- ✅ **Implementação madura e battle-tested** (5+ anos em produção)
- ✅ **Features avançadas** já implementadas (whitelist, custom messages, distributed cache)
- ✅ **Configuração JSON completa** no appsettings.json
- ✅ **4 estratégias diferentes** implementadas e testadas no projeto
- ✅ **Zero breaking changes** - já funciona perfeitamente
- ✅ **Documentação completa** em [RATE-LIMITING.md](RATE-LIMITING.md)

#### Pros - .NET Native
- ✅ **Suporte oficial Microsoft** (parte do framework)
- ✅ **Performance ligeiramente melhor** (integração mais profunda)
- ✅ **Menos dependências externas**
- ✅ **Syntax moderna** com Minimal APIs

#### Cons - AspNetCoreRateLimit
- ⚠️ **Baixa atividade de manutenção** (último commit significativo em 2023)
- ⚠️ **Dependência externa** adicional
- ⚠️ **Compatibilidade futura** incerta com .NET 11+

#### Cons - .NET Native
- ❌ **Migração exige refatoração significativa** (3-5 dias de trabalho)
- ❌ **Menos features out-of-the-box** (precisaria implementar whitelist, custom messages, etc)
- ❌ **Curva de aprendizado** para desenvolvedores acostumados com AspNetCoreRateLimit
- ❌ **Configuração via código** ao invés de JSON

---

## Consequences

### Immediate Actions (Q1 2026)
- ✅ **Manter versão atual** - AspNetCoreRateLimit v5.0.0 funciona perfeitamente
- ✅ **Monitorar issues** no repositório GitHub do pacote
- ✅ **Documentar decisão** neste ADR

### Short-term (Q2-Q3 2026)
- 🔄 **Avaliar novamente** quando .NET 11 for lançado
- 🔄 **Verificar roadmap** do AspNetCoreRateLimit
- 🔄 **Avaliar features** adicionadas ao .NET native rate limiting
- 🔄 **Criar PoC** com .NET native se necessário

### Long-term (2027+)
- 🎯 **Migrar para .NET native** se:
  - AspNetCoreRateLimit não receber atualizações por 12+ meses
  - .NET native adicionar features equivalentes
  - Surgir incompatibilidade crítica com futuras versões do .NET
  
- 🎯 **Criar guia de migração** quando decisão for tomada
- 🎯 **Implementar features faltantes** (whitelist, custom messages, etc)

---

## Comparison Matrix

| Feature | AspNetCoreRateLimit | .NET Native | Winner |
|---------|---------------------|-------------|---------|
| **Maturity** | 5+ years | 2+ years | 🏆 AspNetCoreRateLimit |
| **Official Support** | Community | Microsoft | 🏆 .NET Native |
| **Configuration** | JSON-based | Code-based | 🏆 AspNetCoreRateLimit |
| **Features** | Rich (whitelist, blacklist, custom messages) | Basic | 🏆 AspNetCoreRateLimit |
| **Performance** | Excellent | Slightly better | 🏆 .NET Native |
| **Breaking Changes** | None | Significant refactoring | 🏆 AspNetCoreRateLimit |
| **Maintenance** | Low activity | Active | 🏆 .NET Native |
| **Documentation** | Extensive | Growing | 🏆 AspNetCoreRateLimit |
| **Future-proofing** | Uncertain | Guaranteed | 🏆 .NET Native |

**Current Score:** AspNetCoreRateLimit 6 - .NET Native 3

---

## Technical Debt

**Risk Level:** 🟡 **LOW-MEDIUM**

- **Current:** Funciona perfeitamente, zero issues
- **6 months:** Baixo risco, monitoramento recomendado
- **12 months:** Médio risco, considerar migração
- **24+ months:** Alto risco, migração recomendada

**Estimated Migration Effort:**
- Analysis & Planning: 1 day
- Implementation: 2-3 days
- Testing: 1 day
- Documentation: 0.5 day
- **Total:** 4.5-5.5 days

---

## References

- [AspNetCoreRateLimit GitHub](https://github.com/stefanprodan/AspNetCoreRateLimit)
- [.NET Rate Limiting Documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Project RATE-LIMITING.md](RATE-LIMITING.md)
- [NuGet Package - AspNetCoreRateLimit](https://www.nuget.org/packages/AspNetCoreRateLimit)

---

## Review History

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-01-14 | DEFERRED | Package works perfectly, migration effort not justified yet |
| Q2 2026 | PENDING | Next review scheduled |

---

## Notes

- ✅ AspNetCoreRateLimit é **stable e funcional** para .NET 10
- ✅ Não há **urgência** para migração
- ✅ Decisão é **reversível** e pode ser reavaliada
- ⚠️ Monitorar **activity** do repositório GitHub
- 📅 **Próxima revisão:** Q2 2026 (Abril-Junho)

**Conclusion:** A decisão de **manter AspNetCoreRateLimit** é pragmática e baseada em custo-benefício. A migração deve ser considerada quando houver um motivo técnico forte ou estratégico, não apenas por preferência tecnológica.
