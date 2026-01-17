#!/usr/bin/env python3
"""Validate all markdown files for unclosed code blocks"""

import os
import glob

def validate_file(filepath):
    """Check if a markdown file has properly closed code blocks"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            lines = f.readlines()
    except Exception as e:
        return None, f"Error reading: {e}"

    depth = 0
    issues = []

    for i, line in enumerate(lines, 1):
        stripped = line.strip()
        if stripped.startswith('```'):
            if stripped == '```':
                # Closing tag
                depth -= 1
                if depth < 0:
                    issues.append(f"Line {i}: Closing without opening")
            else:
                # Opening tag
                if depth > 0:
                    issues.append(f"Line {i}: Opening while depth={depth}")
                depth += 1

    if depth > 0:
        issues.append(f"End of file: {depth} block(s) unclosed")

    return depth == 0 and not issues, issues

def main():
    # Find all .md files
    md_files = []
    for root, dirs, files in os.walk('.'):
        for file in files:
            if file.endswith('.md'):
                md_files.append(os.path.join(root, file))

    md_files.sort()

    print(f"🔍 Checking {len(md_files)} markdown files...\n")

    valid_files = []
    invalid_files = []

    for filepath in md_files:
        is_valid, issues = validate_file(filepath)

        if is_valid is None:
            print(f"⚠️  {filepath}: {issues}")
        elif is_valid:
            valid_files.append(filepath)
        else:
            invalid_files.append((filepath, issues))
            print(f"❌ {filepath}:")
            for issue in issues[:5]:
                print(f"   {issue}")
            if len(issues) > 5:
                print(f"   ... and {len(issues) - 5} more issues")
            print()

    print(f"\n{'='*60}")
    print(f"✅ Valid files: {len(valid_files)}")
    print(f"❌ Invalid files: {len(invalid_files)}")

    if invalid_files:
        print(f"\n📋 Files needing fixes:")
        for filepath, _ in invalid_files:
            print(f"   - {filepath}")
    else:
        print(f"\n🎉 All markdown files are properly formatted!")

if __name__ == '__main__':
    main()
