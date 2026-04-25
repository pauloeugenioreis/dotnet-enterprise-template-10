# 📚 Índice de Documentação - .NET 10 Clean Architecture Template

Bem-vindo ao template! Este índice ajuda você a navegar pela documentação completa do projeto.

---

## 🚀 Começando

### Para Iniciantes

1. **[QUICK-START.md](QUICK-START.md)** ⚡
    - Comece aqui! Guia rápido para rodar o projeto em 5 minutos
    - Setup inicial, primeiro projeto, primeira entidade
    - Problemas comuns e soluções

2. **[README.md](README.md)** 📖
    - Visão geral completa do template
    - Características, estrutura, configurações
    - Exemplos de uso detalhados

### Para Desenvolvedores Experientes

1. **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** 🏗️
    - Visão detalhada da arquitetura Clean Architecture
    - Diagramas de camadas e fluxos
    - Padrões implementados e princípios SOLID

2. **[docs/ORM-GUIDE.md](docs/ORM-GUIDE.md)** 🔄
    - Guia completo de ORMs suportados
    - Como alternar entre EF Core, Dapper, NHibernate, Linq2Db
    - Comparações e casos de uso

3. **[docs/MONGODB-GUIDE.md](docs/MONGODB-GUIDE.md)** 🗄️
    - Guia dedicado para MongoDB no template
    - Seed automático, autenticação do container e troubleshooting
    - Exemplos com `CustomerReview` e `MongoDbSeeder`

4. **[docs/KUBERNETES.md](docs/KUBERNETES.md)** ☸️
    - Deploy completo no Kubernetes
    - Minikube (local) e clusters de produção
    - Troubleshooting, monitoramento, segurança

---

## 📖 Referência Completa

### Documentação Principal

| Documento                                                | Descrição                                  | Quando Usar                       |
| -------------------------------------------------------- | ------------------------------------------ | --------------------------------- |
| **[README.md](README.md)**                               | Documentação principal e overview completo | Entender o projeto como um todo   |
| **[QUICK-START.md](QUICK-START.md)**                     | Início rápido em 5 minutos                 | Primeira vez usando o template    |
| **[LICENSE](LICENSE)**                                   | Licença do projeto (MIT)                   | Entender termos de uso            |
| **[sonar-project.properties](sonar-project.properties)** | Configuração do SonarCloud                 | Configurar análise de qualidade   |

### Guias Técnicos

| Documento                                                      | Descrição                                       | Quando Usar                             |
| -------------------------------------------------------------- | ----------------------------------------------- | --------------------------------------- |
| **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)**               | Visão detalhada da arquitetura                  | Entender Clean Architecture e padrões   |
| **[docs/FEATURES.md](docs/FEATURES.md)**                       | Recursos avançados (MongoDB, Queue, Jobs, etc.) | Usar MongoDB, RabbitMQ, Quartz, Storage |
| **[docs/MONGODB-GUIDE.md](docs/MONGODB-GUIDE.md)**             | Guia prático de MongoDB no template             | Configurar seed, auth e troubleshooting |
| **[docs/ORM-GUIDE.md](docs/ORM-GUIDE.md)**                     | Guia de ORMs (EF Core, Dapper, etc.)            | Escolher ou alternar ORM                |
| **[docs/CONFIGURATION-GUIDE.md](docs/CONFIGURATION-GUIDE.md)** | Configuração com IOptions\<T\>                  | Acessar configurações no código         |
| **[docs/AUTHENTICATION.md](docs/AUTHENTICATION.md)**           | JWT & OAuth2 (Google, Microsoft, GitHub)        | Configurar autenticação                 |
| **[docs/SECURITY.md](docs/SECURITY.md)**                       | Headers, CORS, HTTPS, proteção contra ataques   | Revisar segurança da API                |
| **[docs/RATE-LIMITING.md](docs/RATE-LIMITING.md)**             | 4 estratégias de Rate Limiting                  | Controle de taxa de requisições         |
| **[docs/EVENT-SOURCING.md](docs/EVENT-SOURCING.md)**           | Event Sourcing com Marten + PostgreSQL          | Auditoria e time travel                 |
| **[docs/TELEMETRY.md](docs/TELEMETRY.md)**                     | OpenTelemetry, Jaeger, Prometheus, Grafana      | Configurar observabilidade              |
| **[docs/UI-GUIDE.md](docs/UI-GUIDE.md)**                       | Angular, Blazor, React, Vue — integração API    | Desenvolver frontends Web               |
| **[docs/MOBILE-GUIDE.md](docs/MOBILE-GUIDE.md)**               | Flutter, MAUI, React Native — setup e debug     | Desenvolver apps Mobile                 |
| **[docs/TESTING-DATABASES.md](docs/TESTING-DATABASES.md)**     | Testes multi-banco de dados                     | Validar suporte a 4+ bancos             |
| **[docs/KUBERNETES.md](docs/KUBERNETES.md)**                   | Deploy no Kubernetes                            | Deploy em produção ou local             |
| **[docs/CICD.md](docs/CICD.md)**                               | GitHub Actions, Azure DevOps, GitLab CI         | Configurar CI/CD                        |
| **[docs/SONARCLOUD.md](docs/SONARCLOUD.md)**                   | Configuração do SonarCloud                      | Implementar análise de qualidade        |

