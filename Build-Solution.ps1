param(
    [ValidateSet('FullBuild', 'RunUnitTests', 'RunIntegrationTests', 'Build', 'Clean')]
    [string]
    $build = "FullBuild"
    ,
    [ValidateSet('Debug', 'Release')]
    [string]
    $config = "Release"
    ,
    [ValidateSet('Any CPU', 'x86', 'x64')]
    [string]
    $platform = "Any CPU"
    ,
    [string]
    $MSBuildVerbosity = "normal"
)

$scriptPath = Split-Path $MyInvocation.MyCommand.Path
$projFile = join-path $scriptPath GitHubVS.msbuild
 
& "$(get-content env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" $projFile /t:$build /p:Platform=$platform /p:Configuration=$config /verbosity:$MSBuildVerbosity /p:VisualStudioVersion=14.0 /p:DeployExtension=false
