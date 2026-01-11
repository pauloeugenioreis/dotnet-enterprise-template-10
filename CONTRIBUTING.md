# 🤝 Guia de Contribuição

Obrigado por considerar contribuir com este template! Este documento fornece diretrizes para contribuir com o projeto.

---

## 📋 Índice

- [Código de Conduta](#código-de-conduta)
- [Como Posso Contribuir?](#como-posso-contribuir)
- [Processo de Desenvolvimento](#processo-de-desenvolvimento)
- [Padrões de Código](#padrões-de-código)
- [Commit Messages](#commit-messages)
- [Pull Requests](#pull-requests)

---

## 📜 Código de Conduta

Este projeto adota um Código de Conduta. Ao participar, espera-se que você:

- Use linguagem acolhedora e inclusiva
- Respeite pontos de vista e experiências diferentes
- Aceite críticas construtivas de forma elegante
- Foque no que é melhor para a comunidade
- Mostre empatia com outros membros da comunidade

---

## 🎯 Como Posso Contribuir?

### Reportar Bugs

Antes de reportar um bug:

1. **Verifique a documentação** - O comportamento pode estar documentado
2. **Pesquise issues existentes** - Alguém pode ter reportado o mesmo problema
3. **Use a versão mais recente** - O bug pode já ter sido corrigido

Ao reportar um bug, inclua:

- **Descrição clara** do problema
- **Passos para reproduzir** o comportamento
- **Comportamento esperado** vs **comportamento atual**
- **Screenshots** (se aplicável)
- **Versão do .NET**, sistema operacional, e outras informações relevantes
- **Logs de erro** completos

### Sugerir Melhorias

Para sugerir melhorias:

1. **Abra uma issue** com tag `enhancement`
2. **Descreva o problema** que a melhoria resolve
3. **Explique a solução proposta**
4. **Liste alternativas** consideradas
5. **Adicione contexto** de como isso beneficiaria os usuários

### Contribuir com Código

1. **Fork o repositório**
2. **Crie uma branch** descritiva
3. **Faça suas alterações**
4. **Teste suas mudanças**
5. **Commit com mensagens claras**
6. **Push para sua branch**
7. **Abra um Pull Request**

---

## 🔧 Processo de Desenvolvimento

### Setup do Ambiente

```bash
# Clone seu fork
git clone https://github.com/seu-usuario/projecttemplate.git
cd projecttemplate

# Adicione o upstream
git remote add upstream https://github.com/original/projecttemplate.git

# Instale dependências
dotnet restore
```

### Criar uma Branch

```bash
# Sincronize com upstream
git fetch upstream
git checkout main
git merge upstream/main

# Crie sua branch
git checkout -b feature/nome-da-feature
# ou
git checkout -b fix/nome-do-fix
```

### Tipos de Branches

- `feature/*` - Novas funcionalidades
- `fix/*` - Correções de bugs
- `docs/*` - Documentação
- `refactor/*` - Refatoração de código
- `test/*` - Adição ou correção de testes
- `chore/*` - Manutenção geral

### Executar Testes

```bash
# Todos os testes
dotnet test

# Com coverage
dotnet test /p:CollectCoverage=true

# Testes específicos
dotnet test --filter "FullyQualifiedName~Infrastructure"
```

---

## 📝 Padrões de Código

### C# Style Guide

Seguimos as [convenções de código C# da Microsoft](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions):

#### Nomenclatura

```csharp
// Classes, structs, enums - PascalCase
public class ProductService { }
public enum OrderStatus { }

// Interfaces - I + PascalCase
public interface IProductRepository { }

// Métodos públicos - PascalCase
public void ProcessOrder() { }

// Parâmetros e variáveis locais - camelCase
public void AddProduct(string productName, decimal price) 
{
    var totalPrice = price * 1.1m;
}

// Campos privados - _camelCase
private readonly ILogger _logger;

// Constantes - PascalCase
public const int MaxRetries = 3;

// Propriedades - PascalCase
public string ProductName { get; set; }
```

#### Organização

```csharp
namespace MeuProjeto.Domain.Entities;  // File-scoped namespace

// 1. Usings (ordenados alfabeticamente)
using System;
using System.Collections.Generic;
using MeuProjeto.Domain.Interfaces;

// 2. Classe
public class Product : EntityBase
{
    // 3. Constantes
    public const int MaxNameLength = 200;
    
    // 4. Campos privados
    private readonly ILogger _logger;
    
    // 5. Construtores
    public Product(ILogger logger)
    {
        _logger = logger;
    }
    
    // 6. Propriedades públicas
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    // 7. Métodos públicos
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
    }
    
    // 8. Métodos privados
    private void LogChange()
    {
        _logger.LogInformation("Price updated");
    }
}
```

#### Boas Práticas

```csharp
// ✅ BOM - Async/await
public async Task<Product> GetProductAsync(int id, CancellationToken cancellationToken)
{
    return await _repository.GetByIdAsync(id, cancellationToken);
}

// ❌ RUIM - Sem cancellation token
public async Task<Product> GetProduct(int id)
{
    return await _repository.GetById(id);
}

// ✅ BOM - Null checking
public void ProcessOrder(Order? order)
{
    ArgumentNullException.ThrowIfNull(order);
    // ...
}

// ❌ RUIM - Sem null check
public void ProcessOrder(Order order)
{
    order.Process();  // Pode lançar NullReferenceException
}

// ✅ BOM - Using declarations
public async Task SaveProductAsync(Product product)
{
    using var connection = CreateConnection();
    await connection.SaveAsync(product);
}

// ❌ RUIM - Using block desnecessário
public async Task SaveProduct(Product product)
{
    using (var connection = CreateConnection())
    {
        await connection.Save(product);
    }
}
```

### Arquitetura

Mantenha a separação de camadas:

```
Domain      - Apenas entidades e interfaces
Data        - Implementação de repositórios
Application - Serviços e lógica de negócio
Infrastructure - Configurações e extensões
Api         - Controllers e configuração da API
```

**Regras de Dependência:**

- `Domain` não depende de nenhuma outra camada
- `Data` depende apenas de `Domain`
- `Application` depende de `Domain` e `Data`
- `Infrastructure` pode depender de qualquer camada
- `Api` pode depender de qualquer camada

---

## 💬 Commit Messages

Seguimos o [Conventional Commits](https://www.conventionalcommits.org/):

### Formato

```
<tipo>(<escopo>): <descrição>

[corpo opcional]

[rodapé opcional]
```

### Tipos

- `feat` - Nova funcionalidade
- `fix` - Correção de bug
- `docs` - Documentação
- `style` - Formatação (sem mudança de código)
- `refactor` - Refatoração
- `perf` - Melhoria de performance
- `test` - Adição/correção de testes
- `chore` - Manutenção

### Exemplos

```bash
# Feature
feat(api): add product endpoint

# Fix
fix(cache): resolve redis connection timeout

# Documentation
docs(readme): update installation instructions

# Refactor
refactor(repository): simplify query logic

# Breaking change
feat(api)!: change authentication method

BREAKING CHANGE: OAuth2 is now required for all endpoints
```

---

## 🔀 Pull Requests

### Checklist

Antes de abrir um PR, verifique:

- [ ] Código segue os padrões do projeto
- [ ] Todos os testes passam
- [ ] Novos testes foram adicionados (se aplicável)
- [ ] Documentação foi atualizada
- [ ] Commit messages seguem o padrão
- [ ] Não há conflitos com a branch `main`
- [ ] PR tem descrição clara

### Template

```markdown
## Descrição

[Descreva suas mudanças aqui]

## Tipo de Mudança

- [ ] Bug fix (correção não-breaking)
- [ ] Nova feature (funcionalidade não-breaking)
- [ ] Breaking change (fix ou feature que causa mudança em funcionalidade existente)
- [ ] Documentação

## Como Foi Testado?

[Descreva os testes realizados]

## Checklist

- [ ] Meu código segue os padrões do projeto
- [ ] Realizei code review do meu próprio código
- [ ] Comentei código em áreas complexas
- [ ] Atualizei a documentação
- [ ] Minhas mudanças não geram novos warnings
- [ ] Adicionei testes
- [ ] Todos os testes passam localmente

## Screenshots (se aplicável)

[Adicione screenshots aqui]

## Issues Relacionadas

Closes #123
Related to #456
```

### Revisão

Seu PR será revisado considerando:

1. **Qualidade do código** - Segue padrões? Está limpo?
2. **Testes** - Há cobertura adequada?
3. **Documentação** - Está clara e atualizada?
4. **Breaking changes** - São necessárias? Estão documentadas?
5. **Performance** - Há impacto significativo?

---

## 🧪 Testes

### Estrutura

```
tests/
├── Infrastructure.UnitTests/    # Testes unitários
│   ├── Extensions/
│   └── Services/
└── Integration/                 # Testes de integração
    ├── Controllers/
    └── Repositories/
```

### Exemplo de Teste Unitário

```csharp
public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _repositoryMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Product>>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        var productId = 1L;
        var expectedProduct = new Product { Id = productId, Name = "Test" };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _service.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProduct.Id, result.Id);
    }
}
```

---

## 📚 Recursos

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Git Flow](https://nvie.com/posts/a-successful-git-branching-model/)

---

## ❓ Dúvidas?

Se você tem dúvidas sobre como contribuir:

1. Leia a documentação em `docs/`
2. Pesquise issues abertas e fechadas
3. Abra uma issue com a tag `question`
4. Entre em contato com os mantenedores

---

## 🙏 Agradecimentos

Obrigado por contribuir! Sua ajuda é muito apreciada e torna este template melhor para todos.

---

**Happy Contributing! 🚀**
