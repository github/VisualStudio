<#
.SYNOPSIS
    Builds and tests GitHub for Visual Studio
.DESCRIPTION
    Janky runs this script after checking out a revision and cleaning its
    working tree.
.PARAMETER UpdateSubmodules
    When true, submodules will be initialized and (forcefully) updated.
    Defaults to false.
.PARAMETER Clean
    When true, all untracked (and ignored) files will be removed from the work
    tree and all submodules. Defaults to false.
#>

Param(
    [switch]
    $UpdateSubmodules = $false
    ,
    [switch]
    $Clean = $false
    ,
    [ValidateSet('Debug', 'Release')]
    [string]
    $config = "Release"
)

Set-StrictMode -Version Latest

$rootDirectory = Split-Path (Split-Path $MyInvocation.MyCommand.Path)
Import-Module (Join-Path $rootDirectory "script\Modules\BuildUtils.psm1") 3> $null # Ignore warnings
Import-Module (Join-Path $rootDirectory "script\Modules\Debugging.psm1")
Import-Module (Join-Path $rootDirectory "script\Modules\Vsix.psm1")

Push-Location $rootDirectory

# Run-Command -Quiet -Fatal { .\script\Bootstrap.ps1 -ImportCertificatesOnly }

$nuget = Join-Path $rootDirectory "script\nuget\nuget.exe"
& $nuget install xunit.runner.console -OutputDirectory (Join-Path $rootDirectory "script") -ExcludeVersion

if ($UpdateSubmodules) {
    Update-Submodules
}

if ($Clean) {
	Clean-WorkingTree
}

function Run-XUnit([string]$project, [int]$timeoutDuration, [string]$configuration) {
    $dll = "src\$project\bin\$configuration\$project.dll"

    $xunitDirectory = Join-Path $rootDirectory script\xunit.runner.console\tools
    $consoleRunner = Join-Path $xunitDirectory xunit.console.exe
    $xml = Join-Path $rootDirectory "nunit-$project.xml"
    $outputPath = [System.IO.Path]::GetTempFileName()

    $args = $dll, "-noshadow", "-xml", $xml, "-quiet"
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

    $result = New-Object System.Object
    $result | Add-Member -Type NoteProperty -Name Output -Value $output
    $result | Add-Member -Type NoteProperty -Name ExitCode -Value $exitCode
    $result
}


Write-Output "Building GitHub for Visual Studio..."
Write-Output ""
& $nuget restore GitHubVs.sln
$output = .\Build-Solution.ps1 Build $config -MSBuildVerbosity quiet 2>&1
if ($LastExitCode -ne 0) {
    $exitCode = $LastExitCode

    $errors = $output | Select-String ": error"
    if ($errors) {
        $output = "Likely errors:", $errors, "", "Full output:", $output
    }

    Die-WithOutput $exitCode $output
}

$exitCode = 0

Write-Output "Running Unit Tests..."
$result = Run-XUnit UnitTests 180 $config
if ($result.ExitCode -eq 0) {
    # Print out the test result summary.
    Write-Output $result.Output[-1]
} else {
    $exitCode = $result.ExitCode
    Write-Output $result.Output
}
Write-Output ""

exit $exitCode