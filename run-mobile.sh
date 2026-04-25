#!/bin/bash

# Cores para o terminal
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Configurações de Caminhos
PROJECT_ROOT=$(pwd)
FLUTTER_PATH="$PROJECT_ROOT/src/UI/Mobile/FlutterApp"
RN_PATH="$PROJECT_ROOT/src/UI/Mobile/ReactNativeApp"
MAUI_PATH="$PROJECT_ROOT/src/UI/Mobile/MauiApp"
API_URL="http://localhost:5266/health"

show_header() {
    clear
    echo -e "${CYAN}===============================================${NC}"
    echo -e "${CYAN}    Enterprise Template - Mobile Launcher      ${NC}"
    echo -e "${CYAN}===============================================${NC}"
}

check_backend() {
    # Verifica quais bancos estão presentes no docker-compose.yml
    HAS_POSTGRES=$(grep -q "postgres:" docker-compose.yml && echo "yes" || echo "no")
    HAS_MSSQL=$(grep -q "mssql:" docker-compose.yml && echo "yes" || echo "no")
    HAS_MYSQL=$(grep -q "mysql:" docker-compose.yml && echo "yes" || echo "no")
    HAS_ORACLE=$(grep -q "oracle:" docker-compose.yml && echo "yes" || echo "no")

    AVAILABLE_DBS=""
    [ "$HAS_POSTGRES" == "yes" ] && AVAILABLE_DBS="$AVAILABLE_DBS 1"
    [ "$HAS_MSSQL" == "yes" ] && AVAILABLE_DBS="$AVAILABLE_DBS 2"
    [ "$HAS_MYSQL" == "yes" ] && AVAILABLE_DBS="$AVAILABLE_DBS 3"
    [ "$HAS_ORACLE" == "yes" ] && AVAILABLE_DBS="$AVAILABLE_DBS 4"

    DB_COUNT=$(echo $AVAILABLE_DBS | wc -w)

    if [ "$DB_COUNT" -eq "1" ]; then
        DB_OPTION=$(echo $AVAILABLE_DBS | tr -d ' ')
        echo -e "${CYAN}ℹ️ Banco de dados detectado automaticamente baseado no docker-compose.yml.${NC}"
    else
        echo -e "\nEscolha o Banco de Dados Principal:"
        echo -e "1) ${CYAN}PostgreSQL${NC} (Padrão)"
        echo -e "2) ${CYAN}SQL Server${NC}"
        echo -e "3) ${CYAN}MySQL${NC}"
        echo -e "4) ${CYAN}Oracle${NC}"
        read -p "Escolha uma opção [1-4]: " DB_OPTION
    fi
    
    case $DB_OPTION in
        2) DB_TYPE="sqlserver"; DB_SERVICE="sqlserver";
           DB_CONN="Server=sqlserver;Database=ProjectTemplateDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True" ;;
        3) DB_TYPE="mysql"; DB_SERVICE="mysql";
           DB_CONN="Server=mysql;Port=3306;Database=ProjectTemplate;Uid=appuser;Pwd=AppPass123;" ;;
        4) DB_TYPE="oracle"; DB_SERVICE="oracle";
           DB_CONN="Data Source=oracle:1521/ProjectTemplate;User Id=appuser;Password=AppPass123" ;;
        *) DB_TYPE="postgresql"; DB_SERVICE="postgres";
           DB_CONN="Host=postgres;Database=ProjectTemplate;Username=postgres;Password=PostgresPass123;Port=5432" ;;
    esac

    DB_EVENTS_CONN="Host=postgres-events;Database=ProjectTemplateEvents;Username=postgres;Password=postgres"

    # Determina containers de banco
    DB_CONTAINERS="$DB_SERVICE"
    if [ "$DB_TYPE" != "postgresql" ]; then
        DB_CONTAINERS="$DB_SERVICE postgres-events"
    else
        DB_CONTAINERS="postgres postgres-events"
    fi

    # Subir Docker (Infra + API)
    echo -e "${CYAN}Subindo containers ($DB_TYPE)...${NC}"
    DB_TYPE=$DB_TYPE \
    DB_CONNECTION_STRING="$DB_CONN" \
    DB_EVENTS_CONNECTION_STRING="$DB_EVENTS_CONN" \
    docker compose up -d $DB_CONTAINERS api redis rabbitmq mongodb jaeger prometheus grafana
    
    echo -n "Aguardando API ficar pronta..."
    until curl -s "http://localhost:5000/health/ready" | grep "Healthy" > /dev/null; do
        echo -n "."
        sleep 2
    done
    echo -e "\n${GREEN}✔ Backend pronto na porta 5000!${NC}"
}

run_flutter() {
    echo -e "\nEscolha o Alvo do Flutter:"
    echo -e "1) ${CYAN}Android${NC} (Emulador/Físico)"
    echo -e "2) ${CYAN}Linux Desktop${NC}"
    read -p "Opção [1-2]: " TARGET_OPTION

    cd "$FLUTTER_PATH" || exit
    
    case $TARGET_OPTION in
        2)
            echo -e "${GREEN}Iniciando Flutter App no Linux Desktop...${NC}"
            flutter run -d linux
            ;;
        *)
            echo -e "${GREEN}Iniciando Flutter App no Android...${NC}"
            # Verifica se há emuladores se o comando falhar
            if ! flutter run -d android; then
                echo -e "${YELLOW}Dica: Se o Android não abriu, verifique se o emulador está ligado.${NC}"
                echo -e "Use: ${CYAN}flutter emulators --launch <ID>${NC} para iniciar um."
            fi
            ;;
    esac
}

run_react_native() {
    echo -e "${GREEN}Iniciando React Native (Expo)...${NC}"
    if ! command -v npm &> /dev/null; then
        echo -e "${RED}Erro: NPM/Node não encontrado.${NC}"
        return
    fi
    cd "$RN_PATH" || exit
    npm run android
}

run_maui() {
    echo -e "${GREEN}Iniciando .NET MAUI (Android)...${NC}"
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}Erro: .NET SDK não encontrado.${NC}"
        return
    fi
    cd "$MAUI_PATH" || exit
    dotnet build -t:Run -f net10.0-android
}

show_header
echo -e "Escolha a plataforma mobile para iniciar:"
echo -e "1) ${CYAN}Flutter${NC} (Recomendado)"
echo -e "2) ${CYAN}React Native${NC} (Expo)"
echo -e "3) ${CYAN}MAUI${NC} (Android)"
echo -e "q) Sair"
echo

read -p "Opção: " OPTION

case $OPTION in
    1)
        check_backend
        run_flutter
        ;;
    2)
        check_backend
        run_react_native
        ;;
    3)
        check_backend
        run_maui
        ;;
    q|Q)
        echo "Saindo..."
        exit 0
        ;;
    *)
        echo -e "${RED}Opção inválida.${NC}"
        exit 1
        ;;
esac
