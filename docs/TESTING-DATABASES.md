# 🧪 Testing All Database Providers

Este guia explica como testar o projeto com todos os 4 bancos de dados suportados.

---

- Docker Desktop instalado e rodando
- .NET 10 SDK instalado
- Projeto compilado: `dotnet build`

---

## 🚀 Teste Rápido (Automatizado)

### Windows (PowerShell)

cd scripts\windows
.\test-all-databases.ps1

### Linux/macOS

cd scripts/linux
chmod +x test-all-databases.sh
./test-all-databases.sh
O script irá:

1. ✅ Subir os 4 bancos de dados no Docker
2. ✅ Aplicar migrations em cada banco
3. ✅ Compilar o projeto
4. ✅ Iniciar a API com cada banco
5. ✅ Testar endpoints (health check, swagger)
6. ✅ Gerar relatório de testes

**Tempo estimado:** 5-10 minutos

---

## 🐳 Testcontainers (Integração Moderna)

A partir da versão .NET 10, este template utiliza **Testcontainers** para os testes de integração. Isso significa que você não precisa se preocupar em configurar bancos de dados manuais para rodar os testes — o sistema faz isso para você!

### Como funciona

Quando você executa os testes de integração (`tests/Integration`), o template:

1. Sobe automaticamente um container **PostgreSQL** oficial.
2. Executa todas as migrations automaticamente.
3. Roda os testes contra este banco real e isolado.
4. Remove o container ao final dos testes.

### Executar os testes

```bash
dotnet test tests/Integration/Integration.csproj
```

> 💡 **Dica:** Esta é a forma mais confiável de testar sua aplicação, pois garante que o código se comporta exatamente como em produção, mas em um ambiente totalmente efêmero.

---

## 🎯 Teste Manual (Passo a Passo)

### 1. Subir os Bancos de Dados

#### Na raiz do projeto

```bash
docker-compose up -d sqlserver oracle postgres mysql
```

**Aguarde os bancos ficarem prontos (30-60 segundos):**

#### SQL Server

```bash
docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -C -Q "SELECT 1"
```

#### Oracle (pode levar 1-2 minutos)

```bash
docker exec oracle healthcheck.sh
```

#### PostgreSQL

```bash
docker exec postgres pg_isready -U postgres
```

### 2. Testar SQL Server

#### Aplicar migrations (SqlServer)

```powershell
$env:ASPNETCORE_ENVIRONMENT="SqlServer"
dotnet ef database update --project src/Server/Data --startup-project src/Server/Api
```

#### Rodar aplicação (SqlServer)

```powershell
dotnet run --project src/Server/Api --environment SqlServer
```

#### Testar (SqlServer)

```bash
curl http://localhost:5000/health
curl http://localhost:5000/swagger/index.html
```

#### Connection String (SqlServer)

