Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsDirectory = Split-Path (Split-Path $MyInvocation.MyCommand.Path)

. $scriptsDirectory\common.ps1

$gitHubDirectory = Join-Path $rootDirectory src\MsiInstaller

function Get-WiXVersionFile {
    Join-Path $gitHubDirectory Version.wxi
}

function Get-WiXVsixFile {
    Join-Path $gitHubDirectory Vsix.wxi
}

function Get-WixManifestXml {
    $xmlLines = Get-Content (Get-WiXManifestPath)
    # If we don't explicitly join the lines with CRLF, comments in the XML will
    # end up with LF line-endings, which will make Git spew a warning when we
    # try to commit the version bump.
    $xmlText = $xmlLines -join [System.Environment]::NewLine

    [xml] $xmlText
}

function Read-CurrentVersion {
    [System.Version] (Get-WixManifestXml).WiX.Product.Version
}
