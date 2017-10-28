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

& {
    Trap {
        exit $_.Exception.ExitCode
    }

    $args = @()
    if ($AppVeyor) {
        $args = $dll, "-noshadow", "-parallel", "all", "-appveyor"
    } else {
        $xml = Join-Path $rootDirectory "nunit-$Project.xml"
        $args = $dll, "-noshadow", "-xml", $xml, "-parallel", "all"
    }

    Run-Process -Fatal $TimeoutDuration $consoleRunner $args
    if (!$?) {
        Die 111 "xunit $Project failed"
    }
}
