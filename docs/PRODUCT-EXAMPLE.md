# 📦 Exemplo de Entidade: Product

Este exemplo demonstra como usar o template com uma entidade completa, incluindo geração de relatórios Excel.

---

## 📋 O que foi criado

### 1. **Entidade Product** (`Domain/Entities/Product.cs`)

```csharp
public class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public override bool IsActive { get; set; } = true;
}
```

### 2. **DTOs + Validators** (`Domain/Dtos/ProductDtos.cs` + `Domain/Validators/ProductValidators.cs`)

```csharp
// Criação
public record CreateProductRequest
{
  public required string Name { get; init; }
  public string? Description { get; init; }
  public decimal Price { get; init; }
  public int Stock { get; init; }
  public required string Category { get; init; }
  public bool IsActive { get; init; } = true;
}

// Atualização completa
public record UpdateProductRequest
{
  public required string Name { get; init; }
  public string? Description { get; init; }
  public decimal Price { get; init; }
  public int Stock { get; init; }
  public required string Category { get; init; }
  public bool IsActive { get; init; } = true;
}

// Ativação/desativação
public record UpdateProductStatusRequest
{
  public bool? IsActive { get; init; }
}

// Ajuste de estoque
public record UpdateProductStockRequest
{
  public int Quantity { get; init; } // positivo = adiciona; negativo = remove
}

// Resposta da API
public record ProductResponseDto
{
  public long Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? Description { get; init; }
  public decimal Price { get; init; }
  public int Stock { get; init; }
  public string Category { get; init; } = string.Empty;
  public bool IsActive { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime? UpdatedAt { get; init; }
}
```

- **Requests separados** para cada operação: criação, atualização completa, toggle de status e ajuste de estoque.
- **Response dedicado** evita expor entidades e mantém contratos versionáveis.
- **FluentValidation** com regras por campo:

| Campo | Regra |
|-------|-------|
| `Name` | Obrigatório, máx. 200 caracteres |
| `Description` | Opcional, máx. 2.000 caracteres |
| `Price` | Deve ser > 0 |
| `Stock` | Deve ser >= 0 |
| `Category` | Obrigatório, máx. 120 caracteres |
| `IsActive` (status) | Não pode ser `null` |
| `Quantity` (estoque) | Diferente de 0, entre −100.000 e +100.000 |

### 3. **DbSet no Context** (`Data/Context/ApplicationDbContext.cs`)

```csharp
public DbSet<Product> Products { get; set; }
```

### 4. **Controller Completo** (`Api/Controllers/ProductController.cs`)

#### Endpoints Disponíveis

| Método | Rota | Descrição | Cache |
|--------|------|-----------|-------|
| `GET` | `/api/v1/product` | Lista todos os produtos com filtros e métricas de performance | `Expire300` |
| `GET` | `/api/v1/product/{id}` | Busca produto por ID (retorna `ProductResponseDto`) | `Expire300` |
| `POST` | `/api/v1/product` | Cria novo produto a partir de `CreateProductRequest` | — |
| `PUT` | `/api/v1/product/{id}` | Atualiza detalhes via `UpdateProductRequest` | — |
| `DELETE` | `/api/v1/product/{id}` | Remove produto | — |
| `GET` | `/api/v1/product/ExportToExcel` | Gera arquivo Excel com DTOs filtrados | — |
| `PATCH` | `/api/v1/product/{id}/status` | Ativa/desativa produto com `UpdateProductStatusRequest` | — |
| `PATCH` | `/api/v1/product/{id}/stock` | Ajusta estoque com `UpdateProductStockRequest` | — |

---

## ⚡ Destaque: Geração de Excel

### Como Funciona

O endpoint `ExportToExcel` usa a biblioteca **MiniExcel** para gerar arquivos Excel de alta performance (agora exportando `ProductResponseDto`):

```csharp
[HttpGet("ExportToExcel")]
public async Task<ActionResult> ExportToExcelAsync(
  [FromQuery] bool? isActive,
  [FromQuery] string? category,
  CancellationToken cancellationToken)
{
  var products = await productService.GetProductsForExportAsync(isActive, category, cancellationToken);
  var productList = products.ToList();

  var config = new OpenXmlConfiguration
  {
    FastMode = true,
    EnableAutoWidth = true,
    AutoFilter = true
  };

  var memoryStream = new MemoryStream();
  await memoryStream.SaveAsAsync(productList, sheetName: "Products", configuration: config);
  memoryStream.Seek(0, SeekOrigin.Begin);

  return File(
    memoryStream,
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
}
```

