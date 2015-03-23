Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsDirectory = Split-Path (Split-Path (Split-Path $MyInvocation.MyCommand.Path))
Add-Type -Path (Join-Path $scriptsDirectory AWSSDK.1.5.2.0\lib\AWSSDK.dll)

function Write-S3KeysMissing
{
	write-host ""
    write-host "**************" -foregroundcolor White -backgroundcolor Red
    write-host "S3 Keys aren't in the repo or ~/s3_keys.ps1, get them from another Winhubber" -foregroundcolor White -backgroundcolor Red
    write-host "and create your own version of s3_keys.ps1.example as s3_keys.ps1" -foregroundcolor White -backgroundcolor Red
    write-host "**************" -foregroundcolor White -backgroundcolor Red
	write-host ""
}

$S3keys = Join-Path $scriptsDirectory "s3_keys.ps1"
if (!(test-path $S3keys)) {
    $S3keys = Join-Path "~" "s3_keys.ps1"
	if (!(test-path $S3keys)) {
	    Write-S3KeysMissing
		throw "S3 Keys aren't in the repo or ~/s3_keys.ps1, get them from another Winhubber and create your own version of s3_keys.ps1.example as s3_keys.ps1"
	}
}

try {
    . $S3keys
} catch [Exception] {
    Write-S3KeysMissing
    throw $_.Exception
}

function New-S3Client
{
    $config = New-Object Amazon.S3.AmazonS3Config
    $config.BufferSize = 1024 * 64
    [Amazon.AWSClientFactory]::CreateAmazonS3Client($accessKey, $secretKey, $config)
}

function New-S3TransferUtility([Amazon.S3.AmazonS3Client]$client)
{
    if (!$client) {
        $client = New-S3Client
    }

    New-Object Amazon.S3.Transfer.TransferUtility $client
}
