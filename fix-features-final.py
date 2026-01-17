#!/usr/bin/env python3
"""Fix unclosed code blocks in FEATURES.md"""

def fix_blocks():
    with open('docs/FEATURES.md', 'r', encoding='utf-8') as f:
        lines = f.readlines()

    fixed = []
    depth = 0

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
                fixed.append(line)
                depth += 1
        else:
            fixed.append(line)

    # Close any remaining blocks at end of file
    while depth > 0:
        fixed.append('```\n')
        depth -= 1

    with open('docs/FEATURES.md', 'w', encoding='utf-8') as f:
        f.writelines(fixed)

    print(f"✅ Fixed all unclosed code blocks!")

if __name__ == '__main__':
    fix_blocks()
