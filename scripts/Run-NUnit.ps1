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

$rootDirectory = Split-Path (Split-Path $MyInvocation.MyCommand.Path)
Push-Location $rootDirectory

$dll = "$BasePathToProject\$Project\bin\$Configuration\$Project.dll"

if ($AppVeyor) {
    $nunitDirectory = Join-Path $rootDirectory packages\NUnit.ConsoleRunner.3.7.0\tools
    $consoleRunner = Join-Path $nunitDirectory nunit3-console.exe
    & $consoleRunner "$dll --where ""cat != Timings"" --result=myresults.xml;format=AppVeyor"
    if($LastExitCode -ne 0) {
        $host.SetShouldExit($LastExitCode)
    }
} else {
    $nunitDirectory = Join-Path $rootDirectory packages\NUnit.ConsoleRunner.3.7.0\tools
    $consoleRunner = Join-Path $nunitDirectory nunit3-console.exe

    $xml = Join-Path $rootDirectory "nunit-$Project.xml"
    $outputPath = [System.IO.Path]::GetTempFileName()

    $output = ""

    $process = Start-Process -PassThru -NoNewWindow $consoleRunner "$dll --where ""cat != Timings"" --result=$xml"
    Wait-Process -InputObject $process -Timeout $TimeoutDuration -ErrorAction SilentlyContinue
    if ($process.HasExited) {
        $exitCode = $process.ExitCode
    } else {
        $output += "Tests timed out. Backtrace:"
        $output += Get-DotNetStack $process.Id
        Write-Output $output
        $exitCode = 9999
    }

    Stop-Process -InputObject $process
    Remove-Item $outputPath
    Pop-Location

    $result = New-Object System.Object
    $result | Add-Member -Type NoteProperty -Name ExitCode -Value $exitCode
    $result
}
