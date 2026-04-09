# Script interativo para criar novo projeto a partir do template
# Modo interativo:     .\new-project.ps1
# Com nome:            .\new-project.ps1 -ProjectName "MeuProjeto"
# Modo não-interativo: .\new-project.ps1 -ProjectName "MeuProjeto" -Database PostgreSQL -Cache Redis -MongoDB Yes -Queue Yes -Storage Azure -Telemetry Yes -EventSourcing No -GitInit Yes

param(
    [string]$ProjectName,

    [ValidateSet("InMemory", "SqlServer", "Oracle", "PostgreSQL", "MySQL")]
    [string]$Database,

    [ValidateSet("Memory", "Redis")]
    [string]$Cache,

    [ValidateSet("Yes", "No")]
    [string]$MongoDB,

    [ValidateSet("Yes", "No")]
    [string]$Queue,

    [ValidateSet("None", "Azure", "Aws", "Google")]
    [string]$Storage,

    [ValidateSet("Yes", "No")]
    [string]$Telemetry,

    [ValidateSet("Yes", "No")]
    [string]$EventSourcing,

    [ValidateSet("Yes", "No")]
    [string]$GitInit
)

$ErrorActionPreference = "Stop"

# ============================================================
# Project Name (prompt if not provided)
# ============================================================

if ([string]::IsNullOrWhiteSpace($ProjectName)) {
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║  Criar Novo Projeto a partir do Template                ║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""

    do {
        $ProjectName = Read-Host "  Qual o nome do projeto"
        if ([string]::IsNullOrWhiteSpace($ProjectName)) {
            Write-Host "  Nome do projeto não pode ser vazio." -ForegroundColor Red
        }
    } while ([string]::IsNullOrWhiteSpace($ProjectName))
}

# Validate project name (alphanumeric, dots, hyphens, underscores)
if ($ProjectName -notmatch '^[a-zA-Z][a-zA-Z0-9._-]*$') {
    Write-Host "  Erro: Nome do projeto invalido. Use letras, numeros, pontos, hifens ou underscores. Deve comecar com letra." -ForegroundColor Red
    exit 1
}

# ============================================================
# Helper Functions
# ============================================================

function Write-Step {
    param([string]$Icon, [string]$Message)
    Write-Host "  $Icon " -NoNewline
    Write-Host $Message
}

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║  $($Title.PadRight(55))║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "  ── $Title ──" -ForegroundColor Yellow
    Write-Host ""
}

function Show-Menu {
    param(
        [string]$Title,
        [string[]]$Options,
        [int]$Default = 1
    )

    Write-Host "  $Title" -ForegroundColor White
    Write-Host ""

    for ($i = 0; $i -lt $Options.Length; $i++) {
        $num = $i + 1
        if ($num -eq $Default) {
            Write-Host "    [" -NoNewline
            Write-Host "$num" -ForegroundColor Green -NoNewline
            Write-Host "] $($Options[$i]) " -NoNewline
            Write-Host "(padrão)" -ForegroundColor DarkGray
        } else {
            Write-Host "    [$num] $($Options[$i])"
        }
    }

    Write-Host ""
    $choice = Read-Host "  Escolha (1-$($Options.Length)) [padrão: $Default]"

    if ([string]::IsNullOrWhiteSpace($choice)) {
        return $Default
    }

    $parsed = 0
    if ([int]::TryParse($choice, [ref]$parsed) -and $parsed -ge 1 -and $parsed -le $Options.Length) {
        return $parsed
    }

    Write-Host "  Opção inválida. Usando padrão: $Default" -ForegroundColor DarkYellow
    return $Default
}

function Show-YesNo {
    param(
        [string]$Title,
        [bool]$DefaultYes = $false
    )

    $default = if ($DefaultYes) { 1 } else { 2 }
    $result = Show-Menu -Title $Title -Options @("Sim", "Não") -Default $default
    return ($result -eq 1)
}

# ============================================================
# Interactive Mode Detection
# ============================================================

$isInteractive = -not ($PSBoundParameters.ContainsKey('Database') -or
                        $PSBoundParameters.ContainsKey('Cache') -or
                        $PSBoundParameters.ContainsKey('MongoDB') -or
                        $PSBoundParameters.ContainsKey('Queue') -or
                        $PSBoundParameters.ContainsKey('Storage') -or
                        $PSBoundParameters.ContainsKey('Telemetry') -or
                        $PSBoundParameters.ContainsKey('EventSourcing') -or
                        $PSBoundParameters.ContainsKey('GitInit'))

