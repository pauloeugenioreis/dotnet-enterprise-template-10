#!/bin/bash

# Cores para o terminal
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Portas críticas que costumam causar conflitos
PORTS="5000 5001 5432 5433 6379 15672 5672 27017 3000 16686 9090 4200 5173 5174 5188"

clean_environment() {
    echo -e "\n${RED}🛑 Limpando ambiente e liberando portas...${NC}"
    
    # 1. Para todos os containers do projeto atual
    docker-compose down --remove-orphans > /dev/null 2>&1
    
    # 2. Busca e para QUALQUER container docker que esteja usando as portas críticas
    for port in $PORTS; do
        # Encontra IDs de containers que estão usando a porta no HOST
        cont_ids=$(docker ps -a --format "{{.ID}} {{.Ports}}" | grep ":$port->" | awk '{print $1}')
        
        if [ ! -z "$cont_ids" ]; then
            for id in $cont_ids; do
                echo -e "  - Removendo container teimoso ($id) na porta $port"
                docker rm -f $id > /dev/null 2>&1
            done
        fi
        
        # Mata processos locais (ex: dotnet run que ficou pendurado)
        # Tenta fuser primeiro (mais agressivo), depois lsof
        fuser -k $port/tcp > /dev/null 2>&1
        
        pid=$(lsof -t -i:$port)
        if [ ! -z "$pid" ]; then
            echo -e "  - Matando processo na porta $port (PID: $pid)"
            kill -9 $pid 2>/dev/null
        fi
    done

    # 3. Pausa para o SO liberar os sockets
    echo -e "${YELLOW}⏳ Aguardando liberação total das portas...${NC}"
    sleep 3

    # 4. Diagnóstico: Verifica se algo ainda sobrou
    echo -e "\n${CYAN}🔍 Verificação final de portas:${NC}"
    for port in $PORTS; do
        check=$(lsof -i:$port -sTCP:LISTEN -t)
        if [ ! -z "$check" ]; then
            process_name=$(ps -p $check -o comm=)
            echo -e "  ${RED}⚠ ATENÇÃO: A porta $port ainda está ocupada por: [$process_name] (PID: $check)${NC}"
            echo -e "     Dica: Tente rodar 'sudo kill -9 $check' manualmente."
        fi
    done
    
    echo -e "${GREEN}✅ Ambiente pronto!${NC}"
}

# --- BANCO DE DADOS DINÂMICO ---
select_database() {
    echo ""
    echo -e "${YELLOW}==========================================${NC}"
    echo -e "${YELLOW}  SELECIONE O BANCO DE DADOS PRINCIPAL    ${NC}"
    echo -e "${YELLOW}==========================================${NC}"
    echo "1) PostgreSQL (Padrão)"
    echo "2) SQL Server"
    echo "3) MySQL"
    echo "4) Oracle"
    echo "=========================================="
    read -p "Escolha uma opção [1-4]: " db_opt

    case $db_opt in
        2) 
            DB_TYPE="sqlserver"
            DB_SERVICE="sqlserver"
            DB_CONN="Server=sqlserver;Database=ProjectTemplateDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
            ;;
        3) 
            DB_TYPE="mysql"
            DB_SERVICE="mysql"
            DB_CONN="Server=mysql;Port=3306;Database=ProjectTemplate;Uid=appuser;Pwd=AppPass123;"
            ;;
        4) 
            DB_TYPE="oracle"
            DB_SERVICE="oracle"
            DB_CONN="Data Source=oracle:1521/ProjectTemplate;User Id=appuser;Password=AppPass123"
            ;;
        *) 
            DB_TYPE="postgresql"
            DB_SERVICE="postgres"
            DB_CONN="Host=postgres;Database=ProjectTemplate;Username=postgres;Password=PostgresPass123;Port=5432"
            ;;
    esac
    
    # Event Sourcing sempre precisa de Postgres (Marten)
    DB_EVENTS_CONN="Host=postgres-events;Database=ProjectTemplateEvents;Username=postgres;Password=postgres"
}

