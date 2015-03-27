<#
.SYNOPSIS
    Builds and deploys GitHub for Visual Studio
.DESCRIPTION
    First bumps the version number and commits and pushes that change to
    GitHub. Then builds GitHub for Visual Studio and deploys it to the
    specified release channel and bucket.
.PARAMETER ReleaseChannel
    The release channel to which you wish to deploy. Options are: staff
.PARAMETER S3Bucket
    Specifies which S3 bucket to upload to. Options are: development, production. Defaults to production.
.PARAMETER NewVersion
    Specifies the version number with which to stamp the build. Specify "None"
    to avoid changing the version number at all. By default, uses the currently
    checked-in version with the last component increased by one.
.PARAMETER Force
    Allow deploying even if the working tree isn't clean and/or HEAD hasn't
    been pushed to the origin remote.
.PARAMETER NoCampfire
    By default, Campfire is notified when deploys start/end. Passing this
    switch will cause Campfire messages to be printed to the console instead.
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("staff")]
    [string]
    $ReleaseChannel
    ,
    [ValidateSet("development", "production")]
    [string]
    $S3Bucket = "production"
    ,
    [string]
    $NewVersion
    ,
    [switch]
    $Force = $false
    ,
    [switch]
    $NoCampfire = $false
)

Set-StrictMode -Version Latest

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$modulesPath = (Join-Path $scriptsDirectory Modules)
$env:PATH = "$scriptsDirectory;$modulesPath;$env:PATH"
$env:PSModulePath = $env:PSModulePath + ";$modulesPath"
Import-Module "$modulesPath\wix"
Import-Module "$modulesPath\vsix"
#Get-Command -ListImported
Write-Output $scriptsDirectory

$rootDirectory = Split-Path ($scriptsDirectory)
#$git = Get-Command git.cmd
. $scriptsDirectory\common.ps1

Import-Module (Join-Path $scriptsDirectory Modules\CampfireUtilities)

$bucketName = ""
if ($S3Bucket -eq "production") {
    $bucketName = "github-vs"
} else {
    $bucketName = "github-vs-dev"
}

$keyPrefix = ""
if ($ReleaseChannel -ne "production") {
    $keyPrefix = $ReleaseChannel.ToLower() + "/"
}

$configuration = "Release"
$installUrl = "http://$bucketName.s3.amazonaws.com/$keyPrefix"
$vsixUrl = "${installUrl}GitHub.VisualStudio.vsix"

$startTime = Get-Date

Add-Type -AssemblyName "System.Core"
Add-Type -TypeDefinition @"
public class ScriptException : System.Exception
{
    public ScriptException(string message) : base(message)
    {
    }
}
"@

function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Throw (New-Object -TypeName ScriptException -ArgumentList $message)
}

function Run-Command([scriptblock]$Command, [switch]$Fatal, [switch]$Quiet) {
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
    if (!$? -and $LastExitCode -ne 0) {
        $exitCode = $LastExitCode
    } elseif (!$?) {
        $exitCode = 1
    } else {
        return
    }

    Die "``$Command`` failed" $output
}

function Get-CampfireUsername {
    Push-Location $rootDirectory
    $email = & $git config user.email
    Pop-Location

    if ($email -match "haacked") {
        "Haacked"
    } elseif ($email -match "shana") {
        "shana"
    } else {
        $email
    }
}

function Get-HeadSha1 {
    Push-Location $rootDirectory
    $version = & $git rev-parse HEAD 2>&1
    if (!$? -or ($LastExitCode -ne 0)) {
        Die "Error determining HEAD revision" $version
    }
    Pop-Location
    $version
}

function Get-DeployedSha1 {
    $client = New-Object -TypeName System.Net.WebClient
    try {
        $client.DownloadString($installUrl + "VERSION").TrimEnd()
    } catch [System.Net.WebException] {
        $response = $_.Exception.Response
        if ($response -and ($response.StatusCode -eq [System.Net.HttpStatusCode]::Forbidden)) {
            # This is what S3 returns when no VERSION file exists.
            return ""
        }
        Throw
    }
}

function Get-ShortSha1 {
    Param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [AllowEmptyString()]
        [string]
        $sha1
    )

    if ($sha1) {
        $sha1.Substring(0, 7)
    }
}

function Announce-Message([string]$message) {
    if ($NoCampfire) {
        Write-Output $message
    } else {
        HubotTell-NativeRoom $message
    }
}

function Announce-DeployStarted {
    $campfireUser = Get-CampfireUsername
    Push-Location $rootDirectory
    $branch = Get-CheckedOutBranch
    Pop-Location

    $url = "https://github.com/github/VisualStudio/"
    $deployedVersion = Get-DeployedSha1 | Get-ShortSha1
    if ($deployedVersion) {
        $url += "compare/{0}...{1}" -f $deployedVersion, (Get-HeadSha1 | Get-ShortSha1)
    } else {
        $url += "tree/{0}" -f $branch
    }
    $message = "{0} is deploying VisualStudio/{1} to {2} {3}" -f $campfireUser, $branch, $ReleaseChannel, $url
    Announce-Message $message
}

function Announce-DeployCompleted {
    $campfireUser = Get-CampfireUsername
    Push-Location $rootDirectory
    $branch = Get-CheckedOutBranch
    Pop-Location
    $duration = ((Get-Date) - $startTime).TotalSeconds
    $message = "{0}'s {1} deployment of VisualStudio/{2} is done! {3:F1}s {4}" -f $campfireUser, $ReleaseChannel, $branch, $duration, $vsixUrl
    Announce-Message $message
}

