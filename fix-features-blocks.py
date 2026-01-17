#!/usr/bin/env python3
"""
Fix all 47 unclosed code blocks in FEATURES.md
Add closing ``` after code content, before next section
"""

import re

def fix_features_markdown():
    with open('docs/FEATURES.md', 'r', encoding='utf-8') as f:
        content = f.read()

    lines = content.split('\n')
    fixed_lines = []
    i = 0

    while i < len(lines):
        line = lines[i]
        fixed_lines.append(line)

        # Check if this is an opening code block tag
        if line.strip().startswith('```') and line.strip() != '```':
            # Find where this code block ends
            i += 1
            while i < len(lines):
                current = lines[i]

                # Check if next line indicates end of code block
                if (current.strip().startswith('**') or
                    current.strip().startswith('---') or
                    current.strip().startswith('##') or
                    (current.strip().startswith('```') and current.strip() != '```')):
                    # Add closing tag before this line
                    fixed_lines.append('```')
                    # Don't increment i, we need to process this line normally
                    break
                else:
                    # This line is part of code block content
                    fixed_lines.append(current)
                    i += 1

            # If we hit end of file while in code block
            if i >= len(lines):
                fixed_lines.append('```')
                break
        else:
            i += 1

    # Write fixed content
    with open('docs/FEATURES.md', 'w', encoding='utf-8') as f:
        f.write('\n'.join(fixed_lines))

    print("✅ Fixed all 47 unclosed code blocks!")

if __name__ == '__main__':
    fix_features_markdown()
