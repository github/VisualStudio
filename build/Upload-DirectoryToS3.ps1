<#
.SYNOPSIS
    Uploads a directory to GitHub's S3 account
.DESCRIPTION
    All files in the directory (recursively) are uploaded. A file's key is its
    path relative to the directory (plus an optional -KeyPrefix).
.PARAMETER Directory
    The directory to upload.
.PARAMETER S3Bucket
    The S3 bucket to upload to.
.PARAMETER KeyPrefix
    A string to use as a prefix on all file keys.
.PARAMETER Lowercase
    If true, keys will be lowercased when uploading. Defaults to false.
.PARAMETER UseClickOnceSort
    If true, uploads files in an order optimal for ClickOnce deployments. Defaults to false.
.PARAMETER AllowCachingUnless
    A script block that is passed a FileInfo in $_ for each file to be uploaded
    and returns true if that file should not be allowed to be cached. If no
    script block is passed, all files are allowed to be cached.
#>

Param(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Directory
    ,
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]
    $S3Bucket
    ,
    [string]
    $KeyPrefix
    ,
    [switch]
    $Lowercase = $false
    ,
    [switch]
    $UseClickOnceSort = $false
    ,
    [scriptblock]
    $AllowCachingUnless
)

Set-PSDebug -Strict
$ErrorActionPreference = "Stop"

[System.IO.DirectoryInfo] $Directory = [string] (Resolve-Path $Directory)

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
Import-Module (Join-Path $scriptsDirectory Modules\S3Utilities)

$contentTypes = @{
    ".application" = "application/x-ms-application";
    ".manifest" = "application/x-ms-manifest";
    ".json" = "application/json";
    ".jsonp" = "text/javascript";
    ".vsix" = "application/vsix";
}
$defaultContentType = "application/octet-stream"

# AWSSDK automatically adds "x-amz-meta-" when uploading, but doesn't strip it
# when downloading. :facepalm:
$checksumMetadataUploadKey = "checksum"
$checksumMetadataDownloadKey = "x-amz-meta-{0}" -f $checksumMetadataUploadKey

Add-Type -Path (Join-Path $scriptsDirectory AWSSDK.1.4.6.3\lib\AWSSDK.dll)

function Get-FileKey([System.IO.FileInfo]$file) {
    $relativePath = $file.FullName.Substring($Directory.FullName.Length + 1);
    $key = $KeyPrefix + $relativePath.Replace([System.IO.Path]::DirectorySeparatorChar, "/")
    if ($Lowercase) {
        $key = $key.ToLower()
    }
    $key
}

function Get-S3FileChecksum([System.IO.FileInfo]$file, [Amazon.S3.AmazonS3Client]$client) {
    $request = New-Object -TypeName Amazon.S3.Model.GetObjectMetadataRequest
    $request.BucketName = $S3Bucket
    $request.Key = Get-FileKey $file
    try {
        $response = $client.GetObjectMetadata($request)
        $checksumFromMetadata = $response.Metadata.Get($checksumMetadataDownloadKey)
        if ($checksumFromMetadata) {
            return $checksumFromMetadata
        }
        return $response.ETag.Trim('"')
    } catch [Amazon.S3.AmazonS3Exception] {
    }
}

function Get-FileChecksum([System.IO.FileInfo]$file) {
    $fileStream = New-Object System.IO.FileStream($file.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    try {
        return [Amazon.S3.Util.AmazonS3Util]::GenerateChecksumForStream($fileStream, $false)
    } finally {
        $fileStream.Close()
    }
}

function Test-FileAlreadyOnS3([System.IO.FileInfo]$file, [string]$fileChecksum, [Amazon.S3.AmazonS3Client]$client) {
    $fileChecksum -eq (Get-S3FileChecksum $file $client)
}

$client = New-S3Client
$transfer = New-S3TransferUtility $client

$files = @(Get-ChildItem -Recurse $Directory | ?{ !$_.PSIsContainer })
if ($UseClickOnceSort) {
    # We want to upload the top-level deployment manifest (Foo.application) and
    # changelog (changelog.*) last. So first we sort all top-level files to the
    # end, then sort the manifest and changelog to the end of the top-level files.
    $files = $files | Sort-Object { -($_.FullName -split "\\").Count }, { $_.Name -like "*.application" }, { $_.Name -like "changelog.*" }
}

$fileSizes = $files | %{ $_.Length }
$totalBytes = 0
$fileSizes | %{ $totalBytes += $_ }

function Update-Progress([long]$currentFileTransferredBytes, [int]$currentFilePercentDone) {
    $transferredBytes = $currentFileTransferredBytes
    if ($i -gt 0) {
        $fileSizes[0..($i - 1)] | %{ $transferredBytes += $_ }
    }
    $percentComplete = $transferredBytes * 100 / $totalBytes

    $status = "Uploading file {0} of {1}" -f $i, $files.Length
    Write-Progress -Id 1 -Activity "Uploading to S3" -Status $status -PercentComplete $percentComplete

    Write-Progress -Id 2 -ParentId 1 -Activity " " -Status $files[$i] -PercentComplete $currentFilePercentDone
}

$progressHandler = {
    Param(
        [object]
        $sender
        ,
        [Amazon.S3.Transfer.UploadProgressArgs]
        $event
    )

    Update-Progress $event.TransferredBytes $event.PercentDone
}

for ($i = 0; $i -lt $files.Length; $i++) {
    $file = $files[$i]
    Update-Progress 0 0

    $checksum = Get-FileChecksum $file
    if (Test-FileAlreadyOnS3 $file $checksum $client) {
        continue
    }

    $request = New-Object -TypeName Amazon.S3.Transfer.TransferUtilityUploadRequest
    $request.BucketName = $S3Bucket
    $request.CannedACL = [Amazon.S3.Model.S3CannedACL]::PublicRead
    $request.WithMetadata($checksumMetadataUploadKey, $checksum) | Out-Null

    if ($AllowCachingUnless) {
        # Piping the file to $AllowCachingUnless causes it to be accessible via $_
        # within the $AllowCachingUnless script block.
        $disallowCaching = $file | %{ & $AllowCachingUnless }
        if ($disallowCaching) {
            $request.AddHeader("Cache-Control", "max-age=0, no-cache, no-store")
            $request.AddHeader("Expires", "Thu, 01 Dec 1994 16:00:00 GMT")
        }
    }

    $request.add_UploadProgressEvent($progressHandler)

    $request.FilePath = $file.FullName
    $request.Key = Get-FileKey $file
    if ($Lowercase) {
        $request.Key = $request.Key.ToLower()
    }
    if ($contentTypes.ContainsKey($file.Extension)) {
        $request.ContentType = $contentTypes[$file.Extension]
    } else {
        $request.ContentType = $defaultContentType
    }

    $transfer.Upload($request)
}
