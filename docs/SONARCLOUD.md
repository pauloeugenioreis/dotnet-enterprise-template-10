# SonarCloud Integration Guide

Este guia explica como configurar e usar o SonarCloud para análise de qualidade de código no projeto.

---

## 📋 Índice

- [O que é SonarCloud?](#o-que-é-sonarcloud)
- [Configuração Inicial](#configuração-inicial)
- [GitHub Actions](#github-actions)
- [Azure DevOps](#azure-devops)
- [GitLab CI](#gitlab-ci)
- [Análise Local](#análise-local)
- [Quality Gates](#quality-gates)
- [Badges](#badges)
- [Troubleshooting](#troubleshooting)

---

## O que é SonarCloud?

**SonarCloud** é uma plataforma de análise de código em nuvem que fornece:

- ✅ **Code Quality Analysis** - Detecta bugs, code smells e vulnerabilidades
- ✅ **Security Scanning** - Identifica vulnerabilidades de segurança
- ✅ **Code Coverage** - Integração com relatórios de cobertura
- ✅ **Technical Debt** - Calcula tempo necessário para corrigir issues
- ✅ **Quality Gates** - Bloqueia PRs que não atendem critérios
- ✅ **Historical Trending** - Acompanha evolução da qualidade ao longo do tempo
- ✅ **Gratuito para projetos open-source**

**Dashboard**: https://sonarcloud.io

---

## Configuração Inicial

### 1. Criar Conta no SonarCloud

1. Acesse https://sonarcloud.io
2. Clique em "Log in" e conecte com sua conta GitHub
3. Autorize o SonarCloud a acessar seus repositórios

### 2. Importar Projeto

1. No SonarCloud, clique em **"+"** → **"Analyze new project"**
2. Selecione sua organização do GitHub
3. Escolha o repositório `dotnet-enterprise-template-10`
4. Clique em **"Set Up"**

### 3. Obter Tokens

#### Token do Projeto

1. No SonarCloud, vá para seu projeto
2. **Administration** → **Analysis Method**
3. Copie o **SONAR_TOKEN** gerado

#### Organization Key

1. No menu superior, clique no nome da sua organização
2. Copie o **Organization Key** (geralmente seu username do GitHub)

---

## GitHub Actions

### 1. Configurar Secrets

No repositório GitHub:

1. **Settings** → **Secrets and variables** → **Actions**
2. Clique em **"New repository secret"**
3. Adicione:
   - **Name**: `SONAR_TOKEN`
   - **Value**: Cole o token do SonarCloud

### 2. Pipeline Configurado

O pipeline GitHub Actions já está configurado em `.github/workflows/ci.yml` com o job `sonarcloud`.

### 3. Executar Análise

A análise é executada automaticamente em:

- ✅ Push para `main` ou `develop`
- ✅ Pull Requests
- ✅ Workflow manual

**Ver resultados**: https://sonarcloud.io/project/overview?id=pauloeugenioreis_dotnet-enterprise-template-10

---

## Azure DevOps

### 1. Instalar Extensão SonarCloud

1. No Azure DevOps, vá para **Organization Settings** → **Extensions**
2. Procure por **"SonarCloud"**
3. Clique em **"Get it free"** e instale

### 2. Configurar Service Connection

1. **Project Settings** → **Service connections**
2. Clique em **"New service connection"**
3. Selecione **"SonarCloud"**
4. Cole o **SONAR_TOKEN**
5. Nomeie como `SonarCloudConnection`

### 3. Pipeline Configurado

O pipeline Azure DevOps (`azure-pipelines.yml`) já inclui o stage `SonarCloud`.

### 4. Variáveis do Pipeline

Configure as variáveis no pipeline:

- `sonar.organization`: Sua organization key
- `sonar.projectKey`: Key do projeto no SonarCloud

---

## GitLab CI

### 1. Configurar Variáveis

No GitLab:

1. **Settings** → **CI/CD** → **Variables**
2. Adicione:
   - **Key**: `SONAR_TOKEN`
   - **Value**: Token do SonarCloud
   - **Protected**: ✅ Yes
   - **Masked**: ✅ Yes

### 2. Pipeline Configurado

O pipeline GitLab CI (`.gitlab-ci.yml`) já inclui o job `sonarcloud`.

### 3. Executar

Commit e push - o job `sonarcloud` será executado automaticamente.

---

## Análise Local

### Requisitos

```bash
dotnet tool install --global dotnet-sonarscanner
```

### Executar Análise

```bash
# 1. Begin analysis
dotnet sonarscanner begin \
  /k:"pauloeugenioreis_dotnet-enterprise-template-10" \
  /o:"pauloeugenioreis" \
  /d:sonar.host.url="https://sonarcloud.io" \
  /d:sonar.token="YOUR_SONAR_TOKEN"

# 2. Build
dotnet build ProjectTemplate.sln

# 3. Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# 4. End analysis
dotnet sonarscanner end /d:sonar.token="YOUR_SONAR_TOKEN"
```

---

## Quality Gates

### Configuração Padrão

O SonarCloud usa o Quality Gate **"Sonar way"** por padrão:

| Métrica | Condição |
|---------|----------|
| **Coverage** | ≥ 80% em novo código |
| **Duplications** | ≤ 3% em novo código |
| **Maintainability Rating** | ≥ A em novo código |
| **Reliability Rating** | ≥ A em novo código |
| **Security Rating** | ≥ A em novo código |

### Customizar Quality Gate

1. No SonarCloud: **Quality Gates** → **Create**
2. Configure suas próprias regras
3. Aplique ao projeto

### Bloquear PRs

Os PRs serão bloqueados automaticamente se não passarem no Quality Gate.

---

## Badges

Adicione badges do SonarCloud ao README.md:

```markdown
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=pauloeugenioreis_dotnet-enterprise-template-10&metric=alert_status)](https://sonarcloud.io/dashboard?id=pauloeugenioreis_dotnet-enterprise-template-10)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=pauloeugenioreis_dotnet-enterprise-template-10&metric=coverage)](https://sonarcloud.io/dashboard?id=pauloeugenioreis_dotnet-enterprise-template-10)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=pauloeugenioreis_dotnet-enterprise-template-10&metric=bugs)](https://sonarcloud.io/dashboard?id=pauloeugenioreis_dotnet-enterprise-template-10)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=pauloeugenioreis_dotnet-enterprise-template-10&metric=code_smells)](https://sonarcloud.io/dashboard?id=pauloeugenioreis_dotnet-enterprise-template-10)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=pauloeugenioreis_dotnet-enterprise-template-10&metric=security_rating)](https://sonarcloud.io/dashboard?id=pauloeugenioreis_dotnet-enterprise-template-10)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=pauloeugenioreis_dotnet-enterprise-template-10&metric=sqale_index)](https://sonarcloud.io/dashboard?id=pauloeugenioreis_dotnet-enterprise-template-10)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=pauloeugenioreis_dotnet-enterprise-template-10&metric=ncloc)](https://sonarcloud.io/dashboard?id=pauloeugenioreis_dotnet-enterprise-template-10)
```

---

## Troubleshooting

### Erro: "Project not found"

**Solução**: Verifique se o `sonar.projectKey` está correto em `sonar-project.properties`.

### Erro: "Unauthorized"

**Solução**: Verifique se o `SONAR_TOKEN` está configurado corretamente nos secrets.

### Coverage não aparece

**Solução**: 
1. Certifique-se de que os testes estão rodando com coverage
2. Verifique se o path do coverage está correto em `sonar-project.properties`
3. Use formato OpenCover: `--collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover`

### Análise muito lenta

**Solução**:
1. Adicione mais exclusões em `sonar.exclusions`
2. Use cache do scanner
3. Desabilite análise de testes: `sonar.dotnet.excludeTestProjects=true`

### PRs não mostram análise

**Solução**: 
1. Verifique se o SonarCloud tem permissões no repositório
2. GitHub: **Settings** → **Integrations** → **SonarCloud** → Verify permissions

---

## Recursos Adicionais

- **Documentação Oficial**: https://docs.sonarcloud.io/
- **Scanner para .NET**: https://docs.sonarcloud.io/advanced-setup/languages/csharp/
- **Quality Gates**: https://docs.sonarcloud.io/improving/quality-gates/
- **Coverage**: https://docs.sonarcloud.io/enriching/test-coverage/overview/

---

## Suporte

- **Issues do Projeto**: https://github.com/pauloeugenioreis/dotnet-enterprise-template-10/issues
- **SonarCloud Community**: https://community.sonarsource.com/
- **Stack Overflow**: Tag `sonarcloud` + `.net`
