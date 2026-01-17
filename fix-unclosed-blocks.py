#!/usr/bin/env python3
"""Fix unclosed code blocks in FEATURES.md"""

import re

def fix_unclosed_blocks(filename):
    with open(filename, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    fixed_lines = []
    in_code_block = False
    code_block_lang = None

    for i, line in enumerate(lines):
        # Check if line starts a code block
        if line.strip().startswith('```'):
            if line.strip() == '```':
                # Closing tag
                in_code_block = False
                code_block_lang = None
                fixed_lines.append(line)
            else:
                # Opening tag with language
                if in_code_block:
                    # Previous block wasn't closed! Add closing tag
                    fixed_lines.append('```\n')
                in_code_block = True
                code_block_lang = line.strip()[3:].strip()
                fixed_lines.append(line)
        else:
            # Check if we need to close a block
            # A code block should close before: ---, ###, **, or another ```
            if in_code_block:
                next_line = lines[i + 1] if i + 1 < len(lines) else ''

                # Close block if next line is a section marker
                if (next_line.strip().startswith('---') or
                    next_line.strip().startswith('###') or
                    next_line.strip().startswith('##') or
                    next_line.strip().startswith('**') or
                    (next_line.strip().startswith('```') and next_line.strip() != '```')):
                    # Check if current line is not empty or is last content line
                    if line.strip() or not next_line.strip().startswith('```'):
                        fixed_lines.append(line)
                        # Add closing tag after content, before next section
                        if line.strip():  # Only if current line has content
                            fixed_lines.append('```\n')
                            in_code_block = False
                            code_block_lang = None
                        continue

            fixed_lines.append(line)

    # Close any remaining open block at end of file
    if in_code_block:
        fixed_lines.append('```\n')

    # Write fixed content
    with open(filename, 'w', encoding='utf-8') as f:
        f.writelines(fixed_lines)

    print(f"Fixed {filename}")

if __name__ == '__main__':
    fix_unclosed_blocks('docs/FEATURES.md')
    print("✅ Fixed all unclosed code blocks!")