# ============================================================
# Collect Choices
# ============================================================

if ($isInteractive) {
    Write-Header "Novo Projeto: $ProjectName"

    Write-Section "Banco de Dados"
    $dbChoice = Show-Menu -Title "Qual banco de dados utilizar?" -Options @(
        "InMemory (sem container Docker)",
        "SQL Server",
        "Oracle",
        "PostgreSQL",
        "MySQL"
    ) -Default 1

    $Database = switch ($dbChoice) {
        1 { "InMemory" }
        2 { "SqlServer" }
        3 { "Oracle" }
        4 { "PostgreSQL" }
        5 { "MySQL" }
    }

    Write-Section "Cache"
    $useRedis = Show-YesNo -Title "Habilitar cache Redis? (caso contrário, usa cache em memória)"
    $Cache = if ($useRedis) { "Redis" } else { "Memory" }

    Write-Section "NoSQL"
    $useMongo = Show-YesNo -Title "Habilitar MongoDB (document store)?"
    $MongoDB = if ($useMongo) { "Yes" } else { "No" }

    Write-Section "Mensageria"
    $useQueue = Show-YesNo -Title "Habilitar RabbitMQ (fila de mensagens)?"
    $Queue = if ($useQueue) { "Yes" } else { "No" }

    Write-Section "Storage em Nuvem"
    $storageChoice = Show-Menu -Title "Qual provider de storage em nuvem?" -Options @(
        "Nenhum (configurar depois)",
        "Azure Blob Storage",
        "AWS S3",
        "Google Cloud Storage"
    ) -Default 1

    $Storage = switch ($storageChoice) {
        1 { "None" }
        2 { "Azure" }
        3 { "Aws" }
        4 { "Google" }
    }

    Write-Section "Observabilidade"
    $useTelemetry = Show-YesNo -Title "Habilitar Telemetria (Jaeger + Prometheus + Grafana)?"
    $Telemetry = if ($useTelemetry) { "Yes" } else { "No" }

    Write-Section "Event Sourcing"
    $useEventSourcing = Show-YesNo -Title "Habilitar Event Sourcing (Marten + PostgreSQL)?"
    $EventSourcing = if ($useEventSourcing) { "Yes" } else { "No" }

    Write-Section "Git"
    $doGitInit = Show-YesNo -Title "Inicializar repositório Git?" -DefaultYes $true
    $GitInit = if ($doGitInit) { "Yes" } else { "No" }

    # ── Summary ──
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║  Resumo das Configurações                               ║" -ForegroundColor Cyan
    Write-Host "  ╠══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
    Write-Host "  ║  Projeto:        $($ProjectName.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Banco de Dados: $($Database.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Cache:          $($Cache.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  MongoDB:        $($MongoDB.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  RabbitMQ:       $($Queue.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Storage:        $($Storage.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Telemetria:     $($Telemetry.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Event Sourcing: $($EventSourcing.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Git Init:       $($GitInit.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""

    $confirm = Read-Host "  Confirmar e criar projeto? (S/N) [S]"
    if ($confirm -eq 'N' -or $confirm -eq 'n') {
        Write-Host ""
        Write-Host "  Operação cancelada." -ForegroundColor Red
        exit 0
    }
} else {
    # Apply defaults for non-interactive mode
    if (-not $Database) { $Database = "InMemory" }
    if (-not $Cache) { $Cache = "Memory" }
    if (-not $MongoDB) { $MongoDB = "No" }
    if (-not $Queue) { $Queue = "No" }
    if (-not $Storage) { $Storage = "None" }
    if (-not $Telemetry) { $Telemetry = "No" }
    if (-not $EventSourcing) { $EventSourcing = "No" }
    if (-not $GitInit) { $GitInit = "Yes" }
}

# ============================================================
# Project Creation
# ============================================================

$TemplateDir = Split-Path -Parent $PSScriptRoot
$TargetDir = Join-Path (Split-Path -Parent $TemplateDir) $ProjectName

Write-Host ""
Write-Step "🚀" "Criando projeto: $ProjectName"
Write-Step "📁" "Template: $TemplateDir"
Write-Step "📁" "Destino:  $TargetDir"
Write-Host ""

