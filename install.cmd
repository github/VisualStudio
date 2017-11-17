@set Configuration=%1
@if "%Configuration%" == "" echo Please specify Debug or Release
tools\VsixUtil\vsixutil /install "build\%Configuration%\GitHub.VisualStudio.vsix"
@echo Installed %Configuration% build of GitHub for Visual Studio