### Arquivos de Configuração

| Arquivo                                      | Descrição                        | Quando Usar                 |
| -------------------------------------------- | -------------------------------- | --------------------------- |
| **[.env.example](.env.example)**             | Exemplo de variáveis de ambiente | Configurar ambiente local   |
| **[global.json](global.json)**               | Versão do .NET SDK               | Garantir SDK correto        |
| **[docker-compose.yml](docker-compose.yml)** | Configuração Docker Compose      | Rodar com Docker localmente |
| **[Dockerfile](Dockerfile)**                 | Build da imagem Docker           | Criar imagem Docker         |

---

## 🎯 Guia Por Cenário

### "Quero criar um novo projeto"

1. Leia: [QUICK-START.md](QUICK-START.md) - Seção "Criar Novo Projeto"
2. Execute: `cd scripts && ./new-project.sh MeuProjeto` (ou `.ps1` / `.bat`)
3. O script interativo configura banco, mensageria, storage, telemetria e event sourcing
4. Rode: `dotnet run --project src/Server/Api/Api.csproj`

### "Quero usar outro banco de dados"

1. Leia: [README.md](README.md) - Seção "Configuração de Banco de Dados"
2. Leia: [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md) - Seção específica do banco
3. Configure: Connection string em `appsettings.json`
4. Descomente: Provider correto em `Data.csproj`

### "Quero usar outro ORM (Dapper, NHibernate)"

1. Leia: [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md) - Seção completa
2. Altere: `AppSettings.Infrastructure.Database.Provider` em config
3. Implemente: Interfaces `IRepository<T>` no ORM escolhido
4. Teste: Verifique se tudo funciona

### "Quero fazer deploy no Kubernetes"

1. Leia: [docs/KUBERNETES.md](docs/KUBERNETES.md) - Guia completo
2. Para local: Execute `./scripts/linux/minikube-deploy.sh` (ou Windows)
3. Para produção: Siga seção "Deploy em Produção"
4. Configure: Ajuste resources, replicas, secrets

### "Quero rodar com Docker"

1. Leia: [QUICK-START.md](QUICK-START.md) - Seção "Executar com Docker"
2. Build: `docker build -t myapp:latest .`
3. Run: `docker-compose up -d`
4. Acesse: `http://localhost:8080`

### "Quero adicionar autenticação"

1. Leia: [docs/AUTHENTICATION.md](docs/AUTHENTICATION.md) - Guia completo de JWT & OAuth2
2. Configure: `AppSettings.Authentication` em appsettings.json
3. Migration: `dotnet ef migrations add AddAuthentication`
4. Teste: Use o Swagger com "Authorize" button

### "Quero usar MongoDB"

1. Leia: [docs/FEATURES.md](docs/FEATURES.md) - Seção "MongoDB"
2. Configure: Connection string em appsettings.json
3. Descomente: MongoDB.Driver em Infrastructure.csproj
4. Ative: `builder.AddMongo()` em Program.cs

### "Quero usar jobs em background"

1. Leia: [docs/FEATURES.md](docs/FEATURES.md) - Seção "Quartz.NET"
2. Configure: `AppSettings.Quartz` em appsettings.json
3. Descomente: Quartz packages em Infrastructure.csproj
4. Ative: `builder.AddQuartz()` em Program.cs

### "Quero usar filas (Message Queue)"

1. Leia: [docs/FEATURES.md](docs/FEATURES.md) - Seção "RabbitMQ"
2. Configure: Connection string em appsettings.json
3. Descomente: RabbitMQ.Client em Infrastructure.csproj
4. Ative: `builder.AddRabbitMq()` em Program.cs

### "Quero desenvolver um frontend Web"

1. Selecione o framework desejado ao rodar `new-project.sh`
2. Os frontends ficam em `src/UI/Web/<Framework>/`
3. Execute via `./run.sh` (Aspire orquestra tudo) ou `./build-web-all.sh` para build
4. A API está disponível em `http://localhost:5266`

### "Quero desenvolver um app Mobile"

