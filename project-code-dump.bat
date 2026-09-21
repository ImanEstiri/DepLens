@echo off
setlocal enabledelayedexpansion

REM ===================== راه‌اندازی رنگ (ANSI) =====================
for /F "delims=#" %%a in ('"prompt #$E# & for %%b in (1) do rem"') do set "ESC=%%a"
set "C_RESET=%ESC%[0m"
set "C_CS=%ESC%[96m"
set "C_RAZOR=%ESC%[93m"
set "C_CSS=%ESC%[95m"
set "C_JS=%ESC%[92m"
set "C_TS=%ESC%[92m"
set "C_HTML=%ESC%[94m"
set "C_XAML=%ESC%[94m"
set "C_JSON=%ESC%[90m"
set "C_CONFIG=%ESC%[90m"
set "C_PROJ=%ESC%[90m"
set "C_OTHER=%ESC%[37m"
set "C_HEAD=%ESC%[97m"
set "C_OK=%ESC%[92m"

REM ===================== تنظیمات =====================
set "ROOT=%~dp0"
set "ROOT=%ROOT:~0,-1%"
set "OUT=%ROOT%\project-code-dump-result.md"

set "EXTENSIONS=.razor .css .cs .js .ts .html .cshtml .json .scss .less .xaml .config .csproj"
set "EXCLUDE_DIRS=\bin\ \obj\ \node_modules\ \.git\ \.vs\ \.idea\ \wwwroot\lib\"
set "GT=>"

set /a TOTAL=0
set /a CNT_CS=0
set /a CNT_RAZOR=0
set /a CNT_CSS=0
set /a CNT_JS=0
set /a CNT_TS=0
set /a CNT_HTML=0
set /a CNT_XAML=0
set /a CNT_JSON=0
set /a CNT_CONFIG=0
set /a CNT_PROJ=0
set /a CNT_OTHER=0

REM ===================== شروع =====================
if exist "%OUT%" del "%OUT%"

