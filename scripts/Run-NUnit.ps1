<#
.SYNOPSIS
    Runs NUnit
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
$nunitDirectory = Join-Path $rootDirectory packages\NUnit.ConsoleRunner.3.7.0\tools
$consoleRunner = Join-Path $nunitDirectory nunit3-console.exe
$xml = Join-Path $rootDirectory "nunit-$Project.xml"

& {
    Trap {
        Write-Output $_
        exit -1
    }

    Run-Process -Fatal $TimeoutDuration $consoleRunner $dll,"--where ""cat != Timings""","--result=$xml;format=AppVeyor"
    if (!$?) {
        Die 1 "nunit $Project failed"
    }
}
