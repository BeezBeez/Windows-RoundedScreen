@echo off
setlocal

set EXE=RoundedScreen\bin\Release\RoundedScreen.exe
if not exist "%EXE%" (
  echo Build output not found at %EXE%.
  echo Run build.bat first.
  exit /b 1
)

"%EXE%"
endlocal
