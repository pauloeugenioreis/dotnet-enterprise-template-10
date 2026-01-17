# Script para corrigir problemas comuns de markdown linting

$files = Get-ChildItem -Path . -Include *.md -Recurse | Where-Object { $_.FullName -notlike "*node_modules*" }

$totalFiles = $files.Count
$processedFiles = 0
$changedFiles = 0

foreach ($file in $files) {
    $processedFiles++
    Write-Host "[$processedFiles/$totalFiles] Processing: $($file.Name)" -ForegroundColor Cyan

    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $changes = 0

    # Fix 1: Add space after # in headings
    $pattern1 = '(?m)^(#{1,6})([^\s#])'
    if ($content -match $pattern1) {
        $content = $content -replace $pattern1, '$1 $2'
        $changes++
        Write-Host "  [OK] Fixed headings without space" -ForegroundColor Green
    }

    # Fix 2: Remove trailing spaces
    $pattern2 = '(?m)[ \t]+$'
    if ($content -match $pattern2) {
        $content = $content -replace $pattern2, ''
        $changes++
        Write-Host "  [OK] Removed trailing spaces" -ForegroundColor Green
    }

    # Fix 3: Ensure single blank line before headings
    $pattern3 = '(?m)(?<!^)(?<!\n\n)(^#{1,6} .+)'
    if ($content -match $pattern3) {
        $content = $content -replace $pattern3, "`n"+'$1'
        $changes++
        Write-Host "  [OK] Added blank lines before headings" -ForegroundColor Green
    }

    # Fix 4: Add language to code blocks
    $pattern4 = '(?m)^```\s*$\n(?!```)'
    if ($content -match $pattern4) {
        Write-Host "  [WARN] Code blocks without language found" -ForegroundColor Yellow
    }

    # Fix 5: Ensure file ends with newline
    if ($content -notmatch '\n$') {
        $content = $content + "`n"
        $changes++
        Write-Host "  [OK] Added final newline" -ForegroundColor Green
    }

    # Fix 6: Remove multiple consecutive blank lines
    $pattern6 = '\n{4,}'
    if ($content -match $pattern6) {
        $content = $content -replace $pattern6, "`n`n`n"
        $changes++
        Write-Host "  [OK] Reduced multiple blank lines" -ForegroundColor Green
    }

    # Save if changes were made
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $changedFiles++
        $fixWord = if ($changes -eq 1) { "fix" } else { "fixes" }
        Write-Host "  [DONE] File updated with $changes $fixWord" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
