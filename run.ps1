# --- CONFIGURAÇÃO ---
$ErrorActionPreference = "Stop"

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
    Write-Host "`n$($RED)🛑 Limpando ambiente e liberando portas...$($NC)"
    
    # 1. Para todos os containers do projeto atual
    docker-compose down --remove-orphans 2>$null | Out-Null
    
    # 2. Busca e para QUALQUER container docker que esteja usando as portas críticas
    foreach ($port in $PORTS) {
        $dockerPorts = docker ps -a --format "{{.ID}} {{.Ports}}"
        $cont_ids = $dockerPorts | Select-String ":$port->" | ForEach-Object { $_.ToString().Split(' ')[0] }
        
        if ($cont_ids) {
            foreach ($id in $cont_ids) {
                Write-Host "  - Removendo container teimoso ($id) na porta $port"
                docker rm -f $id 2>$null | Out-Null
            }
        }
        
        # Mata processos locais (ex: dotnet run)
        if ($IsWindows) {
            $connections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
            if ($connections) {
                foreach ($conn in $connections) {
                    $pId = $conn.OwningProcess
                    try {
                        $proc = Get-Process -Id $pId -ErrorAction SilentlyContinue
                        if ($proc) {
                            Write-Host "  - Matando processo na porta $port (PID: $pId - $($proc.ProcessName))"
                            Stop-Process -Id $pId -Force -ErrorAction SilentlyContinue
                        }
                    } catch {}
                }
            }
        } else {
            # Linux / macOS
            $pId = (lsof -t -i:$port 2>$null)
            if ($pId) {
                Write-Host "  - Matando processo na porta $port (PID: $pId)"
                kill -9 $pId 2>$null
            }
        }
    }

    Write-Host "$($YELLOW)⏳ Aguardando liberação total das portas...$($NC)"
    Start-Sleep -Seconds 3

    # 4. Diagnóstico final
    Write-Host "`n$($CYAN)🔍 Verificação final de portas:$($NC)"
    foreach ($port in $PORTS) {
        $portBusy = $false
        $pId = $null
        $procName = "Desconhecido"

        if ($IsWindows) {
            $check = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
            if ($check) {
                $portBusy = $true
                $pId = $check[0].OwningProcess
                $procName = (Get-Process -Id $pId -ErrorAction SilentlyContinue).ProcessName
            }
        } else {
            $pId = (lsof -t -i:$port -sTCP:LISTEN 2>$null)
            if ($pId) {
                $portBusy = $true
                $procName = (ps -p $pId -o comm= 2>$null)
            }
        }

        if ($portBusy) {
            Write-Host "  $($RED)⚠ ATENÇÃO: A porta $port ainda está ocupada por: [$procName] (PID: $pId)$($NC)"
            if ($IsWindows) {
                Write-Host "     Dica: Tente rodar 'Stop-Process -Id $pId -Force' em um terminal como Administrador."
            } else {
                Write-Host "     Dica: Tente rodar 'sudo kill -9 $pId' manualmente."
            }
        }
    }
    
    Write-Host "$($GREEN)✅ Ambiente pronto!$($NC)"
}

function Select-Database {
    Write-Host ""
    Write-Host "$($YELLOW)==========================================$($NC)"
    Write-Host "$($YELLOW)  SELECIONE O BANCO DE DADOS PRINCIPAL    $($NC)"
    Write-Host "$($YELLOW)==========================================$($NC)"
    Write-Host "1) PostgreSQL (Padrão)"
    Write-Host "2) SQL Server"
    Write-Host "3) MySQL"
    Write-Host "4) Oracle"
    Write-Host "=========================================="
    $db_opt = Read-Host "Escolha uma opção [1-4]"

    switch ($db_opt) {
        "2" {
            $script:DB_TYPE = "sqlserver"
            $script:DB_SERVICE = "sqlserver"
            $script:DB_CONN = "Server=sqlserver;Database=ProjectTemplateDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
        }
        "3" {
            $script:DB_TYPE = "mysql"
            $script:DB_SERVICE = "mysql"
            $script:DB_CONN = "Server=mysql;Port=3306;Database=ProjectTemplate;Uid=appuser;Pwd=AppPass123;"
        }
        "4" {
            $script:DB_TYPE = "oracle"
            $script:DB_SERVICE = "oracle"
            $script:DB_CONN = "Data Source=oracle:1521/ProjectTemplate;User Id=appuser;Password=AppPass123"
        }
        Default {
            $script:DB_TYPE = "postgresql"
            $script:DB_SERVICE = "postgres"
            $script:DB_CONN = "Host=postgres;Database=ProjectTemplate;Username=postgres;Password=PostgresPass123;Port=5432"
        }
    }
    
    $script:DB_EVENTS_CONN = "Host=postgres-events;Database=ProjectTemplateEvents;Username=postgres;Password=postgres"
}

