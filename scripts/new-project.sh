#!/bin/bash

# Script interativo para criar novo projeto a partir do template
# Modo interativo:     ./new-project.sh
# Com nome:            ./new-project.sh MeuProjeto
# Modo não-interativo: ./new-project.sh MeuProjeto --database PostgreSQL --mongodb yes --queue yes --storage Azure --telemetry yes --event-sourcing no --git-init yes

set -e

# ============================================================
# Colors & Helpers
# ============================================================

CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
MAGENTA='\033[0;35m'
RED='\033[0;31m'
WHITE='\033[1;37m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

write_step() {
    echo -e "  $1 $2"
}

write_header() {
    local text="$1"
    local len=${#text}
    local border
    border=$(printf '═%.0s' $(seq 1 $((len + 4))))
    echo ""
    echo -e "  ${CYAN}╔${border}╗${NC}"
    echo -e "  ${CYAN}║  ${text}  ║${NC}"
    echo -e "  ${CYAN}╚${border}╝${NC}"
    echo ""
}

write_section() {
    echo ""
    echo -e "  ${YELLOW}── $1 ──${NC}"
    echo ""
}

show_menu() {
    local title="$1"
    shift
    local default="$1"
    shift
    local options=("$@")
    local count=${#options[@]}

    echo -e "  ${WHITE}$title${NC}" >&2
    echo "" >&2

    for ((i=0; i<count; i++)); do
        local num=$((i+1))
        if [ "$num" -eq "$default" ]; then
            echo -e "    [${GREEN}$num${NC}] ${options[$i]} ${GRAY}(padrão)${NC}" >&2
        else
            echo "    [$num] ${options[$i]}" >&2
        fi
    done

    echo "" >&2
    read -rp "  Escolha (1-$count) [padrão: $default]: " choice

    if [ -z "$choice" ]; then
        echo "$default"
        return
    fi

    if [[ "$choice" =~ ^[0-9]+$ ]] && [ "$choice" -ge 1 ] && [ "$choice" -le "$count" ]; then
        echo "$choice"
        return
    fi

    echo -e "  ${YELLOW}Opção inválida. Usando padrão: $default${NC}" >&2
    echo "$default"
}

show_yesno() {
    local title="$1"
    local default_is_yes="${2:-false}"

    local result
    if [ "$default_is_yes" = "true" ]; then
        result=$(show_menu "$title" 1 "Sim" "Não")
        [ "$result" -eq 1 ]
    else
        result=$(show_menu "$title" 1 "Não" "Sim")
        [ "$result" -eq 2 ]
    fi
}

# ============================================================
# Parse Arguments
# ============================================================

if [ -z "$1" ]; then
    echo ""
    echo -e "  ${CYAN}╔══════════════════════════════════════════════════════════╗${NC}"
    echo -e "  ${CYAN}║  Criar Novo Projeto a partir do Template                 ║${NC}"
    echo -e "  ${CYAN}╚══════════════════════════════════════════════════════════╝${NC}"
    echo ""

    while true; do
        read -rp "  Qual o nome do projeto: " PROJECT_NAME
        if [ -n "$PROJECT_NAME" ]; then
            break
        fi
        echo -e "  ${RED}Nome do projeto não pode ser vazio.${NC}"
    done
else
    PROJECT_NAME=$1
    shift
fi

# Validate project name
if ! echo "$PROJECT_NAME" | grep -qE '^[a-zA-Z][a-zA-Z0-9._-]*$'; then
    echo -e "  ${RED}Erro: Nome do projeto inválido. Use letras, números, pontos, hífens ou underscores. Deve começar com letra.${NC}"
    exit 1
fi

# Parse optional arguments
OPT_DATABASE=""

OPT_QUEUE=""
OPT_MONGODB=""
OPT_STORAGE=""
OPT_TELEMETRY=""
OPT_GITINIT=""
OPT_UI_MOBILE=""
OPT_UI_WEB=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --database) OPT_DATABASE="$2"; shift 2 ;;

        --queue) OPT_QUEUE="$2"; shift 2 ;;
        --storage) OPT_STORAGE="$2"; shift 2 ;;
        --telemetry) OPT_TELEMETRY="$2"; shift 2 ;;
        --git-init) OPT_GITINIT="$2"; shift 2 ;;
        --ui-mobile) OPT_UI_MOBILE="$2"; shift 2 ;;
        --ui-web) OPT_UI_WEB="$2"; shift 2 ;;
        *) echo "Opção desconhecida: $1"; exit 1 ;;
    esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATE_DIR="$(dirname "$SCRIPT_DIR")"
TARGET_DIR="$(dirname "$TEMPLATE_DIR")/$PROJECT_NAME"

