<#
.SYNOPSIS
    Returns the name of the working directory's currently checked-out branch
#>

Set-PSDebug -Strict

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$rootDirectory = Split-Path $scriptsDirectory

. $scriptsDirectory\common.ps1

function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Write-Error $message
    exit 1
}

$output = & $git symbolic-ref HEAD 2>&1 | %{ "$_" }
if (!$? -or ($LastExitCode -ne 0)) {
    Die "Failed to determine current branch" $output
}

if (!($output -match "^refs/heads/(\S+)$")) {
    Die "Failed to determine current branch. HEAD is $output" $output
}

$matches[1]
