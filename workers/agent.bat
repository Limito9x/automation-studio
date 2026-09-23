@echo off
setlocal
set VENV_PATH=%~dp0worker\venv

if not exist "%VENV_PATH%\Scripts\python.exe" (
    echo [ERROR] Virtual environment not found at %VENV_PATH%
    echo Please ensure the venv is created and dependencies are installed.
    exit /b 1
)

"%VENV_PATH%\Scripts\python.exe" "%~dp0cli.py" %*
endlocal
