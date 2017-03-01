@if "%config%" == "" set config=Debug

call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"

@rem Build the GitHub.VisualStudio project / VSIX file
msbuild GitHubVS.sln /p:Configuration=%Config% /t:GitHub_VisualStudio /p:IsProductComponent=false