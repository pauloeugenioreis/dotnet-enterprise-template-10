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

O script interativo configura banco de dados, cache, mensageria, storage, telemetria e event sourcing automaticamente.

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

> 💡 O script apresenta menus interativos para escolher banco de dados, cache (Redis), mensageria (RabbitMQ), cloud storage, telemetria e event sourcing. Também suporta modo não-interativo para CI/CD — veja [scripts/README.md](scripts/README.md) para detalhes.

---

## ⚙️ 2. Configurar Banco de Dados (se necessário)

> Se você usou o script interativo acima, o banco já foi configurado automaticamente. Esta seção é para ajustes manuais.

Edite `src/Api/appsettings.json` e ajuste a connection string para seu ambiente:

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

---

## 🏃 3. Executar a Aplicação

### Opção A: .NET CLI (Desenvolvimento)

```bash
cd src/Api
dotnet run
```

Acesse:

- **API**: `https://localhost:3060` ou `http://localhost:3062`
- **Swagger UI**: `https://localhost:3060/swagger`

### 🔑 Credenciais Padrão (Admin)

O sistema cria automaticamente um usuário administrador:

```text
Username: admin
Password: Admin@2026!Secure
Email:    admin@projecttemplate.com
```

> ⚠️ **IMPORTANTE**: Altere esta senha em produção!

**Testar autenticação:**

```bash
# Login
curl -X POST https://localhost:3060/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "admin",
    "password": "Admin@2026!Secure"
  }'

# Copie o accessToken da resposta e use:
curl -X GET https://localhost:3060/api/auth/me \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

Swagger: `https://localhost:3060/swagger`

### Opção B: Visual Studio

1. Abra `MeuProjeto.sln`
2. Defina `Api` como projeto de inicialização
3. Pressione F5

### Opção C: VS Code

1. Abra a pasta do projeto
2. Pressione F5 (debug)
3. Ou use terminal: `dotnet run --project src/Api/Api.csproj`

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

`src/Domain/Entities/Product.cs`:

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

`src/Data/Context/ApplicationDbContext.cs`:

```csharp
public DbSet<Product> Products { get; set; }
```

### 3. Criar migration (EF Core)

```bash
cd src/Api
dotnet ef migrations add AddProduct --project ../Data/Data.csproj
dotnet ef database update
```

### 4. Criar o Controller

`src/Api/Controllers/ProductController.cs`:

```csharp
namespace MeuProjeto.Api.Controllers;

[Route("api/[controller]")]
public class ProductController : ApiControllerBase
{
    private readonly IRepository<Product> _repository;

    public ProductController(IRepository<Product> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product, CancellationToken cancellationToken)
    {
        var created = await _repository.CreateAsync(product, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
```

### 5. Testar

Execute a aplicação e acesse: `https://localhost:3060/swagger`

Teste os endpoints criados!

---

## 🔧 8. Configurações Importantes

### Cache (Development = Memory)

`appsettings.Development.json`:

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Cache": {
        "Enabled": true,
        "Provider": "Memory"
      }
    }
  }
}
```

### Cache (Production = Redis)

`appsettings.Production.json`:

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Cache": {
        "Enabled": true,
        "Provider": "Redis",
        "Redis": {
          "ConnectionString": "localhost:6379"
        }
      }
    }
  }
}
```

### Alternar ORM

**Entity Framework Core é o padrão** e está habilitado no código.

Para trocar de ORM, **não use appsettings.json**:

1. Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`
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
dotnet watch --project src/Api/Api.csproj
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
- [ ] Projeto criado com script de inicialização
- [ ] Connection string configurada
- [ ] Aplicação executando (dotnet run)
- [ ] Swagger acessível
- [ ] Primeira entidade criada
- [ ] Migration aplicada
- [ ] Primeiro controller testado

---

## 🎉 Pronto

Seu projeto está configurado e rodando! Agora é só desenvolver sua aplicação.

Para dúvidas ou sugestões, abra uma issue no repositório.

**Happy Coding! 🚀**