# Detect interactive mode
IS_INTERACTIVE=true
if [ -n "$OPT_DATABASE" ] || [ -n "$OPT_MONGODB" ] || [ -n "$OPT_QUEUE" ] || \
   [ -n "$OPT_STORAGE" ] || [ -n "$OPT_TELEMETRY" ] || [ -n "$OPT_EVENTSOURCING" ] || \
   [ -n "$OPT_GITINIT" ]; then
    IS_INTERACTIVE=false
fi

# ============================================================
# Collect Choices
# ============================================================

if [ "$IS_INTERACTIVE" = true ]; then
    write_header "Novo Projeto: $PROJECT_NAME"

    write_section "Banco de Dados"
    DB_CHOICE=$(show_menu "Qual banco de dados utilizar?" 1 \
        "PostgreSQL" \
        "SQL Server" \
        "Oracle" \
        "MySQL")

    case $DB_CHOICE in
        1) DATABASE="PostgreSQL" ;;
        2) DATABASE="SqlServer" ;;
        3) DATABASE="Oracle" ;;
        4) DATABASE="MySQL" ;;
    esac


    MONGODB="yes"

    write_section "Mensageria"
    if show_yesno "Habilitar RabbitMQ (fila de mensagens)?"; then
        QUEUE="yes"
    else
        QUEUE="no"
    fi

    write_section "Storage em Nuvem"
    STORAGE_CHOICE=$(show_menu "Qual provider de storage em nuvem?" 1 \
        "Nenhum (configurar depois)" \
        "Azure Blob Storage" \
        "AWS S3" \
        "Google Cloud Storage")

    case $STORAGE_CHOICE in
        1) STORAGE="None" ;;
        2) STORAGE="Azure" ;;
        3) STORAGE="Aws" ;;
        4) STORAGE="Google" ;;
    esac

    write_section "Observabilidade"
    if show_yesno "Habilitar Telemetria (Jaeger + Prometheus + Grafana)?"; then
        TELEMETRY="yes"
    else
        TELEMETRY="no"
    fi

    EVENTSOURCING="yes"

    write_section "UI - Web"
    WEB_CHOICE=$(show_menu "Quais tecnologias Web deseja manter?" 1 \
        "Nenhum" \
        "Angular" \
        "Blazor" \
        "React" \
        "Vue" \
        "Todas")
    case $WEB_CHOICE in
        1) UI_WEB="none" ;;
        2) UI_WEB="angular" ;;
        3) UI_WEB="blazor" ;;
        4) UI_WEB="react" ;;
        5) UI_WEB="vue" ;;
        6) UI_WEB="all" ;;
    esac

    write_section "UI - Mobile"
    MOBILE_CHOICE=$(show_menu "Quais tecnologias Mobile deseja manter?" 1 \
        "Nenhuma" \
        "Flutter" \
        "MAUI" \
        "React Native" \
        "Todas")
    case $MOBILE_CHOICE in
        1) UI_MOBILE="none" ;;
        2) UI_MOBILE="flutter" ;;
        3) UI_MOBILE="maui" ;;
        4) UI_MOBILE="reactnative" ;;
        5) UI_MOBILE="all" ;;
    esac

    write_section "Git"
    if show_yesno "Inicializar repositório Git?" "true"; then
        GITINIT="yes"
    else
        GITINIT="no"
    fi

    # ── Summary ──
    echo ""
    echo -e "  ${CYAN}╔══════════════════════════════════════════════════════════╗${NC}"
    echo -e "  ${CYAN}║  Resumo das Configurações                                ║${NC}"
    echo -e "  ${CYAN}╠══════════════════════════════════════════════════════════╣${NC}"
    printf "  ${CYAN}  Projeto:        %-38s${NC}\n" "$PROJECT_NAME"
    printf "  ${CYAN}  Banco de Dados: %-38s${NC}\n" "$DATABASE"

    printf "  ${CYAN}  MongoDB:        %-38s${NC}\n" "$MONGODB"
    printf "  ${CYAN}  RabbitMQ:       %-38s${NC}\n" "$QUEUE"
    printf "  ${CYAN}  Storage:        %-38s${NC}\n" "$STORAGE"
    printf "  ${CYAN}  Telemetria:     %-38s${NC}\n" "$TELEMETRY"
    printf "  ${CYAN}  Git Init:       %-38s${NC}\n" "$GITINIT"
    printf "  ${CYAN}  UI Mobile:      %-38s${NC}\n" "$UI_MOBILE"
    printf "  ${CYAN}  UI Web:         %-38s${NC}\n" "$UI_WEB"
    echo -e "  ${CYAN}╚══════════════════════════════════════════════════════════╝${NC}"
    echo ""

    if ! show_yesno "Confirmar e criar projeto?" "true"; then
        echo ""
        echo -e "  ${RED}Operação cancelada.${NC}"
        exit 0
    fi
