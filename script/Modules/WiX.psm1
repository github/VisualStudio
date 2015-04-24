Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsDirectory = Split-Path (Split-Path $MyInvocation.MyCommand.Path)
$rootDirectory = Split-Path $scriptsDirectory
$gitHubDirectory = Join-Path $rootDirectory src\MsiInstaller

function Invoke-Command([scriptblock]$Command, [switch]$Fatal, [switch]$Quiet) {
    $output = ""
    if ($Quiet) {
        $output = & $Command 2>&1
    } else {
        & $Command
    }

    if (!$Fatal) {
        return
    }

    $exitCode = 0
    if ($LastExitCode -ne 0) {
        $exitCode = $LastExitCode
    } elseif (!$?) {
        $exitCode = 1
    } else {
        return
    }

    Die "``$Command`` failed" $output
}

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

function Add-SignatureToWiX {
    Param(
        [Parameter(Mandatory=$true)]
        [string]
        $WiXPath
    )

    $signtool = Join-Path $scriptsDirectory lib\signtool.exe
    $certificate = "GitHub, inc"

    Invoke-Command -Quiet -Fatal { & $signtool sign /n $certificate /v $WiXPath }
}
