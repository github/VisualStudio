@if "%config%" == "" set config=Debug

call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
msbuild GitHubVS.sln /p:Configuration=%Config%