else
    # Apply defaults for non-interactive mode
    DATABASE="${OPT_DATABASE:-PostgreSQL}"

    MONGODB="yes"
    QUEUE="${OPT_QUEUE:-no}"
    STORAGE="${OPT_STORAGE:-None}"
    TELEMETRY="${OPT_TELEMETRY:-no}"
    GITINIT="${OPT_GITINIT:-yes}"
    UI_MOBILE="${OPT_UI_MOBILE:-none}"
    UI_WEB="${OPT_UI_WEB:-none}"
fi

# Normalize to lowercase for comparisons
MONGODB=$(echo "$MONGODB" | tr '[:upper:]' '[:lower:]')
QUEUE=$(echo "$QUEUE" | tr '[:upper:]' '[:lower:]')
TELEMETRY=$(echo "$TELEMETRY" | tr '[:upper:]' '[:lower:]')
EVENTSOURCING=$(echo "$EVENTSOURCING" | tr '[:upper:]' '[:lower:]')
GITINIT=$(echo "$GITINIT" | tr '[:upper:]' '[:lower:]')

# ============================================================
# Project Creation
# ============================================================

echo ""
write_step "🚀" "Criando projeto: $PROJECT_NAME"
write_step "📁" "Template: $TEMPLATE_DIR"
write_step "📁" "Destino:  $TARGET_DIR"
echo ""

if [ -d "$TARGET_DIR" ]; then
    echo -e "  ${RED}❌ Erro: Diretório $TARGET_DIR já existe${NC}"
    exit 1
fi

# Copy template
write_step "📋" "Copiando template..."
cp -r "$TEMPLATE_DIR" "$TARGET_DIR"

cd "$TARGET_DIR"

# Cleanup
write_step "🧹" "Limpando arquivos desnecessários..."
rm -rf .git .gitignore
# Remove scripts exclusivos do template
rm -f scripts/new-project.ps1 scripts/new-project.sh scripts/new-project.bat
rm -f scripts/windows/test-all-databases.ps1 scripts/linux/test-all-databases.sh
find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

# Rename solution and references
write_step "✏️" "Renomeando referências ProjectTemplate → $PROJECT_NAME..."
mv ProjectTemplate.sln "$PROJECT_NAME.sln"

find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "*.yml" -o -name "*.yaml" -o -name "*.md" -o -name "*.props" \) \
    -exec sed -i "s/ProjectTemplate/$PROJECT_NAME/g" {} +

# ============================================================
# UI Cleanup
# ============================================================

write_step "🧹" "Limpando frameworks de UI não selecionados..."

# -- Mobile --
if [ "$UI_MOBILE" = "none" ]; then
    rm -rf src/UI/Mobile
    rm -f run-mobile.sh run-mobile.ps1 build-mobile-all.sh build-mobile-all.ps1
    dotnet sln "$PROJECT_NAME.sln" remove src/UI/Mobile/MauiApp/MauiApp.csproj 2>/dev/null || true
elif [ "$UI_MOBILE" != "all" ]; then
    if [ "$UI_MOBILE" != "flutter" ]; then
        rm -rf src/UI/Mobile/FlutterApp
        sed -i '/# Flutter/,/popd/d' build-mobile-all.sh 2>/dev/null || true
        sed -i '/run_flutter/,/^}/d' run-mobile.sh 2>/dev/null || true
        sed -i '/1) ${CYAN}Flutter/,/;;/d' run-mobile.sh 2>/dev/null || true
    fi
    if [ "$UI_MOBILE" != "maui" ]; then
        rm -rf src/UI/Mobile/MauiApp
        sed -i '/# MAUI/,/MauiApp.csproj/d' build-mobile-all.sh 2>/dev/null || true
        sed -i '/run_maui/,/^}/d' run-mobile.sh 2>/dev/null || true
        sed -i '/3) ${CYAN}MAUI/,/;;/d' run-mobile.sh 2>/dev/null || true
        dotnet sln "$PROJECT_NAME.sln" remove src/UI/Mobile/MauiApp/MauiApp.csproj 2>/dev/null || true
    fi
    if [ "$UI_MOBILE" != "reactnative" ]; then
        rm -rf src/UI/Mobile/ReactNativeApp
        sed -i '/# React Native/,/popd/d' build-mobile-all.sh 2>/dev/null || true
        sed -i '/run_react_native/,/^}/d' run-mobile.sh 2>/dev/null || true
        sed -i '/2) ${CYAN}React Native/,/;;/d' run-mobile.sh 2>/dev/null || true
    fi
fi

# -- Web --
ASPIRE_PROGRAM="src/Aspire/AppHost/Program.cs"

