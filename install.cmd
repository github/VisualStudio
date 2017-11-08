Configuration=%1
@set path=%cd%\tools\VsixUtil;%path%
tools\VsixUtil\vsixutil /install "build\%Configuration%\GitHub.VisualStudio.vsix"
@echo Installed %Configuration% build of GitHub for Visual Studio
