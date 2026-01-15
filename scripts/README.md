# 📜 Scripts

Scripts utilitários para facilitar o desenvolvimento e testes do projeto.

---

## 🧪 test-all-databases

Testa a aplicação com todos os 4 bancos de dados suportados (SQL Server, Oracle, PostgreSQL, MySQL).

### Windows (PowerShell)

```powershell
cd windows
.\test-all-databases.ps1
```

**Opções:**
- `-SkipDocker` - Não reinicia containers Docker (útil se já estiverem rodando)
- `-SkipMigrations` - Não aplica migrations (útil para testes rápidos)
- `-SkipTests` - Não testa a API, apenas migrations e build
- `-ApiStartupTimeout <seconds>` - Timeout para API iniciar (padrão: 30s)

**Exemplos:**
```powershell
# Teste rápido (pula Docker e migrations)
.\test-all-databases.ps1 -SkipDocker -SkipMigrations

# Teste completo com timeout maior
.\test-all-databases.ps1 -ApiStartupTimeout 60
```

### Linux/macOS (Bash)

```bash
cd linux
chmod +x test-all-databases.sh
./test-all-databases.sh
```

**Opções:**
- `--skip-docker` - Não reinicia containers Docker
- `--skip-migrations` - Não aplica migrations
- `--skip-tests` - Não testa a API
- `--timeout <seconds>` - Timeout para API iniciar (padrão: 30s)

**Exemplos:**
```bash
# Teste rápido
./test-all-databases.sh --skip-docker --skip-migrations

# Teste completo com timeout maior
./test-all-databases.sh --timeout 60
```

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
```
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
cd windows
.\new-project.ps1 -ProjectName "MeuProjeto"
```

### Linux/macOS

```bash
cd linux
./new-project.sh MeuProjeto
```

---

## ☸️ minikube-deploy

Deploy da aplicação em cluster Kubernetes local (Minikube).

### Windows (PowerShell)

```powershell
cd windows
.\minikube-deploy.ps1
```

### Linux/macOS

```bash
cd linux
chmod +x minikube-deploy.sh
./minikube-deploy.sh
```

---

## 🗑️ minikube-destroy

Remove o deploy do Minikube.

### Windows (PowerShell)

```powershell
cd windows
.\minikube-destroy.ps1
```

### Linux/macOS

```bash
cd linux
./minikube-destroy.sh
```

---

## 🧪 run-integration-tests

Executa testes de integração no Minikube.

### Windows (PowerShell)

```powershell
cd windows
.\run-integration-tests.ps1
```

### Linux/macOS

```bash
cd linux
./run-integration-tests.sh
```

---

## 📝 Convenções

- **Windows**: Scripts PowerShell (`.ps1`) e Batch (`.bat`)
- **Linux/macOS**: Scripts Bash (`.sh`)
- **Sempre** execute scripts do diretório correto (`windows/` ou `linux/`)
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
- [TESTING-DATABASES.md](../TESTING-DATABASES.md) - Guia completo de testes

Para deploy em Kubernetes:
- [docs/KUBERNETES.md](../docs/KUBERNETES.md) - Guia de deploy K8s