if [ "$UI_WEB" = "none" ]; then
    rm -rf src/UI/Web
    rm -f build-web-all.sh build-web-all.ps1
    dotnet sln "$PROJECT_NAME.sln" remove src/UI/Web/Blazor/WebApp/App/App.csproj 2>/dev/null || true
    dotnet sln "$PROJECT_NAME.sln" remove src/UI/Web/Blazor/WebApp/App.Client/App.Client.csproj 2>/dev/null || true
    dotnet sln "$PROJECT_NAME.sln" remove src/UI/Web/Blazor/Wasm/BlazorWasm.csproj 2>/dev/null || true
    sed -i '/UI.Web.Blazor.WebApp.App.App.csproj/d' src/Aspire/AppHost/AppHost.csproj
    # Remove all web from Aspire
    sed -i '/\/\/ Web Projects/,/builder.AddNpmApp/d' "$ASPIRE_PROGRAM"
elif [ "$UI_WEB" != "all" ]; then
    # Angular
    if [ "$UI_WEB" != "angular" ]; then
        rm -rf src/UI/Web/Angular
        sed -i '/# Angular/,/popd/d' build-web-all.sh 2>/dev/null || true
        sed -i '/builder.AddNpmApp("angular-web"/,/ExternalHttpEndpoints();/d' "$ASPIRE_PROGRAM"
    fi
    # Blazor
    if [ "$UI_WEB" != "blazor" ]; then
        rm -rf src/UI/Web/Blazor
        sed -i '/# Blazor/,/App.csproj/d' build-web-all.sh 2>/dev/null || true
        dotnet sln "$PROJECT_NAME.sln" remove src/UI/Web/Blazor/WebApp/App/App.csproj 2>/dev/null || true
        dotnet sln "$PROJECT_NAME.sln" remove src/UI/Web/Blazor/WebApp/App.Client/App.Client.csproj 2>/dev/null || true
        dotnet sln "$PROJECT_NAME.sln" remove src/UI/Web/Blazor/Wasm/BlazorWasm.csproj 2>/dev/null || true
        sed -i '/UI.Web.Blazor.WebApp.App.App.csproj/d' src/Aspire/AppHost/AppHost.csproj
        sed -i '/builder.AddProject<Projects.App>("blazor-app")/,/ExternalHttpEndpoints();/d' "$ASPIRE_PROGRAM"
    fi
    # React
    if [ "$UI_WEB" != "react" ]; then
        rm -rf src/UI/Web/React
        sed -i '/# React/,/popd/d' build-web-all.sh 2>/dev/null || true
        sed -i '/builder.AddNpmApp("react-web"/,/ExternalHttpEndpoints();/d' "$ASPIRE_PROGRAM"
    fi
    # Vue
    if [ "$UI_WEB" != "vue" ]; then
        rm -rf src/UI/Web/Vue
        sed -i '/# Vue/,/popd/d' build-web-all.sh 2>/dev/null || true
        sed -i '/builder.AddNpmApp("vue-web"/,/ExternalHttpEndpoints();/d' "$ASPIRE_PROGRAM"
    fi
fi

# Update Aspire launchSettings.json with correct DB_TYPE
ASPIRE_LAUNCH="src/Aspire/AppHost/Properties/launchSettings.json"
if [ -f "$ASPIRE_LAUNCH" ]; then
    write_step "🚀" "Configurando DB_TYPE no Aspire launchSettings.json..."
    # Add DB_TYPE to environmentVariables in both http and https profiles
    sed -i "/\"environmentVariables\": {/a \ \ \ \ \ \ \ \ \"DB_TYPE\": \"${DATABASE,,}\"," "$ASPIRE_LAUNCH"
fi

# ============================================================
# Configure appsettings.json
# ============================================================

write_step "⚙️" "Configurando appsettings.json..."

APPSETTINGS="$TARGET_DIR/src/Server/Api/appsettings.json"

# Helper: update JSON value using sed (works without jq)
json_set() {
    local key="$1"
    local value="$2"
    local file="$3"
    # Handle boolean and empty string values
    if [ "$value" = "true" ] || [ "$value" = "false" ]; then
        sed -i "s|\"$key\": *[^,}]*|\"$key\": $value|g" "$file"
    elif [ "$value" = "[]" ]; then
        sed -i "s|\"$key\": *\[[^]]*\]|\"$key\": []|g" "$file"
    elif echo "$value" | grep -q '^\['; then
        sed -i "s|\"$key\": *\[[^]]*\]|\"$key\": $value|g" "$file"
    else
        sed -i "s|\"$key\": *\"[^\"]*\"|\"$key\": \"$value\"|g" "$file"
    fi
}

# -- Database --
json_set "DatabaseType" "$DATABASE" "$APPSETTINGS"

