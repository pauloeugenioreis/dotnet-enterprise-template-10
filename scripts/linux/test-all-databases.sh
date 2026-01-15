#!/bin/bash
# Test All Database Providers
# This script tests the application with all supported database providers

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
SKIP_DOCKER=false
SKIP_MIGRATIONS=false
SKIP_TESTS=false
API_STARTUP_TIMEOUT=30
DATABASES=("SqlServer" "Oracle" "PostgreSQL" "MySQL")

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-docker)
            SKIP_DOCKER=true
            shift
            ;;
        --skip-migrations)
            SKIP_MIGRATIONS=true
            shift
            ;;
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        --timeout)
            API_STARTUP_TIMEOUT="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
API_PROJECT="$PROJECT_ROOT/src/Api/Api.csproj"
DATA_PROJECT="$PROJECT_ROOT/src/Data/Data.csproj"

echo -e "${CYAN}================================================${NC}"
echo -e "${CYAN}  Testing All Database Providers${NC}"
echo -e "${CYAN}================================================${NC}"
echo ""

# Function to check if port is open
check_port() {
    local host=$1
    local port=$2
    timeout 1 bash -c "cat < /dev/null > /dev/tcp/$host/$port" 2>/dev/null
}

# Step 1: Start Docker Compose
if [ "$SKIP_DOCKER" = false ]; then
    echo -e "${YELLOW}[1/4] Starting Docker Compose...${NC}"
    cd "$PROJECT_ROOT"

    docker-compose down -v 2>&1 > /dev/null
    docker-compose up -d sqlserver oracle postgres mysql

    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Failed to start Docker Compose${NC}"
        exit 1
    fi

    echo -e "${GREEN}✅ Docker containers started${NC}"
    echo ""

    # Wait for databases to be ready
    echo -e "${YELLOW}Waiting for databases to be ready...${NC}"

    TIMEOUT=120
    ELAPSED=0

    declare -A DB_STATUS
    DB_STATUS["SqlServer"]=0
    DB_STATUS["Oracle"]=0
    DB_STATUS["PostgreSQL"]=0
    DB_STATUS["MySQL"]=0

    while [ $ELAPSED -lt $TIMEOUT ]; do
        ALL_READY=true

        if [ ${DB_STATUS["SqlServer"]} -eq 0 ]; then
            if check_port localhost 1433; then
                DB_STATUS["SqlServer"]=1
                echo -e "${GREEN}  ✅ SQL Server ready${NC}"
            else
                ALL_READY=false
            fi
        fi

        if [ ${DB_STATUS["Oracle"]} -eq 0 ]; then
            if check_port localhost 1521; then
                DB_STATUS["Oracle"]=1
                echo -e "${GREEN}  ✅ Oracle ready${NC}"
            else
                ALL_READY=false
            fi
        fi

        if [ ${DB_STATUS["PostgreSQL"]} -eq 0 ]; then
            if check_port localhost 5433; then
                DB_STATUS["PostgreSQL"]=1
                echo -e "${GREEN}  ✅ PostgreSQL ready${NC}"
            else
                ALL_READY=false
            fi
        fi

        if [ ${DB_STATUS["MySQL"]} -eq 0 ]; then
            if check_port localhost 3306; then
                DB_STATUS["MySQL"]=1
                echo -e "${GREEN}  ✅ MySQL ready${NC}"
            else
                ALL_READY=false
            fi
        fi

        if [ "$ALL_READY" = true ]; then
            break
        fi

        sleep 2
        ELAPSED=$((ELAPSED + 2))
        echo -n "."
    done

    echo ""

    if [ "$ALL_READY" = false ]; then
        echo -e "${YELLOW}⚠️  Warning: Some databases did not become ready in time${NC}"
        for db in "${!DB_STATUS[@]}"; do
            if [ ${DB_STATUS[$db]} -eq 0 ]; then
                echo -e "${RED}  ❌ $db not ready${NC}"
            fi
        done
        echo ""
    fi
else
    echo -e "${CYAN}[1/4] Skipping Docker Compose (--skip-docker)${NC}"
    echo ""
fi

# Step 2: Test each database
declare -A RESULTS_MIGRATION
declare -A RESULTS_BUILD
declare -A RESULTS_STARTUP
declare -A RESULTS_HEALTH
declare -A RESULTS_ERROR

