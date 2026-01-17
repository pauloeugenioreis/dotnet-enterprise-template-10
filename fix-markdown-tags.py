#!/usr/bin/env python3
"""
Remove invalid markdown tags that were incorrectly added between code blocks.
These are standalone ```markdown, ```bash, etc. lines that shouldn't exist.
"""

import os
import re
from pathlib import Path

def fix_markdown_file(file_path):
    """Remove standalone markdown/language tags that appear after closing code blocks."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    original_lines = lines[:]
    fixed_lines = []
    i = 0

    while i < len(lines):
        current_line = lines[i]

        # Check if current line is a closing ```
        if current_line.strip() == '```':
            # Check if next line is a standalone language tag
            if i + 1 < len(lines):
                next_line = lines[i + 1].strip()
                # If next line is ```markdown, ```bash, etc. (standalone tag)
                if next_line in ['```markdown', '```bash', '```powershell', '```csharp',
                                '```json', '```yaml', '```xml', '```sql', '```text']:
                    # Keep current closing ```, skip the invalid tag line
                    fixed_lines.append(current_line)
                    i += 2  # Skip both current and next line
                    continue

        fixed_lines.append(current_line)
        i += 1

    if fixed_lines != original_lines:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(fixed_lines)
        return True, len(original_lines) - len(fixed_lines)
    return False, 0

def main():
    """Process all markdown files in the project."""
    root_dir = Path('.')
    fixed_files = []
    total_lines_removed = 0

    # Find all .md files
    md_files = list(root_dir.rglob('*.md'))

    print(f"Found {len(md_files)} markdown files\n")

    for md_file in md_files:
        # Skip node_modules and other dependencies
        if 'node_modules' in str(md_file) or '.git' in str(md_file):
            continue

        try:
            was_fixed, lines_removed = fix_markdown_file(md_file)
            if was_fixed:
                fixed_files.append(str(md_file))
                total_lines_removed += lines_removed
                print(f"✓ Fixed: {md_file} ({lines_removed} invalid tags removed)")
        except Exception as e:
            print(f"✗ Error processing {md_file}: {e}")

    print(f"\n{'='*60}")
    print(f"Total files fixed: {len(fixed_files)}")
    print(f"Total invalid tags removed: {total_lines_removed}")