if (Test-Path $TargetDir) {
    Write-Host "  ❌ Erro: Diretório $TargetDir já existe" -ForegroundColor Red
    exit 1
}

# Copy template
Write-Step "📋" "Copiando template..."
Copy-Item -Path $TemplateDir -Destination $TargetDir -Recurse -Exclude @('.git', 'bin', 'obj', '.vs', '.vscode')

Set-Location $TargetDir

# Cleanup
Write-Step "🧹" "Limpando arquivos desnecessários..."
$itemsToRemove = @("scripts", ".git", ".gitignore")
foreach ($item in $itemsToRemove) {
    if (Test-Path $item) {
        Remove-Item -Path $item -Recurse -Force -ErrorAction SilentlyContinue
    }
}
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# Rename solution and references
Write-Step "✏️" "Renomeando referências ProjectTemplate → $ProjectName..."
Rename-Item -Path "ProjectTemplate.sln" -NewName "$ProjectName.sln"

$files = Get-ChildItem -Recurse -Include *.cs,*.csproj,*.sln,*.json,*.yml,*.yaml,*.md,*.props
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match 'ProjectTemplate') {
        $content = $content -replace 'ProjectTemplate', $ProjectName
        Set-Content $file.FullName $content -NoNewline
    }
}

# ============================================================
# Configure appsettings.json
# ============================================================

Write-Step "⚙️" "Configurando appsettings.json..."

$appSettingsPath = Join-Path $TargetDir "src/Api/appsettings.json"
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# -- Database --
$appSettings.AppSettings.Infrastructure.Database.DatabaseType = $Database

$connectionStrings = @{
    "InMemory"   = ""
    "SqlServer"  = "Server=localhost,1433;Database=$ProjectName;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
    "Oracle"     = "User Id=appuser;Password=AppPass123;Data Source=localhost:1521/FREEPDB1;"
    "PostgreSQL" = "Host=localhost;Port=5433;Database=$ProjectName;Username=postgres;Password=PostgresPass123;"
    "MySQL"      = "Server=localhost;Port=3306;Database=$ProjectName;User=appuser;Password=AppPass123;"
}
$appSettings.AppSettings.Infrastructure.Database.ConnectionString = $connectionStrings[$Database]

# Remove database-specific appsettings files that don't match
$dbFileMap = @{
    "SqlServer"  = "appsettings.SqlServer.json"
    "Oracle"     = "appsettings.Oracle.json"
    "PostgreSQL" = "appsettings.PostgreSQL.json"
    "MySQL"      = "appsettings.MySQL.json"
}

foreach ($db in $dbFileMap.Keys) {
    $filePath = Join-Path $TargetDir "src/Api/$($dbFileMap[$db])"
    if (Test-Path $filePath) {
        if ($Database -eq "InMemory" -or $Database -ne $db) {
            Remove-Item $filePath -Force
        }
    }
}

# -- Cache --
$appSettings.AppSettings.Infrastructure.Cache.Provider = $Cache
if ($Cache -eq "Redis") {
    $appSettings.AppSettings.Infrastructure.Cache.ConnectionString = "localhost:6379,password=RedisPass123,ssl=false,abortConnect=false"
}

# -- MongoDB --
if ($MongoDB -eq "Yes") {
    $appSettings.AppSettings.Infrastructure.MongoDB.ConnectionString = "mongodb://mongo:27017/$ProjectName"
}

# -- RabbitMQ --
if ($Queue -eq "Yes") {
    $appSettings.AppSettings.Infrastructure.RabbitMQ.ConnectionString = "amqp://guest:guest@localhost:5672/"
}

# -- Storage --
if ($Storage -ne "None") {
    $appSettings.AppSettings.Infrastructure.Storage.Provider = $Storage
}

# -- Telemetry --
if ($Telemetry -eq "Yes") {
    $appSettings.AppSettings.Infrastructure.Telemetry.Enabled = $true
    $appSettings.AppSettings.Infrastructure.Telemetry.Providers = @("Jaeger", "Prometheus")
} else {
    $appSettings.AppSettings.Infrastructure.Telemetry.Enabled = $false
}

