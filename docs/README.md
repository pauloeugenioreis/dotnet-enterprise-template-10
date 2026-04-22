# 📚 Documentação Adicional

Esta pasta contém documentação técnica adicional e guias especializados para o template.

---

## 📄 Documentos Disponíveis

| Documento | Descrição | Quando Usar |
|-----------|-----------|-------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Clean Architecture, diagramas de camadas, princípios SOLID | Entender a arquitetura do projeto |
| [FEATURES.md](FEATURES.md) | MongoDB, Quartz, RabbitMQ, Storage, Logging, Swagger | Habilitar recursos avançados |
| [ORM-GUIDE.md](ORM-GUIDE.md) | EF Core, Dapper, ADO.NET — como alternar e comparações | Escolher ou alternar ORM |
| [CONFIGURATION-GUIDE.md](CONFIGURATION-GUIDE.md) | IOptions\<T\>, validação de configurações, boas práticas | Acessar configurações no código |
| [AUTHENTICATION.md](AUTHENTICATION.md) | JWT, OAuth2 (Google, Microsoft, GitHub), refresh tokens | Configurar autenticação |
| [SECURITY.md](SECURITY.md) | Headers de segurança, CORS, HTTPS, proteção contra ataques | Revisar segurança da API |
| [RATE-LIMITING.md](RATE-LIMITING.md) | Fixed Window, Sliding Window, Token Bucket, Concurrency — inclui decisão arquitetural | Controle de taxa de requisições |
| [EVENT-SOURCING.md](EVENT-SOURCING.md) | Marten + PostgreSQL, auditoria, time travel | Habilitar event sourcing |
| [TELEMETRY.md](TELEMETRY.md) | OpenTelemetry, Jaeger, Prometheus, Grafana | Configurar observabilidade |
| [TESTING-DATABASES.md](TESTING-DATABASES.md) | Testar com SQL Server, Oracle, PostgreSQL, MySQL | Validar suporte multi-banco |
| [DATA-ANNOTATIONS-GUIDE.md](DATA-ANNOTATIONS-GUIDE.md) | Anotações XML para documentação Swagger | Documentar endpoints |
| [KUBERNETES.md](KUBERNETES.md) | Deploy local (Minikube) e produção (AKS, EKS, GKE) | Deploy em Kubernetes |
| [CICD.md](CICD.md) | GitHub Actions, Azure DevOps, GitLab CI | Configurar CI/CD |
| [SONARCLOUD.md](SONARCLOUD.md) | Análise de qualidade de código | Configurar SonarCloud |
| [PRODUCT-EXAMPLE.md](PRODUCT-EXAMPLE.md) | Exemplo completo de entidade Product | Aprender padrões do template |

### Exemplos

| Documento | Descrição |
|-----------|-----------|
| [examples/ORDER-EXAMPLE.md](examples/ORDER-EXAMPLE.md) | Exemplo completo de entidade Order com Event Sourcing |
| [examples/event-sourcing.http](examples/event-sourcing.http) | Request collection para testar Event Sourcing |

---

## 🚀 Início Rápido

Se você está começando, **não comece por aqui!** Primeiro:

1. Leia [../QUICK-START.md](../QUICK-START.md) para começar rapidamente
2. Consulte [../README.md](../README.md) para overview completo
3. Depois retorne aqui para guias específicos

---

## 📂 Organização

```text
docs/
├── README.md                    # Este arquivo (índice)
├── ARCHITECTURE.md              # Arquitetura Clean Architecture
├── FEATURES.md                  # Recursos avançados
├── ORM-GUIDE.md                 # Guia de ORMs
├── CONFIGURATION-GUIDE.md       # Guia de Configuração (IOptions<T>)
├── AUTHENTICATION.md            # Autenticação JWT & OAuth2
├── SECURITY.md                  # Segurança da API
├── RATE-LIMITING.md             # Rate Limiting (inclui ADR)
├── EVENT-SOURCING.md            # Event Sourcing
├── TELEMETRY.md                 # Observabilidade
├── TESTING-DATABASES.md         # Testes multi-banco
├── DATA-ANNOTATIONS-GUIDE.md    # Documentação Swagger
├── KUBERNETES.md                # Deploy Kubernetes
├── CICD.md                      # CI/CD
├── SONARCLOUD.md                # SonarCloud
├── PRODUCT-EXAMPLE.md           # Exemplo: Product
└── examples/
    ├── ORDER-EXAMPLE.md         # Exemplo: Order
    └── event-sourcing.http      # HTTP requests Event Sourcing
```

### Documentos Futuros

Planejamos adicionar:

- **AUTHENTICATION.md** - Guia de autenticação (JWT, OAuth2)

- **MONITORING.md** - Monitoramento e observabilidade
- **TESTING.md** - Estratégias de testes
- **PERFORMANCE.md** - Otimização de performance
- **SECURITY.md** - Boas práticas de segurança
- **CI-CD.md** - Pipelines de CI/CD
- **MIGRATION.md** - Migração de projetos existentes

### Documentos Recentes

