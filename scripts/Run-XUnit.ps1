<#
.SYNOPSIS
    Runs xUnit
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $project,
	[int]$timeoutDuration,
	[string]$configuration
)

$rootDirectory = Split-Path (Split-Path $MyInvocation.MyCommand.Path)
Push-Location $rootDirectory

$dll = "src\$project\bin\$configuration\$project.dll"

$xunitDirectory = Join-Path $rootDirectory packages\xunit.runner.console.2.1.0\tools
$consoleRunner = Join-Path $xunitDirectory xunit.console.x86.exe
$xml = Join-Path $rootDirectory "nunit-$project.xml"
$outputPath = [System.IO.Path]::GetTempFileName()

$args = $dll, "-noshadow", "-xml", $xml, "-parallel", "all"
[object[]] $output = "$consoleRunner " + ($args -join " ")

$process = Start-Process -PassThru -NoNewWindow -RedirectStandardOutput $outputPath $consoleRunner ($args | %{ "`"$_`"" })
Wait-Process -InputObject $process -Timeout $timeoutDuration -ErrorAction SilentlyContinue
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
	
