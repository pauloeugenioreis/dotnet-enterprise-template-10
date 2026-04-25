# PowerShell script to build all Web projects in src/UI/Web

Write-Host "Starting build of all Web projects..." -ForegroundColor Cyan

# Angular
Write-Host "`nBuilding Angular..." -ForegroundColor Green
Push-Location src/UI/Web/Angular
npm install
npm run build
Pop-Location

# React
Write-Host "`nBuilding React..." -ForegroundColor Green
Push-Location src/UI/Web/React
npm install
npm run build
Pop-Location

# Vue
Write-Host "`nBuilding Vue..." -ForegroundColor Green
Push-Location src/UI/Web/Vue
npm install
npm run build
Pop-Location

# Blazor
Write-Host "`nBuilding Blazor..." -ForegroundColor Green
dotnet build src/UI/Web/Blazor/WebApp/App/App.csproj

Write-Host "`nAll builds completed successfully!" -ForegroundColor Cyan
