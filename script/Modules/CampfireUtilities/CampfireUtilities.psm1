Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsDirectory = Split-Path (Split-Path (Split-Path $MyInvocation.MyCommand.Path))

function Write-KeysMissing
{
	write-host ""
    write-host "**************" -foregroundcolor White -backgroundcolor Red
    write-host "Campfire Keys aren't in the repo or ~/campfire_keys.ps1, get them from another Winhubber" -foregroundcolor White -backgroundcolor Red
    write-host "and create your own version of campfire_keys.ps1.example as campfire_keys.ps1" -foregroundcolor White -backgroundcolor Red
    write-host "**************" -foregroundcolor White -backgroundcolor Red
	write-host ""
}

$CampfireKeys = Join-Path $scriptsDirectory "campfire_keys.ps1"
if (!(test-path $CampfireKeys)) {
    $CampfireKeys = Join-Path "~" "campfire_keys.ps1"
	if (!(test-path $CampfireKeys)) {
	    Write-S3KeysMissing
		throw "Campfire Keys aren't in the repo or ~/campfire_keys.ps1, get them from another Winhubber and create your own version of campfire_keys.ps1.example as campfire_keys.ps1"
	}
}

try {
    . $CampfireKeys
} catch [Exception] {
    Write-CampfireKeysMissing
    throw $_.Exception
}

function Set-BasicAuthenticationHeader([System.Net.WebRequest]$request, [string]$username, [string]$password) {
    $credentials = "{0}:{1}" -f $username, $password
    $credentialsBytes = [System.Text.Encoding]::Default.GetBytes($credentials)
    $credentialsBase64 = [System.Convert]::ToBase64String($credentialsBytes)
    $request.Headers["Authorization"] = "Basic $credentialsBase64"
}

function New-CampfireRequest
{
    $request = [System.Net.WebRequest]::Create(("https://{0}.campfirenow.com/room/{1}/speak.xml" -f $campfireAccount, $campfireRoomId))
    # The password we provide here is ignored by the Campfire API.
    Set-BasicAuthenticationHeader $request $campfireToken "X"
    return $request
}
