<#
.SYNOPSIS
    Runs tests for GitHub for Visual Studio
.DESCRIPTION
    Build GHfVS
.PARAMETER Clean
    When true, all untracked (and ignored) files will be removed from the work
    tree and all submodules. Defaults to false.
#>
[CmdletBinding()]

Param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Config = "Release"
    ,
    [int]
    $TimeoutDuration = 180
    ,
    [switch]
    $AppVeyor = $false
    ,
    [switch]
    $Trace = $false

)

Set-StrictMode -Version Latest
if ($Trace) {
    Set-PSDebug -Trace 1
}

$scriptsDirectory = $PSScriptRoot

Write-Output "Running Tracking Collection Tests..."
& {
    Trap {
        $exitcode = 1
    }
    . $scriptsDirectory\Run-NUnit src TrackingCollectionTests $TimeoutDuration $config -AppVeyor:$AppVeyor
}

Write-Output "Running GitHub.UI.UnitTests..."
& {
    Trap {
        $exitcode = 1
    }
. $scriptsDirectory\Run-NUnit test GitHub.UI.UnitTests $TimeoutDuration $config -AppVeyor:$AppVeyor
}
Write-Output "Running UnitTests..."
& {
    Trap {
        $exitcode = 1
    }
. $scriptsDirectory\Run-XUnit src UnitTests $TimeoutDuration $config -AppVeyor:$AppVeyor
}
Write-Output "Running GitHub.InlineReviews.UnitTests..."
& {
    Trap {
        $exitcode = 1
    }
. $scriptsDirectory\Run-XUnit test GitHub.InlineReviews.UnitTests $TimeoutDuration $config -AppVeyor:$AppVeyor
}