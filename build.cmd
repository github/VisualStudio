@echo off

call vars.cmd

rem Build GitHub for Visual Studio
NuGet restore .\GitHubVS.sln
msbuild .\GitHubVS.sln /p:DeployExtension=False

rem Build GitHub Essentials
NuGet restore .\src\GitHub.VisualStudio.16.sln
msbuild .\src\GitHub.VisualStudio.16.sln /p:DeployExtension=False
