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

$Sent = Send-TopicMessage "visualstudio-ci" $Message
if (!$Sent) {
    exit 1
}
