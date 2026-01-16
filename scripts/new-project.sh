#!/bin/bash

# Script to initialize a new project from template
# Usage: ./new-project.sh <ProjectName>

set -e

if [ -z "$1" ]; then
    echo "Error: Project name is required"
    echo "Usage: ./new-project.sh <ProjectName>"
    exit 1
fi

PROJECT_NAME=$1
TEMPLATE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET_DIR="../$PROJECT_NAME"

echo "Creating new project: $PROJECT_NAME"
echo "Template directory: $TEMPLATE_DIR"
echo "Target directory: $TARGET_DIR"

# Create target directory
if [ -d "$TARGET_DIR" ]; then
    echo "Error: Directory $TARGET_DIR already exists"
    exit 1
fi

echo "Copying template files..."
cp -r "$TEMPLATE_DIR" "$TARGET_DIR"

# Navigate to target directory
cd "$TARGET_DIR"

# Remove unnecessary directories and files from new project
echo "Cleaning up..."
rm -rf scripts .git .gitignore

# Clean bin and obj directories
find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

# Rename solution file
mv ProjectTemplate.sln "$PROJECT_NAME.sln"

# Replace ProjectTemplate with actual project name in all files
echo "Renaming project references..."
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" \) -exec sed -i "s/ProjectTemplate/$PROJECT_NAME/g" {} +

# Update RootNamespace in .csproj files
find . -name "*.csproj" -exec sed -i "s/<RootNamespace>ProjectTemplate\./<RootNamespace>$PROJECT_NAME./g" {} +

echo ""
echo "✅ Project created successfully!"
echo ""
echo "Next steps:"
echo "1. cd $PROJECT_NAME"
echo "2. Update connection strings in src/Api/appsettings.json"
echo "3. (Optional) To change ORM: Edit src/Infrastructure/Extensions/DatabaseExtension.cs (line ~26)"
echo "4. Run: dotnet restore"
echo "5. Run: dotnet build"
echo "6. Run: dotnet ef migrations add InitialCreate --project src/Data --startup-project src/Api"
echo "7. Run: dotnet ef database update --project src/Data --startup-project src/Api"
echo "8. Run: dotnet run --project src/Api"
echo ""
echo "📚 Documentation:"
echo "   - README.md - Complete guide"
echo "   - QUICK-START.md - Quick start in 5 minutes"
echo "   - docs/ORM-GUIDE.md - How to switch ORMs"
echo ""
