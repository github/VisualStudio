<#
.SYNOPSIS
    Builds and (optionally) runs tests for GitHub for Visual Studio
.DESCRIPTION
    Build GHfVS
.PARAMETER Clean
    When true, all untracked (and ignored) files will be removed from the work
    tree and all submodules. Defaults to false.
.PARAMETER Config
    Debug or Release
.PARAMETER RunTests
    Runs the tests (defauls to false)
#>
[CmdletBinding()]

Param(
    [switch]
    $UpdateSubmodules = $false
    ,
    [switch]
    $Clean = $false
    ,
    [ValidateSet('Debug', 'Release')]
    [string]
    $Config = "Release"
    ,
    [switch]
    $Deploy = $false
    ,
    [switch]
    $AppVeyor = $false
    ,
    [switch]
    $SkipVersionBump = $false
    ,
    [switch]
    $Trace = $false
)

Set-StrictMode -Version Latest
if ($Trace) {
    Set-PSDebug -Trace 1
}

. $PSScriptRoot\modules.ps1 | out-null
$env:PATH = "$scriptsDirectory;$scriptsDirectory\Modules;$env:PATH"

Import-Module $scriptsDirectory\Modules\Debugging.psm1
Vsix | out-null
WiX | out-null

Push-Location $rootDirectory

if ($UpdateSubmodules) {
    Update-Submodules
}

if ($Clean) {
	Clean-WorkingTree
}

if ($Deploy -and $Config -eq "Release" -and !$SkipVersionBump) {
    Bump-Version -BumpBuild
}

if ($AppVeyor) {
    #& $git symbolic-ref HEAD
    #if (!$?) { # we're in a detached head, which means we're build a PR merge
        #$parents = Run-Command -Quiet { & $git rev-list -n1 --parents HEAD | %{$_.split(" ")} }
        #$targetBranchHash = Run-Command -Quiet { & $git rev-parse HEAD^1 }
        Write-Output $env:APPVEYOR_PULL_REQUEST_NUMBER
        Write-Output $env:APPVEYOR_PULL_REQUEST_TITLE
        Write-Output $env:APPVEYOR_PULL_REQUEST_HEAD_REPO_NAME
        Write-Output $env:APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH
        Write-Output $env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT
        Write-Output $env:APPVEYOR_REPO_NAME
        Write-Output $env:APPVEYOR_REPO_BRANCH
        Write-Output $env:APPVEYOR_REPO_COMMIT
        Write-Output $env:APPVEYOR_REPO_COMMIT_AUTHOR
        Write-Output $env:APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL
        Write-Output $env:APPVEYOR_REPO_COMMIT_TIMESTAMP
        Write-Output $env:APPVEYOR_REPO_COMMIT_MESSAGE
        Write-Output $env:APPVEYOR_REPO_COMMIT_MESSAGE_EXTENDED
    #}
    #$d = Run-Command -Quiet { & git rev-list -n1 --parents HEAD | %{$_.split(" ")} }
}


Write-Output "Building GitHub for Visual Studio..."
Write-Output ""

Build-Solution GitHubVs.sln "Build" $config $Deploy

Pop-Location
