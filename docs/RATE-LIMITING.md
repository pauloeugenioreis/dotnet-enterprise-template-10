# Rate Limiting - Controle de Taxa de Requisições

Esta documentação descreve o sistema de **Rate Limiting** implementado no template, permitindo controlar a taxa de requisições para proteger a API contra abusos, DDoS e uso excessivo.

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Configuração](#-configuração)
- [Estratégias de Limitação](#-estratégias-de-limitação)
- [Como Usar](#-como-usar)
- [Resposta de Rate Limit Excedido (429)](#-resposta-de-rate-limit-excedido-429)
- [Whitelist de IPs](#-whitelist-de-ips)
- [Testando Rate Limiting](#-testando-rate-limiting)
- [Melhores Práticas](#-melhores-práticas)

---

## 🎯 Visão Geral

O Rate Limiting controla quantas requisições um cliente pode fazer em um determinado período de tempo. Esta implementação oferece:

- ✅ **4 Estratégias de Limitação** (Fixed Window, Sliding Window, Token Bucket, Concurrency)
- ✅ **Configuração via appsettings.json** (liga/desliga sem recompilar)
- ✅ **Resposta HTTP 429** (Too Many Requests) padronizada
- ✅ **Headers X-RateLimit-*** informativos
- ✅ **IP Whitelisting** (IPs confiáveis sem limite)
- ✅ **Suporte a proxies reversos** (X-Forwarded-For, X-Real-IP)
- ✅ **Integração com OpenTelemetry** para observabilidade

### Por que usar Rate Limiting?

1. **Proteger contra DDoS**: Evita que um único cliente sobrecarregue o servidor
2. **Garantir Fair Usage**: Distribui recursos entre todos os usuários
3. **Reduzir custos**: Evita consumo excessivo de recursos de infraestrutura
4. **Melhorar SLA**: Garante disponibilidade para todos os clientes

---

## ⚙️ Configuração

### 1. Estrutura de Configuração (`appsettings.json`)

{
  "AppSettings": {
    "Infrastructure": {
      "RateLimiting": {
        "Enabled": false,                    // Habilita/desabilita Rate Limiting
        "EnableWhitelist": false,            // Ativa whitelist de IPs
        "WhitelistedIps": [],                // Lista de IPs sem limitação
        "Policies": {
          "FixedWindow": { ... },
          "SlidingWindow": { ... },
          "TokenBucket": { ... },
          "Concurrency": { ... }
        }
      }
    }
  }
}
### 2. Habilitar Rate Limiting

No `appsettings.json` ou `appsettings.Production.json`:

{
  "AppSettings": {
    "Infrastructure": {
      "RateLimiting": {
        "Enabled": true,                     // 🔥 Habilitar
        "EnableWhitelist": true,
        "WhitelistedIps": [
          "192.168.1.100",                   // IP do servidor interno
          "10.0.0.0/24"                      // Rede interna (CIDR)
        ],
        "Policies": {
          "FixedWindow": {
            "Enabled": true,
            "PermitLimit": 100,              // 100 requests
            "WindowSeconds": 60,             // por minuto
            "QueueLimit": 10                 // Fila de até 10 requests
          },
          "SlidingWindow": {
            "Enabled": true,
            "PermitLimit": 200,
            "WindowSeconds": 60,
            "SegmentsPerWindow": 6           // Divide janela em 6 segmentos
          },
          "TokenBucket": {
            "Enabled": true,
            "TokenLimit": 50,                // 50 tokens no balde
            "ReplenishmentPeriodSeconds": 10,
            "TokensPerPeriod": 10            // +10 tokens a cada 10 segundos
          },
          "Concurrency": {
            "Enabled": true,
            "PermitLimit": 10,               // 10 requisições simultâneas
            "QueueLimit": 20                 // Fila de até 20 requisições
          }
        }
      }
    }
  }
}
---

## 📊 Estratégias de Limitação

### 1. **Fixed Window** (Janela Fixa)

Limita requisições em janelas de tempo fixas.

**Configuração:**
"FixedWindow": {
  "Enabled": true,
  "PermitLimit": 100,      // 100 requests
  "WindowSeconds": 60,     // por minuto
  "QueueLimit": 10
}
**Comportamento:**
- 100 requisições permitidas a cada 60 segundos
- Janela reseta completamente ao final do período
- Simples e previsível

**Quando usar:**
- APIs públicas com limites claros
- Quando simplicidade é mais importante que precisão

**Exemplo:**
```text
00:00 → 00:59 = 100 requests permitidas
01:00 → 01:59 = Reset, 100 requests permitidas novamente
---

### 2. **Sliding Window** (Janela Deslizante)

Suaviza limites calculando média móvel de requisições.

**Configuração:**
"SlidingWindow": {
  "Enabled": true,
  "PermitLimit": 200,
  "WindowSeconds": 60,
  "SegmentsPerWindow": 6   // Divide em 6 segmentos de 10s
}
**Comportamento:**
- Janela "desliza" suavemente ao longo do tempo
- Evita picos no início de cada janela
- Mais justo que Fixed Window

**Quando usar:**
- APIs com alto volume de tráfego
- Quando precisão é importante
- Para evitar "burst" no reset da janela

**Exemplo:**
```
```text
00:00-00:10 = 33 requests
00:10-00:20 = 33 requests
00:20-00:30 = 33 requests
...
Janela desliza continuamente
---

### 3. **Token Bucket** (Balde de Tokens)

Usa "tokens" que são consumidos e reabastecidos ao longo do tempo.

**Configuração:**
"TokenBucket": {
  "Enabled": true,
  "TokenLimit": 50,                      // Capacidade máxima
  "ReplenishmentPeriodSeconds": 10,      // Reabastecer a cada 10s
  "TokensPerPeriod": 10                  // +10 tokens por período
}
**Comportamento:**
- Cada requisição consome 1 token
- Tokens são reabastecidos continuamente
- Permite bursts curtos (até `TokenLimit`)
- Taxa sustentada = `TokensPerPeriod / ReplenishmentPeriodSeconds`

**Quando usar:**
- APIs que permitem bursts ocasionais
- Quando taxa sustentada é mais importante que picos
- Algoritmo mais flexível e realista

**Exemplo:**
```
```text
Balde começa com 50 tokens
Request 1 → 49 tokens
Request 2 → 48 tokens
...
A cada 10s, +10 tokens (até limite de 50)
**Taxa Sustentada:**
```
```bash
10 tokens / 10 segundos = 1 req/s = 60 req/min
---

### 4. **Concurrency** (Concorrência)

Limita requisições **simultâneas** (não por período de tempo).

**Configuração:**
"Concurrency": {
  "Enabled": true,
  "PermitLimit": 10,      // Máximo 10 requisições simultâneas
  "QueueLimit": 20        // Fila de até 20 aguardando
}
**Comportamento:**
- Controla quantas requisições podem ser processadas ao mesmo tempo
- Quando limite é atingido, novas requisições aguardam na fila
- Se fila encher, retorna 429

**Quando usar:**
- Proteger recursos limitados (conexões DB, threads)
- APIs com operações pesadas/demoradas
- Controle de concorrência global

**Exemplo:**
```
```csharp
10 requisições processando simultaneamente
Request 11 → aguarda na fila
Request 31 → fila cheia, retorna 429
Quando uma requisição termina, próxima da fila é processada
---

## 🎮 Como Usar

### 1. Aplicar Policy em um Endpoint

Use o atributo `[EnableRateLimiting]` no controller ou action:

using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    // Aplica política Fixed Window em todo o controller
    [EnableRateLimiting("fixed")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // ...
    }

    // Aplica política Token Bucket apenas neste endpoint
    [EnableRateLimiting("token")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        // ...
    }

    // Sem rate limiting (público)
    [DisableRateLimiting]
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic()
    {
        // ...
    }
}
### 2. Nomes de Policies Disponíveis

| Nome do Policy | Descrição | Uso Recomendado |
|---|---|---|
| `fixed` | Fixed Window | Endpoints públicos simples |
| `sliding` | Sliding Window | APIs de alta performance |
| `token` | Token Bucket | Endpoints que permitem bursts |
| `concurrent` | Concurrency | Operações pesadas/DB |

### 3. Exemplo de Diferentes Endpoints

[Route("api/v1/[controller]")]
public class OrderController : ControllerBase
{
    // Leitura: limite generoso (Sliding Window)
    [EnableRateLimiting("sliding")]
    [HttpGet]
    public async Task<IActionResult> GetAll() { ... }

    // Escrita: limite mais restritivo (Fixed Window)
    [EnableRateLimiting("fixed")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Order order) { ... }

    // Operação pesada: limite de concorrência
    [EnableRateLimiting("concurrent")]
    [HttpGet("ExportToExcel")]
    public async Task<IActionResult> ExportToExcel() { ... }

    // Operação crítica: Token Bucket (permite bursts)
    [EnableRateLimiting("token")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusDto dto) { ... }
}
---

## ⚠️ Resposta de Rate Limit Excedido (429)

Quando o limite é excedido, a API retorna **HTTP 429 Too Many Requests** com:

### Headers de Resposta

```
```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1705330260
Retry-After: 45
Content-Type: application/json
### Body JSON

{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Limit: 100 per window.",
  "clientIp": "192.168.1.100",
  "retryAfter": 45,
  "resetAt": "2024-01-15T10:51:00.0000000Z"
}
### Descrição dos Headers

| Header | Descrição |
|---|---|
| `X-RateLimit-Limit` | Número máximo de requisições permitidas |
| `X-RateLimit-Remaining` | Requisições restantes na janela atual |
| `X-RateLimit-Reset` | Timestamp Unix de quando o limite reseta |
| `Retry-After` | Segundos até poder tentar novamente |

---

## 🔒 Whitelist de IPs

Permite que IPs confiáveis façam requisições sem limitação.

### Configuração

{
  "RateLimiting": {
    "Enabled": true,
    "EnableWhitelist": true,
    "WhitelistedIps": [
      "192.168.1.100",           // IP único
      "10.0.0.0/24",             // Rede CIDR
      "2001:db8::/32",           // IPv6
      "172.16.0.0/16"            // Rede privada
    ]
  }
}
### Quando usar Whitelist

- ✅ Servidores internos (CI/CD, monitoramento)
- ✅ IPs de parceiros de integração
- ✅ Ambientes de desenvolvimento/staging
- ✅ Load balancers e proxies reversos

### Detecção de IP

A implementação detecta o IP real do cliente considerando:

1. **X-Forwarded-For** (proxies, load balancers)
2. **X-Real-IP** (nginx, outros proxies)
3. **RemoteIpAddress** (conexão direta)

---

## 🧪 Testando Rate Limiting

### 1. Teste Manual com curl

**Enviar múltiplas requisições:**

# Teste Fixed Window (100 req/min)
for i in {1..105}; do
  curl -i http://localhost:5000/api/v1/Product
  echo "Request $i"
done
**Verificar headers:**

curl -i http://localhost:5000/api/v1/Product | grep -i "x-ratelimit"
**Resultado esperado (após 100 requests):**

```
```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1705330260
Retry-After: 42
### 2. Teste com PowerShell

# Testar Fixed Window
1..105 | ForEach-Object {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/Product" -Method Get -SkipHttpErrorCheck
    Write-Host "Request $_: $($response.StatusCode)"
}
### 3. Teste de Carga com k6

```
```javascript
// rate-limit-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 10,        // 10 usuários virtuais
  duration: '30s',
};

export default function () {
  let res = http.get('http://localhost:5000/api/v1/Product');

  check(res, {
    'status is 200': (r) => r.status === 200,
    'status is 429': (r) => r.status === 429,
  });

  console.log(`Status: ${res.status}, RateLimit-Remaining: ${res.headers['X-Ratelimit-Remaining']}`);
}
**Executar:**

k6 run rate-limit-test.js
### 4. Teste de Whitelist

**1. Adicionar IP ao whitelist:**

{
  "RateLimiting": {
    "EnableWhitelist": true,
    "WhitelistedIps": ["127.0.0.1", "::1"]
  }
}
**2. Executar 200 requests (acima do limite):**

for i in {1..200}; do
  curl -s http://localhost:5000/api/v1/Product > /dev/null
done
echo "Todas as 200 requests foram bem-sucedidas!"
---

## 📈 Monitoramento e Observabilidade

### 1. Logs

O Rate Limiting gera logs automáticos:

```
```csharp
⚠️  Rate Limiting is disabled
✅  Rate Limiting enabled: 4 policies configured
📊  Fixed Window: 100 req/60s
📊  Sliding Window: 200 req/60s (6 segments)
📊  Token Bucket: 50 tokens, refill 10/10s
📊  Concurrency: 10 simultaneous requests
### 2. OpenTelemetry Spans

Quando Rate Limiting é rejeitado, um span `RateLimitRejected` é criado:

```
```csharp
Span: RateLimitRejected
  - client_ip: 192.168.1.100
  - policy: fixed
  - limit: 100
  - retry_after: 45
### 3. Métricas (com Prometheus)

Você pode adicionar métricas customizadas:

// Em RateLimitingExtension.cs
var meter = new Meter("RateLimiting");
var rateLimitCounter = meter.CreateCounter<long>("rate_limit_rejections");

options.OnRejected = async (context, cancellationToken) =>
{
    rateLimitCounter.Add(1, new KeyValuePair<string, object>("policy", "fixed"));
    // ...
};
---

## 🎯 Melhores Práticas

### 1. Escolha da Estratégia por Endpoint

| Tipo de Endpoint | Estratégia Recomendada | Motivo |
|---|---|---|
| GET simples (leitura) | `sliding` | Alta performance, suaviza picos |
| POST/PUT (escrita) | `fixed` | Controle claro e previsível |
| Export/Download | `concurrent` | Limita recursos pesados |
| APIs públicas | `token` | Permite bursts ocasionais |

### 2. Configuração por Ambiente

**Development** (`appsettings.Development.json`):

{
  "RateLimiting": {
    "Enabled": false   // Desabilitado em dev
  }
}
**Production** (`appsettings.Production.json`):

{
  "RateLimiting": {
    "Enabled": true,
    "Policies": {
      "FixedWindow": {
        "PermitLimit": 100,
        "WindowSeconds": 60
      }
    }
  }
}
### 3. Limites Recomendados

| Tipo de API | Fixed Window | Token Bucket | Concurrency |
|---|---|---|---|
| **API Pública** | 100 req/min | 50 tokens, 10/10s | 5 simultâneas |
| **API Autenticada** | 1000 req/min | 500 tokens, 100/10s | 20 simultâneas |
| **API Interna** | 5000 req/min | 2000 tokens, 500/10s | 50 simultâneas |
| **API Premium** | 10000 req/min | 5000 tokens, 1000/10s | 100 simultâneas |

### 4. Combinar com Autenticação

[Authorize]  // Requer autenticação
[EnableRateLimiting("token")]  // + Rate Limiting
[HttpPost]
public async Task<IActionResult> Create([FromBody] Order order)
{
    // Apenas usuários autenticados com rate limiting
}
### 5. Documentar Limites na API

No Swagger/OpenAPI:

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "Rate Limits: 100 req/min (public), 1000 req/min (authenticated)"
    });
});
### 6. Resposta Proativa

Configure o cliente para respeitar `Retry-After`:

// Cliente C#
if (response.StatusCode == HttpStatusCode.TooManyRequests)
{
    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
    await Task.Delay(retryAfter);
    // Tenta novamente
}
---

## 🔧 Troubleshooting

### Problema: Rate Limiting não está funcionando

**Solução:**

1. Verificar se `Enabled: true` no appsettings.json
2. Verificar se policy está aplicada no endpoint com `[EnableRateLimiting("policy-name")]`
3. Verificar logs de inicialização:

```
```csharp
✅  Rate Limiting enabled: 4 policies configured
### Problema: IP sempre whitelistado

**Solução:**

1. Verificar se `EnableWhitelist: true`
2. Verificar se IP está na lista `WhitelistedIps`
3. Considerar proxies (X-Forwarded-For pode mudar IP detectado)

### Problema: Todos os clientes compartilham o mesmo limite

**Solução:**

O Rate Limiting padrão é **global por policy**. Para limitar **por cliente/IP**, você pode:

1. Criar policies personalizadas por IP
2. Usar bibliotecas como `AspNetCoreRateLimit`
3. Implementar partição customizada no `AddRateLimiter`

---

## 📚 Referências

- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [RFC 6585 - HTTP Status Code 429](https://tools.ietf.org/html/rfc6585)
- [IETF Draft - RateLimit Headers](https://datatracker.ietf.org/doc/html/draft-polli-ratelimit-headers)
- [Token Bucket Algorithm](https://en.wikipedia.org/wiki/Token_bucket)
- [Sliding Window Algorithm](https://konghq.com/blog/how-to-design-a-scalable-rate-limiting-algorithm)

---

## 📝 Exemplo Completo

// ProductController.cs
[Route("api/v1/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    // Leitura pública: Sliding Window (suave)
    [EnableRateLimiting("sliding")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] string? category)
    {
        // 200 req/min permitidas (janela deslizante)
        var products = await _productService.GetAll(isActive, category);
        return Ok(products);
    }

    // Criação: Fixed Window (previsível)
    [Authorize]
    [EnableRateLimiting("fixed")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        // 100 req/min permitidas (janela fixa)
        await _productService.Create(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // Export pesado: Concurrency (limita processamento simultâneo)
    [EnableRateLimiting("concurrent")]
    [HttpGet("ExportToExcel")]
    public async Task<IActionResult> ExportToExcel()
    {
        // Máximo 10 exports simultâneos
        var file = await _productService.ExportToExcel();
        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    // Endpoint público (sem limitação)
    [DisableRateLimiting]
    [HttpGet("health")]
    public IActionResult Health() => Ok("API is running");
}
```

---

**Template Version:** 1.0.0
**Last Updated:** 2024-01-15
**Author:** Enterprise Template Team
