@echo off
:: run_worker.bat
:: Script khởi chạy Python Worker tự động quản lý và kích hoạt Virtual Environment (venv) trên Windows.

set VENV_DIR=%~dp0venv
set REQUIRE_FILE=%~dp0requirements.txt

echo ===================================================
echo [Automation Pipeline] Dang kiem tra moi truong Python...
echo ===================================================

:: 1. Kiem tra thu muc venv da ton tai chua, neu chua thi tu dong tao moi
if not exist "%VENV_DIR%" (
    echo [i] Khong tim thay Virtual Environment tai: %VENV_DIR%
    echo [i] Dang tien hanh tao moi venv...
    python -m venv "%VENV_DIR%"
    if errorlevel 1 (
        echo [!] ERROR: Khong the tao venv. Vui long kiem tra da cai dat Python va them vao PATH chua.
        pause
        exit /b 1
    )
    echo [OK] Da tao thanh cong venv.
)

:: 2. Tu dong cai dat/cap nhat dependencies tu requirements.txt ma khong can activate venv
echo [i] Dang dong bo hoa dependencies voi requirements.txt...
"%VENV_DIR%\Scripts\python.exe" -m pip install --upgrade pip
"%VENV_DIR%\Scripts\python.exe" -m pip install -r "%REQUIRE_FILE%"
if errorlevel 1 (
    echo [!] WARNING: Cai dat mot so dependencies bi loi. Vui long kiem tra file requirements.txt
)

echo [OK] Moi truong san sang!
echo ===================================================
echo [Automation Pipeline] Dang khoi chay Worker...
echo ===================================================

:: 3. Chay worker bang chinh interpreter cua venv de tranh conflict global path
cd /d "%~dp0"
"%VENV_DIR%\Scripts\python.exe" -m app

pause
