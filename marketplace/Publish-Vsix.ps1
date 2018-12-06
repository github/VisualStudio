[CmdletBinding(SupportsShouldProcess=$true)]
param (
	[parameter(Mandatory=$true,
		HelpMessage="Publisher name which must match publisher in the manifest")]
	[string] $publisherName
	,
	[parameter(Mandatory=$true,
		HelpMessage="Token with Organization: All accessible organizations, Selected scopes: Marketplace (publish)")]
	[string] $personalAccessToken
	,
	[parameter(Mandatory=$true,
		HelpMessage="Path to the .vsix file for publishing")]
	[string] $payload
	,
	[parameter(Mandatory=$true,
		HelpMessage="Path to the publish manifest .json file")]
	[string] $publishManifest = "$PSScriptRoot\publishManifest.json"
)

Install-Module -Name VSSetup -RequiredVersion 2.2.5 -Scope CurrentUser -Force
Import-Module -Name VSSetup -Version 2.2.5

$VSSetupInstance = Get-VSSetupInstance | Select-VSSetupInstance -Product * -Require 'Microsoft.VisualStudio.Component.VSSDK'
$VSInstallDir=$VSSetupInstance.InstallationPath
$VsixPublisher="$VSInstallDir\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"

if ($PSCmdlet.ShouldProcess("VsixPublisher.exe login -publisherName $publisherName -personalAccessToken $personalAccessToken", "Login"))
{
	& $VsixPublisher login -publisherName $publisherName -personalAccessToken $personalAccessToken
}

if ($PSCmdlet.ShouldProcess("VsixPublisher.exe publish -payload $payload -publishManifest $publishManifest", "Publish"))
{
	& $VsixPublisher publish -payload $payload -publishManifest $publishManifest
}
