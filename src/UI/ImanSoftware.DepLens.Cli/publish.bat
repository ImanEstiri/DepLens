@echo off
setlocal

rem Auto-detect the project name from the folder this file lives in
for %%I in ("%~dp0.") do set "PROJECT_NAME=%%~nxI"

echo ==^> Publishing %PROJECT_NAME% ...

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\..\publish.ps1" -ProjectName "%PROJECT_NAME%"
echo.
echo ==^> Done. Press any key to close this window...
pause >nul
endlocal
