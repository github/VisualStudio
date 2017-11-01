<#
.SYNOPSIS
    Bumps the version number of GitHub for Visual Studio
.DESCRIPTION
    By default, just bumps the last component of the version number by one. An
    alternate version number can be specified on the command line.

    The new version number is committed to the local repository and pushed to
    GitHub.
#>

Param(
    # It would be nice to use our Validate-Version function here, but we
    # can't because this Param definition has to come before any other code in the
    # file.
    [ValidateScript({ ($_.Major -ge 0) -and ($_.Minor -ge 0) -and ($_.Build -ge 0) })]
    [System.Version]
    $NewVersion = $null
    ,
    [switch]
    $BumpMajor = $false
    ,
    [switch]
    $BumpMinor = $false
    ,
    [switch]
    $BumpPatch = $false
    ,
    [switch]
    $BumpBuild = $false
    ,
    [int]
    $BuildNumber = -1
    ,
    [switch]
    $Commit = $false
	,
    [switch]
    $Push = $false
    ,
    [switch]
    $Force = $false
    ,
    [switch]
    $Trace = $false
)

Set-StrictMode -Version Latest
if ($Trace) { Set-PSDebug -Trace 1 }

. $PSScriptRoot\modules.ps1 | out-null
. $scriptsDirectory\Modules\Vsix.ps1 | out-null
. $scriptsDirectory\Modules\WiX.ps1 | out-null
. $scriptsDirectory\Modules\SolutionInfo.ps1 | out-null
. $scriptsDirectory\Modules\Versioning.ps1 | out-null

if ($NewVersion -eq $null) {
    if (!$BumpMajor -and !$BumpMinor -and !$BumpPatch -and !$BumpBuild){
       Die -1 "You need to indicate which part of the version to update via -BumpMajor/-BumpMinor/-BumpPatch/-BumpBuild flags or a custom version via -NewVersion"
    }
}

if ($Push -and !$Commit) {
    Die 1 "Cannot push a version bump without -Commit"
}

if ($Commit -and !$Force){
    Require-CleanWorkTree "bump version"
}

if (!$?) {
    exit 1
}

if ($NewVersion -eq $null) {
    $currentVersion = Read-CurrentVersionVsix
    $NewVersion = Generate-Version $currentVersion $BumpMajor $BumpMinor $BumpPatch $BumpBuild $BuildNumber
}

Write-Output "Setting version to $NewVersion"
Write-Version $NewVersion

if ($Commit) {
    Write-Output "Committing version change"
    Commit-Version $NewVersion

    if ($Push) {
        Write-Output "Pushing version change"
        $branch = & $git rev-parse --abbrev-ref HEAD
        Push-Changes $branch
    }
}
