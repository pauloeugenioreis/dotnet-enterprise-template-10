# CI/CD Configuration Guide

Este guia explica como configurar e usar os pipelines de CI/CD incluídos no template para **GitHub Actions**, **Azure DevOps** e **GitLab CI/CD**.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [GitHub Actions](#-github-actions)
- [Azure DevOps](#-azure-devops)
- [GitLab CI/CD](#-gitlab-cicd)
- [Recursos Comuns](#-recursos-comuns)
- [Personalização](#-personalização)

---

## 🎯 Visão Geral

Todos os pipelines incluem as seguintes etapas:

| Etapa | Descrição | GitHub Actions | Azure DevOps | GitLab CI |
|-------|-----------|----------------|--------------|-----------|
| **Build** | Compilação do projeto | ✅ | ✅ | ✅ |
| **Unit Tests** | Testes unitários | ✅ | ✅ | ✅ |
| **Integration Tests** | Testes de integração | ✅ | ✅ | ✅ |
| **Code Coverage** | Cobertura de código | ✅ | ✅ | ✅ |
| **Security Scan** | Verificação de vulnerabilidades | ✅ | ✅ | ✅ |
| **Docker Build** | Build de imagem Docker | ✅ | ✅ | ✅ |
| **Artifacts** | Publicação de artefatos | ✅ | ✅ | ✅ |
| **Deploy** | Deploy automático | ✅ | ✅ | ✅ |

---

## 🐙 GitHub Actions

### 📁 Arquivo de Configuração

`.github/workflows/ci.yml`

### ✨ Features

- ✅ Build e testes em Ubuntu
- ✅ Cache de pacotes NuGet
- ✅ Testes com relatórios (TRX)
- ✅ Code coverage com Codecov
- ✅ Security scan de vulnerabilidades
- ✅ Docker build e push
- ✅ Artifacts upload

### 🔧 Configuração

#### 1. Secrets Necessários

Configure os seguintes secrets no repositório (**Settings → Secrets and variables → Actions**):

| Secret | Descrição | Obrigatório |
|--------|-----------|-------------|
| `DOCKER_USERNAME` | Usuário Docker Hub | ⚠️ Sim (para Docker) |
| `DOCKER_PASSWORD` | Token Docker Hub | ⚠️ Sim (para Docker) |
| `CODECOV_TOKEN` | Token Codecov (opcional) | ❌ Não |

#### 2. Habilitar Actions

1. Acesse **Settings → Actions → General**
2. Em **Actions permissions**, selecione **Allow all actions**
3. Em **Workflow permissions**, selecione **Read and write permissions**

#### 3. Executar Pipeline

O pipeline é executado automaticamente em:
- ✅ Push para `main` ou `develop`
- ✅ Pull Requests para `main` ou `develop`
- ✅ Manualmente via **Actions → CI/CD Pipeline → Run workflow**

### 📊 Visualização de Resultados

1. **Actions Tab**: Ver execuções do pipeline
2. **Pull Request**: Checks automáticos
3. **Artifacts**: Baixar build artifacts (7 dias de retenção)
4. **Coverage**: Ver relatório no Codecov

### 🎨 Badges

Adicione ao seu README.md:

[![CI/CD](https://github.com/seu-usuario/seu-repo/actions/workflows/ci.yml/badge.svg)](https://github.com/seu-usuario/seu-repo/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/seu-usuario/seu-repo/branch/main/graph/badge.svg)](https://codecov.io/gh/seu-usuario/seu-repo)
---

## 🔷 Azure DevOps

### 📁 Arquivo de Configuração

`azure-pipelines.yml`

### ✨ Features

- ✅ Multi-stage pipeline (Build, Quality, Docker, Deploy)
- ✅ Cache de pacotes NuGet
- ✅ Testes com cobertura (Cobertura format)
- ✅ Security scan e outdated packages
- ✅ Docker build e push
- ✅ Deploy para Staging e Production

### 🔧 Configuração

#### 1. Criar Projeto no Azure DevOps

1. Acesse [dev.azure.com](https://dev.azure.com)
2. Crie uma **Organization** (se não tiver)
3. Crie um **Project**
4. Navegue para **Pipelines → Create Pipeline**

#### 2. Conectar Repositório

1. Selecione **Azure Repos Git** (ou GitHub/GitLab)
2. Selecione seu repositório
3. Escolha **Existing Azure Pipelines YAML file**
4. Selecione `/azure-pipelines.yml`

#### 3. Service Connections

Configure as seguintes service connections (**Project Settings → Service connections**):

| Connection | Tipo | Uso |
|------------|------|-----|
| `DockerHubConnection` | Docker Registry | Push de imagens Docker |
| `AzureSubscription` | Azure Resource Manager | Deploy para Azure |
| `KubernetesConnection` | Kubernetes | Deploy para K8s |

**Criar Docker Hub Connection:**
1. **Service connections → New service connection**
2. Selecione **Docker Registry**
3. **Registry type**: Docker Hub
4. **Docker ID**: Seu username Docker Hub
5. **Password**: Seu token do Docker Hub
6. **Service connection name**: `DockerHubConnection`

#### 4. Variáveis

Configure variáveis adicionais (**Pipelines → Edit → Variables**):

| Variável | Valor | Segredo |
|----------|-------|---------|
| `dockerImageName` | Nome da imagem | ❌ |
| `azureSubscription` | Nome da subscription | ❌ |

#### 5. Environments

Crie os environments para deploy manual (**Pipelines → Environments**):

1. `staging` - Ambiente de homologação
2. `production` - Ambiente de produção

Configure **Approvals** em Production:
- **Environments → production → ⋮ → Approvals and checks**
- Adicione aprovadores

### 📊 Visualização de Resultados

1. **Pipelines**: Ver execuções e histórico
2. **Tests**: Relatórios de testes
3. **Code Coverage**: Gráficos de cobertura
4. **Artifacts**: Baixar builds (feed)

### 🎨 Badges

Adicione ao seu README.md:

[![Build Status](https://dev.azure.com/your-org/your-project/_apis/build/status/your-pipeline?branchName=main)](https://dev.azure.com/your-org/your-project/_build/latest?definitionId=1&branchName=main)
---

## 🦊 GitLab CI/CD

### 📁 Arquivo de Configuração

`.gitlab-ci.yml`

### ✨ Features

- ✅ 5 stages (Build, Test, Quality, Docker, Deploy)
- ✅ Cache de pacotes NuGet
- ✅ Testes com JUnit reports
- ✅ Code coverage (Cobertura format)
- ✅ Security e outdated packages scan
- ✅ Docker build e push para GitLab Registry
- ✅ Deploy manual para Staging e Production

### 🔧 Configuração

#### 1. Habilitar CI/CD

O GitLab CI/CD é habilitado automaticamente quando você adiciona `.gitlab-ci.yml` ao repositório.

#### 2. Variáveis

Configure as seguintes variáveis (**Settings → CI/CD → Variables**):

| Variável | Valor | Protected | Masked |
|----------|-------|-----------|--------|
| `CI_REGISTRY` | `registry.gitlab.com` | ❌ | ❌ |
| `CI_REGISTRY_USER` | `$CI_REGISTRY_USER` (built-in) | ✅ | ❌ |
| `CI_REGISTRY_PASSWORD` | `$CI_REGISTRY_PASSWORD` (built-in) | ✅ | ✅ |
| `DOCKER_IMAGE` | `$CI_REGISTRY_IMAGE/api` | ❌ | ❌ |

**Variáveis adicionais (opcional):**
| Variável | Valor | Uso |
|----------|-------|-----|
| `KUBECONFIG` | Conteúdo do kubeconfig | Deploy Kubernetes |
| `DEPLOY_KEY` | SSH key | Deploy em servidores |

#### 3. Docker Runner

Para executar jobs Docker, configure um Runner:

1. **Settings → CI/CD → Runners**
2. **New project runner**
3. Siga as instruções de instalação
4. Selecione **docker** como executor

#### 4. Environments

Os environments são criados automaticamente:

- `staging` - Para branch `develop`
- `production` - Para branch `main`

Configure **Protected Environments** (**Settings → CI/CD → Environments**):

1. Selecione `production`
2. **Deployment tier**: Production
3. **Protected**: ✅
4. **Allowed to deploy**: Maintainers only

#### 5. Container Registry

Habilite o GitLab Container Registry:

1. **Settings → General → Visibility → Container Registry**: ✅ Enabled

Suas imagens ficarão em:
```bash
registry.gitlab.com/seu-usuario/seu-projeto/projecttemplate-api
### 📊 Visualização de Resultados

1. **CI/CD → Pipelines**: Ver execuções
2. **CI/CD → Jobs**: Ver logs individuais
3. **Repository → Analytics → Code Coverage**: Gráficos
4. **Deployments → Environments**: Status dos ambientes
5. **Packages & Registries → Container Registry**: Imagens Docker

### 🎨 Badges

Adicione ao seu README.md:

[![pipeline status](https://gitlab.com/seu-usuario/seu-projeto/badges/main/pipeline.svg)](https://gitlab.com/seu-usuario/seu-projeto/-/commits/main)
[![coverage report](https://gitlab.com/seu-usuario/seu-projeto/badges/main/coverage.svg)](https://gitlab.com/seu-usuario/seu-projeto/-/commits/main)
---

## 🔄 Recursos Comuns

### 1. Triggers (Quando o Pipeline Executa)

| Evento | GitHub Actions | Azure DevOps | GitLab CI |
|--------|----------------|--------------|-----------|
| Push para main | ✅ | ✅ | ✅ |
| Push para develop | ✅ | ✅ | ✅ |
| Pull/Merge Request | ✅ | ✅ | ✅ |
| Manual | ✅ | ✅ | ✅ |

### 2. Artifacts

Todos os pipelines geram os seguintes artifacts:

| Artifact | Descrição | Retenção |
|----------|-----------|----------|
| **API Build** | DLLs compiladas | 7-30 dias |
| **Test Results** | Relatórios de testes (TRX/JUnit) | 30 dias |
| **Coverage** | Relatórios de cobertura | 30 dias |
| **Security Scan** | Log de vulnerabilidades | 30 dias |

### 3. Cache

Para acelerar builds, todos os pipelines fazem cache de:

- ✅ Pacotes NuGet
- ✅ Dependências do .NET
- ✅ Camadas Docker

### 4. Notificações

Configure notificações de falhas:

**GitHub Actions:**
- **Settings → Notifications**: Configure para receber emails

**Azure DevOps:**
- **Project Settings → Notifications**: Configure regras personalizadas

**GitLab CI:**
- **Settings → Integrations → Pipeline emails**: Adicione emails

---

## 🎨 Personalização

### Alterar Versão do .NET

**GitHub Actions** (`.github/workflows/ci.yml`):
env:
  DOTNET_VERSION: '10.0.x'  # Altere aqui
**Azure DevOps** (`azure-pipelines.yml`):
variables:
  dotnetVersion: '10.0.x'  # Altere aqui
**GitLab CI** (`.gitlab-ci.yml`):
image: mcr.microsoft.com/dotnet/sdk:10.0  # Altere aqui
### Adicionar Stages Personalizadas

#### GitHub Actions

  deploy-aws:
    name: Deploy to AWS
    runs-on: ubuntu-latest
    needs: docker
    steps:
      - name: Deploy to ECS
        run: |
          # Comandos de deploy AWS
#### Azure DevOps

- stage: DeployAWS
  displayName: 'Deploy to AWS'
  dependsOn: Docker
  jobs:
  - job: Deploy
    steps:
    - script: |
        # Comandos de deploy AWS
#### GitLab CI

deploy:aws:
  stage: deploy
  script:
    - echo "Deploying to AWS..."
    # Comandos de deploy AWS
### Alterar Docker Registry

**GitHub Actions:**
- name: Login to Docker Hub
  uses: docker/login-action@v3
  with:
    registry: ghcr.io  # GitHub Container Registry
    username: ${{ github.actor }}
    password: ${{ secrets.GITHUB_TOKEN }}
**Azure DevOps:**
- task: Docker@2
  displayName: 'Login to ACR'
  inputs:
    command: login
    containerRegistry: 'AzureContainerRegistry'  # ACR connection
**GitLab CI:**
# GitLab Registry é usado por padrão
# Para Docker Hub, altere:
before_script:
  - echo $DOCKER_HUB_PASSWORD | docker login -u $DOCKER_HUB_USER --password-stdin
### Configurar Deploy Automático

Por padrão, todos os deploys são **manuais** (`when: manual`).

Para deploy automático:

**GitHub Actions:**
  deploy:
    if: github.ref == 'refs/heads/main'  # Remove when: manual
**Azure DevOps:**
- deployment: DeployToProduction
  # Remove: condition: manual
**GitLab CI:**
deploy:production:
  # Remove: when: manual
---

## 🧪 Testando Localmente

### GitHub Actions

Use [act](https://github.com/nektos/act):

# Instalar act
choco install act  # Windows
brew install act   # macOS

# Executar workflow
act -j build-and-test
### Azure DevOps

Não há ferramenta oficial, mas você pode:

# Executar comandos individuais
dotnet restore
dotnet build
dotnet test
### GitLab CI

Use [gitlab-runner](https://docs.gitlab.com/runner/):

# Instalar gitlab-runner
curl -L https://packages.gitlab.com/install/repositories/runner/gitlab-runner/script.deb.sh | sudo bash
sudo apt-get install gitlab-runner

# Executar localmente
gitlab-runner exec docker build
gitlab-runner exec docker test:unit
```

---

## 🔍 Troubleshooting

### Build Falhou

**Erro:** `Error: Process completed with exit code 1.`

**Solução:**
1. Verifique logs detalhados no pipeline
2. Execute localmente: `dotnet build --configuration Release`
3. Verifique se todas as dependências estão no `.csproj`

### Testes Falharam

**Erro:** `Failed!  - Failed: X, Passed: Y`

**Solução:**
1. Execute localmente: `dotnet test --logger "console;verbosity=detailed"`
2. Verifique configurações de ambiente (connection strings, etc.)
3. Verifique se testes de integração precisam de serviços (DB, Redis)

### Docker Build Falhou

**Erro:** `Error: Cannot connect to the Docker daemon`

**Solução:**
1. **GitHub Actions**: Use `docker/setup-buildx-action`
2. **Azure DevOps**: Verifique se agent tem Docker instalado
3. **GitLab CI**: Use `docker:dind` service

### Secrets/Variables não Funcionam

**Solução:**
1. Verifique se secrets estão configurados corretamente
2. Verifique se nome está correto (case-sensitive)
3. **GitLab**: Marque como **Protected** para branches protegidas

---

## 📚 Referências

### GitHub Actions
- [Documentação Oficial](https://docs.github.com/actions)
- [Marketplace](https://github.com/marketplace?type=actions)
- [Workflow Syntax](https://docs.github.com/actions/using-workflows/workflow-syntax-for-github-actions)

### Azure DevOps
- [Documentação Oficial](https://learn.microsoft.com/azure/devops/pipelines)
- [YAML Schema](https://learn.microsoft.com/azure/devops/pipelines/yaml-schema)
- [Tasks Reference](https://learn.microsoft.com/azure/devops/pipelines/tasks)

### GitLab CI/CD
- [Documentação Oficial](https://docs.gitlab.com/ee/ci/)
- [CI/CD YAML Reference](https://docs.gitlab.com/ee/ci/yaml/)
- [Examples](https://docs.gitlab.com/ee/ci/examples/)

---

**Template Version:** 1.0.0
**Last Updated:** Janeiro 2026
**Supported Platforms:** GitHub Actions, Azure DevOps, GitLab CI/CD
