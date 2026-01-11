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
    public bool IsActive { get; set; } = true;
}
```

### 2. **DbSet no Context** (`Data/Context/ApplicationDbContext.cs`)

```csharp
public DbSet<Product> Products { get; set; }
```

### 3. **Controller Completo** (`Api/Controllers/ProductController.cs`)

#### Endpoints Disponíveis:

- **GET** `/api/v1/product` - Lista todos os produtos com filtros e métricas de performance
- **GET** `/api/v1/product/{id}` - Busca produto por ID
- **POST** `/api/v1/product` - Cria novo produto
- **PUT** `/api/v1/product/{id}` - Atualiza produto
- **DELETE** `/api/v1/product/{id}` - Remove produto
- **GET** `/api/v1/product/GerarExcel` - **Gera arquivo Excel com produtos**
- **PATCH** `/api/v1/product/{id}/status` - Ativa/desativa produto
- **PATCH** `/api/v1/product/{id}/stock` - Atualiza estoque

---

## ⚡ Destaque: Geração de Excel

### Como Funciona

O endpoint `GerarExcel` usa a biblioteca **MiniExcel** para gerar arquivos Excel de alta performance:

```csharp
[HttpGet("GerarExcel")]
public async Task<ActionResult> GerarExcelAsync(
    [FromQuery] bool? isActive,
    [FromQuery] string? category,
    CancellationToken cancellationToken)
{
    // Busca produtos com filtros
    var products = await _service.GetAllAsync(cancellationToken);
    var filtered = ApplyFilters(products, isActive, category);

    // Configuração do Excel
    var config = new OpenXmlConfiguration
    {
        FastMode = true,           // Alta performance
        EnableAutoWidth = true,    // Auto-ajuste de colunas
        AutoFilter = true          // Filtro automático no Excel
    };

    // Gera Excel em memória
    var memoryStream = new MemoryStream();
    await memoryStream.SaveAsAsync(filtered, 
        sheetName: "Products", 
        configuration: config);
    
    // Retorna arquivo
    return File(memoryStream, 
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
}
```

### Características:

✅ **Alta Performance**: MiniExcel usa FastMode para geração rápida  
✅ **Filtros**: Suporta filtrar por status ativo e categoria  
✅ **Auto-width**: Colunas ajustadas automaticamente  
✅ **AutoFilter**: Excel gerado com filtros habilitados  
✅ **Timestamp**: Nome do arquivo inclui data/hora  
✅ **Memory Stream**: Não cria arquivos temporários no disco  

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

```
https://localhost:5001/swagger
```

### 4. Testar Endpoints

#### Criar Produto:

```bash
POST /api/v1/product
{
  "name": "Notebook Dell",
  "description": "Notebook Dell Inspiron 15",
  "price": 3500.00,
  "stock": 10,
  "category": "Electronics",
  "isActive": true
}
```

#### Listar Produtos com Filtros:

```bash
GET /api/v1/product?isActive=true&category=Electronics
```

#### Gerar Excel:

```bash
GET /api/v1/product/GerarExcel?isActive=true&category=Electronics
```

O navegador fará download do arquivo `Products_20260111_143022.xlsx`

#### Atualizar Estoque:

```bash
PATCH /api/v1/product/1/stock?quantity=5
```

Adiciona 5 unidades ao estoque do produto ID 1.

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
      "price": 3500.00,
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

### ✅ Boas Práticas:

- **Logging estruturado** com ILogger
- **Métricas de performance** com Stopwatch
- **CancellationToken** em todos os métodos async
- **ModelState validation** com mensagens de erro
- **HTTP status codes** apropriados (200, 201, 204, 400, 404)
- **CreatedAtAction** retornando URI do recurso criado
- **XML Documentation** para Swagger

### ✅ Funcionalidades:

- CRUD completo
- Filtros avançados
- Geração de Excel
- Operações de estoque (PATCH)
- Ativação/desativação (PATCH)
- Métricas de performance

### ✅ Padrões:

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

1. **Adicionar DTOs** para separar entidades de transporte de dados
2. **Implementar Validators** com FluentValidation
3. **Adicionar Paginação** nos endpoints de listagem
4. **Criar Testes** unitários e de integração
5. **Adicionar Cache** nos endpoints de leitura
6. **Implementar Busca** com filtros mais avançados

---

**Navegação:**
- [⬆️ Voltar ao README](../README.md)
- [📖 Ver Índice](../INDEX.md)
- [🚀 Quick Start](../QUICK-START.md)
- [🎛️ Recursos Avançados](FEATURES.md)
