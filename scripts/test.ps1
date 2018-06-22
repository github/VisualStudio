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
    $Trace = $false

)

Set-StrictMode -Version Latest
if ($Trace) {
    Set-PSDebug -Trace 1
}

$env:PATH = "$PSScriptRoot;$env:PATH"

$exitcode = 0

Write-Output "Running Tracking Collection Tests..."
Run-NUnit test TrackingCollectionTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 1
}

Write-Output "Running GitHub.Api.UnitTests..."
Run-NUnit test GitHub.Api.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 2
}

Write-Output "Running GitHub.App.UnitTests..."
Run-NUnit test GitHub.App.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 3
}

Write-Output "Running GitHub.Exports.Reactive.UnitTests..."
Run-NUnit test GitHub.Exports.Reactive.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 4
}

Write-Output "Running GitHub.Exports.UnitTests..."
Run-NUnit test GitHub.Exports.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 5
}

Write-Output "Running GitHub.Extensions.UnitTests..."
Run-NUnit test GitHub.Extensions.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 6
}

Write-Output "Running GitHub.Primitives.UnitTests..."
Run-NUnit test GitHub.Primitives.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 7
}

Write-Output "Running GitHub.TeamFoundation.UnitTests..."
Run-NUnit test GitHub.TeamFoundation.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 8
}

Write-Output "Running GitHub.UI.UnitTests..."
Run-NUnit test GitHub.UI.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 9
}

Write-Output "Running GitHub.VisualStudio.UnitTests..."
Run-NUnit test GitHub.VisualStudio.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 10
}

Write-Output "Running GitHub.InlineReviews.UnitTests..."
Run-NUnit test GitHub.InlineReviews.UnitTests $TimeoutDuration $config
if (!$?) {
    $exitcode = 11
}

if ($exitcode -ne 0) {
    $host.SetShouldExit($exitcode)
}
exit $exitcode