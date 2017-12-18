@if "%1" == "" echo Please specify Debug or Release && EXIT /B
tools\VsixUtil\vsixutil /install "build\%1\GitHub.VisualStudio.vsix"
@echo Installed %1 build of GitHub for Visual Studio
