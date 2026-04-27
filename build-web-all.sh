#!/bin/bash
clear

# Script to build all Web projects in src/UI/Web
set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}Starting build of all Web projects...${NC}"

# Angular
echo -e "\n${GREEN}Building Angular...${NC}"
pushd src/UI/Web/Angular
npm install
npm run build
popd

# React
echo -e "\n${GREEN}Building React...${NC}"
pushd src/UI/Web/React
npm install
npm run build
popd

# Vue
echo -e "\n${GREEN}Building Vue...${NC}"
pushd src/UI/Web/Vue
npm install
npm run build
popd

# Blazor
echo -e "\n${GREEN}Building Blazor...${NC}"
dotnet build src/UI/Web/Blazor/WebApp/App/App.csproj

echo -e "\n${BLUE}All builds completed successfully!${NC}"
