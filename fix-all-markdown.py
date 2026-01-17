#!/usr/bin/env python3
"""
Fix all markdown code blocks that are missing language tags.
Intelligently detects the language based on content.
"""

import re
from pathlib import Path

def detect_language(code_content):
    """Detect programming language from code content."""
    code_lower = code_content.lower().strip()
    lines = code_content.strip().split('\n')
    
    # Check for shell comments
    if any(line.strip().startswith('#') and not line.strip().startswith('##') for line in lines):
        # Could be bash/powershell, check for specific commands
        if re.search(r'\$env:|Get-|Set-|New-|Remove-', code_content):
            return 'powershell'
        return 'bash'
    
    # Check for specific patterns
    if re.search(r'^\s*(dotnet|cd|chmod|ls|cat|grep|mkdir|rm|cp|mv|curl|docker)', code_content, re.MULTILINE):
        return 'bash'
    if re.search(r'^\s*(Get-|Set-|New-|Remove-|Import-|Export-|Select-|Where-|\$env:)', code_content, re.MULTILINE):
        return 'powershell'
    if code_content.strip().startswith('{') or '"AppSettings"' in code_content or '"ConnectionString"' in code_content:
        return 'json'
    if re.search(r'^\s*(apiVersion|kind:|metadata:|spec:)', code_content, re.MULTILINE):
        return 'yaml'
    if re.search(r'^\s*(using |namespace |public class|private |protected |var |async |await )', code_content, re.MULTILINE):
        return 'csharp'
    if re.search(r'^\s*(SELECT|INSERT|UPDATE|DELETE|CREATE TABLE|ALTER TABLE)', code_content, re.IGNORECASE):
        return 'sql'
    if re.search(r'^\s*<\?xml|<[a-zA-Z]', code_content):
        return 'xml'
    if re.search(r'(Username:|Password:|Email:|Role:|Token:)', code_content):
        return 'text'
    if 'npm ' in code_content or 'node ' in code_content:
        return 'bash'
    if re.search(r'Server=|Database=|User Id=|Password=', code_content):
        return 'text'
    
    # Default to text if can't determine
    return 'text'

def fix_markdown_file(file_path):
    """Add language tags to code blocks missing them."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    fixed_lines = []
    i = 0
    fixes = []
    
    while i < len(lines):
        line = lines[i]
        
        # Check if this is a code block start without language tag
        if line.strip() == '```':
            # Collect the code content
            code_content = []
            j = i + 1
            while j < len(lines) and lines[j].strip() != '```':
                code_content.append(lines[j])
                j += 1
            
            # Only fix if there's content and a closing tag
            if code_content and j < len(lines):
                content_str = ''.join(code_content)
                lang = detect_language(content_str)
                fixes.append(f"Line {i+1}: Added '{lang}' tag")
                fixed_lines.append(f'```{lang}\n')
                fixed_lines.extend(code_content)
                fixed_lines.append('```\n')
                i = j + 1
                continue
        
        fixed_lines.append(line)
        i += 1
    
    if fixes:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(fixed_lines)
        return True, fixes
    return False, []

def main():
    """Process all markdown files."""
    root_dir = Path('.')
    md_files = list(root_dir.rglob('*.md'))
    
    print(f"Found {len(md_files)} markdown files\n")
    
    total_fixed = 0
    total_blocks = 0
    
    for md_file in md_files:
        if 'node_modules' in str(md_file) or '.git' in str(md_file):
            continue
        
        try:
            was_fixed, fixes = fix_markdown_file(md_file)
            if was_fixed:
                total_fixed += 1
                total_blocks += len(fixes)
                print(f"✓ {md_file} - Fixed {len(fixes)} code blocks")
        except Exception as e:
            print(f"✗ Error: {md_file}: {e}")
    
    print(f"\n{'='*60}")
    print(f"Files fixed: {total_fixed}")
    print(f"Code blocks fixed: {total_blocks}")

if __name__ == '__main__':
    main()
