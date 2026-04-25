# 🚀 Início Rápido (Quick Start)

Guia rápido para começar a usar o template em menos de 5 minutos!

---

## ⚡ Pré-requisitos Mínimos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (versão RC 2 ou superior)
- Editor de código (VS Code, Visual Studio, Rider, etc.)

**Opcional (para Docker/Kubernetes):**

- [Docker Desktop](https://docs.docker.com/get-docker/)
- [Minikube](https://minikube.sigs.k8s.io/docs/start/)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)

---

## 📦 1. Criar Novo Projeto

O script interativo configura banco de dados, mensageria, storage, telemetria e event sourcing automaticamente.

### Windows (PowerShell)

```powershell
cd scripts
.\new-project.ps1
cd ..\MeuProjeto
```

### Linux/macOS

```bash
cd scripts
chmod +x new-project.sh
./new-project.sh
cd ../MeuProjeto
```

### Windows (CMD)

```cmd
cd scripts
new-project.bat
cd ..\MeuProjeto
```

> 💡 O script apresenta menus interativos para escolher banco de dados, mensageria (RabbitMQ), cloud storage, telemetria e **frontends UI** (Web: Angular/Blazor/React/Vue — Mobile: Flutter/MAUI/React Native). **MongoDB e Event Sourcing são habilitados automaticamente** em todos os projetos. Para modo não-interativo (CI/CD), veja [scripts/README.md](scripts/README.md) para detalhes.

---

## ⚙️ 2. Configurar Banco de Dados (se necessário)

> Se você usou o script interativo acima, o banco já foi configurado automaticamente. Esta seção é para ajustes manuais.

Edite `src/Server/Api/appsettings.json` e ajuste a connection string para seu ambiente:

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Database": {
        "DatabaseType": "SqlServer",
        "ConnectionString": "Server=localhost;Database=MeuProjeto;User Id=sa;Password=SuaSenhaForte123!;TrustServerCertificate=True;"
      }
    }
  }
}
```

> Para PostgreSQL, MySQL e Oracle, veja [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md).

### MongoDB

O template habilita MongoDB automaticamente em todos os projetos gerados, incluindo o container Docker, a connection string e o seed inicial.

- **Connection string padrão de desenvolvimento**: `mongodb://admin:admin@localhost:27017/<SeuProjeto>`
- **Usuário do container**: `admin`
- **Senha do container**: `admin`
- **Seed automático**: `MongoDbSeeder` insere exemplos na coleção `customerreviews` quando o Mongo estiver disponível

Se o container ainda estiver subindo, o seed faz retry automático antes de tentar gravar os documentos.

Mais detalhes em [docs/MONGODB-GUIDE.md](docs/MONGODB-GUIDE.md).

---

## 🏃 3. Executar a Aplicação

### Opção A: Launcher interativo (Recomendado)

Use o script `run.sh` / `run.ps1` na raiz do projeto — ele apresenta um menu para escolher entre rodar diretamente via `dotnet run` ou via **Aspire** (com todos os containers):

```bash
./run.sh         # Linux/Mac
.\run.ps1        # Windows
```

### Opção B: .NET CLI direto

```bash
dotnet run --project src/Server/Api
```

Acesse:

- **API**: `https://localhost:3060` ou `http://localhost:3062`
- **Swagger UI**: `https://localhost:3060/swagger`

### 🔑 Credenciais Padrão (Admin)

O sistema cria automaticamente um usuário administrador:

```text
Email: admin@projecttemplate.com
Password: Admin@2026!Secure
```

> ⚠️ **IMPORTANTE**: Altere esta senha em produção!

**Testar autenticação:**

```bash
# Login
curl -X POST https://localhost:3060/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@projecttemplate.com",
    "password": "Admin@2026!Secure"
  }'

# Copie o accessToken da resposta e use:
curl -X GET https://localhost:3060/api/auth/me \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

Swagger: `https://localhost:3060/swagger`

