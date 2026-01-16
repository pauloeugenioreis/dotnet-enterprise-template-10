# Script to initialize a new project from template
# Usage: .\new-project.ps1 -ProjectName "YourProjectName"

param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectName
)

$ErrorActionPreference = "Stop"

$TemplateDir = Split-Path -Parent $PSScriptRoot
$TargetDir = Join-Path (Split-Path -Parent $TemplateDir) $ProjectName

Write-Host "Creating new project: $ProjectName" -ForegroundColor Green
Write-Host "Template directory: $TemplateDir"
Write-Host "Target directory: $TargetDir"

# Check if target directory exists
if (Test-Path $TargetDir) {
    Write-Host "Error: Directory $TargetDir already exists" -ForegroundColor Red
    exit 1
}

Write-Host "Copying template files..." -ForegroundColor Yellow
Copy-Item -Path $TemplateDir -Destination $TargetDir -Recurse -Exclude @('.git', 'bin', 'obj', '.vs', '.vscode')

# Navigate to target directory
Set-Location $TargetDir

# Remove unnecessary directories and files from new project
Write-Host "Cleaning up..." -ForegroundColor Yellow
$itemsToRemove = @(
    "scripts",
    ".git",
    ".gitignore"
)

foreach ($item in $itemsToRemove) {
    if (Test-Path $item) {
        Remove-Item -Path $item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed: $item" -ForegroundColor Gray
    }
}

# Clean bin and obj directories
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# Rename solution file
Rename-Item -Path "ProjectTemplate.sln" -NewName "$ProjectName.sln"

# Replace ProjectTemplate with actual project name in all files
Write-Host "Renaming project references..." -ForegroundColor Yellow
$files = Get-ChildItem -Recurse -Include *.cs,*.csproj,*.sln,*.json

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $content = $content -replace 'ProjectTemplate', $ProjectName
    Set-Content $file.FullName $content -NoNewline
}

Write-Host ""
Write-Host "✅ Project created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. cd $ProjectName"
Write-Host "2. Update connection strings in src/Api/appsettings.json"
Write-Host "3. (Optional) To change ORM: Edit src/Infrastructure/Extensions/DatabaseExtension.cs (line ~26)"
Write-Host "4. Run: dotnet restore"
Write-Host "5. Run: dotnet build"
Write-Host "6. Run: dotnet ef migrations add InitialCreate --project src/Data --startup-project src/Api"
Write-Host "7. Run: dotnet ef database update --project src/Data --startup-project src/Api"
Write-Host "8. Run: dotnet run --project src/Api"
Write-Host ""
Write-Host "Documentation:" -ForegroundColor Cyan
Write-Host "   - README.md - Complete guide"
Write-Host "   - QUICK-START.md - Quick start in 5 minutes"
Write-Host "   - docs/ORM-GUIDE.md - How to switch ORMs"
Write-Host ""