case $DATABASE in
    "SqlServer")
        DB_CONN="Server=localhost,1433;Database=$PROJECT_NAME;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
        ;;
    "Oracle")
        DB_CONN="User Id=appuser;Password=AppPass123;Data Source=localhost:1521/FREEPDB1;"
        ;;
    "PostgreSQL")
        DB_CONN="Host=localhost;Port=5433;Database=$PROJECT_NAME;Username=postgres;Password=PostgresPass123;"
        ;;
    "MySQL")
        DB_CONN="Server=localhost;Port=3306;Database=$PROJECT_NAME;User=appuser;Password=AppPass123;"
        ;;
esac

# Set connection string — need to target the Database section specifically
# Use python/perl if available for reliable JSON editing, fallback to sed
if command -v python3 &>/dev/null; then
    python3 -c "
import json, sys
with open('$APPSETTINGS', 'r') as f:
    data = json.load(f)
infra = data['AppSettings']['Infrastructure']
infra['Database']['DatabaseType'] = '$DATABASE'
infra['Database']['ConnectionString'] = '$DB_CONN'

if '$QUEUE' == 'yes':
    infra['RabbitMQ']['ConnectionString'] = 'amqp://guest:guest@localhost:5672/'
infra['MongoDB']['ConnectionString'] = 'mongodb://admin:admin@localhost:27017/$PROJECT_NAME'

if '$STORAGE' != 'None':
    infra['Storage']['Provider'] = '$STORAGE'

if '$TELEMETRY' == 'yes':
    infra['Telemetry']['Enabled'] = True
    infra['Telemetry']['Providers'] = ['Jaeger', 'Prometheus']
else:
    infra['Telemetry']['Enabled'] = False

if '$EVENTSOURCING' == 'yes':
    infra['EventSourcing']['Enabled'] = True
    infra['EventSourcing']['ConnectionString'] = 'Host=localhost;Database=' + '$PROJECT_NAME' + 'Events;Username=postgres;Password=postgres;Port=5434'
else:
    infra['EventSourcing']['Enabled'] = False
with open('$APPSETTINGS', 'w') as f:
    json.dump(data, f, indent=2, ensure_ascii=False)
"
else
    # Fallback: use sed for basic edits
    json_set "DatabaseType" "$DATABASE" "$APPSETTINGS"

    # Database ConnectionString — target the first occurrence (Database section)
    sed -i "0,/\"ConnectionString\": *\"[^\"]*\"/s|\"ConnectionString\": *\"[^\"]*\"|\"ConnectionString\": \"$DB_CONN\"|" "$APPSETTINGS"



    # RabbitMQ
    if [ "$QUEUE" = "yes" ]; then
        json_set "ConnectionString" "amqp://guest:guest@localhost:5672/" "$APPSETTINGS"
    fi

    # MongoDB
    if [ "$MONGODB" = "yes" ]; then
      json_set "ConnectionString" "mongodb://admin:admin@localhost:27017/$PROJECT_NAME" "$APPSETTINGS"
    fi

    # Storage
    if [ "$STORAGE" != "None" ]; then
        json_set "Provider" "$STORAGE" "$APPSETTINGS"
    fi

    # Telemetry
    if [ "$TELEMETRY" = "yes" ]; then
        json_set "Enabled" "true" "$APPSETTINGS"
        sed -i 's|"Providers": *\[[^]]*\]|"Providers": ["Jaeger", "Prometheus"]|' "$APPSETTINGS"
    fi

    # Event Sourcing
    if [ "$EVENTSOURCING" = "yes" ]; then
        # Use a more specific replacement for EventSourcing ConnectionString to avoid global sed issues
        sed -i "/\"EventSourcing\":/,/}/ s|\"Enabled\": *[^,}]*|\"Enabled\": true|" "$APPSETTINGS"
        sed -i "/\"EventSourcing\":/,/}/ s|\"ConnectionString\": *\"[^\"]*\"|\"ConnectionString\": \"Host=localhost;Database=${PROJECT_NAME}Events;Username=postgres;Password=postgres;Port=5434\"|" "$APPSETTINGS"
    else
        sed -i "/\"EventSourcing\":/,/}/ s|\"Enabled\": *[^,}]*|\"Enabled\": false|" "$APPSETTINGS"
    fi
fi

# Remove database-specific appsettings files that don't match
declare -A DB_FILES=(
    ["SqlServer"]="appsettings.SqlServer.json"
    ["Oracle"]="appsettings.Oracle.json"
    ["PostgreSQL"]="appsettings.PostgreSQL.json"
    ["MySQL"]="appsettings.MySQL.json"
)

for db in "${!DB_FILES[@]}"; do
    filepath="$TARGET_DIR/src/Server/Api/${DB_FILES[$db]}"
    if [ -f "$filepath" ]; then
        if [ "$DATABASE" != "$db" ]; then
            rm -f "$filepath"
        fi
    fi
