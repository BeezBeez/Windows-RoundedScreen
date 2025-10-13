# Windows-RoundedScreen

A simple workaround to get rounded screen corners on Windows.

> 🖥️ [Download RoundedScreen.exe](https://github.com/BeezBeez/Windows-RoundedScreen/releases/latest/download/RoundedScreen.exe)

LATEST CHANGES (26/01/2023) :

- hidden from alt+tab list
- made corners a bit smaller
- added an AppIcon
- upped the version number
- added a command to quit the program
- added program to taskbar with corner size options

**THIS PROJECT IS NOT MAINTAINED BUT I FREQUENTLY CHECK THE PULL REQUESTS**

## Prerequisites

- **Visual Studio 2022 Build Tools** with the Managed Desktop workload
- **.NET Framework 4.7.2 Targeting Pack** (installed as a component of Build Tools)

Install prerequisites:

```bat
install.bat
```

## Build

Build the program:

```bat
build.bat
```

Program is built to `RoundedScreen/bin/Release/RoundedScreen.exe`.

## Run

Run the program after building:

```bat
run.bat
```