echo -e "${CYAN}================================================================${NC}"
echo -e "${CYAN}          CENTRAL DE COMANDO - ENTERPRISE TEMPLATE 10           ${NC}"
echo -e "${CYAN}================================================================${NC}"
echo -e "Escolha o ambiente (limpeza profunda inclusa):"
echo -e " "
echo -e "1) ${GREEN}Docker Compose${NC} (Ambiente isolado)"
echo -e "2) ${YELLOW}AppHost (Aspire)${NC} (Desenvolvimento Local)"
echo -e "3) ${RED}Limpeza Total${NC} (Para tudo e libera portas)"
echo -e "4) Sair"
echo -e " "
read -p "Escolha uma opção [1-4]: " choice

case $choice in
    1)
        select_database
        clean_environment
        echo -e "\n${GREEN}🚀 Iniciando via Docker Compose com $DB_TYPE...${NC}"
        
        # Determina containers de banco
        DB_CONTAINERS="$DB_SERVICE"
        if [ "$DB_TYPE" != "postgresql" ]; then
            DB_CONTAINERS="$DB_SERVICE postgres-events"
        else
            DB_CONTAINERS="postgres postgres-events"
        fi

        DB_TYPE=$DB_TYPE \
        DB_CONNECTION_STRING="$DB_CONN" \
        DB_EVENTS_CONNECTION_STRING="$DB_EVENTS_CONN" \
        docker-compose up -d --build $DB_CONTAINERS redis rabbitmq mongodb jaeger prometheus grafana api angular-web react-web vue-web blazor-app welcome
        
        echo -e "\n${BLUE}----------------------------------------------------------------${NC}"
        echo -e "${GREEN}🚀 Ambiente Docker Iniciado!${NC}"
        echo -e "${BLUE}----------------------------------------------------------------${NC}"
        
        echo -e "${CYAN}Aguardando serviços subirem... aqui estão os links de acesso:${NC}"
        echo -e "🚀 Aplicações (Frontends & API)"
        echo -e "API (.NET 10):       http://localhost:5000"
        echo -e "Angular:             http://localhost:4200"
        echo -e "React:               http://localhost:5173"
        echo -e "Vue:                 http://localhost:5174"
        echo -e "Blazor WebApp:       http://localhost:5188"
        echo -e " "
        echo -e "📊 Observabilidade & Infraestrutura"
        echo -e "Grafana:             http://localhost:3000"
        echo -e "Jaeger:              http://localhost:16686"
        echo -e "RabbitMQ Management: http://localhost:15672"
        echo -e "Prometheus:          http://localhost:9090"
        echo -e "${BLUE}----------------------------------------------------------------${NC}"

        echo -e "\n${YELLOW}⏳ Verificando saúde da API... (isso pode levar um minuto)${NC}"
        
        # Loop de espera pela API (timeout de 60 segundos)
        for i in {1..60}; do
            if curl -s http://localhost:5000/health > /dev/null 2>&1; then
                API_READY=true
                break
            fi
            echo -n "."
            sleep 1
        done

        if [ "$API_READY" = true ]; then
            echo -e "\n\n${GREEN}✅ API está UP e Saudável!${NC}"
        else
            echo -e "\n\n${RED}⚠ A API demorou para responder. Verifique os logs com: 'docker-compose logs -f api'${NC}"
        fi
        
        echo -e "${BLUE}----------------------------------------------------------------${NC}"
        echo -e "${YELLOW}Dica: O ambiente continuará subindo em background.${NC}"
        ;;
    2)
        select_database
        clean_environment
        echo -e "\n${YELLOW}🔥 Iniciando via AppHost (Aspire) com $DB_TYPE...${NC}"
        DB_TYPE=$DB_TYPE dotnet run --project src/AppHost
        ;;
    3)
        clean_environment
        ;;
    4)
        echo -e "\nAté logo!"
        exit 0
        ;;
    *)
        echo -e "\n❌ Opção inválida."
        exit 1
        ;;
esac
