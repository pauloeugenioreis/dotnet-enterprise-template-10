# Enterprise Template - Mobile Launcher (Windows)
$ProjectRoot = Get-Location
$FlutterPath = "$ProjectRoot\src\UI\Mobile\FlutterApp"
$RNPath = "$ProjectRoot\src\UI\Mobile\ReactNativeApp"
$MauiPath = "$ProjectRoot\src\UI\Mobile\MauiApp"
$ApiUrl = "http://localhost:5266/health"

function Show-Header {
    Clear-Host
    Write-Host "===============================================" -ForegroundColor Cyan
    Write-Host "    Enterprise Template - Mobile Launcher      " -ForegroundColor Cyan
    Write-Host "===============================================" -ForegroundColor Cyan
}

function Check-Backend {
    # Seleção de Banco de Dados
    Write-Host "`nEscolha o Banco de Dados Principal:"
    Write-Host "1) PostgreSQL (Padrão)" -ForegroundColor Cyan
    Write-Host "2) SQL Server" -ForegroundColor Cyan
    Write-Host "3) MySQL" -ForegroundColor Cyan
    Write-Host "4) Oracle" -ForegroundColor Cyan
    $dbOption = Read-Host "Escolha uma opção [1-4]"
    
    $dbType = switch ($dbOption) {
        "2" { "sqlserver" }
        "3" { "mysql" }
        "4" { "oracle" }
        Default { "postgresql" }
    }

    $dbConn = switch ($dbType) {
        "sqlserver" { "Server=sqlserver;Database=ProjectTemplateDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True" }
        "mysql"     { "Server=mysql;Port=3306;Database=ProjectTemplate;Uid=appuser;Pwd=AppPass123;" }
        "oracle"    { "Data Source=oracle:1521/ProjectTemplate;User Id=appuser;Password=AppPass123" }
        Default     { "Host=postgres;Database=ProjectTemplate;Username=postgres;Password=PostgresPass123;Port=5432" }
    }

    $dbEventsConn = "Host=postgres-events;Database=ProjectTemplateEvents;Username=postgres;Password=postgres"

    $dbService = switch ($dbType) {
        "sqlserver" { "sqlserver" }
        "mysql"     { "mysql" }
        "oracle"    { "oracle" }
        Default     { "postgres" }
    }

    $dbContainers = if ($dbType -ne "postgresql") { "$dbService", "postgres-events" } else { "postgres", "postgres-events" }

    # Subir Docker (Infra + API)
    Write-Host "Subindo containers ($dbType)..." -ForegroundColor Cyan
    $env:DB_TYPE = $dbType
    $env:DB_CONNECTION_STRING = $dbConn
    $env:DB_EVENTS_CONNECTION_STRING = $dbEventsConn
    docker compose up -d $dbContainers api redis rabbitmq mongodb jaeger prometheus grafana
    
    Write-Host "Aguardando API ficar pronta..." -NoNewline
    while ($true) {
        try {
            $check = Invoke-WebRequest -Uri "http://localhost:5000/health/ready" -Method Get -UseBasicParsing -ErrorAction Stop
            if ($check.Content -like "*Healthy*") { break }
        } catch {
            Write-Host "." -NoNewline
            Start-Sleep -Seconds 2
        }
    }
    Write-Host "`n✔ Backend pronto na porta 5000!" -ForegroundColor Green
}

function Run-Flutter {
    Write-Host "`nEscolha o Alvo do Flutter:" -ForegroundColor Cyan
    Write-Host "1) Android (Emulador/Físico)"
    Write-Host "2) Windows Desktop"
    $targetOption = Read-Host "Opção [1-2]"

    Set-Location $FlutterPath
    
    switch ($targetOption) {
        "2" {
            Write-Host "Iniciando Flutter App no Windows Desktop..." -ForegroundColor Green
            flutter run -d windows
        }
        Default {
            Write-Host "Iniciando Flutter App no Android..." -ForegroundColor Green
            flutter run -d android
        }
    }
}

Show-Header
Write-Host "Escolha a plataforma mobile para iniciar:"
Write-Host "1) Flutter (Recomendado)" -ForegroundColor Cyan
Write-Host "2) React Native (Expo)" -ForegroundColor Cyan
Write-Host "3) MAUI (Android)" -ForegroundColor Cyan
Write-Host "q) Sair"
Write-Host ""

$option = Read-Host "Opção"

switch ($option) {
    "1" {
        Check-Backend
        Run-Flutter
    }
    "2" {
        Check-Backend
        Set-Location $RNPath
        npm run android
    }
    "3" {
        Check-Backend
        Set-Location $MauiPath
        dotnet build -t:Run -f net10.0-android
    }
    "q" { exit }
    Default { Write-Host "Opção inválida." -ForegroundColor Red }
}