for DB in "${DATABASES[@]}"; do
    echo -e "${CYAN}================================================${NC}"
    echo -e "${CYAN}  Testing: $DB${NC}"
    echo -e "${CYAN}================================================${NC}"

    RESULTS_MIGRATION[$DB]=0
    RESULTS_BUILD[$DB]=0
    RESULTS_STARTUP[$DB]=0
    RESULTS_HEALTH[$DB]=0
    RESULTS_ERROR[$DB]=""

    # Run migrations
    if [ "$SKIP_MIGRATIONS" = false ]; then
        echo -e "${YELLOW}[2/4] Running migrations for $DB...${NC}"

        export ASPNETCORE_ENVIRONMENT=$DB

        # Drop database if exists (clean state)
        dotnet ef database drop --project "$DATA_PROJECT" --startup-project "$API_PROJECT" --force --no-build 2>&1 > /dev/null || true

        # Apply migrations
        if dotnet ef database update --project "$DATA_PROJECT" --startup-project "$API_PROJECT" --no-build 2>&1 > /dev/null; then
            echo -e "${GREEN}  ✅ Migrations applied successfully${NC}"
            RESULTS_MIGRATION[$DB]=1
        else
            echo -e "${RED}  ❌ Migration failed${NC}"
            RESULTS_ERROR[$DB]="Migration failed"
            continue
        fi
    else
        echo -e "${CYAN}[2/4] Skipping migrations (--skip-migrations)${NC}"
        RESULTS_MIGRATION[$DB]=1
    fi

    # Build project
    echo -e "${YELLOW}[3/4] Building project...${NC}"
    if dotnet build "$API_PROJECT" --no-restore 2>&1 > /dev/null; then
        echo -e "${GREEN}  ✅ Build successful${NC}"
        RESULTS_BUILD[$DB]=1
    else
        echo -e "${RED}  ❌ Build failed${NC}"
        RESULTS_ERROR[$DB]="Build failed"
        continue
    fi

    # Start API and test
    if [ "$SKIP_TESTS" = false ]; then
        echo -e "${YELLOW}[4/4] Starting API and testing...${NC}"

        export ASPNETCORE_ENVIRONMENT=$DB
        dotnet run --project "$API_PROJECT" --no-build > /dev/null 2>&1 &
        API_PID=$!

        # Wait for API to start
        API_READY=false
        for ((i=0; i<$API_STARTUP_TIMEOUT; i++)); do
            sleep 1

            if curl -s http://localhost:5000/health > /dev/null 2>&1; then
                API_READY=true
                break
            fi
        done

        if [ "$API_READY" = true ]; then
            echo -e "${GREEN}  ✅ API started successfully${NC}"
            RESULTS_STARTUP[$DB]=1

            # Test health endpoint
            HEALTH_RESPONSE=$(curl -s http://localhost:5000/health)
            if [ $? -eq 0 ]; then
                echo -e "${GREEN}  ✅ Health check passed${NC}"
                RESULTS_HEALTH[$DB]=1

                # Test Swagger endpoint
                if curl -s http://localhost:5000/swagger/index.html > /dev/null 2>&1; then
                    echo -e "${GREEN}  ✅ Swagger UI accessible${NC}"
                fi
            else
                echo -e "${YELLOW}  ⚠️  Health check warning${NC}"
            fi
        else
            echo -e "${RED}  ❌ API failed to start within ${API_STARTUP_TIMEOUT}s${NC}"
            RESULTS_ERROR[$DB]="API startup timeout"
        fi

        # Stop API
        kill $API_PID 2>/dev/null || true
        sleep 2
    else
        echo -e "${CYAN}[4/4] Skipping API tests (--skip-tests)${NC}"
        RESULTS_STARTUP[$DB]=1
        RESULTS_HEALTH[$DB]=1
    fi

    echo ""
done

# Summary
echo -e "${CYAN}================================================${NC}"
echo -e "${CYAN}  Test Summary${NC}"
echo -e "${CYAN}================================================${NC}"
echo ""

ALL_PASSED=true

for DB in "${DATABASES[@]}"; do
    if [ ${RESULTS_MIGRATION[$DB]} -eq 1 ] && \
       [ ${RESULTS_BUILD[$DB]} -eq 1 ] && \
       [ ${RESULTS_STARTUP[$DB]} -eq 1 ] && \
       [ ${RESULTS_HEALTH[$DB]} -eq 1 ]; then
        echo -e "$DB: ${GREEN}✅ PASSED${NC}"
    else
        ALL_PASSED=false
        echo -e "$DB: ${RED}❌ FAILED${NC}"

        if [ -n "${RESULTS_ERROR[$DB]}" ]; then
            echo -e "${RED}    Error: ${RESULTS_ERROR[$DB]}${NC}"
        else
            [ ${RESULTS_MIGRATION[$DB]} -eq 0 ] && echo -e "${RED}    - Migration failed${NC}"
            [ ${RESULTS_BUILD[$DB]} -eq 0 ] && echo -e "${RED}    - Build failed${NC}"
            [ ${RESULTS_STARTUP[$DB]} -eq 0 ] && echo -e "${RED}    - API startup failed${NC}"
            [ ${RESULTS_HEALTH[$DB]} -eq 0 ] && echo -e "${RED}    - Health check failed${NC}"
        fi
    fi
done

echo ""
echo -e "${CYAN}================================================${NC}"

if [ "$ALL_PASSED" = true ]; then
    echo -e "${GREEN}✅ All database tests passed!${NC}"
    exit 0
else
    echo -e "${RED}❌ Some database tests failed${NC}"
    exit 1
fi
