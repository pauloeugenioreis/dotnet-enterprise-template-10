@echo off
REM Script to initialize a new project from template
REM Usage: new-project.bat <ProjectName>

if "%1"=="" (
    echo Error: Project name is required
    echo Usage: new-project.bat ^<ProjectName^>
    exit /b 1
)

set PROJECT_NAME=%1
set TEMPLATE_DIR=%~dp0..
set TARGET_DIR=%TEMPLATE_DIR%\..\%PROJECT_NAME%

echo Creating new project: %PROJECT_NAME%
echo Template directory: %TEMPLATE_DIR%
echo Target directory: %TARGET_DIR%

REM Check if target directory exists
if exist "%TARGET_DIR%" (
    echo Error: Directory %TARGET_DIR% already exists
    exit /b 1
)

echo Copying template files...
xcopy /E /I /Y "%TEMPLATE_DIR%" "%TARGET_DIR%"

cd /d "%TARGET_DIR%"

REM Remove scripts directory
rd /s /q scripts 2>nul

REM Rename solution file
ren ProjectTemplate.sln %PROJECT_NAME%.sln

echo Renaming project references...
powershell -Command "(Get-ChildItem -Recurse -Include *.cs,*.csproj,*.sln,*.json) | ForEach-Object { (Get-Content $_.FullName) -replace 'ProjectTemplate', '%PROJECT_NAME%' | Set-Content $_.FullName }"

echo.
echo ✅ Project created successfully!
echo.
echo Next steps:
echo 1. cd %PROJECT_NAME%
echo 2. Update connection strings in src\Api\appsettings.json
echo 3. (Optional) To change ORM: Edit src\Infrastructure\Extensions\DatabaseExtension.cs (line ~26)
echo 4. Run: dotnet restore
echo 5. Run: dotnet build
echo 6. Run: dotnet ef migrations add InitialCreate --project src\Data --startup-project src\Api
echo 7. Run: dotnet ef database update --project src\Data --startup-project src\Api
echo 8. Run: dotnet run --project src\Api
echo.
echo Documentation:
echo    - README.md - Complete guide
echo    - QUICK-START.md - Quick start in 5 minutes
echo    - docs\ORM-GUIDE.md - How to switch ORMs
echo.