- ✅ **[CONFIGURATION-GUIDE.md](CONFIGURATION-GUIDE.md)** - Guia de Configuração com IOptions<T> (NOVO!)

---

## 🎯 Guia de Uso por Perfil

### Desenvolvedor Backend

Foco principal:

- [CONFIGURATION-GUIDE.md](CONFIGURATION-GUIDE.md) - **ESSENCIAL**: Como usar configurações corretamente
- [ORM-GUIDE.md](ORM-GUIDE.md) - Entender opções de acesso a dados
- ../[README.md](../README.md) - Entender arquitetura e padrões

### DevOps Engineer

Foco principal:

- [KUBERNETES.md](KUBERNETES.md) - Deploy e infraestrutura
- ../[docker-compose.yml](../docker-compose.yml) - Containerização

### Arquiteto de Software

Foco principal:

- ../[README.md](../README.md) - Arquitetura geral
- [ORM-GUIDE.md](ORM-GUIDE.md) - Decisões de arquitetura de dados
- [KUBERNETES.md](KUBERNETES.md) - Arquitetura de infraestrutura

### QA / Tester

Foco principal:

- ../[QUICK-START.md](../QUICK-START.md#6-executar-testes) - Como rodar testes
- [KUBERNETES.md](KUBERNETES.md#-testes) - Testes em ambiente K8s

---

## 🔍 Busca Rápida

### Banco de Dados

- SQL Server: [ORM-GUIDE.md](ORM-GUIDE.md#sql-server)
- PostgreSQL: [ORM-GUIDE.md](ORM-GUIDE.md#postgresql)
- MySQL: [ORM-GUIDE.md](ORM-GUIDE.md#mysql)
- Oracle: [ORM-GUIDE.md](ORM-GUIDE.md#oracle)

### Deployment

- Minikube: [KUBERNETES.md](KUBERNETES.md#-deploy-local-com-minikube)
- Produção: [KUBERNETES.md](KUBERNETES.md#-deploy-em-producao)
- Docker: ../[QUICK-START.md](../QUICK-START.md#4-executar-com-docker-opcional)

### Configuração

- **Configurações**: [CONFIGURATION-GUIDE.md](CONFIGURATION-GUIDE.md) ⭐ **IMPORTANTE**
- ORMs: [ORM-GUIDE.md](ORM-GUIDE.md#como-alternar-entre-orms)

- Health Checks: [KUBERNETES.md](KUBERNETES.md#health-checks)

---

## 📖 Contribuindo com Documentação

Quer melhorar ou adicionar documentação? Ótimo!

### Diretrizes

1. **Clareza**: Escreva de forma clara e objetiva
2. **Exemplos**: Inclua exemplos práticos de código
3. **Screenshots**: Use imagens quando ajudar a compreensão
4. **Atualização**: Mantenha sincronizado com o código
5. **Organização**: Use hierarquia clara de seções
6. **Links**: Faça referência cruzada entre documentos

### Formato

Use Markdown com:

- Emojis para seções principais (📚 🚀 ⚙️ etc.)
- Code blocks com syntax highlighting
- Tabelas para comparações
- Listas para passos ou opções
- Citações para warnings/notas importantes

### Exemplo de Estrutura

```markdown
# Título Principal

Breve descrição do documento.

---

## 📋 Seção 1

Conteúdo da seção com exemplos:

comando exemplo

### Subseção 1.1

Detalhes específicos.

---

## 🔧 Seção 2

Mais conteúdo...
```

---

## 🆘 Precisa de Ajuda?

Se algo não está claro ou faltando:

1. Pesquise nas [Issues](../../issues)
2. Crie uma [Issue](../../issues/new) com sugestão
3. Ou envie um [Pull Request](../../pulls) melhorando a doc

---

## 📚 Recursos Externos

### .NET e C

- [Microsoft Learn - .NET](https://learn.microsoft.com/dotnet/)
- [C# Documentation](https://learn.microsoft.com/dotnet/csharp/)

### Kubernetes

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)

### Clean Architecture

- [Clean Architecture Blog](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Clean Architecture Book](https://www.amazon.com/Clean-Architecture-Craftsmans-Software-Structure/dp/0134494164)

---

## ✅ Checklist de Documentação

Ao criar nova documentação:

- [ ] Título claro e descritivo
- [ ] Índice com links internos
- [ ] Exemplos práticos de código
- [ ] Links para documentação relacionada
- [ ] Screenshots quando necessário
- [ ] Seção de troubleshooting
- [ ] Data de última atualização
- [ ] Revisão ortográfica e gramatical

---

## 📊 Estatísticas

- **Documentos**: 3 guias técnicos + 1 guia de configuração
- **Linhas**: ~4000+ linhas de documentação
- **Exemplos de código**: 60+ snippets
- **Tópicos cobertos**: IOptions<T>, ORMs, Kubernetes, Docker, Clean Architecture

---

**Navegação:**

- [⬆️ Voltar ao README Principal](../README.md)
- [📖 Ver Índice Completo](../INDEX.md)
- [🚀 Quick Start](../QUICK-START.md)