echo %C_HEAD%============================================================%C_RESET%
echo %C_HEAD% Scanning project ...%C_RESET%
echo %C_HEAD% Root: %ROOT%%C_RESET%
echo %C_HEAD%============================================================%C_RESET%
echo(

for /r "%ROOT%" %%F in (*) do (
    set "SKIP=0"
    set "FULLPATH=%%F"
    set "EXT=%%~xF"

    for %%D in (%EXCLUDE_DIRS%) do (
        if not "!FULLPATH:%%D=!"=="!FULLPATH!" set "SKIP=1"
    )

    if !SKIP! == 0 (
        set "MATCH=0"
        for %%E in (%EXTENSIONS%) do (
            if /I "!EXT!"=="%%E" set "MATCH=1"
        )

        if !MATCH! == 1 (
            REM -------- تشخیص دسته و رنگ بر اساس پسوند --------
            set "CATID=OTHER"
            set "CATLABEL=OTHER "
            set "CLR=%C_OTHER%"
            set "LANG="
            if /I "!EXT!"==".cs"      (set "CATID=CS"     & set "CATLABEL=C#    " & set "CLR=%C_CS%"    & set "LANG=csharp")
            if /I "!EXT!"==".razor"   (set "CATID=RAZOR"  & set "CATLABEL=RAZOR " & set "CLR=%C_RAZOR%" & set "LANG=razor")
            if /I "!EXT!"==".cshtml"  (set "CATID=RAZOR"  & set "CATLABEL=RAZOR " & set "CLR=%C_RAZOR%" & set "LANG=cshtml")
            if /I "!EXT!"==".css"     (set "CATID=CSS"    & set "CATLABEL=CSS   " & set "CLR=%C_CSS%"   & set "LANG=css")
            if /I "!EXT!"==".scss"    (set "CATID=CSS"    & set "CATLABEL=CSS   " & set "CLR=%C_CSS%"   & set "LANG=scss")
            if /I "!EXT!"==".less"    (set "CATID=CSS"    & set "CATLABEL=CSS   " & set "CLR=%C_CSS%"   & set "LANG=less")
            if /I "!EXT!"==".js"      (set "CATID=JS"     & set "CATLABEL=JS    " & set "CLR=%C_JS%"    & set "LANG=javascript")
            if /I "!EXT!"==".ts"      (set "CATID=TS"     & set "CATLABEL=TS    " & set "CLR=%C_TS%"    & set "LANG=typescript")
            if /I "!EXT!"==".html"    (set "CATID=HTML"   & set "CATLABEL=HTML  " & set "CLR=%C_HTML%"  & set "LANG=html")
            if /I "!EXT!"==".xaml"    (set "CATID=XAML"   & set "CATLABEL=XAML  " & set "CLR=%C_XAML%"  & set "LANG=xml")
            if /I "!EXT!"==".json"    (set "CATID=JSON"   & set "CATLABEL=JSON  " & set "CLR=%C_JSON%"  & set "LANG=json")
            if /I "!EXT!"==".config"  (set "CATID=CONFIG" & set "CATLABEL=CONFIG" & set "CLR=%C_CONFIG%"& set "LANG=xml")
            if /I "!EXT!"==".csproj"  (set "CATID=PROJ"   & set "CATLABEL=PROJ  " & set "CLR=%C_PROJ%"  & set "LANG=xml")

            set /a TOTAL+=1
            set /a "CNT_!CATID!+=1"
            set "NUMPAD=0000!TOTAL!"
            set "NUMPAD=!NUMPAD:~-4!"

            REM -------- محاسبه‌ی مسیر نمایشی --------
            set "FULLDIR=%%~dpF"
            set "RELDIR=!FULLDIR:%ROOT%\=!"
            if defined RELDIR set "RELDIR=!RELDIR:~0,-1!"
            if defined RELDIR set "RELDIR=!RELDIR:\= %GT% !"

            if defined RELDIR (
                set "DISPLAY=!RELDIR! %GT% %%~nF"
            ) else (
                set "DISPLAY=%%~nF"
            )

            REM -------- لاگ رنگی توی کنسول (وارد فایل txt نمی‌شود) --------
            echo !CLR![!NUMPAD!] [!CATLABEL!] !DISPLAY!!C_RESET!

            REM -------- نوشتن محتوا در فایل خروجی (Markdown) --------
            echo ### !DISPLAY! >> "%OUT%"
            echo(>> "%OUT%"
            echo ```!LANG! >> "%OUT%"
            type "%%F" >> "%OUT%"
            echo(>> "%OUT%"
            echo ``` >> "%OUT%"
            echo(>> "%OUT%"
            echo(>> "%OUT%"
        )
    )
)

echo(
echo %C_HEAD%============================================================%C_RESET%
echo %C_HEAD% Summary:%C_RESET%
if !CNT_CS!     GTR 0 echo %C_CS%   C#     : !CNT_CS!%C_RESET%
if !CNT_RAZOR!  GTR 0 echo %C_RAZOR%   RAZOR  : !CNT_RAZOR!%C_RESET%
if !CNT_CSS!    GTR 0 echo %C_CSS%   CSS    : !CNT_CSS!%C_RESET%
if !CNT_JS!     GTR 0 echo %C_JS%   JS     : !CNT_JS!%C_RESET%
if !CNT_TS!     GTR 0 echo %C_TS%   TS     : !CNT_TS!%C_RESET%
if !CNT_HTML!   GTR 0 echo %C_HTML%   HTML   : !CNT_HTML!%C_RESET%
if !CNT_XAML!   GTR 0 echo %C_XAML%   XAML   : !CNT_XAML!%C_RESET%
if !CNT_JSON!   GTR 0 echo %C_JSON%   JSON   : !CNT_JSON!%C_RESET%
if !CNT_CONFIG! GTR 0 echo %C_CONFIG%   CONFIG : !CNT_CONFIG!%C_RESET%
if !CNT_PROJ!   GTR 0 echo %C_PROJ%   PROJ   : !CNT_PROJ!%C_RESET%
if !CNT_OTHER!  GTR 0 echo %C_OTHER%   OTHER  : !CNT_OTHER!%C_RESET%
echo %C_HEAD%------------------------------------------------------------%C_RESET%
echo %C_OK% Total files : !TOTAL!%C_RESET%
echo %C_OK% Output saved to: %OUT%%C_RESET%
echo %C_HEAD%============================================================%C_RESET%

prompt $P$G
pause