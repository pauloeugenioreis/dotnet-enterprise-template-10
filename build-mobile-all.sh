#!/bin/bash

# Script to build all Mobile projects in src/UI/Mobile
set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}Starting build of all Mobile projects...${NC}"

# MAUI
echo -e "\n${GREEN}Building .NET MAUI...${NC}"
dotnet build src/UI/Mobile/MauiApp/MauiApp.csproj

# Flutter
echo -e "\n${GREEN}Building Flutter...${NC}"
pushd src/UI/Mobile/FlutterApp
flutter pub get
# flutter build apk --debug # Uncomment to actually build
popd

# React Native (Expo)
echo -e "\n${GREEN}Building React Native (Expo)...${NC}"
pushd src/UI/Mobile/ReactNativeApp
npm install
# npx expo export # Uncomment to actually build
popd

echo -e "\n${BLUE}All mobile builds completed (or dependencies installed) successfully!${NC}"
