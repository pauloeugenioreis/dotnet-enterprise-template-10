# 📚 Documentação Adicional

Esta pasta contém documentação técnica adicional e guias especializados para o template.

---

## 📄 Documentos Disponíveis

### [ORM-GUIDE.md](ORM-GUIDE.md)
**Guia Completo de ORMs**

Documentação detalhada sobre os ORMs suportados pelo template:

- **Entity Framework Core** - ORM padrão com suporte a migrations
- **Dapper** - Micro-ORM de alta performance
- **NHibernate** - ORM maduro e completo
- **Linq2Db** - ORM focado em performance

Inclui:
- Como alternar entre ORMs
- Comparação de features
- Casos de uso recomendados
- Exemplos de implementação
- Configuração de cada ORM
- Troubleshooting

**Quando usar:** Ao escolher um ORM ou precisar alternar entre eles.

---

### [KUBERNETES.md](KUBERNETES.md)
**Guia de Deploy no Kubernetes**

Documentação completa sobre deploy da aplicação no Kubernetes:

- **Deploy Local** com Minikube
- **Deploy em Produção** (AKS, EKS, GKE)
- **Configurações Avançadas** (HPA, Network Policies, TLS)
- **Monitoramento e Troubleshooting**
- **Scripts de Automação**

Inclui:
- Pré-requisitos e instalação
- Passo a passo de deploy
- Configuração de manifests
- Health checks e probes
- Resource limits e requests
- Ingress e Service configuration
- Secrets e ConfigMaps
- CI/CD integration
- Problemas comuns e soluções

**Quando usar:** Ao fazer deploy em Kubernetes (local ou produção).

---

### [CONFIGURATION-GUIDE.md](CONFIGURATION-GUIDE.md)
**Guia de Configuração com IOptions<T>**

Documentação essencial sobre como trabalhar com configurações no projeto:

- **Padrão IOptions<T>** - Injeção de dependência de configurações
- **Validação de Configurações** - Validação no startup
- **Melhores Práticas** - DO's e DON'Ts
- **Exemplos Práticos** - Controllers, Services, Repositories

Inclui:
- Como injetar IOptions<AppSettings> corretamente
- 5 exemplos práticos completos
- Padrões de validação com IValidateOptions<T>
- Troubleshooting comum
- Checklist de boas práticas

**Quando usar:** SEMPRE que precisar acessar configurações em qualquer parte do código (controllers, services, repositories, middleware, etc.).

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
├── README.md                # Este arquivo
├── CONFIGURATION-GUIDE.md   # Guia de Configuração (IOptions<T>)
├── ORM-GUIDE.md             # Guia de ORMs
└── KUBERNETES.md            # Guia de Kubernetes
```

### Documentos Futuros

Planejamos adicionar:

- **AUTHENTICATION.md** - Guia de autenticação (JWT, OAuth2)
- **CACHING.md** - Estratégias avançadas de cache
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
- Cache: ../[README.md](../README.md#configuracao-de-cache)
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

```bash
comando exemplo
```markdown
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

### .NET e C#
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

---

*Última atualização: Janeiro 2025 | Versão: 1.0.0*
