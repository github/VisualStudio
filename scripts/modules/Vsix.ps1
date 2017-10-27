Set-StrictMode -Version Latest

New-Module -ScriptBlock {
    $gitHubDirectory = Join-Path $rootDirectory src\GitHub.VisualStudio

    function Get-VsixManifestPath {
        Join-Path $gitHubDirectory source.extension.vsixmanifest
    }

    function Get-VsixManifestXml {
        $xmlLines = Get-Content (Get-VsixManifestPath)
        # If we don't explicitly join the lines with CRLF, comments in the XML will
        # end up with LF line-endings, which will make Git spew a warning when we
        # try to commit the version bump.
        $xmlText = $xmlLines -join [System.Environment]::NewLine

        [xml] $xmlText
    }

    function Read-CurrentVersion {
        [System.Version] (Get-VsixManifestXml).PackageManifest.Metadata.Identity.Version
    }

    function Read-CurrentVersionVsix {
        [System.Version] (Get-VsixManifestXml).PackageManifest.Metadata.Identity.Version
    }
    Export-ModuleMember -Function Read-CurrentVersion,Read-CurrentVersionVsix
}