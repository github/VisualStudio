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
. $scriptsDirectory\modules.ps1

$dll = "$BasePathToProject\$Project\bin\$Configuration\$Project.dll"

& {
    Trap {
        Write-Output $_
        exit 1
    }

    if ($AppVeyor) {
        $xunitDirectory = Join-Path $rootDirectory packages\xunit.runner.console.2.1.0\tools
        $consoleRunner = Join-Path $xunitDirectory xunit.console.x86.exe
        $args = $dll, "-noshadow", "-parallel", "all", "-appveyor"
        [object[]] $output = "$consoleRunner " + ($args -join " ")
        Run-Command -Fatal { & $consoleRunner ($args | %{ "`"$_`"" }) }
    } else {
        $xunitDirectory = Join-Path $rootDirectory packages\xunit.runner.console.2.1.0\tools
        $consoleRunner = Join-Path $xunitDirectory xunit.console.x86.exe
        $xml = Join-Path $rootDirectory "nunit-$Project.xml"
        $outputPath = [System.IO.Path]::GetTempFileName()

        $args = $dll, "-noshadow", "-xml", $xml, "-parallel", "all"
        $output = Run-Command -Fatal -Timeout $TimeoutDuration { $consoleRunner ($args | %{ "`"$_`"" }) }

        #[object[]] $output = "$consoleRunner " + ($args -join " ")

        #$process = Start-Process -PassThru -NoNewWindow -RedirectStandardOutput $outputPath $consoleRunner ($args | %{ "`"$_`"" })
        #Wait-Process -InputObject $process -Timeout $TimeoutDuration -ErrorAction SilentlyContinue
        #if ($process.HasExited) {
        #    $output += Get-Content $outputPath
        #    $exitCode = $process.ExitCode
        #} else {
        #    $output += "Tests timed out. Backtrace:"
        #    $output += Get-DotNetStack $process.Id
        #    $exitCode = 9999
        #}
        #Stop-Process -InputObject $process
        #Remove-Item $outputPath
    }
}

$output
exit 0
