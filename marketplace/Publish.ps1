[CmdletBinding(SupportsShouldProcess=$true)]
param (
	[parameter(Mandatory=$true,
		HelpMessage="Create token using https://dev.azure.com/github-editor-tools/_usersSettings/tokens?action=edit with Organization: All accessible organizations, Selected scopes: Marketplace (publish)")]
	[string] $personalAccessToken
	,
	[parameter(Mandatory=$true,
		HelpMessage="Path to the .vsix file for publishing")]
	[string] $payload
)

$publisherName = "GitHub"
$publishManifest ="$PSScriptRoot\publishManifest.json"

& $PSScriptRoot\Publish-Vsix.ps1 -WhatIf:([bool]$WhatIfPreference.IsPresent) -publisherName:$publisherName -personalAccessToken:$personalAccessToken -payload:$payload -publishManifest:$publishManifest
