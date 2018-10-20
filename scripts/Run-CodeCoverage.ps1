<#
.SYNOPSIS
    Runs NUnit
#>

[CmdletBinding()]
Param(
    [string]
    $Configuration
    ,
    [switch]
    $AppVeyor = $false
)

$scriptsDirectory = $PSScriptRoot
$rootDirectory = Split-Path ($scriptsDirectory)
. $scriptsDirectory\modules.ps1 | out-null

$nunitDirectory = Join-Path $rootDirectory packages\NUnit.ConsoleRunner.3.7.0\tools
$nunitConsoleRunner = Join-Path $nunitDirectory nunit3-console.exe

$testAssemblies = @(
    "test\GitHub.Api.UnitTests\bin\$Configuration\net461\GitHub.Api.UnitTests.dll",
    "test\GitHub.App.UnitTests\bin\$Configuration\net461\GitHub.App.UnitTests.dll",
    "test\GitHub.Exports.Reactive.UnitTests\bin\$Configuration\net461\GitHub.Exports.Reactive.UnitTests.dll",
    "test\GitHub.Exports.UnitTests\bin\$Configuration\net461\GitHub.Exports.UnitTests.dll",
    "test\GitHub.Extensions.UnitTests\bin\$Configuration\net461\GitHub.Extensions.UnitTests.dll",
    "test\GitHub.InlineReviews.UnitTests\bin\$Configuration\net461\GitHub.InlineReviews.UnitTests.dll",
    "test\GitHub.TeamFoundation.UnitTests\bin\$Configuration\net461\GitHub.TeamFoundation.UnitTests.dll",
    "test\GitHub.UI.UnitTests\bin\$Configuration\net461\GitHub.UI.UnitTests.dll",
    "test\GitHub.VisualStudio.UnitTests\bin\$Configuration\net461\GitHub.VisualStudio.UnitTests.dll",
    "test\MetricsTests\MetricsTests\bin\$Configuration\MetricsTests.dll",
    "test\TrackingCollectionTests\bin\$Configuration\net461\TrackingCollectionTests.dll"
)

$opencoverTargetArgs = ($testAssemblies -join " ") + " --where \`"cat!=Timings and cat!=CodeCoverageFlake\`" --inprocess --noresult"

$opencoverDirectory = Join-Path $env:USERPROFILE .nuget\packages\opencover\4.6.519\tools
$opencover = Join-Path $opencoverDirectory OpenCover.Console.exe
$opencoverArgs = @(
    "-target:`"$nunitConsoleRunner`"",
    "-targetargs:`"$opencoverTargetArgs`"",
    "-filter:`"+[GitHub*]* -[GitHub*Unit]GitHub.*.SampleData -[GitHub*UnitTests]*`"",
    "-excludebyfile:*.xaml;*.xaml.cs",
    "-register:user -output:$rootDirectory\coverage.xml"
) -join " "

$codecovDirectory = Join-Path $env:USERPROFILE .nuget\packages\codecov\1.1.0\tools
$codecov = Join-Path $codecovDirectory codecov.exe
$codecovArgs = "-f $rootDirectory\coverage.xml"

& {
    Trap {
        Write-Output "OpenCover trapped"
        exit 0
    }

    Write-Output $opencover

    Run-Process 600 $opencover $opencoverArgs

    if (!$?) {
        Write-Output "OpenCover failed"
        exit 0
    }
}

if($AppVeyor) {
    & {
        Trap {
            Write-Output "Codecov trapped"
            exit 0
        }

        Push-AppveyorArtifact "$rootDirectory\coverage.xml"
        Run-Process 300 $codecov $codecovArgs

        if (!$?) {
            Write-Output "Codecov failed"
            exit 0
        }
    }
}
