<#
.SYNOPSIS
    Builds and deploys GitHub for Windows to Speakeasy

.DESCRIPTION
    Jenkins runs this script after checking out a revision and cleaning its
    working tree.

    Parameters are passed as environment variables.

    SPEAKEASY_BRANCH              - Branch name being deployed.
    SPEAKEASY_SHA1                - SHA1 that has been checked out, can be
                                    ignored.
    SPEAKEASY_VERSION             - Version of this build on SPEAKEASY_BRANCH,
                                    can be ignored.
    SPEAKEASY_UPDATE_URL          - Speakeasy update URL for this branch,
                                    incorporates the version number.
    SPEAKEASY_DEPLOY_CALLBACK_URL - Speakeasy callback URL, this script should
                                    issue a request to this URL before returning
                                    to indicate the build was successful.
#>

Set-StrictMode -Version Latest

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path

. $scriptsDirectory\common.ps1

$newVersion = $null
$noPush = $false
$channel = "dev"
$branch = [string]$env:SPEAKEASY_BRANCH

if ($branch.StartsWith("release")) {
    $channel = "production"
} elseif ($branch.StartsWith("alpha")) {
    $channel = "alpha"
} elseif ($branch.StartsWith("beta")) {
    $channel = "beta"
} else {
    $noPush = $true
    $newVersion = "None"
}

Push-Location $scriptsDirectory
.\Deploy $channel $newVersion $branch -NoChat:$true -NoPush:$noPush
Pop-Location
