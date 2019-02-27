@if "%1" == "" echo Please specify Debug or Release && EXIT /B
powershell -ExecutionPolicy Unrestricted scripts\test.ps1 -Config:%1
