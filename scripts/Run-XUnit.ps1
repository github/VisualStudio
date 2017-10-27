<#
.SYNOPSIS
    Runs xUnit
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $BasePathToProject
    ,
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Project
    ,
    [int]
    $TimeoutDuration
    ,
    [string]
    $Configuration
    ,
    [switch]
    $AppVeyor = $false
)

$scriptsDirectory = $PSScriptRoot
$rootDirectory = Split-Path ($scriptsDirectory)
. $scriptsDirectory\modules.ps1 | out-null

$dll = "$BasePathToProject\$Project\bin\$Configuration\$Project.dll"

$xunitDirectory = Join-Path $rootDirectory packages\xunit.runner.console.2.1.0\tools
$consoleRunner = Join-Path $xunitDirectory xunit.console.x86.exe
if ($AppVeyor) {
    $args = $dll, "-noshadow", "-parallel", "all", "-appveyor"
    & $consoleRunner ($args | %{ "`"$_`"" })
    if($LastExitCode -ne 0) {
        $host.SetShouldExit($LastExitCode)
    }
} else {
    $xml = Join-Path $rootDirectory "nunit-$Project.xml"
    $args = $dll, "-noshadow", "-xml", $xml, "-parallel", "all"
    Write-Output "$consoleRunner $args"
    Run-Process -Fatal $TimeoutDuration $consoleRunner $args
}