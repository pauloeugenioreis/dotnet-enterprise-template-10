#!/usr/bin/env python3
"""Validate markdown code blocks"""

def validate_markdown(filename):
    with open(filename, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    open_blocks = 0
    errors = []

    for i, line in enumerate(lines, 1):
        if line.strip().startswith('```'):
            if line.strip() == '```':
                open_blocks -= 1
                if open_blocks < 0:
                    errors.append(f"Line {i}: Closing block without opening")
            else:
                open_blocks += 1

    if open_blocks > 0:
        print(f"❌ WARNING: {open_blocks} unclosed blocks at end of file")
        return False
    elif open_blocks < 0:
        print(f"❌ WARNING: {abs(open_blocks)} extra closing blocks")
        return False
    elif errors:
        print(f"❌ Found {len(errors)} errors:")
        for err in errors[:10]:
            print(f"  {err}")
        return False
    else:
        print(f"✅ File is valid! All code blocks are properly closed.")
        return True

if __name__ == '__main__':
    validate_markdown('docs/FEATURES.md')
