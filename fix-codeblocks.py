#!/usr/bin/env python3
"""
Script para adicionar tags de linguagem a code blocks em arquivos Markdown
"""

import re
import os
from pathlib import Path

def detect_language(code_content):
    """Detecta a linguagem baseada no conteúdo do code block"""

    # C# patterns
    if re.search(r'\b(using |namespace |class |public |private |protected |internal |async |await |var |Task<)', code_content):
        return 'csharp'

    # JSON patterns
    if re.search(r'^\s*\{[\s\S]*"[^"]+"\s*:', code_content):
        return 'json'

    # PowerShell patterns
    if re.search(r'(^\$|Get-|Set-|New-|Remove-|Write-Host)', code_content):
        return 'powershell'

    # Bash/Shell patterns
    if re.search(r'(^cd |^docker |^kubectl |^npm |^dotnet |^git |^chmod |^\.\/|^bash)', code_content):
        return 'bash'

    # XML patterns
    if re.search(r'(<\?xml|<Project|<ItemGroup|<PropertyGroup|<PackageReference)', code_content):
        return 'xml'

    # YAML patterns
    if re.search(r'(^apiVersion:|^kind:|^metadata:|^spec:|^name:)', code_content, re.MULTILINE):
        return 'yaml'

    # SQL patterns
    if re.search(r'\b(SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP)\b', code_content, re.IGNORECASE):
        return 'sql'

    # Markdown patterns
    if re.search(r'^#{1,6} ', code_content, re.MULTILINE):
        return 'markdown'

    # Text/output (terminal output, logs)
    if re.search(r'(^PS C:|^C:\\|^\[|^✓|^✅|^❌|^\+)', code_content, re.MULTILINE):
        return 'text'

    # Default to text if unknown
    return 'text'

def fix_markdown_file(file_path):
    """Corrige code blocks em um arquivo markdown"""

    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    original_content = content
    changes = 0

    # Pattern para encontrar code blocks sem linguagem
    pattern = r'```\s*\n((?:(?!```).)+)\n```'

    def replace_block(match):
        nonlocal changes
        code_content = match.group(1)
        language = detect_language(code_content)
        changes += 1
        return f'```{language}\n{code_content}\n```'

    content = re.sub(pattern, replace_block, content, flags=re.DOTALL)

    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return changes

    return 0

def main():
    """Processa todos os arquivos markdown"""

    total_files = 0
    total_changes = 0
    changed_files = []

    # Encontra todos os arquivos .md
    for md_file in Path('.').rglob('*.md'):
        if 'node_modules' in str(md_file):
            continue

        total_files += 1
        changes = fix_markdown_file(md_file)

        if changes > 0:
            total_changes += changes
            changed_files.append((str(md_file), changes))
            print(f'[FIXED] {md_file.name}: {changes} code blocks')

    print('\n' + '=' * 50)
    print(f'Markdown Code Block Fixer Complete!')
    print(f'  Total files processed: {total_files}')
    print(f'  Files changed: {len(changed_files)}')
    print(f'  Total code blocks fixed: {total_changes}')
    print('=' * 50)

    if changed_files:
        print('\nChanged files:')
        for file_path, changes in changed_files:
            print(f'  - {file_path} ({changes} blocks)')

if __name__ == '__main__':
    main()
