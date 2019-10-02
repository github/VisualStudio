Set-StrictMode -Version Latest

New-Module -ScriptBlock {

    function Get-VsixManifestPath([string]$projectPath) {
		$gitHubDirectory = Join-Path $rootDirectory $projectPath
        Join-Path $gitHubDirectory source.extension.vsixmanifest
    }

    function Get-VsixManifestXml([string]$projectPath) {
        $xmlLines = Get-Content (Get-VsixManifestPath($projectPath))
        # If we don't explicitly join the lines with CRLF, comments in the XML will
        # end up with LF line-endings, which will make Git spew a warning when we
        # try to commit the version bump.
        $xmlText = $xmlLines -join [System.Environment]::NewLine

        [xml] $xmlText
    }

    function Read-CurrentVersionVsix([string]$projectPath) {
        [System.Version] (Get-VsixManifestXml($projectPath)).PackageManifest.Metadata.Identity.Version
    }

    function Write-VersionVsixManifest([System.Version]$version,[string]$projectPath) {

        $document = Get-VsixManifestXml($projectPath)

        $numberOfReplacements = 0
        $document.PackageManifest.Metadata.Identity.Version = $version.ToString()

        $document.Save((Get-VsixManifestPath($projectPath)))
    }

    Export-ModuleMember -Function Read-CurrentVersionVsix,Write-VersionVsixManifest
}