done

# ============================================================
# Enable optional features in Program.cs
# ============================================================

PROGRAM_CS="$TARGET_DIR/src/Server/Api/Program.cs"

write_step "🍃" "Habilitando MongoDB no Program.cs..."
sed -i 's|^// \(builder\.Services\.AddMongo<Program>();\)|\1|' "$PROGRAM_CS"
write_step "🌱" "Habilitando seed inicial do MongoDB no Program.cs..."
sed -i 's|^\([[:space:]]*\)// \(await MongoDbSeeder\.SeedAsync(scope\.ServiceProvider);\)|\1\2|' "$PROGRAM_CS"

if [ "$QUEUE" = "yes" ]; then
    write_step "📨" "Habilitando RabbitMQ no Program.cs..."
    sed -i 's|^// \(builder\.Services\.AddRabbitMq();\)|\1|' "$PROGRAM_CS"
fi

if [ "$STORAGE" != "None" ]; then
    write_step "☁️" "Habilitando Storage no Program.cs..."
    sed -i 's|^// \(builder\.Services\.AddStorage<Program>();\)|\1|' "$PROGRAM_CS"
fi

# ============================================================
# Generate docker-compose.yml
# ============================================================

NEEDS_COMPOSE=false
if [ "$DATABASE" != "InMemory" ] || [ "$MONGODB" = "yes" ] || [ "$QUEUE" = "yes" ] || \
   [ "$TELEMETRY" = "yes" ] || [ "$EVENTSOURCING" = "yes" ]; then
    NEEDS_COMPOSE=true
fi

if [ "$NEEDS_COMPOSE" = true ]; then
    write_step "🐳" "Gerando docker-compose.yml..."

    COMPOSE_FILE="$TARGET_DIR/docker-compose.yml"
    cat > "$COMPOSE_FILE" << 'COMPOSE_HEADER'
services:
COMPOSE_HEADER

    # ── Database containers ──
    case $DATABASE in
        "SqlServer")
            cat >> "$COMPOSE_FILE" << COMPOSE_DB
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

COMPOSE_DB
            VOLUMES_LIST="  sqlserver-data:"
            ;;
        "Oracle")
            cat >> "$COMPOSE_FILE" << COMPOSE_DB
  oracle:
    image: gvenzl/oracle-free:latest
    container_name: oracle
    environment:
      - ORACLE_PASSWORD=OraclePass123
      - ORACLE_DATABASE=$PROJECT_NAME
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

COMPOSE_DB
            VOLUMES_LIST="  oracle-data:"
            ;;
        "PostgreSQL")
            cat >> "$COMPOSE_FILE" << COMPOSE_DB
  postgres:
    image: postgres:17-alpine
    container_name: postgres
    environment:
      - POSTGRES_DB=$PROJECT_NAME
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

COMPOSE_DB
            VOLUMES_LIST="  postgres-data:"
            ;;
        "MySQL")
            cat >> "$COMPOSE_FILE" << COMPOSE_DB
  mysql:
    image: mysql:8
    container_name: mysql
    environment:
      - MYSQL_ROOT_PASSWORD=MySqlPass123
      - MYSQL_DATABASE=$PROJECT_NAME
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

COMPOSE_DB
            VOLUMES_LIST="  mysql-data:"
            ;;
        *)
            VOLUMES_LIST=""
            ;;
    esac



    # ── MongoDB ──
    if [ "$MONGODB" = "yes" ]; then
        cat >> "$COMPOSE_FILE" << COMPOSE_MONGO
  mongo:
    image: mongo:7
    container_name: mongo
    environment:
      - MONGO_INITDB_ROOT_USERNAME=admin
      - MONGO_INITDB_ROOT_PASSWORD=admin
      - MONGO_INITDB_DATABASE=$PROJECT_NAME
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

COMPOSE_MONGO
        VOLUMES_LIST="$VOLUMES_LIST
  mongo-data:"
    fi

    # ── RabbitMQ ──
    if [ "$QUEUE" = "yes" ]; then
        cat >> "$COMPOSE_FILE" << 'COMPOSE_RABBIT'
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

COMPOSE_RABBIT
        VOLUMES_LIST="$VOLUMES_LIST
  rabbitmq-data:"
    fi

    # ── Telemetry (Jaeger + Prometheus + Grafana) ──
    if [ "$TELEMETRY" = "yes" ]; then
        cat >> "$COMPOSE_FILE" << 'COMPOSE_TELEMETRY'
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

