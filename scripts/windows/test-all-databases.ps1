# Test All Database Providers
# This script tests the application with all supported database providers

param(
    [switch]$SkipDocker,
    [switch]$SkipMigrations,
    [switch]$SkipTests,
    [int]$ApiStartupTimeout = 30
)

$ErrorActionPreference = "Stop"
$databases = @("SqlServer", "Oracle", "PostgreSQL", "MySQL")
$projectRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$apiProject = Join-Path $projectRoot "src\Api\Api.csproj"
$dataProject = Join-Path $projectRoot "src\Data\Data.csproj"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Testing All Database Providers" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if port is open
function Test-Port {
    param($hostname, $port)
    $connection = New-Object System.Net.Sockets.TcpClient
    try {
        $connection.Connect($hostname, $port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# Step 1: Start Docker Compose
if (-not $SkipDocker) {
    Write-Host "[1/4] Starting Docker Compose..." -ForegroundColor Yellow
    Set-Location $projectRoot

    docker-compose down -v 2>&1 | Out-Null
    docker-compose up -d sqlserver oracle postgres mysql

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to start Docker Compose" -ForegroundColor Red
        exit 1
    }

    Write-Host "✅ Docker containers started" -ForegroundColor Green
    Write-Host ""

    # Wait for databases to be ready
    Write-Host "Waiting for databases to be ready..." -ForegroundColor Yellow

    $timeout = 120
    $elapsed = 0
    $dbStatus = @{
        "SqlServer" = $false
        "Oracle" = $false
        "PostgreSQL" = $false
        "MySQL" = $false
    }

    while ($elapsed -lt $timeout) {
        if (-not $dbStatus["SqlServer"]) {
            $dbStatus["SqlServer"] = Test-Port "localhost" 1433
            if ($dbStatus["SqlServer"]) { Write-Host "  ✅ SQL Server ready" -ForegroundColor Green }
        }

        if (-not $dbStatus["Oracle"]) {
            $dbStatus["Oracle"] = Test-Port "localhost" 1521
            if ($dbStatus["Oracle"]) { Write-Host "  ✅ Oracle ready" -ForegroundColor Green }
        }

        if (-not $dbStatus["PostgreSQL"]) {
            $dbStatus["PostgreSQL"] = Test-Port "localhost" 5433
            if ($dbStatus["PostgreSQL"]) { Write-Host "  ✅ PostgreSQL ready" -ForegroundColor Green }
        }

        if (-not $dbStatus["MySQL"]) {
            $dbStatus["MySQL"] = Test-Port "localhost" 3306
            if ($dbStatus["MySQL"]) { Write-Host "  ✅ MySQL ready" -ForegroundColor Green }
        }

        if ($dbStatus.Values -notcontains $false) {
            break
        }

        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Host "." -NoNewline
    }

    Write-Host ""

    if ($dbStatus.Values -contains $false) {
        Write-Host "⚠️  Warning: Some databases did not become ready in time" -ForegroundColor Yellow
        foreach ($db in $dbStatus.Keys) {
            if (-not $dbStatus[$db]) {
                Write-Host "  ❌ $db not ready" -ForegroundColor Red
            }
        }
        Write-Host ""
    }
}
else {
    Write-Host "[1/4] Skipping Docker Compose (--SkipDocker)" -ForegroundColor Gray
    Write-Host ""
}

# Step 2: Test each database
$results = @()

foreach ($db in $databases) {
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Testing: $db" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan

    $result = @{
        Database = $db
        Migration = $false
        Build = $false
        Startup = $false
        HealthCheck = $false
        Error = $null
    }

    try {
        # Run migrations
        if (-not $SkipMigrations) {
            Write-Host "[2/4] Running migrations for $db..." -ForegroundColor Yellow

            $env:ASPNETCORE_ENVIRONMENT = $db

            # Drop database if exists (clean state)
            & dotnet ef database drop --project $dataProject --startup-project $apiProject --force --no-build 2>&1 | Out-Null

            # Apply migrations
            $migrationOutput = & dotnet ef database update --project $dataProject --startup-project $apiProject --no-build 2>&1

            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✅ Migrations applied successfully" -ForegroundColor Green
                $result.Migration = $true
            }
            else {
                Write-Host "  ❌ Migration failed" -ForegroundColor Red
                $result.Error = "Migration failed: $migrationOutput"
                $results += $result
                continue
            }
        }
        else {
            Write-Host "[2/4] Skipping migrations (--SkipMigrations)" -ForegroundColor Gray
            $result.Migration = $true
        }

        # Build project
        Write-Host "[3/4] Building project..." -ForegroundColor Yellow
        $buildOutput = & dotnet build $apiProject --no-restore 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Build successful" -ForegroundColor Green
            $result.Build = $true
        }
        else {
            Write-Host "  ❌ Build failed" -ForegroundColor Red
            $result.Error = "Build failed"
            $results += $result
            continue
        }

        # Start API and test
        if (-not $SkipTests) {
            Write-Host "[4/4] Starting API and testing..." -ForegroundColor Yellow

            $env:ASPNETCORE_ENVIRONMENT = $db
            $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$apiProject`" --no-build" -PassThru -WindowStyle Hidden

            # Wait for API to start
            $apiReady = $false
            for ($i = 0; $i -lt $ApiStartupTimeout; $i++) {
                Start-Sleep -Seconds 1

                try {
                    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
                    if ($response.StatusCode -eq 200) {
                        $apiReady = $true
                        break
                    }
                }
                catch {
                    # Continue waiting
                }
            }

            if ($apiReady) {
                Write-Host "  ✅ API started successfully" -ForegroundColor Green
                $result.Startup = $true

                # Test health endpoint
                try {
                    $healthResponse = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
                    Write-Host "  ✅ Health check passed: $($healthResponse.status)" -ForegroundColor Green
                    $result.HealthCheck = $true

                    # Test Swagger endpoint
                    $swaggerResponse = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -UseBasicParsing -ErrorAction SilentlyContinue
                    if ($swaggerResponse.StatusCode -eq 200) {
                        Write-Host "  ✅ Swagger UI accessible" -ForegroundColor Green
                    }
                }
                catch {
                    Write-Host "  ⚠️  Health check warning: $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }
            else {
                Write-Host "  ❌ API failed to start within ${ApiStartupTimeout}s" -ForegroundColor Red
                $result.Error = "API startup timeout"
            }

            # Stop API
            Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
        }
        else {
            Write-Host "[4/4] Skipping API tests (--SkipTests)" -ForegroundColor Gray
            $result.Startup = $true
            $result.HealthCheck = $true
        }
    }
    catch {
        Write-Host "  ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        $result.Error = $_.Exception.Message
    }

    $results += $result
    Write-Host ""
}

# Summary
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$allPassed = $true

foreach ($result in $results) {
    $status = if ($result.Migration -and $result.Build -and $result.Startup -and $result.HealthCheck) {
        "✅ PASSED"
        Write-Host "$($result.Database): " -NoNewline
        Write-Host "✅ PASSED" -ForegroundColor Green
    }
    else {
        $allPassed = $false
        "❌ FAILED"
        Write-Host "$($result.Database): " -NoNewline
        Write-Host "❌ FAILED" -ForegroundColor Red

        if ($result.Error) {
            Write-Host "    Error: $($result.Error)" -ForegroundColor Red
        }
        else {
            if (-not $result.Migration) { Write-Host "    - Migration failed" -ForegroundColor Red }
            if (-not $result.Build) { Write-Host "    - Build failed" -ForegroundColor Red }
            if (-not $result.Startup) { Write-Host "    - API startup failed" -ForegroundColor Red }
            if (-not $result.HealthCheck) { Write-Host "    - Health check failed" -ForegroundColor Red }
        }
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan

if ($allPassed) {
    Write-Host "✅ All database tests passed!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "❌ Some database tests failed" -ForegroundColor Red
    exit 1
}
