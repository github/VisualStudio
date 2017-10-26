<#
.SYNOPSIS
    Builds and (optionally) runs tests for GitHub for Visual Studio
.DESCRIPTION
    Build GHfVS
.PARAMETER Clean
    When true, all untracked (and ignored) files will be removed from the work
    tree and all submodules. Defaults to false.
.PARAMETER Config
    Debug or Release
.PARAMETER RunTests
    Runs the tests (defauls to false)
#>
[CmdletBinding()]

Param(
    [switch]
    $UpdateSubmodules = $false
    ,
    [switch]
    $Clean = $false
    ,
    [ValidateSet('Debug', 'Release')]
    [string]
    $Config = "Release"
    ,
    [switch]
    $RunTests = $false
)

Set-StrictMode -Version Latest

$scriptsDirectory = Split-Path ($MyInvocation.MyCommand.Path)
$rootDirectory = Split-Path ($scriptsDirectory)
$env:PATH = "$scriptsDirectory;$env:PATH"


Write-Output $scriptsDirectory  $rootDirectory

Import-Module (Join-Path $scriptsDirectory "\Modules\BuildUtils.psm1") 3> $null # Ignore warnings
Import-Module (Join-Path $scriptsDirectory "\Modules\Debugging.psm1")
Import-Module (Join-Path $scriptsDirectory "\Modules\Vsix.psm1")

. $scriptsDirectory\common.ps1

Push-Location $rootDirectory

$nuget = Join-Path $rootDirectory "tools\nuget\nuget.exe"

if ($UpdateSubmodules) {
    Update-Submodules
}

if ($Clean) {
	Clean-WorkingTree
}

Write-Output "Building GitHub for Visual Studio..."
Write-Output ""
Build-Solution GitHubVs.sln "Build" $config

$exitCode = 0

if ($RunTests) {
    .\Run-Tests $Config
}

Pop-Location
exit $exitCode