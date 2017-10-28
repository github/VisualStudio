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
    $Trace = $false
)

Set-StrictMode -Version Latest
if ($Trace) {
    Set-PSDebug -Trace 1
}

$scriptsDirectory = $PSScriptRoot
$rootDirectory = Split-Path ($scriptsDirectory)
$env:PATH = "$scriptsDirectory;$env:PATH"

. $scriptsDirectory\modules.ps1 | out-null

Import-Module (Join-Path $scriptsDirectory "\Modules\Debugging.psm1")
. $scriptsDirectory\Modules\Vsix.ps1 | out-null

Push-Location $rootDirectory

if ($UpdateSubmodules) {
    Update-Submodules
}

if ($Clean) {
	Clean-WorkingTree
}

Write-Output "Building GitHub for Visual Studio..."
Write-Output ""

Build-Solution GitHubVs.sln "Build" $config -Deploy:$Deploy

Pop-Location
