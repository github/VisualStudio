Set-StrictMode -Version Latest

New-Module -ScriptBlock {

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

    function Read-VersionInstaller {
        [System.Version] (Get-WixManifestXml).WiX.Product.Version
    }

    function Write-VersionInstaller([System.Version]$version) {

        $content = @"
<?xml version="1.0" encoding="utf-8"?>
<Include>
  <?define VersionNumber="$version" ?>
</Include>
"@

        $file = Get-WiXVersionFile
        $content | Set-Content $file
    }

    Export-ModuleMember -Function Get-WiXVersionFile,Get-WiXVsixFile,Get-WixManifestXml,Read-VersionInstaller,Write-VersionInstaller
}