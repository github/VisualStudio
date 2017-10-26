<#
.SYNOPSIS
	Returns SHA1 of head revision
#>

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$rootDirectory = Split-Path $scriptsDirectory

. $scriptsDirectory\common.ps1

Push-Location $rootDirectory
$version = & $git rev-parse HEAD 2>&1 | %{ "$_" }
if (!$? -or ($LastExitCode -ne 0)) {
     Die "Error determining HEAD revision" $version
  }
Pop-Location

$version
