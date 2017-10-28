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

$xunitDirectory = Join-Path $rootDirectory packages\xunit.runner.console.2.3.1\tools\net452
$consoleRunner = Join-Path $xunitDirectory xunit.console.exe

& {
    Trap {
        Write-Output $_
        exit -1
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
        Die 1 "xunit $Project failed"
    }
}
