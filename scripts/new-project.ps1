# Script interativo para criar novo projeto a partir do template
# Modo interativo:     .\new-project.ps1
# Com nome:            .\new-project.ps1 -ProjectName "MeuProjeto"
# Modo não-interativo: .\new-project.ps1 -ProjectName "MeuProjeto" -Database PostgreSQL -MongoDB Yes -Queue Yes -Storage Azure -Telemetry Yes -EventSourcing No -GitInit Yes

param(
    [string]$ProjectName,

    [ValidateSet("SqlServer", "Oracle", "PostgreSQL", "MySQL")]
    [string]$Database,


    [ValidateSet("Yes", "No")]
    [string]$Queue,

    [ValidateSet("None", "Azure", "Aws", "Google")]
    [string]$Storage,

    [ValidateSet("Yes", "No")]
    [string]$Telemetry,


    [ValidateSet("Yes", "No")]
    [string]$GitInit,

    [ValidateSet("None", "Flutter", "Maui", "ReactNative", "All")]
    [string]$UIMobile,

    [ValidateSet("None", "Angular", "Blazor", "React", "Vue", "All")]
    [string]$UIWeb
)

$ErrorActionPreference = "Stop"

# ============================================================
# Project Name (prompt if not provided)
# ============================================================

if ([string]::IsNullOrWhiteSpace($ProjectName)) {
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║  Criar Novo Projeto a partir do Template                 ║" -ForegroundColor Cyan
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
    $border = '═' * ($Title.Length + 4)
    Write-Host ""
    Write-Host "  ╔${border}╗" -ForegroundColor Cyan
    Write-Host "  ║  ${Title}  ║" -ForegroundColor Cyan
    Write-Host "  ╚${border}╝" -ForegroundColor Cyan
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

    if ($DefaultYes) {
        $result = Show-Menu -Title $Title -Options @("Sim", "Não") -Default 1
        return ($result -eq 1)
    } else {
        $result = Show-Menu -Title $Title -Options @("Não", "Sim") -Default 1
        return ($result -eq 2)
    }
}

# ============================================================
# Interactive Mode Detection
# ============================================================

$isInteractive = -not ($PSBoundParameters.ContainsKey('Database') -or
                        $PSBoundParameters.ContainsKey('Queue') -or
                        $PSBoundParameters.ContainsKey('Storage') -or
                        $PSBoundParameters.ContainsKey('Telemetry') -or
                        $PSBoundParameters.ContainsKey('GitInit') -or
                        $PSBoundParameters.ContainsKey('UIMobile') -or
                        $PSBoundParameters.ContainsKey('UIWeb'))

# ============================================================
# Collect Choices
# ============================================================