### Características

- ✅ **Alta Performance**: MiniExcel usa FastMode para geração rápida
- ✅ **Filtros**: Suporta filtrar por status ativo e categoria
- ✅ **Auto-width**: Colunas ajustadas automaticamente
- ✅ **AutoFilter**: Excel gerado com filtros habilitados
- ✅ **Timestamp**: Nome do arquivo inclui data/hora
- ✅ **Memory Stream**: Não cria arquivos temporários no disco

---

## 🚀 Como Testar

### 1. Criar Migration

```bash
cd template
dotnet ef migrations add AddProduct --project src/Data --startup-project src/Api
dotnet ef database update --project src/Data --startup-project src/Api
```

### 2. Executar a API

```bash
dotnet run --project src/Api
```

### 3. Acessar Swagger

```text
https://localhost:5001/swagger
```

### 4. Testar Endpoints

#### Criar Produto

```http
POST /api/v1/product
Content-Type: application/json

{
  "name": "Notebook Dell",
  "description": "Notebook Dell Inspiron 15",
  "price": 3500.00,
  "stock": 10,
  "category": "Electronics",
  "isActive": true
}
```

#### Listar Produtos com Filtros

```http
GET /api/v1/product?isActive=true&category=Electronics
```

#### Gerar Excel

```http
GET /api/v1/product/ExportToExcel?isActive=true&category=Electronics
```

O navegador fará download do arquivo `Products_20260111_143022.xlsx`

#### Atualizar Estoque

```http
PATCH /api/v1/product/1/stock
Content-Type: application/json

{
  "quantity": 5
}
```

Adiciona 5 unidades ao estoque do produto ID 1 (valores negativos removem itens).

#### Alterar Status

```http
PATCH /api/v1/product/1/status
Content-Type: application/json

{
  "isActive": false
}
```

Desativa o produto ID 1 utilizando o `UpdateProductStatusRequest`.

---

## 📊 Exemplo de Resposta com Métricas

```json
{
  "executionTime": "45ms",
  "totalCount": 3,
  "items": [
    {
      "id": 1,
      "name": "Notebook Dell",
      "description": "Notebook Dell Inspiron 15",
      "price": 3500.0,
      "stock": 15,
      "category": "Electronics",
      "isActive": true,
      "createdAt": "2026-01-11T14:30:00Z",
      "updatedAt": "2026-01-11T14:35:00Z"
    }
  ]
}
```

---

## 🎨 Features Demonstradas

### Boas Práticas

- **Logging estruturado** com ILogger
- **Métricas de performance** com Stopwatch
- **CancellationToken** em todos os métodos async
- **ModelState validation** com mensagens de erro
- **HTTP status codes** apropriados (200, 201, 204, 400, 404)
- **`Created(location, dto)`** retornando URI via `Url.Action`
- **OutputCache** nos endpoints GET (política `Expire300` = 300 s)
- **XML Documentation** para Swagger

### Funcionalidades

- CRUD completo
- Filtros avançados
- Geração de Excel
- Operações de estoque (PATCH)
- Ativação/desativação (PATCH)
- Métricas de performance

### Padrões

- Clean Architecture
- Repository Pattern
- Service Layer
- Dependency Injection
- Async/Await
- RESTful API

---

## 📦 Pacote Adicionado

```xml
<PackageReference Include="MiniExcel" Version="1.37.3" />
```

**MiniExcel** é uma biblioteca de alta performance para geração de arquivos Excel (.xlsx):

- ⚡ 10x mais rápido que EPPlus
- 💾 Baixo consumo de memória
- 📊 Suporta grandes volumes de dados
- 🎯 API simples e intuitiva

---

## 🔍 Próximos Passos

Para estender este exemplo:

1. **Adicionar Paginação/Ordenação** nos endpoints de listagem
2. **Criar filtros avançados** (preço, categoria, busca textual)
3. **Adicionar Cache** nos endpoints de leitura
4. **Implementar Busca** com índices full-text ou Elastic
5. **Publicar eventos de domínio** (por exemplo, quando estoque atinge nível crítico)

---

**Navegação:**

- [⬆️ Voltar ao README](../README.md)
- [📖 Ver Índice](../INDEX.md)
- [🚀 Quick Start](../QUICK-START.md)
- [🎛️ Recursos Avançados](FEATURES.md)
