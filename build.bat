@echo off
setlocal

set "MSBUILD_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if not exist "%MSBUILD_PATH%" (
  set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
  if exist "%VSWHERE%" (
    for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD_PATH=%%i"
  )
)

if not exist "%MSBUILD_PATH%" (
  echo MSBuild not found. Ensure Visual Studio 2022 Build Tools are installed.
  exit /b 1
)

"%MSBUILD_PATH%" "RoundedScreen.sln" /p:Configuration=Release /m
set "ERR=%ERRORLEVEL%"
if not "%ERR%"=="0" (
  echo Build failed with exit code %ERR%.
  exit /b %ERR%
)

echo Build succeeded. Output: RoundedScreen\bin\Release\RoundedScreen.exe
endlocal