1. Selecione Flutter, MAUI ou React Native ao rodar `new-project.sh`
2. Os apps ficam em `src/UI/Mobile/<Framework>/`
3. Execute via `./run-mobile.sh` (Linux/Mac) ou `.\run-mobile.ps1` (Windows)
4. A API está disponível em `http://localhost:5266`

### "Preciso de help com erros"

1. Veja: [QUICK-START.md](QUICK-START.md) - Seção "Problemas Comuns"
2. Veja: [docs/KUBERNETES.md](docs/KUBERNETES.md) - Seção "Problemas Comuns"
3. Pesquise: Issues abertas no repositório
4. Abra: Nova issue se não encontrar solução

---

## 📂 Estrutura de Arquivos

```text
ProjectTemplate/
├── 📄 README.md / QUICK-START.md / INDEX.md
├── 📄 global.json / ProjectTemplate.sln / docker-compose.yml
├── 📄 run.sh / run.ps1                    # Launcher principal (API ou Aspire)
├── 📄 run-mobile.sh / run-mobile.ps1      # Launcher mobile
├── 📄 build-web-all.sh / build-mobile-all.sh
│
├── 📁 src/
│   ├── 📁 Aspire/
│   │   ├── 📁 AppHost/                    # Orquestrador .NET Aspire
│   │   └── 📁 ServiceDefaults/            # OTel, Health Checks padrão
│   ├── 📁 Server/                         # Backend — Clean Architecture
│   │   ├── 📁 Api/
│   │   ├── 📁 Application/
│   │   ├── 📁 Domain/
│   │   ├── 📁 Data/
│   │   └── 📁 Infrastructure/
│   ├── 📁 Shared/                         # Modelos compartilhados
│   └── 📁 UI/
│       ├── 📁 Web/ (Angular / Blazor / React / Vue)
│       └── 📁 Mobile/ (Flutter / MAUI / React Native)
│
├── 📁 tests/
│   ├── 📁 Integration/                    # Testcontainers
│   ├── 📁 UnitTests/
│   └── 📁 ArchitectureTests/
│
├── 📁 docs/                               # Guias técnicos detalhados
│   └── 📁 examples/
│
├── 📁 .k8s/                               # Kubernetes manifests
└── 📁 scripts/
    ├── 📁 linux/ / 📁 windows/
    ├── 📄 new-project.sh / .ps1 / .bat
    └── 📁 mongo-init/
```

---

## 🔍 Busca Rápida

### Por Tecnologia

- **Entity Framework Core**: [ORM-GUIDE.md](docs/ORM-GUIDE.md#entity-framework-core)
- **Dapper**: [ORM-GUIDE.md](docs/ORM-GUIDE.md#dapper)
- **NHibernate**: [ORM-GUIDE.md](docs/ORM-GUIDE.md#nhibernate)
- **Docker**: [QUICK-START.md](QUICK-START.md#4-executar-com-docker-opcional)
- **Kubernetes**: [KUBERNETES.md](docs/KUBERNETES.md)
- **MongoDB**: [MONGODB-GUIDE.md](docs/MONGODB-GUIDE.md)
- **Quartz.NET**: [FEATURES.md](docs/FEATURES.md#2-quartznet)
- **RabbitMQ**: [FEATURES.md](docs/FEATURES.md#3-rabbitmq)
- **JWT Auth**: [AUTHENTICATION.md](docs/AUTHENTICATION.md)
- **API Versioning**: [FEATURES.md](docs/FEATURES.md#6-api-versioning)
- **Angular / React / Vue / Blazor**: `src/UI/Web/<Framework>/`
- **Flutter / MAUI / React Native**: `src/UI/Mobile/<Framework>/`
- **.NET Aspire**: `src/Aspire/AppHost/`

### Por Tarefa

- **Criar projeto**: [QUICK-START.md](QUICK-START.md#1-criar-novo-projeto)
- **Configurar DB**: [QUICK-START.md](QUICK-START.md#2-configurar-banco-de-dados)
- **Primeira entidade**: [QUICK-START.md](QUICK-START.md#7-criar-sua-primeira-entidade)
- **Deploy K8s**: [KUBERNETES.md](docs/KUBERNETES.md#-deploy-local-com-minikube)
- **Rodar testes**: [QUICK-START.md](QUICK-START.md#6-executar-testes)

---

## 🆘 Precisa de Ajuda?

### Documentação Oficial

- [Documentação .NET 10](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [Kubernetes](https://kubernetes.io/docs/)
- [Docker](https://docs.docker.com/)

### Comunidade

- Abra uma [Issue](../../issues) para reportar bugs
- Crie uma [Discussion](../../discussions) para perguntas
- Envie um [Pull Request](../../pulls) para contribuir

---

Comece pelo [QUICK-START.md](QUICK-START.md) e navegue pelos guias em [docs/](docs/README.md) conforme necessário.
