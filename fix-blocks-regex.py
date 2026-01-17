#!/usr/bin/env python3
"""
Fix unclosed code blocks in FEATURES.md using regex replacement
"""

import re

def fix_code_blocks():
    with open('docs/FEATURES.md', 'r', encoding='utf-8') as f:
        content = f.read()

    # Pattern: ```lang\n(content)\n**NextSection or --- or ##
    # Replace with: ```lang\n(content)\n```\n\n**NextSection

    # Fix blocks before **X.
    content = re.sub(
        r'(```(?:xml|json|csharp|bash|powershell|yaml|sql|text|http)\n(?:.*?\n)*?)(\*\*\d+\.)',
        r'\1```\n\n\2',
        content,
        flags=re.MULTILINE
    )

    # Fix blocks before ---
    content = re.sub(
        r'(```(?:xml|json|csharp|bash|powershell|yaml|sql|text|http)\n(?:.*?\n)*?)(^---$)',
        r'\1```\n\n\2',
        content,
        flags=re.MULTILINE
    )

    # Fix blocks before ##
    content = re.sub(
        r'(```(?:xml|json|csharp|bash|powershell|yaml|sql|text|http)\n(?:.*?\n)*?)(^##)',
        r'\1```\n\n\2',
        content,
        flags=re.MULTILINE
    )

    with open('docs/FEATURES.md', 'w', encoding='utf-8') as f:
        f.write(content)

    print("✅ Fixed code blocks!")

if __name__ == '__main__':
    fix_code_blocks()
