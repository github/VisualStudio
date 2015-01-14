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
)

Set-StrictMode -Version Latest

$buildDirectory = Split-Path $MyInvocation.MyCommand.Path
Import-Module (Join-Path $buildDirectory "Modules\BuildUtils.psm1") 3> $null # Ignore warnings
Import-Module (Join-Path $buildDirectory "Modules\Debugging.psm1")
Import-Module (Join-Path $buildDirectory "Modules\Vsix.psm1")

$rootDirectory = Split-Path ($buildDirectory)
Push-Location $rootDirectory

# Run-Command -Quiet -Fatal { (Join-Path $buildDirectory "Bootstrap.ps1") -ImportCertificatesOnly }
$nuget = Join-Path $buildDirectory "nuget\nuget.exe"
& $nuget install xunit.runners -OutputDirectory $buildDirectory -ExcludeVersion

if ($UpdateSubmodules) {
    Update-Submodules
}

if ($Clean) {
	Clean-WorkingTree
}

function Run-XUnit([string]$project, [int]$timeoutDuration, [string]$configuration = "Release") {
    $dll = "src\$project\bin\$configuration\$project.dll"

    $xunitDirectory = Join-Path $buildDirectory xunit.runners\tools
    $consoleRunner = Join-Path $xunitDirectory xunit.console.clr4.x86.exe
    $xml = Join-Path $rootDirectory "nunit-$project.xml"
    $outputPath = [System.IO.Path]::GetTempFileName()

    $args = $dll, "/noshadow", "/nunit", $xml, "/silent"
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
$msbuild = Join-Path $Env:WinDir Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
$output = & $msbuild GitHubVs.sln /property:Configuration=Debug /property:VisualStudioVersion=12.0 /property:DeployExtension=false /verbosity:quiet 2>&1
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
$result = Run-XUnit UnitTests 180
if ($result.ExitCode -eq 0) {
    # Print out the test result summary.
    Write-Output $result.Output[-1]
} else {
    $exitCode = $result.ExitCode
    Write-Output $result.Output
}
Write-Output ""

exit $exitCode