#!/usr/bin/env python3
"""
Add closing ``` before opening a new block when previous block is unclosed
"""

def fix_unclosed():
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
                    # Previous block not closed! Add closing tag
                    fixed.append('```\n')
                    depth -= 1
                fixed.append(line)
                depth += 1
        else:
            fixed.append(line)

    # Close any remaining
    if depth > 0:
        fixed.append('```\n')

    with open('docs/FEATURES.md', 'w', encoding='utf-8') as f:
        f.writelines(fixed)

    print(f"✅ Fixed all unclosed blocks!")

if __name__ == '__main__':
    fix_unclosed()