# -- Event Sourcing --
if ($EventSourcing -eq "Yes") {
    $appSettings.AppSettings.Infrastructure.EventSourcing.Enabled = $true
    $appSettings.AppSettings.Infrastructure.EventSourcing.ConnectionString = "Host=localhost;Database=${ProjectName}Events;Username=postgres;Password=postgres"
} else {
    $appSettings.AppSettings.Infrastructure.EventSourcing.Enabled = $false
}

# Save appsettings.json (ConvertTo-Json in PS 5.1 uses non-standard indentation; normalize to 2-space)
$jsonContent = $appSettings | ConvertTo-Json -Depth 20
$lines = $jsonContent -split '\r?\n'
$depth = 0
$result = @()
foreach ($line in $lines) {
    $trimmed = $line.TrimStart()
    if ($trimmed -match '^[}\]]') { $depth-- }
    $result += ('  ' * $depth) + ($trimmed -replace ':\s+', ': ')
    if ($trimmed -match '[\[{]\s*$') { $depth++ }
}
$jsonContent = $result -join "`n"
Set-Content $appSettingsPath $jsonContent -Encoding UTF8

# ============================================================
# Enable optional features in Program.cs
# ============================================================

$programPath = Join-Path $TargetDir "src/Api/Program.cs"
$programContent = Get-Content $programPath -Raw

if ($MongoDB -eq "Yes") {
    Write-Step "🍃" "Habilitando MongoDB no Program.cs..."
    $programContent = $programContent -replace '// (builder\.Services\.AddMongo<Program>\(\);)', '$1'
}

if ($Queue -eq "Yes") {
    Write-Step "📨" "Habilitando RabbitMQ no Program.cs..."
    $programContent = $programContent -replace '// (builder\.Services\.AddRabbitMq\(\);)', '$1'
}

if ($Storage -ne "None") {
    Write-Step "☁️" "Habilitando Storage no Program.cs..."
    $programContent = $programContent -replace '// (builder\.Services\.AddStorage<Program>\(\);)', '$1'
}

Set-Content $programPath $programContent -NoNewline

# ============================================================
# Generate docker-compose.yml
# ============================================================

$needsCompose = ($Database -ne "InMemory") -or ($Cache -eq "Redis") -or ($MongoDB -eq "Yes") -or ($Queue -eq "Yes") -or ($Telemetry -eq "Yes") -or ($EventSourcing -eq "Yes")

