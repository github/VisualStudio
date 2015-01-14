Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsDirectory = Split-Path (Split-Path (Split-Path $MyInvocation.MyCommand.Path))
$rootDirectory = Split-Path $scriptsDirectory
$gitHubDirectory = Join-Path $rootDirectory GitHub

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
    [System.Version] (Get-VsixManifestXml).Vsix.Identifier.Version
}

# Code came from http://go.microsoft.com/fwlink/?LinkId=183989
function Add-SignatureToVsix {
    Param(
        [Parameter(Mandatory=$true)]
        [string]
        $VsixPath
    )

    $certificate = Get-ChildItem Cert:\CurrentUser\My | ?{ $_.Thumbprint -eq "1DFD43125FA770742AC7E01E621A737657454380" }
    if (!$certificate) {
        throw "Can't find GitHub certificate"
    }

    [Reflection.Assembly]::Load("WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35") | Out-Null

    $package = [IO.Packaging.Package]::Open((Resolve-Path $VsixPath).Path, [IO.FileMode]::Open)

    try {
        $manager = New-Object IO.Packaging.PackageDigitalSignatureManager($package) -Property @{
            CertificateOption = [IO.Packaging.CertificateEmbeddingOption]::InSignaturePart
        }

        $parts = New-Object Collections.Generic.List[Uri]
        foreach ($part in $package.GetParts()) {
            $parts.Add($part.Uri)
        }
        $parts.Add([IO.Packaging.PackUriHelper]::GetRelationshipPartUri($manager.SignatureOrigin))
        $parts.Add($manager.SignatureOrigin)
        $parts.Add([IO.Packaging.PackUriHelper]::GetRelationshipPartUri((New-Object Uri("/", [UriKind]::RelativeOrAbsolute))))

        $manager.Sign($parts, $certificate) | Out-Null
    } finally {
        $package.Close()
    }
}
