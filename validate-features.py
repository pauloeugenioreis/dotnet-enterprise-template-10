#!/usr/bin/env python3
"""Validate and fix FEATURES.md code blocks"""

def validate_and_fix():
    with open('docs/FEATURES.md', 'r', encoding='utf-8') as f:
        lines = f.readlines()

    # First pass: identify issues
    depth = 0
    issues = []

    for i, line in enumerate(lines, 1):
        stripped = line.strip()
        if stripped.startswith('```'):
            if stripped == '```':
                # Closing tag
                depth -= 1
                if depth < 0:
                    issues.append(f"Line {i}: Closing without opening (depth={depth})")
            else:
                # Opening tag
                if depth > 0:
                    issues.append(f"Line {i}: Opening while previous block unclosed (depth was {depth})")
                depth += 1

    if depth > 0:
        issues.append(f"End of file: {depth} block(s) remain unclosed")

    if issues:
        print(f"❌ Found {len(issues)} issues:")
        for issue in issues[:20]:
            print(f"  {issue}")
        return False
    else:
        print("✅ All code blocks are properly closed!")
        return True

if __name__ == '__main__':
    validate_and_fix()