if ($needsCompose) {
    Write-Step "🐳" "Gerando docker-compose.yml..."

    $services = [System.Text.StringBuilder]::new()
    $volumes = [System.Collections.Generic.List[string]]::new()

    [void]$services.AppendLine("services:")

    # ── Database containers ──
    switch ($Database) {
        "SqlServer" {
            [void]$services.AppendLine(@"
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - app-network
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -C -Q 'SELECT 1' || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
"@)
            $volumes.Add("  sqlserver-data:")
        }
        "Oracle" {
            [void]$services.AppendLine(@"
  oracle:
    image: gvenzl/oracle-free:latest
    container_name: oracle
    environment:
      - ORACLE_PASSWORD=OraclePass123
      - ORACLE_DATABASE=$ProjectName
      - APP_USER=appuser
      - APP_USER_PASSWORD=AppPass123
    ports:
      - "1521:1521"
    volumes:
      - oracle-data:/opt/oracle/oradata
    networks:
      - app-network
    healthcheck:
      test: ["CMD-SHELL", "healthcheck.sh"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 60s
"@)
            $volumes.Add("  oracle-data:")
        }
        "PostgreSQL" {
            [void]$services.AppendLine(@"
  postgres:
    image: postgres:17-alpine
    container_name: postgres
    environment:
      - POSTGRES_DB=$ProjectName
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=PostgresPass123
    ports:
      - "5433:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - app-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
"@)
            $volumes.Add("  postgres-data:")
        }
        "MySQL" {
            [void]$services.AppendLine(@"
  mysql:
    image: mysql:8
    container_name: mysql
    environment:
      - MYSQL_ROOT_PASSWORD=MySqlPass123
      - MYSQL_DATABASE=$ProjectName
      - MYSQL_USER=appuser
      - MYSQL_PASSWORD=AppPass123
    ports:
      - "3306:3306"
    volumes:
      - mysql-data:/var/lib/mysql
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "root", "-pMySqlPass123"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
"@)
            $volumes.Add("  mysql-data:")
        }
    }

    # ── Redis ──
    if ($Cache -eq "Redis") {
        [void]$services.AppendLine(@"
  redis:
    image: redis:7-alpine
    container_name: redis
    command: redis-server --requirepass RedisPass123
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "RedisPass123", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
"@)
        $volumes.Add("  redis-data:")
    }

    # ── MongoDB ──
    if ($MongoDB -eq "Yes") {
        [void]$services.AppendLine(@"
  mongo:
    image: mongo:7
    container_name: mongo
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 20s
"@)
        $volumes.Add("  mongo-data:")
    }

    # ── RabbitMQ ──
    if ($Queue -eq "Yes") {
        [void]$services.AppendLine(@"
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: rabbitmq
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
"@)
        $volumes.Add("  rabbitmq-data:")
    }

    # ── Telemetry (Jaeger + Prometheus + Grafana) ──
    if ($Telemetry -eq "Yes") {
        [void]$services.AppendLine(@"
  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: jaeger
    restart: unless-stopped
    ports:
      - "16686:16686"
      - "4317:4317"
      - "4318:4318"
      - "14250:14250"
      - "9411:9411"
    environment:
      - COLLECTOR_OTLP_ENABLED=true
      - COLLECTOR_OTLP_HTTP_ENABLED=true
      - COLLECTOR_OTLP_GRPC_HOST_PORT=0.0.0.0:4317
      - COLLECTOR_OTLP_HTTP_HOST_PORT=0.0.0.0:4318
      - COLLECTOR_ZIPKIN_HOST_PORT=:9411
      - METRICS_STORAGE_TYPE=prometheus
      - LOG_LEVEL=info
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "wget", "--spider", "-q", "http://localhost:14269/"]
      interval: 10s
      timeout: 5s
      retries: 5

  prometheus:
    image: prom/prometheus:latest
    container_name: prometheus
    restart: unless-stopped
    ports:
      - "9090:9090"
    volumes:
      - prometheus-data:/prometheus
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "wget", "--spider", "-q", "http://localhost:9090/-/ready"]
      interval: 15s
      timeout: 5s
      retries: 5

  grafana:
    image: grafana/grafana:latest
    container_name: grafana
    restart: unless-stopped
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_USER=admin
      - GF_SECURITY_ADMIN_PASSWORD=admin
      - GF_USERS_ALLOW_SIGN_UP=false
    volumes:
      - grafana-data:/var/lib/grafana
    depends_on:
      - prometheus
      - jaeger
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "wget", "--spider", "-q", "http://localhost:3000/api/health"]
      interval: 15s
      timeout: 5s
      retries: 5
"@)
        $volumes.Add("  prometheus-data:")
        $volumes.Add("  grafana-data:")
    }

    # ── Event Sourcing (dedicated PostgreSQL) ──
    if ($EventSourcing -eq "Yes") {
        [void]$services.AppendLine(@"
  postgres-events:
    image: postgres:17-alpine
    container_name: postgres-events
    environment:
      - POSTGRES_DB=${ProjectName}Events
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres-events-data:/var/lib/postgresql/data
    networks:
      - app-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
"@)
        $volumes.Add("  postgres-events-data:")
    }

    # ── Network ──
    [void]$services.AppendLine(@"
networks:
  app-network:
    driver: bridge
"@)

    # ── Volumes ──
    if ($volumes.Count -gt 0) {
        [void]$services.AppendLine("volumes:")
        foreach ($vol in $volumes) {
            [void]$services.AppendLine($vol)
        }
    }

    $composePath = Join-Path $TargetDir "docker-compose.yml"
    $services.ToString().TrimEnd() | Set-Content $composePath -Encoding UTF8
}

# Remove template's original docker-compose files if no containers needed
if (-not $needsCompose) {
    $composeFiles = @("docker-compose.yml", "compose-observability.yml")
    foreach ($f in $composeFiles) {
        $p = Join-Path $TargetDir $f
        if (Test-Path $p) { Remove-Item $p -Force }
    }
} else {
    # Remove observability compose — it's been merged into the generated compose
    $obsCompose = Join-Path $TargetDir "compose-observability.yml"
    if (Test-Path $obsCompose) { Remove-Item $obsCompose -Force }
}

# ============================================================
# Git init
# ============================================================

if ($GitInit -eq "Yes") {
    Write-Step "📦" "Inicializando repositório Git..."
    git init --quiet 2>$null

    $gitignoreLines = @(
        "## .NET"
        "bin/"
        "obj/"
        "*.user"
        "*.suo"
        "*.cache"
        "*.dll"
        "*.pdb"
        ""
        "## IDE"
        ".vs/"
        ".vscode/"
        "*.swp"
        ""
        "## OS"
        "Thumbs.db"
        ".DS_Store"
        ""
        "## Secrets"
        "appsettings.*.local.json"
    )
    $gitignorePath = Join-Path $TargetDir ".gitignore"
    $gitignoreLines | Set-Content $gitignorePath -Encoding UTF8
}

# ============================================================
# Final Summary
# ============================================================

Write-Host ""
Write-Host "  ╔══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "  ║  ✅ Projeto criado com sucesso!                         ║" -ForegroundColor Green
Write-Host "  ╚══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  Próximos passos:" -ForegroundColor Cyan
Write-Host ""

$stepNum = 1

Write-Host "  $stepNum. cd $ProjectName" -ForegroundColor White
$stepNum++

if ($needsCompose) {
    Write-Host "  $stepNum. docker-compose up -d" -ForegroundColor White
    $stepNum++

    if ($Database -eq "Oracle") {
        Write-Host "     ⏳ Oracle pode levar ~60s para iniciar" -ForegroundColor DarkGray
    }
}

Write-Host "  $stepNum. dotnet restore" -ForegroundColor White
$stepNum++

Write-Host "  $stepNum. dotnet build" -ForegroundColor White
$stepNum++

if ($Database -ne "InMemory") {
    Write-Host "  $stepNum. dotnet ef migrations add InitialCreate --project src/Data --startup-project src/Api" -ForegroundColor White
    $stepNum++
    Write-Host "  $stepNum. dotnet ef database update --project src/Data --startup-project src/Api" -ForegroundColor White
    $stepNum++
}

Write-Host "  $stepNum. dotnet run --project src/Api" -ForegroundColor White
$stepNum++

Write-Host ""
Write-Host "  Serviços configurados:" -ForegroundColor Cyan

Write-Host "    📊 Banco de Dados: $Database" -ForegroundColor White
if ($Database -ne "InMemory") {
    Write-Host "       Connection String configurada em appsettings.json" -ForegroundColor DarkGray
}

Write-Host "    💾 Cache: $Cache" -ForegroundColor White
if ($Cache -eq "Redis") {
    Write-Host "       Redis UI: não incluída (instale RedisInsight se desejar)" -ForegroundColor DarkGray
}

if ($MongoDB -eq "Yes") {
    Write-Host "    🍃 MongoDB: habilitado" -ForegroundColor White
    Write-Host "       Connection: mongodb://mongo:27017/$ProjectName" -ForegroundColor DarkGray
}

if ($Queue -eq "Yes") {
    Write-Host "    📨 RabbitMQ: habilitado" -ForegroundColor White
    Write-Host "       Management UI: http://localhost:15672 (guest/guest)" -ForegroundColor DarkGray
}

if ($Storage -ne "None") {
    Write-Host "    ☁️  Storage: $Storage" -ForegroundColor White
    Write-Host "       Configure credenciais em appsettings.json → Infrastructure → Storage → $Storage" -ForegroundColor DarkGray
}

if ($Telemetry -eq "Yes") {
    Write-Host "    📡 Telemetria: habilitada" -ForegroundColor White
    Write-Host "       Jaeger UI:     http://localhost:16686" -ForegroundColor DarkGray
    Write-Host "       Prometheus:    http://localhost:9090" -ForegroundColor DarkGray
    Write-Host "       Grafana:       http://localhost:3000 (admin/admin)" -ForegroundColor DarkGray
}

if ($EventSourcing -eq "Yes") {
    Write-Host "    📜 Event Sourcing: habilitado (Marten + PostgreSQL)" -ForegroundColor White
}

Write-Host ""
Write-Host "  Documentação:" -ForegroundColor Cyan
Write-Host "    📖 README.md          - Guia completo"
Write-Host "    ⚡ QUICK-START.md     - Início rápido"
Write-Host "    🔧 docs/ORM-GUIDE.md  - Como trocar ORM"
Write-Host ""