function Announce-DeployFailed([string]$error) {
    $campfireUser = Get-CampfireUsername
    Push-Location $rootDirectory
    $branch = Get-CheckedOutBranch
    Pop-Location
    $message = "{0}'s deploy of VisualStudio/{1} to {2} failed: {3}" -f $campfireUser, $branch, $ReleaseChannel, $error
    Announce-Message $message
}

function Require-HeadIsPushedToOrigin {
    Push-Location $rootDirectory

    $branchesOnOriginContainingHead = & $git branch --remote --contains HEAD | Select-String "origin/"

    if (!$branchesOnOriginContainingHead) {
        $error = "You must first push HEAD to the origin remote."
        if ($Force) {
            Write-Warning "$error Continuing anyway."
        } else {
            Die "Cannot deploy: $error"
        }
    }

    Pop-Location
}

function Clean-BuildDirectory {
    Write-Output "Cleaning build directory..."

    $solution = Join-Path $rootDirectory GitHubVs.sln
    Run-Command -Quiet -Fatal { & msbuild $solution /target:Clean /property:Configuration=$configuration }
}

function Create-TempDirectory {
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
    New-Item -Type Directory $path
}

function Build-Vsix([string]$directory) {
    $solution = Join-Path $rootDirectory GitHubVs.sln
    Run-Command -Quiet -Fatal { msbuild $solution /property:Configuration=$configuration /property:ReleaseChannel=$ReleaseChannel /property:S3Bucket=$S3Bucket /property:DeployExtension=false }

    Copy-Item (Join-Path $rootDirectory build\$configuration\GitHub.VisualStudio.vsix) $directory
}

function Build-Installer([string]$directory) {
    $solution = Join-Path $rootDirectory GitHubVs.sln
    Run-Command -Quiet -Fatal { msbuild $solution /property:Configuration=Publish }

    Copy-Item (Join-Path $rootDirectory build\$configuration\ghfvs.msi) $directory
}

function Write-Manifest([string]$directory) {
    Add-Type -Path (Join-Path $rootDirectory packages\Newtonsoft.Json.6.0.8\lib\net35\Newtonsoft.Json.dll)

    $manifest = @{
        NewestExtension = @{
            Version = [string](Read-CurrentVersionVsix)
            Url = $vsixUrl
        }
    }

    $manifestPath = Join-Path $directory manifest
    [Newtonsoft.Json.JsonConvert]::SerializeObject($manifest) | Out-File $manifestPath -Encoding UTF8
}

function Write-VersionFile([string]$directory) {
    $versionFile = Join-Path $directory VERSION
    Get-HeadSha1 | Set-Content $versionFile
}

function Save-TopLevelFiles([string]$directory) {
    $files = Get-ChildItem $directory | %{ $_.FullName }

    $versionSpecificDirectory = New-Item (Join-Path $directory (Read-CurrentVersionVsix)) -Type Directory

    Copy-Item $files $versionSpecificDirectory.FullName
}

function Upload-Symbols {
    Write-Host "Generating symbols..."

    $symbols = Create-TempDirectory

    $symstore = Join-Path $scriptsDirectory "Debugging Tools for Windows\symstore.exe"
    $buildDirectory = Join-Path $rootDirectory build\$configuration
    Run-Command -Quiet -Fatal { & $symstore add /r /f "$buildDirectory\*.*" /t "GitHub for Visual Studio" /s $symbols }

    if ($S3Bucket -eq "production") {
        # Upload our symbols to the same place as GHfW's symbols so that
        # developers only need to use a single symbol server.
        $symbolsBucket = "github-windows"
    } else {
        # This is a test deploy, so we shouldn't pollute the standard symbol server.
        $symbolsBucket = $bucketName
    }

    Write-Output "Uploading symbols to S3..."
    Run-Command -Quiet -Fatal { Upload-DirectoryToS3 $symbols -S3Bucket $symbolsBucket -KeyPrefix "symbols/" -Lowercase }

    Remove-Item -Recurse $symbols
}

function Upload-Vsix([string]$directory) {
    Write-Output "Uploading extension to S3..."
    # We don't allow the top-level files to be cached to keep caching proxies from hiding our updates.
    Run-Command -Quiet -Fatal { Upload-DirectoryToS3 $directory -S3Bucket $bucketName -KeyPrefix $keyPrefix -AllowCachingUnless { $_.Directory.FullName -eq $directory } }
}

Run-Command -Fatal {
    if ($NewVersion) {
        if ($NewVersion -ne "None") {
            Bump-Version $NewVersion
        }
    } else {
        Bump-Version
    }
}

Run-Command -Fatal { Require-CleanWorkTree "deploy" -WarnOnly:$Force }

Require-HeadIsPushedToOrigin

Announce-DeployStarted

& {
    Trap {
        Announce-DeployFailed $_
        break
    }

    Clean-BuildDirectory
    $tempDirectory = Create-TempDirectory
    Build-Vsix $tempDirectory
    Add-SignatureToVsix (Join-Path $tempDirectory GitHub.VisualStudio.vsix)
	Build-Installer $tempDirectory
	Add-SignatureToWiX (Join-Path $tempDirectory ghfvs.msi)
    Write-Manifest $tempDirectory
    Write-VersionFile $tempDirectory
    Save-TopLevelFiles $tempDirectory

	Write-Output "Ready at ${tempDirectory}"

    Upload-Symbols
    Upload-Vsix $tempDirectory

    Remove-Item -Recurse $tempDirectory

    Write-Output "Finished deploying GitHub for Visual Studio to ${vsixUrl}"
    Announce-DeployCompleted
}
