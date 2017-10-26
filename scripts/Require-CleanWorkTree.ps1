<#
.SYNOPSIS
    Ensures the working tree has no uncommitted changes
.PARAMETER Action
    The action that requires a clean work tree. This will appear in error messages.
.PARAMETER WarnOnly
    When true, warns rather than dies when uncommitted changes are found.
#>

[CmdletBinding()]
Param(
    [ValidateNotNullOrEmpty()]
    [string]
    $Action
    ,
    [switch]
    $WarnOnly = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$rootDirectory = Split-Path ($scriptsDirectory)
#$git = Get-Command git.cmd
. $scriptsDirectory\common.ps1

function Die([string]$message) {
    Write-Error $message
    exit 1
}

# Based on git-sh-setup.sh:require_clean_work_tree in git.git, but changed not
# to ignore submodules.

Push-Location $rootDirectory

& $git rev-parse --verify HEAD | Out-Null
if (!$? -or ($LastExitCode -ne 0)) {
    Die
}
& $git update-index -q --refresh

& $git diff-files --quiet
$error = ""
if ($LastExitCode -ne 0) {
    $error = "You have unstaged changes."
}

& $git diff-index --cached --quiet HEAD --
if ($LastExitCode -ne 0) {
    if ($error) {
        $error += " Additionally, your index contains uncommitted changes."
    } else {
        $error = "Your index contains uncommitted changes."
    }
}

if ($error) {
    if ($WarnOnly) {
        Write-Warning "$error Continuing anyway."
    } else {
        Die ("Cannot $Action" + ": $error")
    }
}

Pop-Location
