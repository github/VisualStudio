<#
.SYNOPSIS
    Runs tests for GitHub for Visual Studio
.DESCRIPTION
    Build GHfVS
.PARAMETER Clean
    When true, all untracked (and ignored) files will be removed from the work
    tree and all submodules. Defaults to false.
#>
[CmdletBinding()]

Param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Config = "Release"
    ,
    [int]
    $TimeoutDuration = 180
    ,
    [switch]
    $AppVeyor = $false

)

Set-StrictMode -Version Latest

$scriptsDirectory = Split-Path ($MyInvocation.MyCommand.Path)
$rootDirectory = Split-Path ($scriptsDirectory)
$env:PATH = "$scriptsDirectory;$env:PATH"

Write-Output $scriptsDirectory $rootDirectory

. $scriptsDirectory\common.ps1

Push-Location $rootDirectory

Write-Output "Running Tracking Collection Tests..."
$result = & scripts\Run-NUnit src TrackingCollectionTests $TimeoutDuration $config
if ($result.ExitCode -eq 0) {
} else {
    $exitCode = $result.ExitCode
}

Write-Output "Running Unit Tests..."
$result = & scripts\Run-XUnit src UnitTests $TimeoutDuration $config
if ($result.ExitCode -eq 0) {
} else {
    $exitCode = $result.ExitCode
}

Write-Output "Running GitHub.InlineReviews.UnitTests..."
$result = & scripts\Run-XUnit test GitHub.InlineReviews.UnitTests $TimeoutDuration $config
if ($result.ExitCode -eq 0) {
} else {
    $exitCode = $result.ExitCode
}

Write-Output "Running GitHub.UI.UnitTests..."
$result = & scripts\Run-NUnit test GitHub.UI.UnitTests $TimeoutDuration $config
if ($result.ExitCode -eq 0) {
} else {
    $exitCode = $result.ExitCode
}

Pop-Location
exit $exitCode