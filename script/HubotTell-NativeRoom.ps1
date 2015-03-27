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

$request = New-CampfireRequest

$xmlString = (Create-MessageXml $Message).OuterXml
$request.ContentType = "application/xml"
Set-PostData $request $xmlString

$response = $request.GetResponse()
if ($response.StatusCode -ne [System.Net.HttpStatusCode]::Created) {
    Write-Error ("Expected status code 'Created' but got '{0}'" -f $response.StatusDescription)
    exit 1
}