### Opção C: Visual Studio

1. Abra `MeuProjeto.sln`
2. Defina `Api` (ou `AppHost` para Aspire) como projeto de inicialização
3. Pressione F5

### Opção D: VS Code

1. Abra a pasta do projeto
2. Pressione F5 (debug via `.vscode/launch.json`)
3. Ou use terminal: `dotnet run --project src/Server/Api`

### Opção E: .NET Aspire (Orquestração)

O Aspire sobe automaticamente todos os containers (PostgreSQL, Redis, RabbitMQ, MongoDB) e fornece um dashboard de observabilidade.

```bash
dotnet run --project src/Aspire/AppHost
```

Acesse o dashboard em `http://localhost:18888`.

### Aplicativos Mobile

```bash
./run-mobile.sh   # Linux/Mac
.\run-mobile.ps1  # Windows
```

---

## 🐳 4. Executar com Docker (Opcional)

### Docker Run

```bash
# Build
docker build -t meuprojeto-api:latest .

# Run
docker run -p 8080:8080 meuprojeto-api:latest
```

Acesse: `http://localhost:8080`

### Docker Compose

```bash
docker-compose up -d
```

Acesse: `http://localhost:8080`

Para parar:

```bash
docker-compose down
```

---

## ☸️ 5. Deploy no Kubernetes (Opcional)

### Minikube (Local)

#### Windows (PowerShell)

```powershell
cd scripts/windows
.\minikube-deploy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
chmod +x minikube-deploy.sh
./minikube-deploy.sh
```

### Acessar aplicação

```bash
# Port forward
kubectl port-forward svc/meuprojeto-api 8080:80 -n meuprojeto

# Abrir no navegador
start http://localhost:8080  # Windows
open http://localhost:8080   # macOS
xdg-open http://localhost:8080  # Linux
```

### Limpar deploy

#### Windows

```powershell
cd scripts/windows
.\minikube-destroy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
./minikube-destroy.sh
```

---

## 🧪 6. Executar Testes

### Todos os testes

```bash
dotnet test
```

### Testes específicos

```bash
# Testes de integração
dotnet test tests/Integration/

# Testes unitários
dotnet test tests/Infrastructure.UnitTests/
```

### Script automatizado (Minikube)

#### Windows

```powershell
cd scripts/windows
.\run-integration-tests.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
./run-integration-tests.sh
```

---

## 📝 7. Criar sua Primeira Entidade

### 1. Criar a entidade

`src/Server/Domain/Entities/Product.cs`:

```csharp
namespace MeuProjeto.Domain.Entities;

public class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

### 2. Adicionar ao DbContext

`src/Server/Data/Context/ApplicationDbContext.cs`:

```csharp
public DbSet<Product> Products { get; set; }
```

### 3. Criar migration (EF Core)

```bash
cd src/Server/Api
dotnet ef migrations add AddProduct --project ../Data/Data.csproj
dotnet ef database update
```

### 4. Criar Interface e Repositório concreto

`src/Server/Domain/Interfaces/IProductRepository.cs`:

```csharp
namespace MeuProjeto.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    // Métodos específicos do Product aqui
}
```

`src/Server/Data/Repository/ProductRepository.cs`:

```csharp
namespace MeuProjeto.Data.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}
```

### 5. Criar Interface e Service concreto

`src/Server/Domain/Interfaces/IProductService.cs`:

```csharp
namespace MeuProjeto.Domain.Interfaces;

public interface IProductService : IService<Product>
{
    // Métodos específicos do Product aqui
}
```

`src/Server/Application/Services/ProductService.cs`:

```csharp
namespace MeuProjeto.Application.Services;

public class ProductService : Service<Product>, IProductService
{
    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
        : base(repository, logger)
    {
    }
}
```

> 💡 O **Scrutor** registra automaticamente `ProductRepository → IProductRepository` e `ProductService → IProductService` via `AsMatchingInterface()`.

### 6. Criar o Controller

`src/Server/Api/Controllers/ProductController.cs`:

```csharp
namespace MeuProjeto.Api.Controllers;

