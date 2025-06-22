#!/bin/bash

echo "Installing Message Documentation dependencies..."

# Remove existing node_modules and package-lock.json
rm -rf node_modules package-lock.json

# Install dependencies
npm install

echo ""
echo "Installation complete! You can now run:"
echo "  npm run docs:dev     (to start development server)"
echo "  npm run docs:build   (to build for production)"
echo "  npm run docs:preview (to preview production build)"