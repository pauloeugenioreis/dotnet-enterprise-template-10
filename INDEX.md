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

3. **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** 🏗️
   - Visão detalhada da arquitetura Clean Architecture
   - Diagramas de camadas e fluxos
   - Padrões implementados e princípios SOLID

4. **[docs/ORM-GUIDE.md](docs/ORM-GUIDE.md)** 🔄
   - Guia completo de ORMs suportados
   - Como alternar entre EF Core, Dapper, NHibernate, Linq2Db
   - Comparações e casos de uso

5. **[docs/KUBERNETES.md](docs/KUBERNETES.md)** ☸️
   - Deploy completo no Kubernetes
   - Minikube (local) e clusters de produção
   - Troubleshooting, monitoramento, segurança

---

## 📖 Referência Completa

### Documentação Principal

| Documento | Descrição | Quando Usar |
|-----------|-----------|-------------|
| **[README.md](README.md)** | Documentação principal e overview completo | Entender o projeto como um todo |
| **[QUICK-START.md](QUICK-START.md)** | Início rápido em 5 minutos | Primeira vez usando o template |
| **[CHANGELOG.md](CHANGELOG.md)** | Histórico de mudanças e versões | Ver o que mudou entre versões |
| **[CONTRIBUTING.md](CONTRIBUTING.md)** | Guia de contribuição | Contribuir com código ou documentação |
| **[DEPRECATED-PACKAGES-REPORT.md](DEPRECATED-PACKAGES-REPORT.md)** | 📦 Relatório de pacotes deprecated | Manutenção e atualização de dependências |
| **[LICENSE](LICENSE)** | Licença do projeto (MIT) | Entender termos de uso |

### Guias Técnicos

| Documento | Descrição | Quando Usar |
|-----------|-----------|-------------|
| **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** | Visão detalhada da arquitetura | Entender Clean Architecture e padrões |
| **[docs/FEATURES.md](docs/FEATURES.md)** | Recursos avançados (MongoDB, Queue, Jobs, etc.) | Usar MongoDB, RabbitMQ, Quartz, Storage |
| **[docs/ORM-GUIDE.md](docs/ORM-GUIDE.md)** | Guia de ORMs (EF Core, Dapper, etc.) | Escolher ou alternar ORM |
| **[docs/KUBERNETES.md](docs/KUBERNETES.md)** | Deploy no Kubernetes | Deploy em produção ou local |

### Arquivos de Configuração

| Arquivo | Descrição | Quando Usar |
|---------|-----------|-------------|
| **[.env.example](.env.example)** | Exemplo de variáveis de ambiente | Configurar ambiente local |
| **[global.json](global.json)** | Versão do .NET SDK | Garantir SDK correto |
| **[docker-compose.yml](docker-compose.yml)** | Configuração Docker Compose | Rodar com Docker localmente |
| **[Dockerfile](Dockerfile)** | Build da imagem Docker | Criar imagem Docker |

---

## 🎯 Guia Por Cenário

### "Quero criar um novo projeto"

1. Leia: [QUICK-START.md](QUICK-START.md) - Seção "Criar Novo Projeto"
2. Execute: `./scripts/new-project.sh MeuProjeto` (ou .ps1/.bat)
3. Configure: `src/Api/appsettings.Development.json`
4. Rode: `dotnet run --project src/Api/Api.csproj`

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

### "Quero contribuir com o projeto"

1. Leia: [CONTRIBUTING.md](CONTRIBUTING.md) - Guia completo
2. Setup: Clone, crie branch, faça alterações
3. Teste: Rode todos os testes
4. PR: Abra Pull Request com descrição clara

### "Quero configurar cache Redis"

1. Leia: [README.md](README.md) - Seção "Configuração de Cache"
2. Configure: `AppSettings.Infrastructure.Cache.Provider = "Redis"`
3. Defina: Connection string do Redis
4. Teste: Verifique health check

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

### "Preciso de help com erros"

1. Veja: [QUICK-START.md](QUICK-START.md) - Seção "Problemas Comuns"
2. Veja: [docs/KUBERNETES.md](docs/KUBERNETES.md) - Seção "Problemas Comuns"
3. Pesquise: Issues abertas no repositório
4. Abra: Nova issue se não encontrar solução

---

## 📂 Estrutura de Arquivos

