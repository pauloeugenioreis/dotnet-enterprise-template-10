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

# Validate that all database containers are running and healthy
Write-Host "Validating database containers..." -ForegroundColor Yellow
$requiredContainers = @{
    "sqlserver" = @{ Port = 1433; Name = "SQL Server" }
    "oracle" = @{ Port = 1521; Name = "Oracle" }
    "postgres" = @{ Port = 5433; Name = "PostgreSQL" }
    "mysql" = @{ Port = 3306; Name = "MySQL" }
}

$containerValidation = $true

foreach ($container in $requiredContainers.Keys) {
    $info = $requiredContainers[$container]

    # Check if container is running
    $containerStatus = docker inspect -f '{{.State.Status}}' $container 2>$null

    if ($containerStatus -ne "running") {
        Write-Host "  ❌ $($info.Name) container '$container' is not running" -ForegroundColor Red
        $containerValidation = $false
        continue
    }

    # Check health status if available
    $healthStatus = docker inspect -f '{{.State.Health.Status}}' $container 2>$null
    if ($healthStatus -and $healthStatus -ne "healthy" -and $healthStatus -ne "<no value>") {
        Write-Host "  ⚠️  $($info.Name) container is running but not healthy (status: $healthStatus)" -ForegroundColor Yellow
    }

    # Check if port is accessible
    if (Test-Port "localhost" $info.Port) {
        Write-Host "  ✅ $($info.Name) is ready (port $($info.Port))" -ForegroundColor Green
    }
    else {
        Write-Host "  ❌ $($info.Name) port $($info.Port) is not accessible" -ForegroundColor Red
        $containerValidation = $false
    }
}

Write-Host ""

