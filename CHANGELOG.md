# Changelog

Todas as mudanças notáveis neste template serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [1.0.0] - 2025-01-XX

### ✨ Adicionado

#### Arquitetura
- Clean Architecture com 5 camadas (Domain, Data, Application, Infrastructure, Api)
- Separação clara de responsabilidades e dependências
- Padrão Repository genérico com interface `IRepository<T>`
- Padrão Service genérico com classe base `Service<T>`

#### Multi-ORM
- Suporte a múltiplos ORMs através de abstrações
- Entity Framework Core 10.0.1 (padrão)
- Dapper 2.1.66 (pronto para uso)
- NHibernate 5.5.2 (preparado)
- Linq2Db 5.4.1 (preparado)
- Documentação completa em `docs/ORM-GUIDE.md`

#### Infraestrutura
- **Cache distribuído**: Memory (dev), Redis (prod), SQL Server (opcional)
- **Health Checks**: Basic, Ready, com suporte a bancos de dados
- **Logging estruturado**: Console, Debug, EventLog, Google Cloud Logging
- **Observabilidade**: OpenTelemetry configurado
- **CORS**: Configuração por ambiente
- **Compression**: Brotli e Gzip
- **Rate Limiting**: Configurável por ambiente
- **Dependency Injection**: Scrutor para registro automático de serviços
- **MongoDB**: Suporte a NoSQL com MongoExtension
- **Quartz.NET**: Background jobs e agendamento de tarefas
- **RabbitMQ**: Message queue para comunicação assíncrona
- **Google Cloud Storage**: Serviço de armazenamento de arquivos
- **JWT Authentication**: Autenticação baseada em tokens
- **API Versioning**: Versionamento via URL, Header ou Query String
- **Global Exception Handler**: Tratamento centralizado com ProblemDetails
- **Validation Filter**: Validação automática com FluentValidation
- **Swagger Customizado**: UI melhorada, agrupamento, JWT, XML docs
- **Exception Notifications**: Serviço extensível para notificações (email/Slack/etc)

#### Banco de Dados
- Suporte a SQL Server (padrão)
- Suporte a Oracle
- Suporte a PostgreSQL
- Suporte a MySQL/MariaDB
- Connection string configurável por ambiente

#### Configurações
- `AppSettings.cs` fortemente tipado
- Validação em startup com `IValidateOptions`
- Suporte a múltiplos ambientes (Development, Staging, Production)
- Secrets via User Secrets ou Azure Key Vault

#### Docker
- Multi-stage Dockerfile otimizado
- Docker Compose para desenvolvimento local
- Health checks no container
- Imagem baseada em ASP.NET Core Runtime

#### Kubernetes
- Manifests prontos para deploy
  - Namespace
  - ConfigMap (variáveis de ambiente)
  - Deployment (2 réplicas, health probes, resource limits)
  - Service (ClusterIP)
  - Ingress (Nginx)
  - Kustomization (customizações)
- Security context (non-root user, read-only filesystem)
- Rolling update strategy
- Liveness e Readiness probes
- Resource requests e limits configurados

#### Scripts de Automação
- **Linux/macOS (Bash)**:
  - `minikube-deploy.sh` - Deploy automático no Minikube
  - `minikube-destroy.sh` - Limpeza do Minikube
  - `run-integration-tests.sh` - Execução de testes
  
- **Windows (PowerShell)**:
  - `minikube-deploy.ps1` - Deploy automático no Minikube
  - `minikube-destroy.ps1` - Limpeza do Minikube
  - `run-integration-tests.ps1` - Execução de testes
  
- **Windows (Batch)**:
  - `minikube-deploy.bat` - Deploy automático no Minikube
  - `minikube-destroy.bat` - Limpeza do Minikube
  - `run-integration-tests.bat` - Execução de testes

#### Scripts de Inicialização
- `new-project.sh` - Inicialização de projeto (Linux/macOS)
- `new-project.ps1` - Inicialização de projeto (Windows PowerShell)
- `new-project.bat` - Inicialização de projeto (Windows CMD)

