@echo off

rem Add path to Visual Studio 2019 Tools
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\Common7\Tools" set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\Common7\Tools
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools" set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools" set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Preview\Common7\Tools" set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Preview\Common7\Tools

rem Set up Developer Command Prompt
call VsDevCmd.bat

rem Use local NuGet version
set PATH=%cd%\tools\nuget;%PATH%
