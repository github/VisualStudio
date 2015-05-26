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

$speakEasyVersion = [System.Version]$env:SPEAKEASY_VERSION
#$newVersion =  read-currentVersion | %{ "$($_.major).$($_.minor).$($_.build).$($_.revision + $speakEasyVersion.minor)" }
$newVersion = "None"

Push-Location $scriptsDirectory
.\Deploy "speakeasy" -NoChat:$true -NoPush:$true -NewVersion $newVersion
Pop-Location
