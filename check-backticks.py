#!/usr/bin/env python3
"""Check backtick encoding"""

with open('docs/FEATURES.md', 'r', encoding='utf-8') as f:
    lines = f.readlines()

line44 = lines[43].strip()
literal = '```'

print(f'line44: {repr(line44)}')
print(f'literal: {repr(literal)}')
print(f'line44 bytes: {line44.encode()}')
print(f'literal bytes: {literal.encode()}')
print(f'Equal: {line44 == literal}')
print(f'line44 chars: {[hex(ord(c)) for c in line44]}')
print(f'literal chars: {[hex(ord(c)) for c in literal]}')
