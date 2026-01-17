# 📊 Guia de Telemetria e Observabilidade

Este guia explica como configurar e usar telemetria (tracing, metrics, logs) no ProjectTemplate usando OpenTelemetry.

> ⚠️ **IMPORTANTE - Atualização Jaeger (Janeiro 2026):**  
> O pacote `OpenTelemetry.Exporter.Jaeger` foi descontinuado (deprecated/legacy).  
> Este template agora usa **OTLP (OpenTelemetry Protocol)** para enviar dados ao Jaeger.  
> Jaeger suporta nativamente OTLP desde a versão 1.35+.  
> Todas as configurações foram atualizadas para usar OTLP (portas 4317/4318).

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Quick Start](#-quick-start)
- [Configuração por Provedor](#-configuração-por-provedor)
- [Configurações Avançadas](#-configurações-avançadas)
- [Métricas Customizadas](#-métricas-customizadas)
- [O que é Rastreado Automaticamente](#-o-que-é-rastreado-automaticamente)
- [Troubleshooting](#-troubleshooting)
- [Recursos Adicionais](#-recursos-adicionais)
- [Melhores Práticas](#-melhores-práticas)
- [Próximos Passos](#-próximos-passos)

---

## 🎯 Visão Geral

O template suporta **múltiplos backends de telemetria** através do **OpenTelemetry**, permitindo que você escolha o provedor que melhor atende suas necessidades:

### ✅ Provedores Suportados

| Provedor | Tipo | Uso | Custo |
|----------|------|-----|-------|
| **Jaeger (via OTLP)** | Traces | Local/Self-hosted | 🆓 Gratuito |
| **Grafana Cloud** | Traces + Metrics | Cloud/Self-hosted | 💰 Freemium |
| **Prometheus** | Metrics | Local/Self-hosted | 🆓 Gratuito |
| **Application Insights** | APM Completo | Azure Cloud | 💰💰 Pago |
| **Datadog** | APM Completo | Cloud | 💰💰💰 Pago |
| **Dynatrace** | APM Completo | Cloud/On-premise | 💰💰💰 Pago |
| **Console** | Debug | Development | 🆓 Gratuito |

---

## 🚀 Quick Start

### 1️⃣ Habilitar Telemetria (appsettings.json)

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Telemetry": {
        "Enabled": true,
        "Providers": ["jaeger", "prometheus", "console"],
        "SamplingRatio": 1.0
      }
    }
  }
}
```markdown
### 2️⃣ Iniciar Stack Completa com Docker

```bash
docker-compose up -d
```markdown
### 3️⃣ Acessar as UIs

- **Jaeger UI**: http://localhost:16686 (Distributed Tracing)
- **Prometheus**: http://localhost:9090 (Metrics)
- **Grafana**: http://localhost:3000 (Visualization)
  - User: `admin`
  - Password: `admin`

---

## 📋 Configuração por Provedor

### 🔵 Jaeger (Local - Desenvolvimento)

**Melhor para:** Desenvolvimento local, aprendizado, POCs

**Configuração Atualizada (OTLP Protocol):**
```json
{
  "Telemetry": {
    "Enabled": true,
    "Providers": ["jaeger"],
    "Jaeger": {
      "Host": "localhost",
      "Port": 4317,
      "UseGrpc": true
    }
  }
}
```text
**⚠️ Mudança Importante:**
- **Antes (Deprecated):** Porta 6831 (protocolo nativo Jaeger)
- **Agora (Recomendado):** Porta 4317 (OTLP gRPC) ou 4318 (OTLP HTTP)
- O exporter nativo `OpenTelemetry.Exporter.Jaeger` foi removido
- Agora usamos `OpenTelemetry.Exporter.OpenTelemetryProtocol` (OTLP)

**Docker (com OTLP habilitado):**
```bash
docker run -d --name jaeger \
  -e COLLECTOR_OTLP_ENABLED=true \
  -p 16686:16686 \
  -p 4317:4317 \
  -p 4318:4318 \
  jaegertracing/all-in-one:latest
```markdown
**Portas do Jaeger:**
- `16686` - Jaeger UI (Web interface)
- `4317` - OTLP gRPC receiver ✅ **RECOMENDADO**
- `4318` - OTLP HTTP receiver ✅ **RECOMENDADO**
- `14250` - Jaeger gRPC (model.proto)
- `9411` - Zipkin compatible endpoint
- ~~`6831`~~ - ❌ **DEPRECATED** (Jaeger native UDP)

**Acessar UI:** http://localhost:16686

**Documentação:**
- [Jaeger OTLP Support](https://www.jaegertracing.io/docs/1.46/apis/#opentelemetry-protocol-otlp)
- [OpenTelemetry Migration Guide](https://opentelemetry.io/docs/instrumentation/net/exporters/)

---

### 📊 Prometheus + Grafana (Métricas)

**Melhor para:** Monitoramento de métricas, alertas

```json
{
  "Telemetry": {
    "Enabled": true,
    "Providers": ["prometheus"]
  }
}
```markdown
**Métricas disponíveis:**
- `http_server_request_duration_seconds` - Latência HTTP
- `http_server_active_requests` - Requests ativas
- `process_runtime_dotnet_gc_collections_count` - GC collections
- `process_cpu_usage` - CPU usage
- Custom metrics da aplicação

**Endpoint:** http://localhost:5000/metrics

**Visualizar no Grafana:**
1. Acesse http://localhost:3000
2. Datasources já configurados automaticamente
3. Crie dashboards personalizados

---

### ☁️ Application Insights (Azure)

**Melhor para:** Apps hospedados no Azure

```json
{
  "Telemetry": {
    "Enabled": true,
    "Providers": ["applicationinsights"],
    "SamplingRatio": 0.5,
    "ApplicationInsights": {
      "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=https://...",
      "EnableAdaptiveSampling": true,
      "EnableLiveMetrics": true
    }
  }
}
```markdown
**Como obter a Connection String:**
1. Portal Azure → Application Insights
2. Overview → Connection String
3. Copiar e colar no appsettings

**Features:**
- ✅ Live Metrics Stream
- ✅ Application Map
- ✅ Smart Detection
- ✅ Availability Tests
- ✅ Profiler
- ✅ Snapshot Debugger

---

### 🐶 Datadog

**Melhor para:** APM enterprise completo

**Passo 1: Instalar Datadog Agent**
```bash
# Windows
msiexec /qn /i datadog-agent-latest.msi

# Linux
DD_API_KEY=<your-api-key> DD_SITE="datadoghq.com" bash -c "$(curl -L https://s3.amazonaws.com/dd-agent/scripts/install_script.sh)"

# Docker
docker run -d --name datadog-agent \
  -e DD_API_KEY=<your-api-key> \
  -e DD_SITE="datadoghq.com" \
  -e DD_APM_ENABLED=true \
  -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT="0.0.0.0:4317" \
  -p 4317:4317 \
  gcr.io/datadoghq/agent:latest
```text
**Passo 2: Configurar appsettings.json**
```json
{
  "Telemetry": {
    "Enabled": true,
    "Providers": ["datadog"],
    "Datadog": {
      "Endpoint": "http://localhost:4317",
      "ApiKey": "your-api-key-here",
      "Site": "datadoghq.com",
      "Environment": "production"
    }
  }
}
```markdown
**Obter API Key:**
https://app.datadoghq.com/organization-settings/api-keys

---

### 🟣 Dynatrace

**Melhor para:** Enterprise, análise avançada de performance

```json
{
  "Telemetry": {
    "Enabled": true,
    "Providers": ["dynatrace"],
    "Dynatrace": {
      "Endpoint": "https://{your-environment-id}.live.dynatrace.com/api/v2/otlp",
      "ApiToken": "dt0c01.YOUR.TOKEN.HERE",
      "Environment": "production"
    }
  }
}
```markdown
**Como obter credenciais:**
1. Dynatrace UI → Settings → Integration → OpenTelemetry
2. Copy OTLP endpoint
3. Generate API token com scope `openTelemetryTrace.ingest`

---

### 🎨 Grafana Cloud (OTLP)

**Melhor para:** Stack completa gerenciada

```json
{
  "Telemetry": {
    "Enabled": true,
    "Providers": ["otlp"],
    "Otlp": {
      "Endpoint": "https://otlp-gateway-prod-us-east-0.grafana.net/otlp",
      "Protocol": "http",
      "Headers": "Authorization=Basic <base64-encoded-instance-id:token>"
    }
  }
}
```markdown
**Como obter credenciais:**
1. Grafana Cloud → Connections → Add new connection → OpenTelemetry
2. Copy endpoint e token
3. Encode: `echo -n "instance-id:token" | base64`

---

## 🎛️ Configurações Avançadas

### Sampling (Reduzir Volume)

Em produção, use sampling para reduzir custos:

```json
{
  "Telemetry": {
    "Enabled": true,
    "SamplingRatio": 0.1  // 10% das requests
  }
}
```markdown
**Recomendações:**
- **Development**: 1.0 (100%)
- **Staging**: 0.5 (50%)
- **Production (low traffic)**: 0.2 (20%)
- **Production (high traffic)**: 0.05 (5%)

### Desabilitar Instrumentação

```json
{
  "Telemetry": {
    "EnableSqlInstrumentation": false,  // Não rastrear queries SQL
    "EnableHttpInstrumentation": false  // Não rastrear HTTP calls
  }
}
```markdown
### Múltiplos Backends

Você pode usar múltiplos backends simultaneamente:

```json
{
  "Telemetry": {
    "Enabled": true,
    "Providers": ["jaeger", "prometheus", "applicationinsights"]
  }
}
```markdown
**Exemplo:** Jaeger local para debug + Application Insights para produção

---

## 📈 Métricas Customizadas

### Criar Contador

```csharp
public class ProductService : Service<Product>
{
    private readonly Counter<long> _productCreatedCounter;
    
    public ProductService(IRepository<Product> repository, IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("ProjectTemplate.Api");
        _productCreatedCounter = meter.CreateCounter<long>(
            "products.created",
            description: "Total number of products created"
        );
    }
    
    public override async Task<Product> AddAsync(Product entity, CancellationToken ct = default)
    {
        var result = await base.AddAsync(entity, ct);
        _productCreatedCounter.Add(1, new KeyValuePair<string, object>("category", entity.Category));
        return result;
    }
}
```markdown
### Criar Histograma (Latência)

```csharp
private readonly Histogram<double> _requestDuration;

_requestDuration = meter.CreateHistogram<double>(
    "api.request.duration",
    unit: "ms",
    description: "API request duration"
);

var stopwatch = Stopwatch.StartNew();
// ... operação ...
stopwatch.Stop();
_requestDuration.Record(stopwatch.ElapsedMilliseconds);
```markdown
---

## 🔍 O que é Rastreado Automaticamente

### ✅ Traces

- **HTTP Requests**: Todas as requests ASP.NET Core
- **HTTP Client**: Calls externos via HttpClient
- **SQL Queries**: Queries Entity Framework Core e SqlClient
- **Exceptions**: Exceções capturadas automaticamente

### ✅ Métricas

- **HTTP**: Request count, duration, status codes
- **Runtime**: GC collections, heap size, thread pool
- **Process**: CPU usage, memory, uptime
- **Custom**: Suas métricas personalizadas

---

## 🐛 Troubleshooting

### Telemetria não aparece

**1. Verificar se está habilitado:**
```json
"Enabled": true
```text
**2. Verificar logs de startup:**
```
✅ Telemetry enabled: jaeger, prometheus
  📊 Jaeger exporter enabled: localhost:6831
  📈 Prometheus exporter enabled (endpoint: /metrics)
```text
**3. Testar endpoint Prometheus:**
```bash
curl http://localhost:5000/metrics
```markdown
### Jaeger não recebe traces

**Verificar se Jaeger está rodando:**
```bash
docker ps | grep jaeger
```text
**Testar conectividade:**
```bash
telnet localhost 6831
```text
**Verificar logs:**
```bash
docker logs jaeger
```

### Application Insights não funciona

**Verificar Connection String:**
- Deve começar com `InstrumentationKey=`
- Incluir `IngestionEndpoint=`

**Verificar no Azure Portal:**
- Application Insights → Live Metrics
- Deve aparecer servidor conectado

---

## 📚 Recursos Adicionais

### Documentação Oficial

- **OpenTelemetry**: https://opentelemetry.io/docs/
- **Jaeger**: https://www.jaegertracing.io/docs/
- **Prometheus**: https://prometheus.io/docs/
- **Grafana**: https://grafana.com/docs/
- **Application Insights**: https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview
- **Datadog**: https://docs.datadoghq.com/tracing/
- **Dynatrace**: https://www.dynatrace.com/support/help/

### Dashboards Prontos

- **Grafana Dashboards**: https://grafana.com/grafana/dashboards/
  - .NET Runtime: https://grafana.com/grafana/dashboards/13413
  - ASP.NET Core: https://grafana.com/grafana/dashboards/15651

---

## 💡 Melhores Práticas

### ✅ DO

- ✅ Use sampling em produção (0.05 - 0.2)
- ✅ Monitore custos de telemetria
- ✅ Use tags descritivas em metrics
- ✅ Configure alertas no Grafana/Datadog
- ✅ Revise traces regularmente

### ❌ DON'T

- ❌ Não logar dados sensíveis (passwords, tokens)
- ❌ Não usar sampling 1.0 em produção high-traffic
- ❌ Não ignorar custos de Application Insights/Datadog
- ❌ Não rastrear health checks (filtrado automaticamente)

---

## 🎯 Próximos Passos

1. **Escolha seu provedor** baseado no seu ambiente
2. **Configure appsettings.json** com as credenciais
3. **Inicie a aplicação** e gere tráfego
4. **Acesse a UI** do provedor escolhido
5. **Crie dashboards** e alertas personalizados
6. **Configure alertas** para anomalias

---

**Dúvidas?** Abra uma issue no repositório!
