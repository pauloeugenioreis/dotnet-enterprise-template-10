# --- CONFIGURAÇÃO ---
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
chcp 65001 > $null
$script:IsWindowsHost = $env:OS -eq "Windows_NT"

# Cores (Simulando o padrão do shell)
$CYAN = "$([char]27)[0;36m"
$GREEN = "$([char]27)[0;32m"
$YELLOW = "$([char]27)[1;33m"
$BLUE = "$([char]27)[0;34m"
$RED = "$([char]27)[0;31m"
$NC = "$([char]27)[0m"

# Portas críticas
$PORTS = @(5000, 5001, 5432, 5433, 6379, 15672, 5672, 27017, 3000, 16686, 9090, 4200, 5173, 5174, 5188)

function Clean-Environment {
    Write-Host "`n$($RED)Limpando ambiente e liberando portas...$($NC)"

    # 1. Para todos os containers do projeto atual (incluindo volumes)
    docker-compose down --remove-orphans -v 2>$null | Out-Null

    # 2. Busca e para QUALQUER container docker que esteja usando as portas críticas
    foreach ($port in $PORTS) {
        $dockerPorts = docker ps -a --format "{{.ID}} {{.Ports}}"
        $cont_ids = $dockerPorts | Select-String ":$port->" | ForEach-Object { $_.ToString().Split(' ')[0] }

        if ($cont_ids) {
            foreach ($id in $cont_ids) {
                Write-Host "  - Removendo container ($id) na porta $port"
                docker rm -f $id 2>$null | Out-Null
            }
        }

        # Mata processos locais (ex: dotnet run)
        if ($script:IsWindowsHost) {
            $connections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
            if ($connections) {
                foreach ($conn in $connections) {
                    $processId = $conn.OwningProcess
                    try {
                        $proc = Get-Process -Id $processId -ErrorAction SilentlyContinue
                        if ($proc) {
                            Write-Host "  - Matando processo na porta $port (PID: $processId - $($proc.ProcessName))"
                            Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                        }
                    } catch {}
                }
            }
        } else {
            # Linux / macOS
            $processId = (lsof -t -i:$port 2>$null)
            if ($processId) {
                Write-Host "  - Matando processo na porta $port (PID: $processId)"
                kill -9 $processId 2>$null
            }
        }
    }

    Write-Host "$($YELLOW)Aguardando liberacao total das portas...$($NC)"
    Start-Sleep -Seconds 3

    # 4. Diagnóstico final
    Write-Host "`n$($CYAN)Verificacao final de portas:$($NC)"
    foreach ($port in $PORTS) {
        $portBusy = $false
        $processId = $null
        $procName = "Desconhecido"

        if ($script:IsWindowsHost) {
            $check = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
            if ($check) {
                $portBusy = $true
                $processId = $check[0].OwningProcess
                $procName = (Get-Process -Id $processId -ErrorAction SilentlyContinue).ProcessName
            }
        } else {
            $processId = (lsof -t -i:$port -sTCP:LISTEN 2>$null)
            if ($processId) {
                $portBusy = $true
                $procName = (ps -p $processId -o comm= 2>$null)
            }
        }

        if ($portBusy) {
            Write-Host "  $($RED)ATENCAO: A porta $port ainda esta ocupada por: [$procName] (PID: $processId)$($NC)"
            if ($script:IsWindowsHost) {
                Write-Host "     Dica: Tente rodar 'Stop-Process -Id $processId -Force' em um terminal como Administrador."
            } else {
                Write-Host "     Dica: Tente rodar 'sudo kill -9 $processId' manualmente."
            }
        }
    }

    Write-Host "$($GREEN)Ambiente pronto!$($NC)"
}

