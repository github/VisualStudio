param(
    [ValidateSet('Full', 'Tests', 'Build', 'Clean')]
    [string]
    $build = "Build"
    ,
    [ValidateSet('Debug', 'Release')]
    [string]
    $config = "Release"
    ,
    [ValidateSet('Any CPU', 'x86', 'x64')]
    [string]
    $platform = "Any CPU"
    ,
    [string]
    $verbosity = "minimal"
)

$rootDirectory = Split-Path $MyInvocation.MyCommand.Path
$projFile = join-path $rootDirectory GitHubVS.msbuild
$msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Throw (New-Object -TypeName ScriptException -ArgumentList $message)
}

function Run-Command([scriptblock]$Command, [switch]$Fatal, [switch]$Quiet) {
    $output = ""
    if ($Quiet) {
        $output = & $Command 2>&1
    } else {
        & $Command
    }

    if (!$Fatal) {
        return
    }

    $exitCode = 0
    if (!$? -and $LastExitCode -ne 0) {
        $exitCode = $LastExitCode
    } elseif (!$?) {
        $exitCode = 1
    } else {
        return
    }

    Die "``$Command`` failed" $output
}

function Run-XUnit([string]$project, [int]$timeoutDuration, [string]$configuration) {
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

    $result = New-Object System.Object
    $result | Add-Member -Type NoteProperty -Name Output -Value $output
    $result | Add-Member -Type NoteProperty -Name ExitCode -Value $exitCode
    $result
}

function Run-NUnit([string]$project, [int]$timeoutDuration, [string]$configuration) {
    $dll = "src\$project\bin\$configuration\$project.dll"

    $nunitDirectory = Join-Path $rootDirectory packages\NUnit.Runners.2.6.4\tools
    $consoleRunner = Join-Path $nunitDirectory nunit-console-x86.exe
    $xml = Join-Path $rootDirectory "nunit-$project.xml"
    $outputPath = [System.IO.Path]::GetTempFileName()

    $args = "-noshadow", "-xml:$xml", "-framework:net-4.5", "-exclude:Timings", $dll
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

function Build-Solution([string]$solution) {
    Run-Command -Fatal { & $msbuild $solution /t:Build /property:Configuration=$config /verbosity:$verbosity /p:VisualStudioVersion=14.0 /p:DeployExtension=false }
}

Write-Output "Building GitHub for Visual Studio..."
Write-Output ""

Build-Solution GitHubVs.sln

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

Write-Output "Running TrackingCollection Tests..."
$result = Run-NUnit TrackingCollectionTests 180 $config
if ($result.ExitCode -eq 0) {
    # Print out the test result summary.
    Write-Output $result.Output[-3]
} else {
    $exitCode = $result.ExitCode
    Write-Output $result.Output
}
Write-Output ""

exit $exitCode