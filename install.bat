@echo off
setlocal

:: Install Visual Studio 2022 Build Tools with Managed Desktop workload and .NET 4.7.2 targeting pack
winget install -e --id Microsoft.VisualStudio.2022.BuildTools --override "--quiet --wait --norestart --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools --add Microsoft.Net.Component.4.7.2.TargetingPack --includeRecommended"

if %ERRORLEVEL% NEQ 0 (
  echo.
  echo Installation may have failed or no upgrade was available. Review winget output above.
)

endlocal
