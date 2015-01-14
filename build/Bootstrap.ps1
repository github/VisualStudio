<#
.SYNOPSIS
    Bootstraps a machine for GitHub for Visual Studio development
.PARAMETER ImportCertificatesOnly
    When true, only imports our code-sigining certificates and does nothing else. Defaults to false.
#>

Param(
    [switch]
    $ImportCertificatesOnly = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$rootDirectory = Split-Path $scriptsDirectory

function Get-RandomTempFileName {
    Join-Path ([IO.Path]::GetTempPath()) ([IO.Path]::GetRandomFileName())
}

function Create-TempDirectory {
    New-Item (Get-RandomTempFileName) -Type Directory
}

function Download-File($url, $file) {
    $client = (New-Object Net.WebClient)
    $client.DownloadFile($url, $file)
}

function Install-VisualStudioSdk($productName, $url) {
    $query = "SELECT * FROM Win32_Product WHERE Name = `"$productName`""
    if ((Get-WmiObject -Query $query)) {
        return
    }

    Write-Host "Downloading $productName..."
    $tempFile = (Get-RandomTempFileName) + ".exe"
    Download-File $url $tempFile
    try {
        & $tempFile
        do {
            Start-Sleep -Seconds 2
        } while (!(Get-WmiObject -Query $query))
    } finally {
        Remove-Item -Force $tempFile
    }
}

function Import-GitHubCertificates {
    $storeLocation = "CurrentUser"
    $storeName = "My"
    $thumbprint = "1DFD43125FA770742AC7E01E621A737657454380"
    $previouslyImported = Get-ChildItem Cert:\$storeLocation\$storeName | ?{ $_.Thumbprint -eq $thumbprint }
    if ($previouslyImported) {
        return
    }

    $certificatePath = Join-Path $rootDirectory Certificates.pfx
    $pfx = New-Object Security.Cryptography.X509Certificates.X509Certificate2
    $password = "hubernaugt99"
    $pfx.Import($certificatePath, $password, "PersistKeySet")
    if ($pfx.Thumbprint -ne $thumbprint) {
        $error = "ERROR: Expected certificate thumbprint {0} but got {1}" -f $thumbprint, $pfx.Thumbprint
        throw $error
    }

    $store = New-Object Security.Cryptography.X509Certificates.X509Store($storeName, $storeLocation)
    $store.Open("ReadWrite")
    $store.Add($pfx)
    $store.Close()
}

Import-GitHubCertificates
if ($ImportCertificatesOnly) {
    exit
}

Install-VisualStudioSdk "Microsoft Visual Studio 2013 SDK" " http://download.microsoft.com/download/9/1/0/910EE61D-A231-4DAB-BD56-DCE7092687D5/vssdk_full.exe"

Write-Host "Your machine is now bootstrapped!"
