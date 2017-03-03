@if "%Configuration%" == "" set Configuration=Debug
@if "%IsExperimental%" == "" set IsExperimental=true
@if "%IsProductComponent%" == "" set IsProductComponent=false

call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"

msbuild GitHubVS.sln /t:GitHub_VisualStudio /p:Configuration=%Configuration% /p:IsExperimental=%IsExperimental% /p:IsProductComponent=%IsProductComponent%
@echo Built GitHub.VisualStudio with Configuration=%Configuration% IsExperimental=%IsExperimental% IsProductComponent=%IsProductComponent%
