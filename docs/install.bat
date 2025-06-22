@echo off
echo Installing Message Documentation dependencies...

REM Remove existing node_modules and package-lock.json
if exist node_modules rmdir /s /q node_modules
if exist package-lock.json del package-lock.json

REM Install dependencies
npm install

echo.
echo Installation complete! You can now run:
echo   npm run docs:dev     (to start development server)
echo   npm run docs:build   (to build for production)
echo   npm run docs:preview (to preview production build)

pause