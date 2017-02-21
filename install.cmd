@set PATH=%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\IDE;%PATH%
vsixinstaller /q /u:"c3d3dc68-c977-411f-b3e8-03b0dccf7dfc"
vsixinstaller "%cd%\build\Debug\GitHub.VisualStudio.vsix"