```
template/
├── 📄 README.md                     # Documentação principal
├── 📄 QUICK-START.md                # Início rápido
├── 📄 CHANGELOG.md                  # Histórico de mudanças
├── 📄 CONTRIBUTING.md               # Guia de contribuição
├── 📄 LICENSE                       # Licença MIT
├── 📄 INDEX.md                      # Este arquivo
├── 📄 .env.example                  # Exemplo de variáveis de ambiente
├── 📄 .gitignore                    # Git ignore
├── 📄 global.json                   # SDK do .NET
├── 📄 ProjectTemplate.sln           # Solution file
├── 📄 Dockerfile                    # Docker image
├── 📄 docker-compose.yml            # Docker compose
│
├── 📁 docs/                         # Documentação adicional
│   ├── 📄 ARCHITECTURE.md          # Arquitetura detalhada
│   ├── 📄 FEATURES.md              # Recursos avançados
│   ├── 📄 ORM-GUIDE.md             # Guia de ORMs
│   ├── 📄 TELEMETRY.md             # Telemetria e observabilidade
│   ├── 📄 RATE-LIMITING.md         # Rate limiting
│   ├── 📄 EVENT-SOURCING.md        # Event sourcing
│   ├── 📄 AUTHENTICATION.md        # JWT & OAuth2
│   ├── 📄 CICD.md                  # CI/CD pipelines
│   ├── 📄 KUBERNETES.md            # Guia Kubernetes
│   └── 📁 examples/                # Exemplos práticos
│
├── 📁 src/                          # Código fonte
│   ├── 📁 Api/                     # Camada de apresentação
│   ├── 📁 Application/             # Camada de aplicação
│   ├── 📁 Domain/                  # Camada de domínio
│   ├── 📁 Data/                    # Camada de dados
│   └── 📁 Infrastructure/          # Camada de infraestrutura
│
├── 📁 tests/                        # Testes
│   ├── 📁 Integration/             # Testes de integração
│   └── 📁 Infrastructure.UnitTests/ # Testes unitários
│
├── 📁 .k8s/                         # Kubernetes manifests
│   ├── 📄 namespace.yaml
│   ├── 📄 configmap.yaml
│   ├── 📄 deployment.yaml
│   ├── 📄 service.yaml
│   ├── 📄 ingress.yaml
│   └── 📄 kustomization.yaml
│
└── 📁 scripts/                      # Scripts de automação
    ├── 📁 linux/                   # Scripts bash
    ├── 📁 windows/                 # Scripts PowerShell/Batch
    ├── 📄 new-project.sh
    ├── 📄 new-project.ps1
    └── 📄 new-project.bat
```

---

## 🔍 Busca Rápida

### Por Tecnologia

- **Entity Framework Core**: [ORM-GUIDE.md](docs/ORM-GUIDE.md#entity-framework-core)
- **Dapper**: [ORM-GUIDE.md](docs/ORM-GUIDE.md#dapper)
- **NHibernate**: [ORM-GUIDE.md](docs/ORM-GUIDE.md#nhibernate)
- **Docker**: [QUICK-START.md](QUICK-START.md#4-executar-com-docker-opcional)
- **Kubernetes**: [KUBERNETES.md](docs/KUBERNETES.md)
- **Redis**: [README.md](README.md#redis-recomendado-para-producao)
- **MongoDB**: [FEATURES.md](docs/FEATURES.md#1-mongodb)
- **Quartz.NET**: [FEATURES.md](docs/FEATURES.md#2-quartznet)
- **RabbitMQ**: [FEATURES.md](docs/FEATURES.md#3-rabbitmq)
- **JWT Auth**: [FEATURES.md](docs/FEATURES.md#5-jwt-authentication)
- **API Versioning**: [FEATURES.md](docs/FEATURES.md#6-api-versioning)

### Por Tarefa

- **Criar projeto**: [QUICK-START.md](QUICK-START.md#1-criar-novo-projeto)
- **Configurar DB**: [QUICK-START.md](QUICK-START.md#2-configurar-banco-de-dados)
- **Primeira entidade**: [QUICK-START.md](QUICK-START.md#7-criar-sua-primeira-entidade)
- **Deploy K8s**: [KUBERNETES.md](docs/KUBERNETES.md#-deploy-local-com-minikube)
- **Rodar testes**: [QUICK-START.md](QUICK-START.md#6-executar-testes)
- **Contribuir**: [CONTRIBUTING.md](CONTRIBUTING.md)

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

## 📈 Próximos Passos

Depois de dominar o básico:

1. **Explore recursos avançados**: Health checks, observability, rate limiting
2. **Configure CI/CD**: GitHub Actions, Azure DevOps
3. **Adicione testes**: Unit tests, integration tests, e2e tests
4. **Implemente features**: Auth, file upload, background jobs
5. **Otimize performance**: Caching, compression, query optimization
6. **Monitore em produção**: Application Insights, Prometheus, Grafana

---

## 🎓 Recursos de Aprendizado

### Clean Architecture

- [The Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Clean Architecture with .NET](https://jasontaylor.dev/clean-architecture-getting-started/)

### ASP.NET Core

- [Microsoft Learn - ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/aspnet/core/fundamentals/best-practices)

### Kubernetes

- [Kubernetes for .NET Developers](https://docs.microsoft.com/dotnet/architecture/kubernetes/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)

### ORMs

- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [Dapper Tutorial](https://github.com/DapperLib/Dapper)

---

## ✅ Checklist de Uso

### Primeiro Uso

- [ ] Li o [QUICK-START.md](QUICK-START.md)
- [ ] Criei meu primeiro projeto
- [ ] Configurei o banco de dados
- [ ] Executei a aplicação
- [ ] Acessei o Swagger
- [ ] Criei minha primeira entidade

### Antes de Deploy

- [ ] Li o [KUBERNETES.md](docs/KUBERNETES.md)
- [ ] Testei localmente com Minikube
- [ ] Configurei variáveis de ambiente (`.env`)
- [ ] Configurei secrets (senhas, tokens)
- [ ] Executei todos os testes
- [ ] Revisei logs e health checks

### Antes de Contribuir

- [ ] Li o [CONTRIBUTING.md](CONTRIBUTING.md)
- [ ] Pesquisei issues existentes
- [ ] Criei branch descritiva
- [ ] Escrevi testes
- [ ] Segui padrões de código
- [ ] Atualizei documentação

---

## 🎉 Conclusão

Este índice deve ajudá-lo a navegar pela documentação do template. Comece pelo [QUICK-START.md](QUICK-START.md) e explore conforme necessário!

**Happy Coding! 🚀**

---

*Última atualização: Janeiro 2025 | Versão: 1.0.0*
