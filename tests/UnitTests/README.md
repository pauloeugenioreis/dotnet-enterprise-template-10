# Testes Unitários

Este projeto contém testes unitários para os controllers da API, utilizando **Moq** para criar mocks dos serviços e **FluentAssertions** para asserções mais legíveis.

## 📦 Tecnologias Utilizadas

- **xUnit 2.9.2** - Framework de testes
- **Moq 4.20.72** - Framework de mocking para criar objetos simulados
- **FluentAssertions 7.0.0** - Biblioteca para asserções mais expressivas e legíveis

## 🧪 Estrutura dos Testes

### Controllers/ProductControllerTests.cs
Testes unitários para `ProductController`:
- ✅ `GetAll_ReturnsOkResult_WithListOfProducts` - Testa listagem de produtos
- ✅ `GetById_WithValidId_ReturnsOkResult_WithProduct` - Testa busca de produto por ID válido
- ✅ `GetById_WithInvalidId_ReturnsNotFound` - Testa busca com ID inválido
- ✅ `Create_WithValidProduct_ReturnsCreatedAtAction` - Testa criação de produto
- ✅ `Update_WithValidIdAndProduct_ReturnsOkResult` - Testa atualização de produto
- ✅ `Update_WithInvalidId_ReturnsNotFound` - Testa atualização com ID inválido
- ✅ `Update_WithMismatchedIds_ReturnsBadRequest` - Testa atualização com IDs incompatíveis
- ✅ `Delete_WithValidId_ReturnsNoContent` - Testa exclusão de produto
- ✅ `Delete_WithInvalidId_ReturnsNotFound` - Testa exclusão com ID inválido
- ✅ `Delete_WhenDeleteFails_ReturnsInternalServerError` - Testa falha na exclusão

**Total: 10 testes**

### Controllers/OrderControllerTests.cs
Testes unitários para `OrderController`:
- ✅ `GetAll_ReturnsOkResult_WithListOfOrders` - Testa listagem de pedidos
- ✅ `GetById_WithValidId_ReturnsOkResult_WithOrderDetails` - Testa busca de pedido por ID
- ✅ `GetById_WithInvalidId_ReturnsNotFound` - Testa busca com ID inválido
- ✅ `Create_WithValidOrder_ReturnsCreatedAtAction` - Testa criação de pedido
- ✅ `UpdateStatus_WithValidData_ReturnsOkResult` - Testa atualização de status
- ✅ `UpdateStatus_WithInvalidId_ReturnsNotFound` - Testa atualização com ID inválido
- ✅ `Cancel_WithValidId_ReturnsOkResult` - Testa cancelamento de pedido
- ✅ `GetByCustomer_WithValidEmail_ReturnsOkResult_WithOrders` - Testa busca por cliente
- ✅ `GetByStatus_WithValidStatus_ReturnsOkResult_WithOrders` - Testa busca por status
- ✅ `GetStatistics_ReturnsOkResult_WithStatistics` - Testa estatísticas de pedidos

**Total: 10 testes**

## 🎯 Padrões de Teste

### Arrange-Act-Assert (AAA)
Todos os testes seguem o padrão AAA:
[Fact]
public async Task GetById_WithValidId_ReturnsOkResult_WithProduct()
{
    // Arrange - Configurar mocks e dados
    var productId = 1L;
    var product = new Product { Id = productId, Name = "Test Product" };
    _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(product);

    // Act - Executar o método a ser testado
    var result = await _controller.GetById(productId);

    // Assert - Verificar o resultado
    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    var returnedProduct = okResult.Value.Should().BeAssignableTo<Product>().Subject;
    returnedProduct.Should().BeEquivalentTo(product);
}
### Uso de Mocks
Os testes utilizam **Moq** para criar mocks dos serviços:
// Mock do serviço
_mockService = new Mock<IService<Product>>();

// Configurar comportamento do mock
_mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(product);

// Verificar se método foi chamado
_mockService.Verify(s => s.GetByIdAsync(productId), Times.Once);
### FluentAssertions
Asserções mais legíveis e expressivas:
// Ao invés de:
Assert.IsType<OkObjectResult>(result);

// Use:
result.Should().BeOfType<OkObjectResult>();

// Verificações complexas:
returnedProducts.Should().HaveCount(2);
returnedProducts.Should().BeEquivalentTo(expectedProducts);
returnedProduct.Name.Should().Be("Test Product");
## 🚀 Executando os Testes

### Via linha de comando
# Executar todos os testes
dotnet test

# Executar testes de um projeto específico
dotnet test tests/UnitTests/UnitTests.csproj

# Executar com verbosidade
dotnet test --verbosity detailed

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"
### Via Visual Studio Code
1. Instalar extensão **.NET Core Test Explorer**
2. Abrir painel de testes (Test Explorer)
3. Clicar em "Run All Tests" ou executar testes individuais

## 📊 Cobertura de Testes

Os testes unitários cobrem:
- ✅ **Casos de sucesso** - Operações bem-sucedidas
- ✅ **Casos de erro** - IDs inválidos, dados não encontrados
- ✅ **Validações** - Dados inválidos, IDs incompatíveis
- ✅ **Respostas HTTP corretas** - 200 OK, 201 Created, 204 No Content, 404 Not Found, 400 Bad Request

## 🔍 Diferença entre Testes Unitários e de Integração

### Testes Unitários (este projeto)
- Testam unidades isoladas (controllers)
- Usam **mocks** para dependências
- Rápidos e independentes
- Focam na lógica do controller

### Testes de Integração (projeto Integration)
- Testam a aplicação inteira
- Usam banco de dados real (InMemory)
- Verificam integração entre camadas
- Testam fluxo completo da requisição

## 📝 Boas Práticas

1. **Um teste por comportamento** - Cada teste verifica um comportamento específico
2. **Nomes descritivos** - `Method_Scenario_ExpectedResult`
3. **Arrange-Act-Assert** - Estrutura clara em 3 partes
4. **Independência** - Testes não dependem uns dos outros
5. **Mocks simples** - Configure apenas o necessário para o teste
6. **Asserções claras** - Use FluentAssertions para melhor legibilidade

## 🎓 Exemplos de Uso

### Testar retorno de lista vazia
```csharp
_mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Product>());
var result = await _controller.GetAll();
var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
var products = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
products.Should().BeEmpty();
```

### Testar exceção
```csharp
_mockService.Setup(s => s.GetByIdAsync(It.IsAny<long>()))
    .ThrowsAsync(new Exception("Database error"));

var result = await _controller.GetById(1);
result.Should().BeOfType<ObjectResult>();
```

### Verificar chamadas ao serviço
```csharp
_mockService.Verify(s => s.AddAsync(It.IsAny<Product>()), Times.Once);
_mockService.Verify(s => s.UpdateAsync(It.IsAny<Product>()), Times.Never);
```

## 📚 Recursos Adicionais

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