if ($isInteractive) {
    Write-Header "Novo Projeto: $ProjectName"

    Write-Section "Banco de Dados"
    $dbChoice = Show-Menu -Title "Qual banco de dados utilizar?" -Options @(
        "PostgreSQL",
        "SQL Server",
        "Oracle",
        "MySQL"
    ) -Default 1

    $Database = switch ($dbChoice) {
        1 { "PostgreSQL" }
        2 { "SqlServer" }
        3 { "Oracle" }
        4 { "MySQL" }
    }



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


    Write-Section "UI - Web"
    $webChoice = Show-Menu -Title "Quais tecnologias Web deseja manter?" -Options @(
        "Nenhuma",
        "Angular",
        "Blazor",
        "React",
        "Vue",
        "Todas"
    ) -Default 1
    $UIWeb = switch ($webChoice) {
        1 { "None" }
        2 { "Angular" }
        3 { "Blazor" }
        4 { "React" }
        5 { "Vue" }
        6 { "All" }
    }

    Write-Section "UI - Mobile"
    $mobileChoice = Show-Menu -Title "Quais tecnologias Mobile deseja manter?" -Options @(
        "Nenhuma",
        "Flutter",
        "MAUI",
        "React Native",
        "Todas"
    ) -Default 1
    $UIMobile = switch ($mobileChoice) {
        1 { "None" }
        2 { "Flutter" }
        3 { "Maui" }
        4 { "ReactNative" }
        5 { "All" }
    }

    Write-Section "Git"
    $doGitInit = Show-YesNo -Title "Inicializar repositório Git?" -DefaultYes $true
    $GitInit = if ($doGitInit) { "Yes" } else { "No" }

    # ── Summary ──
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║  Resumo das Configurações                                ║" -ForegroundColor Cyan
    Write-Host "  ╠══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
    Write-Host "  ║  Projeto:        $($ProjectName.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Banco de Dados: $($Database.PadRight(38))║" -ForegroundColor Cyan

    Write-Host "  ║  RabbitMQ:       $($Queue.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Storage:        $($Storage.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Telemetria:     $($Telemetry.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  Git Init:       $($GitInit.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  UI Mobile:      $($UIMobile.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ║  UI Web:         $($UIWeb.PadRight(38))║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""

    if (-not (Show-YesNo -Title "Confirmar e criar projeto?" -DefaultYes $true)) {
        Write-Host ""
        Write-Host "  Operação cancelada." -ForegroundColor Red
        exit 0
    }
} else {
    $MongoDB = "Yes"
    $EventSourcing = "Yes"
    # Apply defaults for non-interactive mode
    if (-not $Database) { $Database = "PostgreSQL" }

    if (-not $Queue) { $Queue = "No" }
    if (-not $Storage) { $Storage = "None" }
    if (-not $Telemetry) { $Telemetry = "No" }
    if (-not $GitInit) { $GitInit = "Yes" }
    if (-not $UIMobile) { $UIMobile = "None" }
    if (-not $UIWeb) { $UIWeb = "None" }
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
$itemsToRemove = @(".git", ".gitignore")
foreach ($item in $itemsToRemove) {
    if (Test-Path $item) {
        Remove-Item -Path $item -Recurse -Force -ErrorAction SilentlyContinue
    }
}
# Remove migrations do template para permitir que o usuário crie a sua própria InitialCreate
$migrationsPath = Join-Path $TargetDir "src/Server/Data/Migrations"
if (Test-Path $migrationsPath) {
    Get-ChildItem -Path $migrationsPath -File | Remove-Item -Force -ErrorAction SilentlyContinue
}
# Remove scripts exclusivos do template
$templateOnlyScripts = @(
    "scripts/new-project.ps1",
    "scripts/new-project.sh",
    "scripts/new-project.bat",
    "scripts/windows/test-all-databases.ps1",
    "scripts/linux/test-all-databases.sh"
)
foreach ($script in $templateOnlyScripts) {
    $scriptPath = Join-Path $TargetDir $script
    if (Test-Path $scriptPath) {
        Remove-Item -Path $scriptPath -Force -ErrorAction SilentlyContinue
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
# UI Cleanup
# ============================================================

Write-Step "🧹" "Limpando frameworks de UI não selecionados..."

# -- Mobile --
if ($UIMobile -eq "None") {
    Remove-Item "src/UI/Mobile" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "run-mobile.sh", "run-mobile.ps1", "build-mobile-all.sh", "build-mobile-all.ps1" -Force -ErrorAction SilentlyContinue
    dotnet sln "$ProjectName.sln" remove src/UI/Mobile/MauiApp/MauiApp.csproj 2>$null
} elseif ($UIMobile -ne "All") {
    if ($UIMobile -ne "Flutter") {
        Remove-Item "src/UI/Mobile/FlutterApp" -Recurse -Force -ErrorAction SilentlyContinue
        $b = Get-Content "build-mobile-all.sh" -Raw; $b = $b -replace '(?s)# Flutter.*?popd', ''; Set-Content "build-mobile-all.sh" $b -NoNewline
        $r = Get-Content "run-mobile.sh" -Raw; $r = $r -replace '(?s)run_flutter.*?\n}', ''; $r = $r -replace '1\) \$\{CYAN\}Flutter.*?;;', ''; Set-Content "run-mobile.sh" $r -NoNewline
    }
    if ($UIMobile -ne "Maui") {
        Remove-Item "src/UI/Mobile/MauiApp" -Recurse -Force -ErrorAction SilentlyContinue
        $b = Get-Content "build-mobile-all.sh" -Raw; $b = $b -replace '(?s)# MAUI.*?MauiApp.csproj', ''; Set-Content "build-mobile-all.sh" $b -NoNewline
        $r = Get-Content "run-mobile.sh" -Raw; $r = $r -replace '(?s)run_maui.*?\n}', ''; $r = $r -replace '3\) \$\{CYAN\}MAUI.*?;;', ''; Set-Content "run-mobile.sh" $r -NoNewline
        dotnet sln "$ProjectName.sln" remove src/UI/Mobile/MauiApp/MauiApp.csproj 2>$null
    }
    if ($UIMobile -ne "ReactNative") {
        Remove-Item "src/UI/Mobile/ReactNativeApp" -Recurse -Force -ErrorAction SilentlyContinue
        $b = Get-Content "build-mobile-all.sh" -Raw; $b = $b -replace '(?s)# React Native.*?popd', ''; Set-Content "build-mobile-all.sh" $b -NoNewline
        $r = Get-Content "run-mobile.sh" -Raw; $r = $r -replace '(?s)run_react_native.*?\n}', ''; $r = $r -replace '2\) \$\{CYAN\}React Native.*?;;', ''; Set-Content "run-mobile.sh" $r -NoNewline
    }
}

# -- Web --
$aspireProgram = Join-Path $TargetDir "src/Aspire/AppHost/Program.cs"
$programContent = Get-Content $aspireProgram -Raw

if ($UIWeb -eq "None") {
    Remove-Item "src/UI/Web" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "build-web-all.sh", "build-web-all.ps1" -Force -ErrorAction SilentlyContinue
    dotnet sln "$ProjectName.sln" remove src/UI/Web/Blazor/WebApp/App/App.csproj 2>$null
    dotnet sln "$ProjectName.sln" remove src/UI/Web/Blazor/WebApp/App.Client/App.Client.csproj 2>$null
    dotnet sln "$ProjectName.sln" remove src/UI/Web/Blazor/Wasm/BlazorWasm.csproj 2>$null
    # Remove ProjectReference from AppHost
    $appHostCsproj = Join-Path $TargetDir "src/Aspire/AppHost/AppHost.csproj"
    $appHostContent = Get-Content $appHostCsproj -Raw
    $appHostContent = $appHostContent -replace '<ProjectReference Include=".*?UI\\Web\\Blazor\\WebApp\\App\\App\.csproj" />', ''
    Set-Content $appHostCsproj $appHostContent -NoNewline
    # Remove all web from Aspire (between comments)
    $programContent = $programContent -replace '(?s)// Web Projects.*?// End Web Projects', '// Web UI Removed'
} elseif ($UIWeb -ne "All") {
    # Angular
    if ($UIWeb -ne "Angular") {
        Remove-Item "src/UI/Web/Angular" -Recurse -Force -ErrorAction SilentlyContinue
        $b = Get-Content "build-web-all.sh" -Raw; $b = $b -replace '(?s)# Angular.*?popd', ''; Set-Content "build-web-all.sh" $b -NoNewline
        $programContent = $programContent -replace '(?s)builder\.AddNpmApp\("angular-web".*?\.WithExternalHttpEndpoints\(\);', ''
    }
    # Blazor
    if ($UIWeb -ne "Blazor") {
        Remove-Item "src/UI/Web/Blazor" -Recurse -Force -ErrorAction SilentlyContinue
        $b = Get-Content "build-web-all.sh" -Raw; $b = $b -replace '(?s)# Blazor.*?App.csproj', ''; Set-Content "build-web-all.sh" $b -NoNewline
        dotnet sln "$ProjectName.sln" remove src/UI/Web/Blazor/WebApp/App/App.csproj 2>$null
        dotnet sln "$ProjectName.sln" remove src/UI/Web/Blazor/WebApp/App.Client/App.Client.csproj 2>$null
        dotnet sln "$ProjectName.sln" remove src/UI/Web/Blazor/Wasm/BlazorWasm.csproj 2>$null
        # Remove ProjectReference from AppHost
        $appHostCsproj = Join-Path $TargetDir "src/Aspire/AppHost/AppHost.csproj"
        $appHostContent = Get-Content $appHostCsproj -Raw
        $appHostContent = $appHostContent -replace '<ProjectReference Include=".*?UI\\Web\\Blazor\\WebApp\\App\\App\.csproj" />', ''
        Set-Content $appHostCsproj $appHostContent -NoNewline
        $programContent = $programContent -replace '(?s)builder\.AddProject<Projects\.App>\("blazor-app"\).*?\.WithExternalHttpEndpoints\(\);', ''
    }
    # React
    if ($UIWeb -ne "React") {
        Remove-Item "src/UI/Web/React" -Recurse -Force -ErrorAction SilentlyContinue
        $b = Get-Content "build-web-all.sh" -Raw; $b = $b -replace '(?s)# React.*?popd', ''; Set-Content "build-web-all.sh" $b -NoNewline
        $programContent = $programContent -replace '(?s)builder\.AddNpmApp\("react-web".*?\.WithExternalHttpEndpoints\(\);', ''
    }
    # Vue
    if ($UIWeb -ne "Vue") {
        Remove-Item "src/UI/Web/Vue" -Recurse -Force -ErrorAction SilentlyContinue
        $b = Get-Content "build-web-all.sh" -Raw; $b = $b -replace '(?s)# Vue.*?popd', ''; Set-Content "build-web-all.sh" $b -NoNewline
        $programContent = $programContent -replace '(?s)builder\.AddNpmApp\("vue-web".*?\.WithExternalHttpEndpoints\(\);', ''
    }
}
Set-Content $aspireProgram $programContent -NoNewline

# ============================================================
# Configure appsettings.json
# ============================================================

Write-Step "⚙️" "Configurando appsettings.json..."

$appSettingsPath = Join-Path $TargetDir "src/Server/Api/appsettings.json"
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# -- Database --
$appSettings.AppSettings.Infrastructure.Database.DatabaseType = $Database

$connectionStrings = @{
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
    $filePath = Join-Path $TargetDir "src/Server/Api/$($dbFileMap[$db])"
    if (Test-Path $filePath) {
        if ($Database -ne $db) {
            Remove-Item $filePath -Force
        }
    }
}


# -- MongoDB --
$appSettings.AppSettings.Infrastructure.MongoDB.ConnectionString = "mongodb://admin:admin@localhost:27017/$ProjectName"

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
$appSettings.AppSettings.Infrastructure.EventSourcing.Enabled = $true
$appSettings.AppSettings.Infrastructure.EventSourcing.ConnectionString = "Host=localhost;Database=${ProjectName}Events;Username=postgres;Password=postgres;Port=5432"

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

$programPath = Join-Path $TargetDir "src/Server/Api/Program.cs"
$programContent = Get-Content $programPath -Raw

    Write-Step "🍃" "Habilitando MongoDB no Program.cs..."
    $programContent = $programContent -replace '// (builder\.Services\.AddMongo<Program>\(\);)', '$1'
    Write-Step "🌱" "Habilitando seed inicial do MongoDB no Program.cs..."
    $programContent = $programContent -replace '// (await MongoDbSeeder\.SeedAsync\(scope\.ServiceProvider\);)', '$1'

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

$needsCompose = ($true) -or ($MongoDB -eq "Yes") -or ($Queue -eq "Yes") -or ($Telemetry -eq "Yes") -or ($EventSourcing -eq "Yes")

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


    # ── MongoDB ──
    if ($MongoDB -eq "Yes") {
        [void]$services.AppendLine(@"
  mongo:
    image: mongo:7
    container_name: mongo
    environment:
      - MONGO_INITDB_ROOT_USERNAME=admin
      - MONGO_INITDB_ROOT_PASSWORD=admin
      - MONGO_INITDB_DATABASE=$ProjectName
      - MONGO_APP_USERNAME=admin
      - MONGO_APP_PASSWORD=admin
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
      - ./scripts/mongo-init:/docker-entrypoint-initdb.d
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "mongosh", "mongodb://admin:admin@localhost:27017/admin", "--eval", "db.adminCommand('ping')"]
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

    # ── Redis (Always needed by API) ──
    [void]$services.AppendLine(@"
  redis:
    image: redis:alpine
    container_name: redis
    ports:
      - "6379:6379"
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
"@)

    # ── API ──
    $isEventSourcing = if ($EventSourcing -eq "Yes") { "true" } else { "false" }
    $isTelemetry = if ($Telemetry -eq "Yes") { "true" } else { "false" }

    $dependsOn = [System.Text.StringBuilder]::new()
    [void]$dependsOn.AppendLine("    depends_on:")
    [void]$dependsOn.AppendLine("      redis: { condition: service_healthy }")
    if ($Queue -eq "Yes") { [void]$dependsOn.AppendLine("      rabbitmq: { condition: service_healthy }") }
    if ($MongoDB -eq "Yes") { [void]$dependsOn.AppendLine("      mongo: { condition: service_healthy }") }
    if ($Telemetry -eq "Yes") {
        [void]$dependsOn.AppendLine("      jaeger: { condition: service_healthy }")
        [void]$dependsOn.AppendLine("      prometheus: { condition: service_healthy }")
    }

    [void]$services.AppendLine(@"
  api:
    container_name: api
    build:
      context: .
      dockerfile: src/Server/Api/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - AppSettings__Infrastructure__Database__DatabaseType=$Database
      - AppSettings__Infrastructure__Database__ConnectionString=$DbConn
      - AppSettings__Infrastructure__EventSourcing__Enabled=$isEventSourcing
      - AppSettings__Infrastructure__EventSourcing__ConnectionString=$DbEventsConn
      - AppSettings__Infrastructure__Redis__ConnectionString=redis:6379
      - AppSettings__Infrastructure__RabbitMQ__ConnectionString=amqp://guest:guest@rabbitmq:5672
      - AppSettings__Infrastructure__MongoDB__ConnectionString=mongodb://admin:admin@mongo:27017/$ProjectName
      - AppSettings__Infrastructure__Telemetry__Enabled=$isTelemetry
      - AppSettings__Infrastructure__Telemetry__Jaeger__Host=jaeger
      - AppSettings__Infrastructure__Telemetry__Jaeger__Port=4317
      - AppSettings__Infrastructure__Telemetry__Jaeger__UseGrpc=true
$($dependsOn.ToString().TrimEnd())
    networks:
      - app-network
"@)

    # ── Web Projects ──
    if ($UIWeb -ne "None") {
        if ($UIWeb -eq "All" -or $UIWeb -eq "Angular") {
            [void]$services.AppendLine(@"
  angular-web:
    container_name: web-angular
    build:
      context: src/UI/Web/Angular
      dockerfile: Dockerfile
    environment:
      - API_URL=http://localhost:5000
    ports:
      - "4200:80"
    depends_on:
      - api
    networks:
      - app-network
"@)
        }
        if ($UIWeb -eq "All" -or $UIWeb -eq "React") {
            [void]$services.AppendLine(@"
  react-web:
    container_name: web-react
    build:
      context: src/UI/Web/React
      dockerfile: Dockerfile
    ports:
      - "5173:80"
    environment:
      - VITE_API_BASE_URL=http://localhost:5000
    depends_on:
      - api
    networks:
      - app-network
"@)
        }
        if ($UIWeb -eq "All" -or $UIWeb -eq "Vue") {
            [void]$services.AppendLine(@"
  vue-web:
    container_name: web-vue
    build:
      context: src/UI/Web/Vue
      dockerfile: Dockerfile
    ports:
      - "5174:80"
    environment:
      - VITE_API_BASE_URL=http://localhost:5000
    depends_on:
      - api
    networks:
      - app-network
"@)
        }
        if ($UIWeb -eq "All" -or $UIWeb -eq "Blazor") {
            [void]$services.AppendLine(@"
  blazor-app:
    container_name: web-blazor
    build:
      context: .
      dockerfile: src/UI/Web/Blazor/WebApp/App/Dockerfile
    ports:
      - "5188:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ApiBaseUrl=http://api:8080
    depends_on:
      - api
    networks:
      - app-network
"@)
        }
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
# Generate clean scripts/README.md (without template-only sections)
# ============================================================

$scriptsReadmePath = Join-Path $TargetDir "scripts/README.md"
$scriptsReadmeContent = @'
# 📜 Scripts

Scripts utilitários para facilitar o desenvolvimento e testes do projeto.

---

## 📋 Índice

- [run-sonar-analysis](#-run-sonar-analysis) - Executar análise SonarCloud local
- [Kubernetes (Minikube)](#%EF%B8%8F-kubernetes-minikube) - Deploy, destroy e testes de integração no Minikube

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

## ☸️ Kubernetes (Minikube)

### minikube-deploy

Deploy da aplicação em cluster Kubernetes local (Minikube).

#### Windows (PowerShell)

```powershell
cd scripts\windows
.\minikube-deploy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
chmod +x minikube-deploy.sh
./minikube-deploy.sh
```

### minikube-destroy

Remove o deploy do Minikube.

#### Windows (PowerShell)

```powershell
cd scripts\windows
.\minikube-destroy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
./minikube-destroy.sh
```

### run-integration-tests

Executa testes de integração no Minikube.

#### Windows (PowerShell)

```powershell
cd scripts\windows
.\run-integration-tests.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
./run-integration-tests.sh
```

---

## 📝 Convenções

- **Windows**: Scripts PowerShell (`.ps1`) e Batch (`.bat`) em `scripts/windows/`
- **Linux/macOS**: Scripts Bash (`.sh`) em `scripts/linux/`
- **Multiplataforma**: Scripts de análise na raiz de `scripts/`
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

Para deploy em Kubernetes:

- [docs/KUBERNETES.md](../docs/KUBERNETES.md) - Guia de deploy K8s
'@
Set-Content $scriptsReadmePath $scriptsReadmeContent -Encoding UTF8

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
Write-Host "  ║  ✅ Projeto criado com sucesso!                          ║" -ForegroundColor Green
Write-Host "  ╚══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  Próximos passos:" -ForegroundColor White
Write-Host ""
Write-Host "  1. cd $ProjectName" -ForegroundColor White
Write-Host "  2. .\run.ps1" -ForegroundColor White

Write-Host ""
Write-Host "  Serviços configurados:" -ForegroundColor Cyan

Write-Host "    📊 Banco de Dados: $Database" -ForegroundColor White
if ($Database -ne "InMemory") {
    Write-Host "       Connection String configurada em appsettings.json" -ForegroundColor DarkGray
}




if ($MongoDB -eq "Yes") {
    Write-Host "    🍃 MongoDB: habilitado" -ForegroundColor White
  Write-Host "       Connection: mongodb://admin:admin@localhost:27017/$ProjectName" -ForegroundColor DarkGray
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
