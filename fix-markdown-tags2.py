#!/usr/bin/env python3
"""
Remove invalid markdown tags that were incorrectly added.
These are standalone ```markdown, ```bash, etc. lines.
"""

from pathlib import Path

def fix_markdown_file(file_path):
    """Remove standalone markdown/language tags that appear incorrectly."""
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    original_lines = lines[:]
    fixed_lines = []
    removed_count = 0

    language_tags = ['```markdown', '```bash', '```powershell', '```csharp',
                     '```json', '```yaml', '```xml', '```sql', '```text']

    for i, line in enumerate(lines):
        # Check if current line is a standalone language tag
        if line.strip() in language_tags:
            # Check if this looks like an error (not at start of code block)
            # A valid opening tag would have content after it
            # An invalid one would be followed by a heading or paragraph
            if i + 1 < len(lines):
                next_line = lines[i + 1].strip()
                # If next line is a heading or empty (paragraph separator), skip this tag
                if next_line.startswith('#') or next_line == '' or not next_line.startswith('```'):
                    removed_count += 1
                    continue  # Skip adding this line

        fixed_lines.append(line)

    if fixed_lines != original_lines:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(fixed_lines)
        return True, removed_count
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
    if fixed_files:
        print("\nFixed files:")
        for file in fixed_files:
            print(f"  - {file}")

if __name__ == '__main__':
    main()
