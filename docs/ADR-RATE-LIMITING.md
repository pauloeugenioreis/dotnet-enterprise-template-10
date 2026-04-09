# ADR: Rate Limiting Strategy - Migração para .NET Native

**Status:** ACCEPTED (Migração concluída)
**Date:** January 14, 2026
**Updated:** April 2026
**Decision Makers:** Architecture Team

---

## Context

O projeto inicialmente utilizava `AspNetCoreRateLimit` v5.0.0 para gerenciamento de rate limiting. O .NET 7+ introduziu rate limiting nativo através de `Microsoft.AspNetCore.RateLimiting`, oferecendo uma alternativa oficial da Microsoft.

Após avaliação, a migração para .NET native rate limiting foi realizada com sucesso.

### Implementação Atual (.NET Native)

```csharp
// RateLimitingExtension.cs - Implementação atual
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt => { ... });
    options.AddSlidingWindowLimiter("sliding", opt => { ... });
    options.AddTokenBucketLimiter("token", opt => { ... });
    options.AddConcurrencyLimiter("concurrent", opt => { ... });
});

app.UseRateLimiter();
```

### Policy Names

| Policy | Estratégia | Uso |
|--------|-----------|-----|
| `"fixed"` | Fixed Window | Endpoints públicos simples |
| `"sliding"` | Sliding Window | APIs de alta performance |
| `"token"` | Token Bucket | Endpoints que permitem bursts |
| `"concurrent"` | Concurrency | Operações pesadas/DB |

---

## Decision

**MIGRAR** para .NET native rate limiting (`Microsoft.AspNetCore.RateLimiting`).

### Rationale

- ✅ **Suporte oficial Microsoft** — parte do framework, manutenção garantida
- ✅ **Zero dependências externas** — removeu `AspNetCoreRateLimit` do projeto
- ✅ **Performance integrada** — integração mais profunda com o pipeline ASP.NET Core
- ✅ **Configuração via appsettings.json** — implementada via `RateLimitingExtension.cs` que lê `AppSettings.Infrastructure.RateLimiting`
- ✅ **IP Whitelisting** — implementado com suporte a CIDR e X-Forwarded-For
- ✅ **Custom response** — resposta JSON + headers X-RateLimit-* implementados
- ✅ **4 estratégias** — Fixed Window, Sliding Window, Token Bucket, Concurrency
- ✅ **Integração OpenTelemetry** — métricas de rate limiting exportadas

---

## Comparison (Post-Migration)

| Feature | Antes (AspNetCoreRateLimit) | Agora (.NET Native) |
|---------|----------------------------|---------------------|
| **Suporte** | Community | ✅ Microsoft |
| **Performance** | Excelente | ✅ Ligeiramente melhor |
| **Dependências** | Package externo | ✅ Built-in |
| **Configuração JSON** | ✅ Nativa | ✅ Via extension |
| **IP Whitelisting** | ✅ Built-in | ✅ Implementado |
| **Custom Messages** | ✅ Built-in | ✅ Implementado |
| **Future-proofing** | ⚠️ Incerto | ✅ Garantido |

---

## References

- [.NET Rate Limiting Documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Project RATE-LIMITING.md](RATE-LIMITING.md)
- Implementation: `src/Infrastructure/Extensions/RateLimitingExtension.cs`

---

## Review History

| Date       | Decision | Rationale                                                   |
| ---------- | -------- | ----------------------------------------------------------- |
| 2026-01-14 | DEFERRED | AspNetCoreRateLimit funcional, migração não justificada     |
| 2026-04    | ACCEPTED | Migração para .NET native concluída com sucesso             |