# --- MENU PRINCIPAL ---
Write-Host "$($CYAN)================================================================$($NC)"
Write-Host "$($CYAN)          CENTRAL DE COMANDO - ENTERPRISE TEMPLATE 10           $($NC)"
Write-Host "$($CYAN)================================================================$($NC)"
Write-Host "Escolha o ambiente (limpeza profunda inclusa):"
Write-Host " "
Write-Host "1) $($GREEN)Docker Compose$($NC) (Ambiente isolado)"
Write-Host "2) $($YELLOW)AppHost (Aspire)$($NC) (Desenvolvimento Local)"
Write-Host "3) $($RED)Limpeza Total$($NC) (Para tudo e libera portas)"
Write-Host "4) Sair"
Write-Host " "
$choice = Read-Host "Escolha uma opção [1-4]"

switch ($choice) {
    "1" {
        Select-Database
        Clean-Environment
        Write-Host "`n$($GREEN)🚀 Iniciando via Docker Compose com $script:DB_TYPE...$($NC)"
        
        $DB_CONTAINERS = $script:DB_SERVICE
        if ($script:DB_TYPE -ne "postgresql") {
            $DB_CONTAINERS = "$($script:DB_SERVICE) postgres-events"
        } else {
            $DB_CONTAINERS = "postgres postgres-events"
        }

        # Configura variáveis de ambiente para o comando
        $env:DB_TYPE = $script:DB_TYPE
        $env:DB_CONNECTION_STRING = $script:DB_CONN
        $env:DB_EVENTS_CONNECTION_STRING = $script:DB_EVENTS_CONN

        docker-compose up -d --build $DB_CONTAINERS redis rabbitmq mongodb jaeger prometheus grafana api angular-web react-web vue-web blazor-app welcome
        
        Write-Host "`n$($BLUE)----------------------------------------------------------------$($NC)"
        Write-Host "$($GREEN)🚀 Ambiente Docker Iniciado!$($NC)"
        Write-Host "$($BLUE)----------------------------------------------------------------$($NC)"
        
        Write-Host "$($CYAN)Aguardando serviços subirem... aqui estão os links de acesso:$($NC)"
        Write-Host "🚀 Aplicações (Frontends & API)"
        Write-Host "API (.NET 10):       http://localhost:5000"
        Write-Host "Angular:             http://localhost:4200"
        Write-Host "React:               http://localhost:5173"
        Write-Host "Vue:                 http://localhost:5174"
        Write-Host "Blazor WebApp:       http://localhost:5188"
        Write-Host " "
        Write-Host "📊 Observabilidade & Infraestrutura"
        Write-Host "Grafana:             http://localhost:3000"
        Write-Host "Jaeger:              http://localhost:16686"
        Write-Host "RabbitMQ Management: http://localhost:15672"
        Write-Host "Prometheus:          http://localhost:9090"
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
            Write-Host "`n`n$($GREEN)✅ API está UP e Saudável!$($NC)"
        } else {
            Write-Host "`n`n$($RED)⚠ A API demorou para responder. Verifique os logs com: 'docker-compose logs -f api'$($NC)"
        }
        
        Write-Host "$($BLUE)----------------------------------------------------------------$($NC)"
        Write-Host "$($YELLOW)Dica: O ambiente continuará subindo em background.$($NC)"
    }
    "2" {
        Select-Database
        Clean-Environment
        Write-Host "`n$($YELLOW)🔥 Iniciando via AppHost (Aspire) com $script:DB_TYPE...$($NC)"
        $env:DB_TYPE = $script:DB_TYPE
        dotnet run --project src/AppHost
    }
    "3" {
        Clean-Environment
    }
    "4" {
        Write-Host "`nAté logo!"
        exit
    }
    Default {
        Write-Host "`n$($RED)❌ Opção inválida.$($NC)"
        exit
    }
}
