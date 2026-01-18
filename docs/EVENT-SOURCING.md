# 📜 Event Sourcing - Guia Completo

## Índice

- [O Que É Event Sourcing?](#o-que-e-event-sourcing)
- [Por Que Usar Event Sourcing?](#por-que-usar-event-sourcing)
- [Arquitetura](#arquitetura)
- [Modos de Operação](#modos-de-operação)
- [Configuração](#configuracao)
- [Uso Básico](#uso-basico)
- [API de Auditoria](#api-de-auditoria)
- [Eventos Disponíveis](#eventos-disponiveis)
- [Time Travel](#time-travel)
- [Melhores Práticas](#melhores-praticas)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)

---

<a id="o-que-e-event-sourcing"></a>

## O Que É Event Sourcing?

**Event Sourcing** é um padrão de arquitetura onde o estado da aplicação é determinado por uma sequência de eventos, ao invés de armazenar apenas o estado atual.

### Abordagem Tradicional vs Event Sourcing

#### ❌ **CRUD Tradicional**

- Você só tem o estado atual

```json
{
  "Id": 1,
  "Status": "Delivered",
  "Total": 150.0,
  "UpdatedAt": "2026-01-14"
}

```

- Perdeu: quem criou? quando mudou o status? por quê?

#### ✅ **Event Sourcing**

Eventos registrados:

```text
1. OrderCreated     { OrderId: 1, Total: 150.00, CreatedBy: "user@email.com" } 2026-01-10 10:00
2. OrderApproved    { OrderId: 1, ApprovedBy: "manager@email.com" }           2026-01-10 14:30
3. OrderShipped     { OrderId: 1, Carrier: "FedEx", TrackingCode: "ABC123" } 2026-01-11 09:00
4. OrderDelivered   { OrderId: 1, DeliveredAt: "2026-01-14 16:00" }           2026-01-14 16:00

```

Resultado: histórico completo, auditoria total, rastreabilidade e time travel.

<a id="por-que-usar-event-sourcing"></a>

## Por Que Usar Event Sourcing?

### ✅ **Vantagens**

| Benefício                     | Descrição                                                          |
| ----------------------------- | ------------------------------------------------------------------ |
| **Auditoria Completa**        | Histórico completo de todas as mudanças com quem, quando e por quê |
| **Time Travel**               | Veja o estado de qualquer entidade em qualquer momento do passado  |
| **Rastreabilidade**           | Compliance regulatório (SOX, GDPR, HIPAA, etc.)                    |
| **Debug Avançado**            | Replay de eventos para reproduzir bugs                             |
| **Análise de Negócio**        | Insights sobre comportamento do usuário e fluxos                   |
| **Event-Driven Architecture** | Base para CQRS, microserviços e event streaming                    |

### ⚠️ **Quando Usar**

- Sistemas financeiros (transações, pagamentos)
- E-commerce (pedidos, estoque, preços)
- Healthcare (prontuários, prescrições)
- Sistemas com requisitos de auditoria
- Aplicações com histórico crítico

### ⚠️ **Quando Não Usar**

- Aplicações CRUD simples sem requisitos de auditoria
- Sistemas com baixa necessidade de rastreabilidade
- Protótipos ou MVPs
- Quando a equipe não tem experiência com o padrão

---

<a id="arquitetura"></a>

## Arquitetura

```json
┌─────────────────┐
│   Controller    │  HTTP Request (POST /orders)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Application    │  CreateOrderCommand
│    Service      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ HybridRepository│  Dual Write Strategy
│  (Data Layer)   │
└───┬─────────┬───┘
    │         │
    ▼         ▼
┌────────┐ ┌────────────┐
│EF Core │ │Event Store │
│(SQLSrv)│ │(PostgreSQL)│
└────────┘ └────────────┘
    │            │
    ▼            ▼
  [Read]     [Audit/Time Travel]
```

### Componentes Principais

1. **Domain Events** (`Domain/Events/`)
   - Classes que representam eventos de negócio
   - `OrderCreatedEvent`, `ProductUpdatedEvent`, etc.

2. **IEventStore** (`Domain/Interfaces/IEventStore.cs`)
   - Interface para armazenar e recuperar eventos

3. **MartenEventStore** (`Infrastructure/Services/MartenEventStore.cs`)
   - Implementação usando Marten (PostgreSQL)

4. **HybridRepository** (`Data/Repository/HybridRepository.cs`)
   - Salva no EF Core E registra eventos simultaneamente

5. **AuditController** (`Api/Controllers/AuditController.cs`)
   - API REST para consultar histórico e estatísticas

---

## Modos de Operação

### 1️⃣ **Traditional** (Padrão - Event Sourcing Desabilitado)

```json

{
  "EventSourcing": {
    "Enabled": false
  }
}

```

- ✅ CRUD normal (apenas EF Core)
- ❌ Nenhum evento registrado
- ✅ Performance máxima
- ❌ Sem auditoria

### 2️⃣ **Hybrid** (Recomendado para Templates)

```json

{
  "EventSourcing": {
    "Enabled": true,
    "Mode": "Hybrid",
    "AuditEntities": ["Order", "Product"]
  }
}

```

- ✅ EF Core como fonte da verdade
- ✅ Eventos para auditoria e histórico
- ✅ Consultas rápidas (EF Core)
- ✅ Histórico completo (Event Store)
- ⚠️ Dual write (pequeno overhead)

### 3️⃣ **EventStore** (Event Sourcing Puro)

```json

{
  "EventSourcing": {
    "Enabled": true,
    "Mode": "EventStore"
  }
}

```

- ✅ Eventos como fonte da verdade
- ✅ Event Sourcing completo
- ✅ Time travel avançado
- ⚠️ Requer Projections para consultas
- ⚠️ Maior complexidade

---

<a id="configuracao"></a>

## Configuração

### 1. **appsettings.json**

```json

{
  "Infrastructure": {
    "EventSourcing": {
      "Enabled": true,
      "Mode": "Hybrid",
      "Provider": "Marten",
      "ConnectionString": "Host=localhost;Database=ProjectTemplateEvents;Username=postgres;Password=postgres",
      "AuditEntities": ["Order", "Product"],
      "StoreSnapshots": true,
      "SnapshotInterval": 10,
      "EnableAuditApi": true,
      "StoreMetadata": true
    }
  }
}

```

### 2. **Propriedades de Configuração**

| Propriedade | Tipo | Padrão | Descrição |
| ----------- | ---- | ------ | --------- |
| `Enabled` | bool | `false` | Liga/desliga Event Sourcing globalmente |
| `Mode` | enum | `Hybrid` | `Traditional`, `Hybrid`, `EventStore` |
| `Provider` | string | `Marten` | Provider do Event Store (`Marten`, `Custom`) |
| `ConnectionString` | string | - | String de conexão PostgreSQL |
| `AuditEntities` | array | `[]` | Entidades a auditar (vazio = todas) |
| `StoreSnapshots` | bool | `true` | Armazenar snapshots para performance |
| `SnapshotInterval` | int | `10` | Criar snapshot a cada N eventos |
| `EnableAuditApi` | bool | `true` | Habilitar endpoints de auditoria |
| `StoreMetadata` | bool | `true` | Armazenar IP, User-Agent, etc. |

### 3. **Docker Compose**

O PostgreSQL para Event Store já está configurado no `docker-compose.yml`:

```bash

docker-compose up -d postgres-events

```

Acesso:

- **Host**: localhost
- **Port**: 5432
- **Database**: ProjectTemplateEvents
- **User**: postgres
- **Password**: postgres

---

<a id="uso-basico"></a>

## Uso Básico

### 1. **Criar uma Entidade (Gera Evento Automaticamente)**

```csharp

// POST /api/order
public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto dto)
{
  var order = new Order
  {
    OrderNumber = GenerateOrderNumber(),
    CustomerName = dto.CustomerName,
    CustomerEmail = dto.CustomerEmail,
    Total = dto.Total
  };

  // HybridRepository salva no EF Core E registra evento automaticamente
  await _repository.AddAsync(order);

  // Evento OrderCreatedEvent foi registrado no Event Store
  return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
}

```

### 2. **Atualizar uma Entidade (Registra Mudanças)**

```csharp

// PUT /api/order/{id}
public async Task<ActionResult> UpdateOrder(long id, UpdateOrderDto dto)
{
  var order = await _repository.GetByIdAsync(id);
  if (order == null)
  {
    return NotFound();
  }

  order.Status = dto.Status;
  order.ShippingAddress = dto.ShippingAddress;

  // HybridRepository detecta mudanças e registra OrderUpdatedEvent
  await _repository.UpdateAsync(order);

  return NoContent();
}

```

### 3. **O Que Acontece nos Bastidores**

```csharp

// HybridRepository.UpdateAsync()
public override async Task UpdateAsync(TEntity entity)
{
  // 1. Detecta mudanças
  var changes = await DetectChangesAsync(entity);
  // changes = { "Status": { Old: "Pending", New: "Shipped" } }

  // 2. Salva no EF Core
  await base.UpdateAsync(entity);
  await _context.SaveChangesAsync();

  // 3. Registra evento (se habilitado)
  if (ShouldAuditEntity("Order"))
  {
    await _eventStore.AppendEventAsync(
      "Order",
      entity.Id.ToString(),
      new OrderUpdatedEvent { OrderId = entity.Id, Changes = changes },
      userId: GetCurrentUserId(),
      metadata: GetMetadata() // IP, User-Agent, etc.
    );
  }
}

```

---

<a id="api-de-auditoria"></a>

## API de Auditoria

### Endpoints Disponíveis

#### 1. **Histórico Completo de uma Entidade**

```http

GET /api/audit/Order/123

```

**Response:**

```json

[
  {
    "eventId": "abc-123",
    "eventType": "OrderCreatedEvent",
    "aggregateType": "Order",
    "aggregateId": "123",
    "occurredOn": "2026-01-10T10:00:00Z",
    "userId": "user@email.com",
    "version": 1,
    "metadata": {
      "IpAddress": "192.168.1.10",
      "UserAgent": "Mozilla/5.0..."
    }
  },
  {
    "eventId": "def-456",
    "eventType": "OrderUpdatedEvent",
    "occurredOn": "2026-01-11T14:30:00Z",
    "version": 2
  }
]

```

#### 2. **Time Travel - Estado em um Momento Específico**

```http

GET /api/audit/Order/123/at/2026-01-11T12:00:00Z

```

**Response:**

```json

{
  "entityType": "Order",
  "entityId": "123",
  "timestamp": "2026-01-11T12:00:00Z",
  "eventCount": 2,
  "events": [
    {
      "eventType": "OrderCreatedEvent",
      "version": 1,
      "occurredOn": "2026-01-10T10:00:00Z"
    },
    {
      "eventType": "OrderApprovedEvent",
      "version": 2,
      "occurredOn": "2026-01-10T14:30:00Z"
    }
  ]
}

```

#### 3. **Eventos por Versão**

```http

GET /api/audit/Order/123/versions/1/5

```

Retorna eventos da versão 1 até a versão 5.

#### 4. **Todos os Eventos de um Tipo**

```http

GET /api/audit/type/Order?from=2026-01-01&to=2026-01-31&limit=100

```

#### 5. **Eventos por Usuário**

```http

GET /api/audit/user/user@email.com?limit=50

```

#### 6. **Estatísticas**

```http

GET /api/audit/statistics?from=2026-01-01&to=2026-01-31

```

**Response:**

```json

{
  "totalEvents": 15420,
  "eventsByType": {
    "OrderCreatedEvent": 4500,
    "OrderUpdatedEvent": 8920,
    "ProductCreatedEvent": 2000
  },
  "eventsByAggregateType": {
    "Order": 13420,
    "Product": 2000
  },
  "oldestEvent": "2025-01-01T00:00:00Z",
  "latestEvent": "2026-01-14T23:59:59Z"
}

```

#### 7. **Replay de Eventos**

```http

POST /api/audit/Order/123/replay

```

Reconstrói o estado atual a partir dos eventos.

---

## Cenários Reais End-to-End

### 1. Preparando o cenário (payloads reais)

**Criar produto (gera `ProductCreatedEvent`)**

```http
POST /api/v1/Product
Content-Type: application/json

{
  "name": "Telemetry Headphones",
  "description": "Noise cancelling headset criada para auditoria",
  "price": 199.9,
  "stock": 25,
  "category": "audit",
  "isActive": true
}
```

**Criar pedido (gera `OrderCreatedEvent`)**

```http
POST /api/v1/Order
Content-Type: application/json

{
  "customerName": "Audit Customer",
  "customerEmail": "audit@example.com",
  "shippingAddress": "Rua Teste, 123 - São Paulo",
  "phone": "+55 11 99999-1234",
  "items": [
    {
      "productId": 42,
      "quantity": 1,
      "unitPrice": 199.9
    }
  ],
  "notes": "Pedido criado via cenário Event Sourcing"
}
```

### 2. Mutação controlada (gera `OrderUpdatedEvent`)

```http
PATCH /api/v1/Order/42/status
Content-Type: application/json

{
  "status": "Processing",
  "notes": "Priorizar envio para o cliente de auditoria"
}
```

### 3. Consultar histórico completo

```http
GET /api/Audit/Order/42
```

**Resposta truncada:**

```json
[
  {
    "eventType": "OrderCreatedEvent",
    "aggregateType": "Order",
    "aggregateId": "42",
    "occurredOn": "2026-01-18T13:15:11Z",
    "version": 1,
    "metadata": {
      "MachineName": "devbox",
      "Timestamp": "2026-01-18T13:15:11Z"
    }
  },
  {
    "eventType": "OrderUpdatedEvent",
    "aggregateType": "Order",
    "aggregateId": "42",
    "occurredOn": "2026-01-18T13:16:02Z",
    "version": 2,
    "metadata": {
      "MachineName": "devbox",
      "Timestamp": "2026-01-18T13:16:02Z"
    }
  }
]
```

### 4. Time travel com checkpoint

1. Capture o timestamp *antes* de atualizar o pedido (por exemplo `2026-01-18T13:15:30Z`).
2. Chame `GET /api/Audit/Order/42/at/2026-01-18T13:15:30Z`.

```json
{
  "entityType": "Order",
  "entityId": "42",
  "timestamp": "2026-01-18T13:15:30Z",
  "eventCount": 1,
  "events": [
    {
      "eventType": "OrderCreatedEvent",
      "version": 1,
      "occurredOn": "2026-01-18T13:15:11Z"
    }
  ]
}
```

### 5. Replay automatizado

- **Coleção REST Client/VS Code:** execute os passos descritos em [docs/examples/event-sourcing.http](docs/examples/event-sourcing.http). Ela já define as variáveis `@productId`, `@orderId` e `@timeTravelCutoff` para que você replique o fluxo acima com um clique.
- **Script shell:** use [scripts/event-sourcing/replay-order.sh](scripts/event-sourcing/replay-order.sh) para baixar o histórico e disparar o replay direto do terminal (requer `curl` + `jq`).

```bash
API_URL=http://localhost:8080 ./scripts/event-sourcing/replay-order.sh 42 Order
```

### 6. Garantias automatizadas

- Os testes de integração em [tests/Integration/Controllers/AuditControllerTests.cs](tests/Integration/Controllers/AuditControllerTests.cs) criam pedidos reais, modificam o status e validam os endpoints `/api/Audit/*` (histórico, time travel e replay) usando um `IEventStore` em memória.
- Rode `dotnet test` (ou `dotnet test tests/Integration/Integration.csproj --filter AuditController`) para garantir que regressões no AuditController sejam detectadas junto com o restante da suíte.

---

<a id="eventos-disponiveis"></a>

## Eventos Disponíveis

### **Order Events**

| Evento | Quando Dispara | Dados |
| ------ | -------------- | ----- |
| `OrderCreatedEvent` | Novo pedido criado | OrderNumber, Customer, Items, Total |
| `OrderUpdatedEvent` | Pedido atualizado | Changes (campo: old/new) |
| `OrderStatusChangedEvent` | Status mudou | OldStatus, NewStatus, Reason |
| `OrderShippedEvent` | Pedido enviado | Carrier, TrackingNumber |
| `OrderDeliveredEvent` | Pedido entregue | DeliveredAt, ReceivedBy |
| `OrderCancelledEvent` | Pedido cancelado | Reason, CancelledBy |
| `OrderDeletedEvent` | Pedido deletado | Reason |

### **Product Events**

| Evento | Quando Dispara | Dados |
| ------ | -------------- | ----- |
| `ProductCreatedEvent` | Novo produto criado | Name, Price, StockQuantity |
| `ProductUpdatedEvent` | Produto atualizado | Changes |
| `ProductStockChangedEvent` | Estoque mudou | OldQuantity, NewQuantity, Reason |
| `ProductPriceChangedEvent` | Preço mudou | OldPrice, NewPrice |
| `ProductDeletedEvent` | Produto deletado | Reason |

---

<a id="time-travel"></a>

## Time Travel

### Exemplo Prático: Investigar Disputa de Cliente

```csharp

// Cliente reclama que pedido nunca foi enviado
var orderId = "order-456";

// 1. Ver estado atual
var currentOrder = await _repository.GetByIdAsync(456);
Console.WriteLine(currentOrder.Status); // "Cancelled"

// 2. Ver estado quando cliente fez pedido (dia 10)
var eventsAt10Jan = await _eventStore.GetEventsAsync(
  "Order",
  orderId,
  until: new DateTime(2026, 1, 10)
);
// Status era "Pending"

// 3. Ver todos os eventos
var allEvents = await _eventStore.GetEventsAsync("Order", orderId);
/*
1. OrderCreated    - 10/01 10:00
2. OrderApproved   - 10/01 14:30
3. OrderCancelled  - 11/01 09:00 (motivo: "Payment failed")
*/

// 4. Conclusão: Pagamento falhou, pedido foi cancelado

```

---

<a id="melhores-praticas"></a>

## Melhores Práticas

### ✅ **DO (Faça)**

1. **Comece com Modo Hybrid**

    ```json
    { "EventSourcing": { "Enabled": true, "Mode": "Hybrid" } }
    ```

2. **Audite Apenas Entidades Críticas**

    ```json
    { "AuditEntities": ["Order", "Payment", "Invoice"] }
    ```

3. **Use Snapshots para Performance**

    ```json
    { "StoreSnapshots": true, "SnapshotInterval": 10 }
    ```

4. **Armazene Metadados**

    ```json
    { "StoreMetadata": true }
    ```

    - IP Address
    - User-Agent
    - Request Path

5. **Eventos São Imutáveis**

    - Nunca delete ou modifique eventos
    - Use eventos de compensação (ex: `OrderCancelledEvent`)

### ❌ **DON'T (Não Faça)**

1. **Não Habilite para Todas as Entidades**
   - Gera overhead desnecessário
   - Audite apenas o que é crítico

2. **Não Armazene Dados Sensíveis em Eventos**
   - Use criptografia se necessário
   - GDPR/LGPD: direito ao esquecimento

3. **Não Use Event Sourcing para Dados Efêmeros**
   - Logs de acesso
   - Cache temporário

---

<a id="performance"></a>

## Performance

### Otimizações Implementadas

#### 1. **Snapshots**

```csharp
// A cada 10 eventos, salva snapshot do estado completo
if (eventCount % 10 == 0)
{
  await _eventStore.SaveSnapshotAsync("Order", orderId, currentState, eventCount);
}

// Recuperação rápida
var (snapshot, version) = await _eventStore.GetSnapshotAsync<OrderSnapshot>("Order", orderId);
// Aplica apenas eventos após snapshot (version 10 -> atual)
```

#### 2. **Indexação no PostgreSQL**

```sql
-- Marten cria automaticamente
CREATE INDEX idx_events_aggregate ON mt_events (stream_id);
CREATE INDEX idx_events_timestamp ON mt_events (timestamp);
CREATE INDEX idx_events_type ON mt_events (type);
```

#### 3. **Batch Inserts**

```csharp
// Marten otimiza automaticamente
session.Events.Append(streamId, event1, event2, event3);
await session.SaveChangesAsync(); // Um único INSERT
```

### Benchmarks (Aproximados)

| Operação | Sem Event Sourcing | Modo Hybrid |
| -------- | ------------------ | ----------- |
| Create | 5ms | 8ms (+60%) |
| Update | 7ms | 10ms (+43%) |
| Read | 2ms | 2ms (sem mudança) |
| Query Events | - | 15ms |
| Time Travel | - | 25ms |

---

<a id="troubleshooting"></a>

## Troubleshooting

### ❌ **Problema: Build falha com erro Marten**

error CS0246: The type or namespace name 'Marten' could not be found
**Solução:**

```bash

cd src/Infrastructure
dotnet add package Marten --version 8.3.0
dotnet restore

```

---

### ❌ **Problema: PostgreSQL não conecta**

```text
Npgsql.NpgsqlException: Connection refused
```

**Solução:**

```bash
# Verificar se PostgreSQL está rodando
docker ps | grep postgres-events

# Iniciar PostgreSQL
docker-compose up -d postgres-events

# Verificar logs
docker logs postgres-events
```

---

### ❌ **Problema: Eventos não estão sendo registrados**

**Diagnóstico:**

```bash
docker logs api | grep EventStore
```

```text
// 1. Verificar se está habilitado
GET /api/health
// EventSourcing: Enabled = true?

// 2. Verificar se entidade está na lista
// appsettings.json
"AuditEntities": ["Order"] // Product não está auditando!

// 3. Verificar logs
docker logs api | grep EventStore
```

**Solução:**

```json

{
  "EventSourcing": {
    "Enabled": true,
    "AuditEntities": [] // Vazio = audita TODAS as entidades
  }
}

```

---

### ❌ **Problema: Performance degradada**

**Sintomas:**

- Latência aumentou após habilitar Event Sourcing

**Soluções:**

1. **Audite apenas entidades críticas**

   ```json
   { "AuditEntities": ["Order"] } // Só Order, não Product
   ```

2. **Habilite snapshots**

   ```json
   { "StoreSnapshots": true, "SnapshotInterval": 10 }
   ```

3. **Desabilite metadados em dev**

   ```json
   { "StoreMetadata": false } // Em desenvolvimento
   ```

4. **Use índices no PostgreSQL**

   ```sql
   -- Marten já cria, mas você pode adicionar customizados
   CREATE INDEX idx_events_userid ON mt_events ((data ->> 'userId'));
   ```

---

## Recursos Adicionais

### Documentação Oficial

- [Marten Documentation](https://martendb.io/)
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS and Event Sourcing](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)

### Ferramentas

| Tool | Uso | Link |
| ---- | --- | ---- |
| **pgAdmin** | Gerenciar PostgreSQL | https://www.pgadmin.org/ |
| **DBeaver** | Cliente SQL universal | https://dbeaver.io/ |
| **Postman** | Testar API de auditoria | https://www.postman.com/ |

### Exemplos de Consultas SQL

```sql

-- Ver todos os eventos de um pedido
SELECT *
FROM mt_events
WHERE stream_id = 'Order-123'
ORDER BY version;

-- Contar eventos por tipo
SELECT type, COUNT(*) AS total
FROM mt_events
GROUP BY type
ORDER BY total DESC;

-- Eventos de hoje
SELECT *
FROM mt_events
WHERE timestamp::date = CURRENT_DATE;

-- Top 10 usuários mais ativos
SELECT data->>'UserId' AS user_id, COUNT(*) AS events
FROM mt_events
GROUP BY data->>'UserId'
ORDER BY events DESC
LIMIT 10;

```

---

## Próximos Passos

1. ✅ Habilitar Event Sourcing em produção
2. ⏳ Implementar Projections para consultas complexas
3. ⏳ Adicionar Event Bus para comunicação entre microserviços
4. ⏳ Implementar CQRS completo
5. ⏳ Adicionar Event Store UI para visualização

---

## Suporte

Dúvidas ou problemas? Abra uma issue no GitHub ou consulte a documentação oficial do Marten.

**Happy Event Sourcing! 📜🚀**
