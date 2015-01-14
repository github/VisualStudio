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

Set-PSDebug -Strict
$ErrorActionPreference = "Stop"

$campfireAccount = "github"
$campfireToken = "f890cb46e983068e421975cc12fb76c6ff3f4f4f"
$campfireRoomId = "317644"

function Set-BasicAuthenticationHeader([System.Net.WebRequest]$request, [string]$username, [string]$password) {
    $credentials = "{0}:{1}" -f $username, $password
    $credentialsBytes = [System.Text.Encoding]::Default.GetBytes($credentials)
    $credentialsBase64 = [System.Convert]::ToBase64String($credentialsBytes)
    $request.Headers["Authorization"] = "Basic $credentialsBase64"
}

function Create-MessageXml([string]$message) {
    $xml = New-Object Xml
    $bodyElement = $xml.CreateElement('body')
    $bodyElement.InnerText = $message
    $messageElement = $xml.CreateElement('message')
    $messageElement.AppendChild($bodyElement) | Out-Null
    $xml.AppendChild($messageElement) | Out-Null
    $xml
}

function Set-PostData([System.Net.WebRequest]$request, [string]$data) {
    $request.Method = "POST"

    $bytes = [System.Text.Encoding]::Default.GetBytes($data)
    $request.ContentLength = $bytes.Length

    $stream = $request.GetRequestStream()
    $stream.Write($bytes, 0, $bytes.Length)
    $stream.Close()
}

$request = [System.Net.WebRequest]::Create(("https://{0}.campfirenow.com/room/{1}/speak.xml" -f $campfireAccount, $campfireRoomId))
# The password we provide here is ignored by the Campfire API.
Set-BasicAuthenticationHeader $request $campfireToken "X"

$xmlString = (Create-MessageXml $Message).OuterXml
$request.ContentType = "application/xml"
Set-PostData $request $xmlString

$response = $request.GetResponse()
if ($response.StatusCode -ne [System.Net.HttpStatusCode]::Created) {
    Write-Error ("Expected status code 'Created' but got '{0}'" -f $response.StatusDescription)
    exit 1
}
