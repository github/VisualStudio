@if "%Configuration%" == "" set Configuration=Debug

@set path=%cd%\tools\VsixUtil;%path%

vsixutil /install "%cd%\build\%Configuration%\GitHub.VisualStudio.vsix"
@echo Installed %Configuration% build of GitHub for Visual Studio
