[CmdletBinding()]

Param(
    [switch]
    $AppVeyor = $false
    ,
    [switch]
    $Trace = $false
)

Set-StrictMode -Version Latest
if ($Trace) {
    Set-PSDebug -Trace 1
}

. $PSScriptRoot\modules.ps1 | out-null
. $PSScriptRoot\Modules\Vsix.ps1 | out-null

$full_build = Test-Path env:GHFVS_KEY
$publishable = $env:APPVEYOR_PULL_REQUEST_NUMBER -or $env:APPVEYOR_REPO_BRANCH -eq "master"

if ($full_build -and $publishable) {
    Write-Manifest build
    Push-AppveyorArtifact build\Release\GitHub.VisualStudio.vsix
    Push-AppveyorArtifact build\version
    Push-AppveyorArtifact build\manifest
} else {
    Write-Output "Skipping publishing artifacts, we're not in a PR"
}
