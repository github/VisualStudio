@if "%1" == "" echo Please specify Debug or Release && EXIT /B
powershell -ExecutionPolicy Unrestricted scripts\build.ps1 -Package:$true -Config:%1