function Select-Database {
    Write-Host ""
    # Verifica quais bancos estão presentes no docker-compose.yml
    $composeContent = Get-Content "docker-compose.yml" -Raw
    $hasPostgres = $composeContent -match "postgres:"
    $hasMssql = $composeContent -match "mssql:"
    $hasMysql = $composeContent -match "mysql:"
    $hasOracle = $composeContent -match "oracle:"

    $availableDbs = @()
    if ($hasPostgres) { $availableDbs += "1" }
    if ($hasMssql) { $availableDbs += "2" }
    if ($hasMysql) { $availableDbs += "3" }
    if ($hasOracle) { $availableDbs += "4" }

    if ($availableDbs.Count -eq 1) {
        $dbChoice = $availableDbs[0]
        Write-Host "Banco de dados detectado automaticamente com base no docker-compose.yml." -ForegroundColor Cyan
    }
    else {
        Write-Host "`n$($CYAN)==========================================$($NC)"
        Write-Host "$($CYAN)  SELECIONE O BANCO DE DADOS PRINCIPAL    $($NC)"
        Write-Host "$($CYAN)==========================================$($NC)"
        Write-Host "1) $($GREEN)PostgreSQL$($NC) (Padrão)"
        Write-Host "2) $($YELLOW)SQL Server$($NC)"
        Write-Host "3) $($BLUE)MySQL$($NC)"
        Write-Host "4) $($RED)Oracle$($NC)"
        Write-Host "$($CYAN)==========================================$($NC)"
        Write-Host -NoNewline "$($CYAN)Escolha uma opção [1-4]: $($NC)"
        $dbChoice = Read-Host
    }

    switch ($dbChoice) {
        "2" {
            $script:DB_TYPE = "sqlserver"
            $script:DB_SERVICE = "sqlserver"
            $script:DB_PROFILE = "db-sqlserver"
            $script:DB_CONN = "Server=sqlserver;Database=ProjectTemplateDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
        }
        "3" {
            $script:DB_TYPE = "mysql"
            $script:DB_SERVICE = "mysql"
            $script:DB_PROFILE = "db-mysql"
            $script:DB_CONN = "Server=mysql;Port=3306;Database=ProjectTemplate;Uid=appuser;Pwd=AppPass123;"
        }
        "4" {
            $script:DB_TYPE = "oracle"
            $script:DB_SERVICE = "oracle"
            $script:DB_PROFILE = "db-oracle"
            $script:DB_CONN = "Data Source=oracle:1521/ProjectTemplate;User Id=appuser;Password=AppPass123"
        }
        Default {
            $script:DB_TYPE = "postgresql"
            $script:DB_SERVICE = "postgres"
            $script:DB_PROFILE = "db-postgres"
            $script:DB_CONN = "Host=postgres;Database=ProjectTemplate;Username=postgres;Password=PostgresPass123;Port=5432"
        }
    }

    $script:DB_EVENTS_CONN = "Host=postgres-events;Database=ProjectTemplateEvents;Username=postgres;Password=postgres"
}

# --- MENU PRINCIPAL ---
Write-Host "$($CYAN)================================================================$($NC)"
Write-Host "$($CYAN)          CENTRAL DE COMANDO - ENTERPRISE TEMPLATE 10           $($NC)"
Write-Host "$($CYAN)================================================================$($NC)"
Write-Host "Escolha o ambiente (limpeza profunda incluída):"
Write-Host " "
Write-Host "1) $($GREEN)Docker Compose$($NC) (Ambiente isolado)"
Write-Host "2) $($YELLOW)AppHost (Aspire)$($NC) (Desenvolvimento Local)"
Write-Host "3) $($RED)Limpeza Total$($NC) (Para tudo e libera portas)"
Write-Host "4) Sair"
Write-Host " "
Write-Host -NoNewline "$($CYAN)Escolha uma opção [1-4]: $($NC)"
$choice = Read-Host

