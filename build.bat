@echo off
setlocal

set MSBUILD_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe
if not exist "%MSBUILD_PATH%" (
  echo MSBuild not found at: %MSBUILD_PATH%
  echo Ensure Visual Studio 2022 Build Tools are installed.
  exit /b 1
)

"%MSBUILD_PATH%" "RoundedScreen.sln" /p:Configuration=Release /m
set ERR=%ERRORLEVEL%
if %ERR% NEQ 0 (
  echo Build failed with exit code %ERR%.
  exit /b %ERR%
)

echo Build succeeded. Output: RoundedScreen\bin\Release\RoundedScreen.exe
endlocal