COMPOSE_TELEMETRY
        VOLUMES_LIST="$VOLUMES_LIST
  prometheus-data:
  grafana-data:"
    fi

    # ── Event Sourcing (dedicated PostgreSQL) ──
    if [ "$EVENTSOURCING" = "yes" ]; then
        cat >> "$COMPOSE_FILE" << COMPOSE_ES
  postgres-events:
    image: postgres:17-alpine
    container_name: postgres-events
    environment:
      - POSTGRES_DB=${PROJECT_NAME}Events
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5434:5432"
    volumes:
      - postgres-events-data:/var/lib/postgresql/data
    networks:
      - app-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

COMPOSE_ES
        VOLUMES_LIST="$VOLUMES_LIST
  postgres-events-data:"
    fi

    # ── Redis (Always needed by API) ──
    cat >> "$COMPOSE_FILE" << 'COMPOSE_REDIS'
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

COMPOSE_REDIS

    # ── API ──
    cat >> "$COMPOSE_FILE" << COMPOSE_API
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
      - AppSettings__Infrastructure__Database__DatabaseType=$DATABASE
      - AppSettings__Infrastructure__Database__ConnectionString=$(
          case "$DATABASE" in
            "SqlServer")  echo "Server=sqlserver,1433;Database=${PROJECT_NAME};User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;" ;;
            "Oracle")     echo "User Id=appuser;Password=AppPass123;Data Source=oracle:1521/FREEPDB1;" ;;
            "PostgreSQL")  echo "Host=postgres;Port=5432;Database=${PROJECT_NAME};Username=postgres;Password=PostgresPass123;" ;;
            "MySQL")       echo "Server=mysql;Port=3306;Database=${PROJECT_NAME};User=appuser;Password=AppPass123;" ;;
          esac
        )
      - AppSettings__Infrastructure__EventSourcing__Enabled=$( [ "$EVENTSOURCING" = "yes" ] && echo "true" || echo "false" )
      - AppSettings__Infrastructure__EventSourcing__ConnectionString=Host=postgres-events;Database=${PROJECT_NAME}Events;Username=postgres;Password=postgres;Port=5432
      - AppSettings__Infrastructure__Redis__ConnectionString=redis:6379
      - AppSettings__Infrastructure__RabbitMQ__ConnectionString=amqp://guest:guest@rabbitmq:5672
      - AppSettings__Infrastructure__MongoDB__ConnectionString=mongodb://admin:admin@mongo:27017/$PROJECT_NAME
      - AppSettings__Infrastructure__Telemetry__Enabled=$( [ "$TELEMETRY" = "yes" ] && echo "true" || echo "false" )
      - AppSettings__Infrastructure__Telemetry__Jaeger__Host=jaeger
      - AppSettings__Infrastructure__Telemetry__Jaeger__Port=4317
      - AppSettings__Infrastructure__Telemetry__Jaeger__UseGrpc=true
    depends_on:
      redis: { condition: service_healthy }
COMPOSE_API

    if [ "$QUEUE" = "yes" ]; then
        echo "      rabbitmq: { condition: service_healthy }" >> "$COMPOSE_FILE"
    fi
    if [ "$MONGODB" = "yes" ]; then
        echo "      mongo: { condition: service_healthy }" >> "$COMPOSE_FILE"
    fi
    if [ "$TELEMETRY" = "yes" ]; then
        echo "      jaeger: { condition: service_healthy }" >> "$COMPOSE_FILE"
        echo "      prometheus: { condition: service_healthy }" >> "$COMPOSE_FILE"
    fi

    cat >> "$COMPOSE_FILE" << 'COMPOSE_API_END'
    networks:
      - app-network

COMPOSE_API_END

    # ── Web Projects ──
    if [ "$UI_WEB" != "none" ]; then
        if [ "$UI_WEB" = "all" ] || [ "$UI_WEB" = "angular" ]; then
            cat >> "$COMPOSE_FILE" << 'COMPOSE_WEB_ANGULAR'
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

COMPOSE_WEB_ANGULAR
        fi
        if [ "$UI_WEB" = "all" ] || [ "$UI_WEB" = "react" ]; then
            cat >> "$COMPOSE_FILE" << 'COMPOSE_WEB_REACT'
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

COMPOSE_WEB_REACT
        fi
        if [ "$UI_WEB" = "all" ] || [ "$UI_WEB" = "vue" ]; then
            cat >> "$COMPOSE_FILE" << 'COMPOSE_WEB_VUE'
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

COMPOSE_WEB_VUE
        fi
        if [ "$UI_WEB" = "all" ] || [ "$UI_WEB" = "blazor" ]; then
            cat >> "$COMPOSE_FILE" << 'COMPOSE_WEB_BLAZOR'
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

COMPOSE_WEB_BLAZOR
        fi
    fi

    # ── Networks ──
    cat >> "$COMPOSE_FILE" << 'COMPOSE_NETWORKS_FOOTER'
