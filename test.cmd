@rem Tests currently only work on `Release` build.
@if "%config%" == "" set config=Release

call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"

msbuild GitHubVS.sln /p:Configuration=%Config%
VSTest.Console.exe src\UnitTests\bin\%Config%\UnitTests.dll /TestAdapterPath:"."
VSTest.Console.exe src\TrackingCollectionTests\bin\%Config%\TrackingCollectionTests.dll /TestAdapterPath:"."
