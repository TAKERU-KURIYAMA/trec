@echo off
REM Windows batch wrapper for auto-confirm.py
REM Usage: auto-confirm.bat <command> [args...]

if "%1"=="" (
    echo Usage: auto-confirm.bat ^<command^> [args...]
    echo Example: auto-confirm.bat claude
    echo Example: auto-confirm.bat git push origin main
    exit /b 1
)

python "%~dp0auto-confirm.py" %*