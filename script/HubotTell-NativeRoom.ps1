<#
.SYNOPSIS
    Makes Hubot post a message to The Native Room
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Message
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Import-Module C:\psmodules\Chatterbox.psm1

$Response = Send-TopicMessage "visualstudio-ci" $Message
if ($Response.StatusCode -ne [System.Net.HttpStatusCode]::Created) {
    Write-Error ("Expected status code 'Created' but got '{0}'" -f $Response.StatusDescription)
    exit 1
}