networks:
  app-network:
    driver: bridge
COMPOSE_NETWORKS_FOOTER

    # ── Volumes ──
    if [ -n "$VOLUMES_LIST" ]; then
        echo "" >> "$COMPOSE_FILE"
        echo "volumes:" >> "$COMPOSE_FILE"
        echo "$VOLUMES_LIST" >> "$COMPOSE_FILE"
    fi
fi

# Remove template's original docker-compose files if no containers needed
if [ "$NEEDS_COMPOSE" = false ]; then
    rm -f "$TARGET_DIR/docker-compose.yml" "$TARGET_DIR/compose-observability.yml"
else
    rm -f "$TARGET_DIR/compose-observability.yml"
fi

# ============================================================
# Generate clean scripts/README.md (without template-only sections)
# ============================================================

cat > "$TARGET_DIR/scripts/README.md" << 'SCRIPTS_README'
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
SCRIPTS_README

# ============================================================
# Git init
# ============================================================

if [ "$GITINIT" = "yes" ]; then
    write_step "📦" "Inicializando repositório Git..."
    git init --quiet 2>/dev/null || true

    cat > "$TARGET_DIR/.gitignore" << 'GITIGNORE'
## .NET
bin/
obj/
*.user
*.suo
*.cache
*.dll
*.pdb

## IDE
.vs/
.vscode/
*.swp

## OS
Thumbs.db
.DS_Store

## Secrets
appsettings.*.local.json
GITIGNORE
fi

# ============================================================
# Final Summary
# ============================================================

echo ""
echo -e "  ${GREEN}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "  ${GREEN}║  ✅ Projeto criado com sucesso!                          ║${NC}"
echo -e "  ${GREEN}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "  ${CYAN}Próximos passos:${NC}"
echo ""

STEP=1

echo -e "  $STEP. cd $PROJECT_NAME"
STEP=$((STEP+1))

if [ "$NEEDS_COMPOSE" = true ]; then
    echo -e "  $STEP. docker-compose up -d"
    STEP=$((STEP+1))

    if [ "$DATABASE" = "Oracle" ]; then
        echo -e "     ${GRAY}⏳ Oracle pode levar ~60s para iniciar${NC}"
    fi
fi

echo -e "  $STEP. dotnet restore"
STEP=$((STEP+1))

echo -e "  $STEP. dotnet build"
STEP=$((STEP+1))

if [ "$DATABASE" != "InMemory" ]; then
    echo -e "  $STEP. dotnet ef migrations add InitialCreate --project src/Server/Data --startup-project src/Server/Api"
    STEP=$((STEP+1))
    echo -e "  $STEP. dotnet ef database update --project src/Server/Data --startup-project src/Server/Api"
    STEP=$((STEP+1))
fi

echo -e "  $STEP. dotnet run --project src/Server/Api"
STEP=$((STEP+1))

echo ""
echo -e "  ${CYAN}Serviços configurados:${NC}"

echo -e "    📊 Banco de Dados: $DATABASE"
if [ "$DATABASE" != "InMemory" ]; then
    echo -e "       ${GRAY}Connection String configurada em appsettings.json${NC}"
fi




if [ "$MONGODB" = "yes" ]; then
    echo -e "    🍃 MongoDB: habilitado"
  echo -e "       ${GRAY}Connection: mongodb://admin:admin@localhost:27017/$PROJECT_NAME${NC}"
fi

if [ "$QUEUE" = "yes" ]; then
    echo -e "    📨 RabbitMQ: habilitado"
    echo -e "       ${GRAY}Management UI: http://localhost:15672 (guest/guest)${NC}"
fi

if [ "$STORAGE" != "None" ]; then
    echo -e "    ☁️  Storage: $STORAGE"
    echo -e "       ${GRAY}Configure credenciais em appsettings.json → Infrastructure → Storage → $STORAGE${NC}"
fi

if [ "$TELEMETRY" = "yes" ]; then
    echo -e "    📡 Telemetria: habilitada"
    echo -e "       ${GRAY}Jaeger UI:     http://localhost:16686${NC}"
    echo -e "       ${GRAY}Prometheus:    http://localhost:9090${NC}"
    echo -e "       ${GRAY}Grafana:       http://localhost:3000 (admin/admin)${NC}"
fi

if [ "$EVENTSOURCING" = "yes" ]; then
    echo -e "    📜 Event Sourcing: habilitado (Marten + PostgreSQL)"
fi

echo ""
echo -e "  ${CYAN}Documentação:${NC}"
echo "    📖 README.md          - Guia completo"
echo "    ⚡ QUICK-START.md     - Início rápido"
echo "    🔧 docs/ORM-GUIDE.md  - Como trocar ORM"
echo ""
