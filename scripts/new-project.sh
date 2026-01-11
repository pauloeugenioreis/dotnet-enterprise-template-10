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

# Remove scripts directory from new project
rm -rf scripts

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
echo "3. Choose your database provider and uncomment in src/Data/Data.csproj"
echo "4. Run: dotnet restore"
echo "5. Run: dotnet build"
echo "6. Run: dotnet ef migrations add InitialCreate --project src/Data --startup-project src/Api"
echo "7. Run: dotnet ef database update --project src/Data --startup-project src/Api"
echo "8. Run: dotnet run --project src/Api"
echo ""