if (-not $containerValidation) {
    Write-Host "❌ Not all database containers are ready. Please start them first:" -ForegroundColor Red
    Write-Host "   docker-compose up -d sqlserver oracle postgres mysql" -ForegroundColor Gray
    exit 1
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
            $ErrorActionPreference = 'Continue'
            & dotnet ef database drop --project $dataProject --startup-project $apiProject --force --no-build 2>&1 | Out-Null

            # Apply migrations
            $migrationOutput = & dotnet ef database update --project $dataProject --startup-project $apiProject --no-build 2>&1 | Out-String
            $ErrorActionPreference = 'Stop'

            # Check if migration succeeded (look for "Done." in output)
            if ($migrationOutput -match "Done\." -or $migrationOutput -match "No migrations were applied") {
                Write-Host "  ✅ Migrations applied successfully" -ForegroundColor Green
                $result.Migration = $true
            }
            else {
                Write-Host "  ❌ Migration failed" -ForegroundColor Red
                Write-Host "Full output:" -ForegroundColor Gray
                Write-Host "$migrationOutput" -ForegroundColor Gray
                $result.Error = $migrationOutput.Trim()
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
        $buildOutput = & dotnet build $apiProject --no-restore 2>&1 | Out-String

        # Check if build succeeded (look for success message or absence of errors)
        if ($buildOutput -match "Build succeeded" -or $buildOutput -notmatch "Build FAILED|error CS|error MSB") {
            Write-Host "  ✅ Build successful" -ForegroundColor Green
            $result.Build = $true
        }
        else {
            Write-Host "  ❌ Build failed" -ForegroundColor Red
            Write-Host "Output: $buildOutput" -ForegroundColor Gray
            $result.Error = "Build failed"
            $results += $result
            continue
        }

        # Start API and test
        if (-not $SkipTests) {
            Write-Host "[4/4] Starting API and testing..." -ForegroundColor Yellow

            $env:ASPNETCORE_ENVIRONMENT = $db

            # Create log file for API output
            $logFile = Join-Path $env:TEMP "api-test-$db.log"

            try {
                # Try alternative simpler approach if event handlers aren't working
                $tempOutputFile = Join-Path $env:TEMP "api-output-$db.txt"
                $tempErrorFile = Join-Path $env:TEMP "api-error-$db.txt"

                # Start API process with file redirection as backup
                $apiProcess = Start-Process -FilePath "dotnet" `
                    -ArgumentList "run --project `"$apiProject`" --no-build --no-launch-profile --urls `"http://localhost:5000`"" `
                    -PassThru `
                    -NoNewWindow `
                    -RedirectStandardOutput $tempOutputFile `
                    -RedirectStandardError $tempErrorFile

                Write-Host "  Process started (PID: $($apiProcess.Id))" -ForegroundColor Gray
                Write-Host "  Logs: $tempOutputFile" -ForegroundColor DarkGray

                # Wait for API to start
                $apiReady = $false
                for ($i = 0; $i -lt $ApiStartupTimeout; $i++) {
                    Start-Sleep -Seconds 1

                    # Check if process is still running
                    if ($apiProcess.HasExited) {
                        # Give time for file writes to complete
                        Start-Sleep -Milliseconds 1000

                        Write-Host "  ❌ API process exited unexpectedly (Exit Code: $($apiProcess.ExitCode))" -ForegroundColor Red

                        if (Test-Path $tempOutputFile) {
                            $stdOut = Get-Content $tempOutputFile -Raw -ErrorAction SilentlyContinue
                            if ($stdOut) {
                                Write-Host "  === Standard Output ===" -ForegroundColor Yellow
                                Write-Host $stdOut -ForegroundColor Gray
                            }
                        }

                        if (Test-Path $tempErrorFile) {
                            $stdErr = Get-Content $tempErrorFile -Raw -ErrorAction SilentlyContinue
                            if ($stdErr) {
                                Write-Host "  === Standard Error ===" -ForegroundColor Yellow
                                Write-Host $stdErr -ForegroundColor Red
                            }
                        }

                        # Also try to get from event log if it's a .NET crash
                        Write-Host "  Checking Windows Event Log for .NET errors..." -ForegroundColor DarkGray
                        $recentErrors = Get-EventLog -LogName Application -Source ".NET Runtime" -Newest 5 -ErrorAction SilentlyContinue |
                            Where-Object { $_.TimeGenerated -gt (Get-Date).AddMinutes(-1) }

                        if ($recentErrors) {
                            Write-Host "  === Recent .NET Runtime Errors ===" -ForegroundColor Yellow
                            $recentErrors | ForEach-Object {
                                Write-Host "  $($_.Message)" -ForegroundColor Red
                            }
                        }

                        $result.Error = "API process exited with code $($apiProcess.ExitCode)"
                        break
                    }

                    try {
                        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
                        if ($response.StatusCode -eq 200) {
                            $apiReady = $true
                            break
                        }
                    }
                    catch {
                        # Continue waiting
                        Write-Host "." -NoNewline -ForegroundColor Gray
                    }
                }

                Write-Host ""

                if ($apiReady) {
                    Write-Host "  ✅ API started successfully" -ForegroundColor Green
                    $result.Startup = $true

                    # Test health endpoint
                    try {
                        $healthResponse = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
                        $healthStatus = if ($healthResponse -is [string]) { $healthResponse } else { $healthResponse.status }
                        Write-Host "  ✅ Health check passed: $healthStatus" -ForegroundColor Green
                        $result.HealthCheck = $true

                        # Test Swagger endpoint (optional - don't fail if not available)
                        try {
                            $swaggerResponse = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -UseBasicParsing -ErrorAction Stop
                            if ($swaggerResponse.StatusCode -eq 200) {
                                Write-Host "  ✅ Swagger UI accessible" -ForegroundColor Green
                            }
                        }
                        catch {
                            # Swagger not configured or not accessible - not critical
                        }
                    }
                    catch {
                        Write-Host "  ⚠️  Health check failed: $($_.Exception.Message)" -ForegroundColor Yellow
                    }
                }
                elseif (-not $apiProcess.HasExited) {
                    Write-Host "  ❌ API failed to start within ${ApiStartupTimeout}s" -ForegroundColor Red

                    if (Test-Path $tempOutputFile) {
                        Write-Host "  === Last Output ===" -ForegroundColor Yellow
                        $lastLines = Get-Content $tempOutputFile -Tail 15 -ErrorAction SilentlyContinue
                        $lastLines | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
                    }

                    $result.Error = "API startup timeout"
                }

                # Stop API and cleanup
                if (-not $apiProcess.HasExited) {
                    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
                    Start-Sleep -Seconds 1
                }

                # Cleanup temp files
                Remove-Item $tempOutputFile -ErrorAction SilentlyContinue
                Remove-Item $tempErrorFile -ErrorAction SilentlyContinue

                Start-Sleep -Seconds 1
            }
            catch {
                Write-Host "  ❌ Error starting API: $($_.Exception.Message)" -ForegroundColor Red
                Write-Host "  Stack trace:" -ForegroundColor Yellow
                Write-Host $_.ScriptStackTrace -ForegroundColor Gray
                $result.Error = "API start exception: $($_.Exception.Message)"
            }
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