#### API
- `ApiControllerBase` com métodos helper para respostas
- Versionamento de API preparado
- Swagger configurado com documentação
- Validação automática de modelos
- Response caching
- Suporte a paginação

#### Testes
- Estrutura de testes de integração
- Estrutura de testes unitários de infraestrutura
- Script automatizado de testes

#### Documentação
- **README.md** - Guia completo do template
- **FEATURES.md** - Recursos avançados (MongoDB, Queue, Jobs, Storage, Auth)
- **ORM-GUIDE.md** - Guia de ORMs e como alternar
- **KUBERNETES.md** - Guia de deploy Kubernetes
- **CHANGELOG.md** - Histórico de mudanças
- Comentários inline no código

#### Configuração de Projeto
- `.gitignore` completo para .NET
- `global.json` com SDK .NET 10.0
- Estrutura de solution organizada
- LICENSE (MIT)

---

## 🔮 Planejado para Futuras Versões

### [1.1.0] - Planejado

#### Segurança
- ~~Implementação de Authentication/Authorization~~ ✅ **Implementado em 1.0.0**
- ~~Suporte a JWT~~ ✅ **Implementado em 1.0.0**
- Suporte a OAuth2/OpenID Connect
- Network Policies para Kubernetes

#### Testes
- Projetos de testes com xUnit configurado
- Testes de exemplo (unit, integration, e2e)
- Code coverage configurado

#### CI/CD
- GitHub Actions workflows
- Azure DevOps pipelines
- GitLab CI/CD
- Integração com SonarQube

#### Monitoramento
- Prometheus metrics
- Grafana dashboards
- Elastic APM
- Application Insights

#### Recursos Adicionais
- API Gateway (Ocelot/YARP)
- ~~Message Broker (RabbitMQ/Azure Service Bus)~~ ✅ **RabbitMQ implementado em 1.0.0**
- Event Sourcing pattern
- CQRS pattern
- Outbox pattern

#### Banco de Dados
- Migrations automáticas
- Seed data
- Backup automatizado

#### Kubernetes Avançado
- Horizontal Pod Autoscaler (HPA)
- Vertical Pod Autoscaler (VPA)
- Pod Disruption Budget
- Service Mesh (Istio/Linkerd)
- Helm Charts
- Kustomize overlays (dev/staging/prod)

---

## 📝 Como Contribuir

Se você tem sugestões de melhorias ou encontrou bugs:

1. Abra uma issue descrevendo o problema ou sugestão
2. Fork o repositório
3. Crie uma branch para sua feature/fix
4. Commit suas mudanças
5. Push para sua branch
6. Abra um Pull Request

---

## 📊 Estatísticas da Versão

### v1.0.0
- **Arquivos de código**: 50+
- **Linhas de código**: 4000+
- **Documentação**: 7500+ linhas
- **Scripts**: 9 arquivos
- **Manifestos K8s**: 6 arquivos
- **Pacotes NuGet**: 40+
- **Recursos avançados**: 11 (MongoDB, Quartz, RabbitMQ, Storage, Auth, Versioning, Exception Handler, Validation, Logging, Swagger, Notifications)

---

## 🎯 Roadmap

- [x] Clean Architecture base
- [x] Multi-ORM support
- [x] Docker e Docker Compose
- [x] Kubernetes manifests
- [x] Scripts de automação
- [x] Documentação completa
- [x] Authentication/Authorization (JWT)
- [x] MongoDB support
- [x] Background jobs (Quartz.NET)
- [x] Message queue (RabbitMQ)
- [x] Cloud storage (Google Cloud Storage)
- [x] API Versioning
- [x] Global exception handling
- [x] Automatic validation
- [ ] CI/CD pipelines
- [ ] Monitoring e observability
- [ ] Helm Charts
- [ ] Message broker integration
- [ ] Event Sourcing/CQRS examples

---

Para mais informações sobre cada feature, consulte a documentação específica em `docs/`.