```text
Server=localhost,1433;Database=ProjectTemplate;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

---

### 3. Testar Oracle

#### Aplicar migrations (Oracle)

```powershell
$env:ASPNETCORE_ENVIRONMENT="Oracle"
dotnet ef database update --project src/Server/Data --startup-project src/Server/Api
```

#### Rodar aplicação (Oracle)

```powershell
dotnet run --project src/Server/Api --environment Oracle
```

#### Testar (Oracle)

```bash
curl http://localhost:5000/health
curl http://localhost:5000/swagger/index.html
```

#### Connection String (Oracle)

```text
User Id=appuser;Password=AppPass123;Data Source=localhost:1521/FREEPDB1;
```

**⚠️ Nota:** Oracle pode levar 1-2 minutos para ficar pronto na primeira execução.

---

### 4. Testar PostgreSQL

#### Aplicar migrations (PostgreSQL)

```powershell
$env:ASPNETCORE_ENVIRONMENT="PostgreSQL"
dotnet ef database update --project src/Server/Data --startup-project src/Server/Api
```

#### Rodar aplicação (PostgreSQL)

```powershell
dotnet run --project src/Server/Api --environment PostgreSQL
```

#### Testar (PostgreSQL)

```bash
curl http://localhost:5000/health
curl http://localhost:5000/swagger/index.html
```

#### Connection String (PostgreSQL)

```text
Host=localhost;Port=5433;Database=ProjectTemplate;Username=postgres;Password=PostgresPass123;
```

**⚠️ Nota:** PostgreSQL principal roda na porta **5433** (5432 é usada pelo Event Store).

---

### 5. Testar MySQL

#### Aplicar migrations (MySQL)

```powershell
$env:ASPNETCORE_ENVIRONMENT="MySQL"
dotnet ef database update --project src/Server/Data --startup-project src/Server/Api
```

#### Rodar aplicação (MySQL)

```powershell
dotnet run --project src/Server/Api --environment MySQL
```

#### Testar (MySQL)

```bash
curl http://localhost:5000/health
curl http://localhost:5000/swagger/index.html
```

#### Connection String (MySQL)

```text
Server=localhost;Port=3306;Database=ProjectTemplate;User=appuser;Password=AppPass123;
```

---

## 📝 Arquivos de Configuração

Cada banco tem seu próprio arquivo `appsettings.{Database}.json`:

```text
src/Server/Api/
├── appsettings.json              # Base (InMemory)
├── appsettings.Development.json  # Overrides de desenvolvimento
├── appsettings.SqlServer.json    # SQL Server config
├── appsettings.Oracle.json       # Oracle config
├── appsettings.PostgreSQL.json   # PostgreSQL config
└── appsettings.MySQL.json        # MySQL config
```

**Como funciona:**

Quando você roda `dotnet run --environment SqlServer`, o .NET carrega:

1. `appsettings.json` (base)
2. `appsettings.SqlServer.json` (sobrescreve configurações específicas)

---

## 🐳 Docker Compose - Portas e Credenciais

| Banco      | Porta | Usuário  | Senha               | Database        |
|------------|-------|----------|---------------------|-----------------|
| SQL Server | 1433  | sa       | YourStrong@Passw0rd | ProjectTemplate |
| Oracle     | 1521  | appuser  | AppPass123          | FREEPDB1        |
| PostgreSQL | 5433  | postgres | PostgresPass123     | ProjectTemplate |
| MySQL      | 3306  | appuser  | AppPass123          | ProjectTemplate |

---

## 🔧 Opções do Script de Teste

### Windows PowerShell

#### Testar tudo

```powershell
.\test-all-databases.ps1
```

#### Pular Docker (se já estiver rodando)

```powershell
.\test-all-databases.ps1 -SkipDocker
```

#### Pular migrations

```powershell
.\test-all-databases.ps1 -SkipMigrations
```

#### Pular testes de API

```powershell
.\test-all-databases.ps1 -SkipTests
```

#### Combinar opções

```powershell
.\test-all-databases.ps1 -SkipDocker -SkipMigrations
```

#### Aumentar timeout de startup da API (padrão: 30s)

```powershell
.\test-all-databases.ps1 -ApiStartupTimeout 60
```

### Linux/macOS

#### Testar tudo

```bash
./test-all-databases.sh
```

#### Pular Docker

```bash
./test-all-databases.sh --skip-docker
```

#### Pular migrations

```bash
./test-all-databases.sh --skip-migrations
```

#### Pular testes

```bash
./test-all-databases.sh --skip-tests
```

#### Combinar opções

```bash
./test-all-databases.sh --skip-docker --skip-migrations
```

#### Aumentar timeout

```bash
./test-all-databases.sh --timeout 60
```

---

## 🧹 Limpeza

### Parar todos os bancos

```bash
docker-compose down
```

### Parar e remover volumes (limpar dados)

```bash
docker-compose down -v
```

### Remover apenas um banco específico

```bash
docker-compose stop sqlserver
docker-compose rm -f sqlserver
docker volume rm template_sqlserver-data
```

---

## 🐛 Troubleshooting

### Oracle não inicia ou demora muito

Oracle Free Edition pode levar 1-2 minutos para inicializar na primeira vez.

#### Verificar logs

```bash
docker logs oracle -f
```

#### Aguardar até ver: "DATABASE IS READY TO USE!"

### Erro de conexão "Network unreachable"

Aguarde mais tempo. Os health checks do Docker podem levar até 30-60 segundos.

#### Verificar status dos containers

```bash
docker-compose ps
```

#### Verificar health status

```bash
docker inspect sqlserver --format='{{.State.Health.Status}}'
```

### Migrations falham com "database already exists"

Limpe o banco antes de rodar migrations:

```bash
dotnet ef database drop --project src/Server/Data --startup-project src/Server/Api --force
dotnet ef database update --project src/Server/Data --startup-project src/Server/Api
```

### Porta já em uso

Verifique se há outro serviço usando a porta:

#### Windows

```powershell
netstat -ano | findstr :1433
```

#### Linux/macOS

```bash
lsof -i :1433
```

Pare o serviço conflitante ou altere a porta no `docker-compose.yml`.

---

## ✅ Checklist de Testes

Para cada banco de dados:

- [ ] Container inicia sem erros
- [ ] Health check passa
- [ ] Migrations aplicam com sucesso
- [ ] API inicia sem erros
- [ ] `/health` retorna 200 OK
- [ ] `/swagger` é acessível
- [ ] Logs não mostram erros críticos

---

## 📊 Exemplo de Saída do Script

```text
================================================
  Testing All Database Providers
================================================

[1/4] Starting Docker Compose...
✅ Docker containers started

Waiting for databases to be ready...
  ✅ SQL Server ready
  ✅ Oracle ready
  ✅ PostgreSQL ready
  ✅ MySQL ready

================================================
  Testing: SqlServer
================================================
[2/4] Running migrations for SqlServer...
  ✅ Migrations applied successfully
[3/4] Building project...
  ✅ Build successful
[4/4] Starting API and testing...
  ✅ API started successfully
  ✅ Health check passed: Healthy
  ✅ Swagger UI accessible

================================================
  Testing: Oracle
================================================
[2/4] Running migrations for Oracle...
  ✅ Migrations applied successfully
[3/4] Building project...
  ✅ Build successful
[4/4] Starting API and testing...
  ✅ API started successfully
  ✅ Health check passed: Healthy
  ✅ Swagger UI accessible

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

## 🔗 Links Úteis

- **SQL Server**: http://localhost:1433
- **Oracle**: http://localhost:1521
- **PostgreSQL**: http://localhost:5433
- **MySQL**: http://localhost:3306
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

---

## 📚 Próximos Passos

Após validar que todos os bancos funcionam:

1. Escolha o banco para seu projeto
2. Atualize `appsettings.Production.json`
3. Configure CI/CD para usar o banco escolhido
4. Documente a escolha no README do seu projeto
