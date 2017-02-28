@if "%config%" == "" set config=Debug

@set path=%cd%\tools\VsixUtil;%path%

@echo Installing `%config%` build of GitHub for Visual Studio
vsixutil /install "%cd%\build\%config%\GitHub.VisualStudio.vsix" /s Enterprise;Professional;Community
