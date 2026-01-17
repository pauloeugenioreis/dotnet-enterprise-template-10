# Script para adicionar linguagens aos code blocks

$files = Get-ChildItem -Path . -Include *.md -Recurse | Where-Object { $_.FullName -notlike "*node_modules*" }

$totalChanges = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $changed = $false

    # Detect context and add appropriate language tags
    # Shell commands (bash/powershell/cmd)
    $content = $content -replace '(?m)^```\s*\n((?:cd |docker |kubectl |npm |dotnet |git |.\/))', '```bash\n$1'
    $content = $content -replace '(?m)^```\s*\n((?:Get-|Set-|New-|Remove-|\$))', '```powershell\n$1'

    # JSON blocks
    $content = $content -replace '(?m)^```\s*\n(\s*\{[\s\S]*?"[^"]+"\s*:)', '```json\n$1'

    # C# code
    $content = $content -replace '(?m)^```\s*\n((?:public |private |protected |internal |using |namespace |class |interface |var |await |async ))', '```csharp\n$1'

    # YAML
    $content = $content -replace '(?m)^```\s*\n((?:apiVersion|kind|metadata|spec|name):', '```yaml\n$1'

    # XML
    $content = $content -replace '(?m)^```\s*\n(<\?xml|<Project|<ItemGroup|<PropertyGroup|<PackageReference)', '```xml\n$1'

    # SQL
    $content = $content -replace '(?m)^```\s*\n((?:SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP) )', '```sql\n$1'

    # Text/output blocks (terminal output, logs)
    $content = $content -replace '(?m)^```\s*\n((?:PS C:|C:\\|\/|~|>|\+|✓|✅|❌|\[))', '```text\n$1'

    # Markdown blocks (for documentation examples)
    $content = $content -replace '(?m)^```\s*\n(#{1,6} )', '```markdown\n$1'

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $changed = $true
        $totalChanges++
        Write-Host "[FIXED] $($file.Name)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Code Block Language Tags Added!" -ForegroundColor Green
Write-Host "  Total files changed: $totalChanges" -ForegroundColor Yellow
Write-Host "===============================================" -ForegroundColor Cyan
