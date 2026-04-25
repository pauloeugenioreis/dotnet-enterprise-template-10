# PowerShell script to build all Mobile projects in src/UI/Mobile

Write-Host "Starting build of all Mobile projects..." -ForegroundColor Cyan

# MAUI
Write-Host "`nBuilding .NET MAUI..." -ForegroundColor Green
dotnet build src/UI/Mobile/MauiApp/MauiApp.csproj

# Flutter
Write-Host "`nBuilding Flutter..." -ForegroundColor Green
Push-Location src/UI/Mobile/FlutterApp
flutter pub get
# flutter build apk --debug # Uncomment to actually build
Pop-Location

# React Native (Expo)
Write-Host "`nBuilding React Native (Expo)..." -ForegroundColor Green
Push-Location src/UI/Mobile/ReactNativeApp
npm install
# npx expo export # Uncomment to actually build
Pop-Location

Write-Host "`nAll mobile builds completed (or dependencies installed) successfully!" -ForegroundColor Cyan