switch ($choice) {
    "1" {
        Select-Database
        Clean-Environment

        # Define o nome do projeto baseado no diretório atual (necessário para o docker-compose)
        $env:PROJECT_NAME = Split-Path (Get-Location) -Leaf

        Write-Host "`n$($GREEN)🚀 Iniciando via Docker Compose com $script:DB_TYPE...$($NC)"

        # Monta a lista de profiles do compose com base no banco escolhido,
        # observabilidade (sempre ligada por padrão) e UIs presentes no workspace.
        $composeContent = Get-Content "docker-compose.yml" -Raw
        $profiles = @($script:DB_PROFILE, "observability")
        if ((Test-Path "src/UI/Web/Angular") -and ($composeContent -match "(?m)^  angular-web:")) { $profiles += "web-angular" }
        if ((Test-Path "src/UI/Web/React")   -and ($composeContent -match "(?m)^  react-web:"))   { $profiles += "web-react" }
        if ((Test-Path "src/UI/Web/Vue")     -and ($composeContent -match "(?m)^  vue-web:"))     { $profiles += "web-vue" }
        if ((Test-Path "src/UI/Web/Blazor")  -and ($composeContent -match "(?m)^  blazor-app:"))  { $profiles += "web-blazor" }

        $env:COMPOSE_PROFILES = ($profiles -join ",")
        $env:DB_TYPE = $script:DB_TYPE
        $env:DB_CONNECTION_STRING = $script:DB_CONN
        $env:DB_EVENTS_CONNECTION_STRING = $script:DB_EVENTS_CONN

        Write-Host "$($CYAN)Profiles ativos: $($env:COMPOSE_PROFILES)$($NC)"

        Write-Host "`n$($YELLOW)Compilando serviços...$($NC)"
        docker-compose build
        if ($LASTEXITCODE -ne 0) {
            Write-Host "`n$($RED)Falha ao compilar os serviços.$($NC)"
            Write-Host "$($YELLOW)Dica: Tente rodar 'docker-compose build' manualmente para ver o erro detalhado.$($NC)"
            exit 1
        }

        Write-Host "`n$($GREEN)Subindo containers...$($NC)"
        docker-compose up -d

        Write-Host "`n$($BLUE)----------------------------------------------------------------$($NC)"
        Write-Host "$($GREEN)Ambiente Docker iniciado!$($NC)"
        Write-Host "$($BLUE)----------------------------------------------------------------$($NC)"

        Write-Host "$($CYAN)Aguardando serviços subirem... aqui estão os links de acesso:$($NC)"
        Write-Host "Aplicações (Frontends & API)"
        Write-Host "API (.NET 10):       http://localhost:5000"

        if (Test-Path "src/UI/Web/Angular") { Write-Host "Angular:             http://localhost:4200" }
        if (Test-Path "src/UI/Web/React") { Write-Host "React:               http://localhost:5173" }
        if (Test-Path "src/UI/Web/Vue") { Write-Host "Vue:                 http://localhost:5174" }
        if (Test-Path "src/UI/Web/Blazor") { Write-Host "Blazor WebApp:       http://localhost:5188" }

        Write-Host " "
        Write-Host "Observabilidade e Infraestrutura"

        # Verifica serviços de infra no docker-compose.yml
        $composeContent = Get-Content "docker-compose.yml" -Raw
        if ($composeContent -match "grafana:") { Write-Host "Grafana:             http://localhost:3000" }
        if ($composeContent -match "jaeger:") { Write-Host "Jaeger:              http://localhost:16686" }
        if ($composeContent -match "rabbitmq:") { Write-Host "RabbitMQ Management: http://localhost:15672" }
        if ($composeContent -match "prometheus:") { Write-Host "Prometheus:          http://localhost:9090" }

        Write-Host "$($BLUE)----------------------------------------------------------------$($NC)"

        Write-Host "`n$($YELLOW)⏳ Verificando saúde da API... (isso pode levar um minuto)$($NC)"

        $API_READY = $false
        for ($i=1; $i -le 60; $i++) {
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -ErrorAction SilentlyContinue
                if ($response.StatusCode -eq 200) {
                    $API_READY = $true
                    break
                }
            } catch {}
            Write-Host "." -NoNewline
            Start-Sleep -Seconds 1
        }

        if ($API_READY) {
            Write-Host "`n`n$($GREEN)API está UP e saudável!$($NC)"
        } else {
                Write-Host "`n`n$($RED)A API demorou para responder. Verifique os logs com: 'docker-compose logs -f api'$($NC)"
        }

        Write-Host "$($BLUE)----------------------------------------------------------------$($NC)"
            Write-Host "$($YELLOW)Dica: O ambiente continuará subindo em background.$($NC)"
    }
    "2" {
        Select-Database
        Clean-Environment

        # Verifica node_modules para projetos UI
        foreach ($ui in @("Angular", "React", "Vue")) {
            $uiPath = "src/UI/Web/$ui"
            if ((Test-Path $uiPath) -and -not (Test-Path "$uiPath/node_modules")) {
                Write-Host "$($YELLOW)Instalando dependências para $ui (isso pode levar alguns minutos)...$($NC)"
                Push-Location $uiPath
                npm install
                Pop-Location
            }
        }

        Write-Host "`n$($YELLOW)Iniciando via AppHost (Aspire) com $script:DB_TYPE...$($NC)"
        $env:DB_TYPE = $script:DB_TYPE
        dotnet run --project src/Aspire/AppHost
    }
    "3" {
        Clean-Environment
    }
    "4" {
        Write-Host "`nAté logo!"
        exit
    }
    Default {
        Write-Host "`n$($RED)Opção inválida.$($NC)"
        exit
    }
}