[Route("api/[controller]")]
public class ProductController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _productService.GetAllAsync(page, pageSize, cancellationToken);

        if (page.HasValue && pageSize.HasValue)
            return HandlePagedResult(items, total, page.Value, pageSize.Value);

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest dto, CancellationToken cancellationToken)
    {
        var created = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
```

### 7. Testar

Execute a aplicação e acesse: `https://localhost:3060/swagger`

Teste os endpoints criados!

---

### Alternar ORM

**Entity Framework Core é o padrão** e está habilitado no código.

Para trocar de ORM, **não use appsettings.json**:

1. Abra `src/Server/Infrastructure/Extensions/DatabaseExtension.cs`
2. Comente a linha do EF Core (linha ~26)
3. Descomente a linha do ORM desejado (Dapper, NHibernate ou Linq2Db)

**Documentação completa**: [`docs/ORM-GUIDE.md`](docs/ORM-GUIDE.md)

---

## 📚 Próximos Passos

1. **Documentação completa**: [README.md](README.md)
2. **Guia de ORMs**: [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md)
3. **Autenticação**: [docs/AUTHENTICATION.md](docs/AUTHENTICATION.md)
4. **Observabilidade**: [docs/TELEMETRY.md](docs/TELEMETRY.md)
5. **Deploy Kubernetes**: [docs/KUBERNETES.md](docs/KUBERNETES.md)
6. **Todos os guias**: [docs/README.md](docs/README.md)

---

## 🆘 Problemas Comuns

### Erro: SDK do .NET 10 não encontrado

**Solução**: Instale o .NET 10 RC 2 ou superior:

- https://dotnet.microsoft.com/download/dotnet/10.0

### Erro: Não é possível conectar ao banco de dados

**Solução**:

1. Verifique se o SQL Server está rodando
2. Confirme a connection string em `appsettings.Development.json`
3. Teste a conexão: `dotnet ef database update`

### Erro: Porta já está em uso

**Solução**:

```bash
# Use outra porta
dotnet run --urls "http://localhost:5005"
```

### Docker: erro de build

**Solução**:

1. Verifique se o Docker está rodando
2. Limpe o cache: `docker system prune -a`
3. Rebuild: `docker build -t meuprojeto-api:latest . --no-cache`

### Minikube: erro ao carregar imagem

**Solução**:

```bash
# Configure o Docker para usar o Minikube
eval $(minikube docker-env)

# Rebuild a imagem
docker build -t meuprojeto-api:latest .
```

---

## 💡 Dicas

### Hot Reload

```bash
dotnet watch --project src/Server/Api/Api.csproj
```

### Ver logs estruturados

Use uma ferramenta como [dotnet-serilog-viewer](https://github.com/datalust/seqcli) ou configure Seq, Elasticsearch, etc.

### IntelliSense no VS Code

Instale as extensões:

- C# Dev Kit
- .NET Extension Pack

### Debug no VS Code

O template já inclui `.vscode/launch.json` configurado. Pressione F5!

---

## ✅ Checklist de Início

- [ ] .NET 10 SDK instalado
- [ ] Projeto criado com `new-project.sh` (banco, mensageria, UI selecionados)
- [ ] Connection string configurada
- [ ] Aplicação executando (`./run.sh` ou `dotnet run`)
- [ ] Swagger acessível em `https://localhost:3060/swagger`
- [ ] Login com credenciais padrão testado
- [ ] Primeira entidade criada e migration aplicada
- [ ] Primeiro controller testado

---

## 🎉 Pronto

Seu projeto está configurado e rodando! Agora é só desenvolver sua aplicação.

Para dúvidas ou sugestões, abra uma issue no repositório.

**Happy Coding! 🚀**
