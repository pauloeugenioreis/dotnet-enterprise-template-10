#!/usr/bin/env python3
"""Fix unclosed code blocks in all markdown files"""

import os

def fix_file(filepath):
    """Fix unclosed code blocks in a markdown file"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            lines = f.readlines()
    except Exception as e:
        return False, f"Error reading: {e}"

    fixed = []
    depth = 0
    changes = 0

    for i, line in enumerate(lines):
        stripped = line.strip()

        if stripped.startswith('```'):
            if stripped == '```':
                # Closing tag
                depth -= 1
                fixed.append(line)
            else:
                # Opening tag
                if depth > 0:
                    # Previous block not closed! Add closing tag before this line
                    fixed.append('```\n')
                    depth -= 1
                    changes += 1
                fixed.append(line)
                depth += 1
        else:
            fixed.append(line)

    # Close any remaining blocks at end of file
    while depth > 0:
        fixed.append('```\n')
        depth -= 1
        changes += 1

    if changes > 0:
        try:
            with open(filepath, 'w', encoding='utf-8') as f:
                f.writelines(fixed)
            return True, f"Fixed {changes} issues"
        except Exception as e:
            return False, f"Error writing: {e}"
    else:
        return True, "No changes needed"

def main():
    # Find all .md files
    md_files = []
    for root, dirs, files in os.walk('.'):
        # Skip node_modules, .git, etc
        if 'node_modules' in root or '.git' in root:
            continue
        for file in files:
            if file.endswith('.md'):
                md_files.append(os.path.join(root, file))

    md_files.sort()

    print(f"🔧 Fixing {len(md_files)} markdown files...\n")

    fixed_count = 0
    error_count = 0

    for filepath in md_files:
        success, message = fix_file(filepath)

        if success:
            if "Fixed" in message:
                print(f"✅ {filepath}: {message}")
                fixed_count += 1
        else:
            print(f"❌ {filepath}: {message}")
            error_count += 1

    print(f"\n{'='*60}")
    print(f"✅ Files fixed: {fixed_count}")
    print(f"❌ Errors: {error_count}")
    print(f"📁 Total files processed: {len(md_files)}")

if __name__ == '__main__':
    main()
