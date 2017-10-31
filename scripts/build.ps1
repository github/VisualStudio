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
    $Deploy = $false
    ,
    [switch]
    $AppVeyor = $false
    ,
    [switch]
    $SkipVersionBump = $false
    ,
    [switch]
    $Trace = $false
)

Set-StrictMode -Version Latest
if ($Trace) {
    Set-PSDebug -Trace 1
}

. $PSScriptRoot\modules.ps1 | out-null
$env:PATH = "$scriptsDirectory;$scriptsDirectory\Modules;$env:PATH"

Import-Module $scriptsDirectory\Modules\Debugging.psm1
Vsix | out-null
WiX | out-null

Push-Location $rootDirectory

if ($UpdateSubmodules) {
    Update-Submodules
}

if ($Clean) {
	Clean-WorkingTree
}

$fullBuild = Test-Path env:GHFVS_KEY
$publishable = $fullBuild -and $AppVeyor -and ($env:APPVEYOR_PULL_REQUEST_NUMBER -or $env:APPVEYOR_REPO_BRANCH -eq "master")
if ($publishable) { #forcing a deploy flag for CI
    $Deploy = $true
}

if ($publishable -or ($Deploy -and $Config -eq "Release" -and !$SkipVersionBump)) {
    Write-Output "Bumping the version"
    Bump-Version -BumpBuild
}

if ($publishable) {
    Write-Output "Building and packaging GitHub for Visual Studio"
} else {
    Write-Output "Building GitHub for Visual Studio"
}

Build-Solution GitHubVs.sln "Build" $config $Deploy

Pop-Location
