# 📜 Scripts

Scripts utilitários para facilitar o desenvolvimento e testes do projeto.

---

## 📋 Índice

- [test-all-databases](#-test-all-databases) - Testar com todos os bancos de dados
- [new-project](#-new-project) - Criar novo projeto a partir do template
- [run-sonar-analysis](#-run-sonar-analysis) - Executar análise SonarCloud local
- [Kubernetes Scripts](#kubernetes-scripts) - Deploy/destroy no Minikube

---

## 🔍 run-sonar-analysis

Executa análise SonarCloud localmente.

### Windows (PowerShell)

```powershell
# Set token
$env:SONAR_TOKEN = "your-sonarcloud-token"

# Run analysis
.\scripts\run-sonar-analysis.ps1

# OR pass token as parameter
.\scripts\run-sonar-analysis.ps1 -SonarToken "your-token"
```

### Linux/macOS (Bash)

```bash
# Set token
export SONAR_TOKEN="your-sonarcloud-token"

# Run analysis
chmod +x scripts/run-sonar-analysis.sh
./scripts/run-sonar-analysis.sh

# OR pass token as parameter
./scripts/run-sonar-analysis.sh "your-token"
```

### O que o script faz?

1. ✅ **Instala** dotnet-sonarscanner (se necessário)
2. ✅ **Begin** - Inicia análise SonarCloud
3. ✅ **Build** - Compila o projeto
4. ✅ **Test** - Executa testes com cobertura (OpenCover)
5. ✅ **End** - Finaliza e envia dados ao SonarCloud
6. ✅ **Link** - Mostra URL para ver resultados

**Requisitos:**

- Token SonarCloud (obtenha em https://sonarcloud.io/account/security)
- .NET 10 SDK instalado
- Projeto configurado no SonarCloud

**📖 Para configuração completa**, veja [docs/SONARCLOUD.md](../docs/SONARCLOUD.md)

---

## 🧪 test-all-databases

Testa a aplicação com todos os 4 bancos de dados suportados (SQL Server, Oracle, PostgreSQL, MySQL).

### Windows (PowerShell)

```powershell
cd scripts\windows
.\test-all-databases.ps1
```

**Opções:**

- `-SkipDocker` - Não reinicia containers Docker (útil se já estiverem rodando)
- `-SkipMigrations` - Não aplica migrations (útil para testes rápidos)
- `-SkipTests` - Não testa a API, apenas migrations e build
- `-ApiStartupTimeout <seconds>` - Timeout para API iniciar (padrão: 30s)

**Exemplos:**

#### Teste rápido (pula Docker e migrations)

.\test-all-databases.ps1 -SkipDocker -SkipMigrations

#### Teste completo com timeout maior

.\test-all-databases.ps1 -ApiStartupTimeout 60

### Linux/macOS (Bash)

```bash
cd scripts/linux
chmod +x test-all-databases.sh
./test-all-databases.sh
```

**Opções:**

- `--skip-docker` - Não reinicia containers Docker
- `--skip-migrations` - Não aplica migrations
- `--skip-tests` - Não testa a API
- `--timeout <seconds>` - Timeout para API iniciar (padrão: 30s)

**Exemplos:**

#### Teste rápido

./test-all-databases.sh --skip-docker --skip-migrations

#### Teste completo com timeout maior

./test-all-databases.sh --timeout 60

### O que o script faz?

1. ✅ **Docker Compose** - Sobe os 4 bancos de dados
2. ✅ **Aguarda** - Espera os bancos ficarem prontos (health checks)
3. ✅ **Migrations** - Aplica migrations em cada banco
4. ✅ **Build** - Compila o projeto
5. ✅ **Startup** - Inicia a API com cada banco
6. ✅ **Health Check** - Testa endpoint `/health`
7. ✅ **Swagger** - Verifica se Swagger está acessível
8. ✅ **Relatório** - Mostra resumo com resultados

**Saída esperada:**

```bash
================================================
  Test Summary
================================================

SqlServer: ✅ PASSED
Oracle: ✅ PASSED
PostgreSQL: ✅ PASSED
MySQL: ✅ PASSED

================================================
✅ All database tests passed!
```

---

## 🐳 new-project

Scripts para criar um novo projeto a partir do template.

### Windows (PowerShell)

```powershell
cd scripts
.\new-project.ps1 -ProjectName "MeuProjeto"
```

### Linux/macOS

```bash
cd scripts
chmod +x new-project.sh
./new-project.sh MeuProjeto
```

**O que o script faz:**

1. ✅ **Copia** - Template completo para novo diretório
2. ✅ **Limpa** - Remove `.git`, `scripts`, `bin`, `obj`
3. ✅ **Renomeia** - Solution e namespaces para novo nome
4. ✅ **Substitui** - Todas referências de "ProjectTemplate" para nome escolhido
5. ✅ **Instruções** - Mostra próximos passos

---

## ☸️ minikube-deploy

Deploy da aplicação em cluster Kubernetes local (Minikube).

### Windows (PowerShell)

```powershell
cd scripts\windows
.\minikube-deploy.ps1
```

### Linux/macOS

```bash
cd scripts/linux
chmod +x minikube-deploy.sh
./minikube-deploy.sh
```

---

## 🗑️ minikube-destroy

Remove o deploy do Minikube.

### Windows (PowerShell)

```powershell
cd scripts\windows
.\minikube-destroy.ps1
```

### Linux/macOS

```bash
cd scripts/linux
./minikube-destroy.sh
```

---

## 🧪 run-integration-tests

Executa testes de integração no Minikube.

### Windows (PowerShell)

```powershell
cd scripts\windows
.\run-integration-tests.ps1
```

### Linux/macOS

```bash
cd scripts/linux
./run-integration-tests.sh
```

---

## 📝 Convenções

- **Windows**: Scripts PowerShell (`.ps1`) e Batch (`.bat`) em `scripts/windows/`
- **Linux/macOS**: Scripts Bash (`.sh`) em `scripts/linux/`
- **Multiplataforma**: Scripts na raiz de `scripts/` (new-project.*)
- **Sempre** execute scripts do diretório correto
- Scripts Linux precisam de permissão de execução: `chmod +x script.sh`

---

## 🐛 Troubleshooting

### PowerShell Execution Policy

Se encontrar erro de execution policy no Windows:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Permission Denied (Linux/macOS)

```bash
chmod +x script.sh
```

### Docker não encontrado

Certifique-se que o Docker Desktop está instalado e rodando:

```bash
docker --version
docker-compose --version
```

---

## 📚 Documentação Completa

Para mais detalhes sobre testes de banco de dados, veja:

- [TESTING-DATABASES.md](../docs/TESTING-DATABASES.md) - Guia completo de testes

Para deploy em Kubernetes:

- [docs/KUBERNETES.md](../docs/KUBERNETES.md) - Guia de deploy K8s
