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

$rootDirectory = Split-Path (Split-Path $MyInvocation.MyCommand.Path)
Push-Location $rootDirectory

$dll = "$BasePathToProject\$Project\bin\$Configuration\$Project.dll"

if ($AppVeyor) {
    $xunitDirectory = Join-Path $rootDirectory packages\xunit.runner.console.2.1.0\tools
    $consoleRunner = Join-Path $xunitDirectory xunit.console.x86.exe
    $args = $dll, "-noshadow", "-parallel", "all", "-appveyor"
    [object[]] $output = "$consoleRunner " + ($args -join " ")
    & $consoleRunner ($args | %{ "`"$_`"" })
    if($LastExitCode -ne 0) {
        $host.SetShouldExit($LastExitCode)
    }
} else {
    $xunitDirectory = Join-Path $rootDirectory packages\xunit.runner.console.2.1.0\tools
    $consoleRunner = Join-Path $xunitDirectory xunit.console.x86.exe
    $xml = Join-Path $rootDirectory "nunit-$Project.xml"
    $outputPath = [System.IO.Path]::GetTempFileName()

    $args = $dll, "-noshadow", "-xml", $xml, "-parallel", "all"

    [object[]] $output = "$consoleRunner " + ($args -join " ")

    $process = Start-Process -PassThru -NoNewWindow -RedirectStandardOutput $outputPath $consoleRunner ($args | %{ "`"$_`"" })
    Wait-Process -InputObject $process -Timeout $TimeoutDuration -ErrorAction SilentlyContinue
    if ($process.HasExited) {
        $output += Get-Content $outputPath
        $exitCode = $process.ExitCode
    } else {
        $output += "Tests timed out. Backtrace:"
        $output += Get-DotNetStack $process.Id
        $exitCode = 9999
    }
    Stop-Process -InputObject $process
    Remove-Item $outputPath
    Pop-Location

    $result = New-Object System.Object
    $result | Add-Member -Type NoteProperty -Name Output -Value $output
    $result | Add-Member -Type NoteProperty -Name ExitCode -Value $exitCode
    $result
}
