<#
.SYNOPSIS
    Builds and deploys GitHub for Visual Studio
.DESCRIPTION
    First bumps the version number and commits and pushes that change to
    GitHub. Then builds GitHub for Visual Studio and deploys it to the
    specified release channel and bucket.
.PARAMETER ReleaseChannel
    The release channel to which you wish to deploy. Options are: dev, alpha, beta, production
.PARAMETER NewVersion
    Specifies the version number with which to stamp the build. Specify "None"
    to avoid changing the version number at all. By default, uses the currently
    checked-in version with the last component increased by one.
.PARAMETER Force
    Allow deploying even if the working tree isn't clean and/or HEAD hasn't
    been pushed to the origin remote.
.PARAMETER NoChat
    By default, Campfire is notified when deploys start/end. Passing this
    switch will cause Campfire messages to be printed to the console instead.
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("dev", "alpha", "beta", "production")]
    [string]
    $ReleaseChannel = "dev"
    ,
    [string]
    $NewVersion
    ,
    [string]
    $Branch
    ,
    [switch]
    $Force = $false
    ,
    [switch]
    $NoChat = $false
    ,
    [switch]
    $NoPush = $false
)

Set-StrictMode -Version Latest

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$modulesPath = (Join-Path $scriptsDirectory Modules)
$env:PATH = "$scriptsDirectory;$modulesPath;$env:PATH"
$env:PSModulePath = $env:PSModulePath + ";$modulesPath"
Import-Module "$modulesPath\wix"
Import-Module "$modulesPath\vsix"

$rootDirectory = Split-Path ($scriptsDirectory)

. $scriptsDirectory\common.ps1

if (!$NoChat) {
    Import-Module (Join-Path $scriptsDirectory Modules\CampfireUtilities)
}

if (!$Branch) {
    $Branch = Get-CheckedOutBranch
}

Write-Output "Branch is $Branch"

$bucketName = "github-vs"

$keyPrefix = "releases/" + $ReleaseChannel.ToLower() + "/"
$symbolsPrefix = "symbols/" + $ReleaseChannel.ToLower() + "/"

$configuration = "Release"
$installUrl = "http://$bucketName.s3.amazonaws.com/$keyPrefix"

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
    if ($NoChat) {
        Write-Output $message
    } else {
        HubotTell-NativeRoom $message
    }
}

function Announce-DeployStarted {
    $campfireUser = Get-CampfireUsername

    $deployedVersion = Get-DeployedSha1 | Get-ShortSha1
    if ($deployedVersion) {
        $url += "compare/{0}...{1}" -f $deployedVersion, (Get-HeadSha1 | Get-ShortSha1)
    } else {
        $url += "tree/{0}" -f $Branch
    }
    $message = "{0} is deploying VisualStudio/{1} to {2}" -f $campfireUser, $Branch, $ReleaseChannel
    Announce-Message $message
}

function Announce-DeployCompleted {
    $campfireUser = Get-CampfireUsername
    $duration = ((Get-Date) - $startTime).TotalSeconds
    $message = "{0}'s {1} deployment of VisualStudio/{2} is done! {3:F1}s" -f $campfireUser, $ReleaseChannel, $Branch, $duration
    Announce-Message $message
}

function Announce-DeployFailed([string]$error) {
    $campfireUser = Get-CampfireUsername
    $message = "{0}'s deploy of VisualStudio/{1} to {2} failed: {3}" -f $campfireUser, $Branch, $ReleaseChannel, $error
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
    Try {
        $path = pushd $rootDirectory\build\$configuration -PassThru -ErrorAction Stop
        Run-Command -Fatal { & $git clean -xdf }
        popd
    } Catch {}

    Run-Command -Fatal { & $msbuild $solution /target:Clean /property:Configuration=$configuration /verbosity:minimal }
}

function Create-TempDirectory {
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
    New-Item -Type Directory $path
}

function Build-Vsix([string]$directory) {
    $solution = Join-Path $rootDirectory GitHubVs.sln
    Run-Command -Fatal { & $nuget restore $solution -NonInteractive -Verbosity detailed }
    Run-Command -Fatal { & $msbuild $solution /target:Rebuild /property:Configuration=$configuration /p:ReleaseChannel=$ReleaseChannel /p:DeployExtension=false /verbosity:minimal /p:VisualStudioVersion=14.0 }

    Copy-Item (Join-Path $rootDirectory build\$configuration\GitHub.VisualStudio.vsix) $directory
}

function Build-Installer([string]$directory) {
    $solution = Join-Path $rootDirectory GitHubVs.sln

    Run-Command -Fatal { & $msbuild $solution /property:Configuration=Publish /verbosity:minimal }

    Copy-Item (Join-Path $rootDirectory build\$configuration\ghfvs.msi) $directory
}

function Write-Manifest([string]$directory) {
    Add-Type -Path (Join-Path $rootDirectory packages\Newtonsoft.Json.6.0.8\lib\net35\Newtonsoft.Json.dll)

    $manifest = @{
        NewestExtension = @{
            Version = [string](Read-CurrentVersionVsix)
            Commit = [string](Get-HeadSha1)
        }
    }

    $manifestPath = Join-Path $directory manifest
    [Newtonsoft.Json.JsonConvert]::SerializeObject($manifest) | Out-File $manifestPath -Encoding UTF8
}

function Get-MD5($path) {
    $fullPath = Resolve-Path $path
    $md5 = new-object -TypeName System.Security.Cryptography.MD5CryptoServiceProvider
    $file = [System.IO.File]::Open($fullPath,[System.IO.Filemode]::Open, [System.IO.FileAccess]::Read)
    [System.BitConverter]::ToString($md5.ComputeHash($file)) | %{$_ -replace "-", ""}
    $file.Dispose()
}

function Save-MD5($path, $file) {
    $outpath = (Join-Path $path.FullName "$file.md5")
    $hash = Get-MD5 (Join-Path $path $file)
    [System.IO.File]::AppendAllText("$outpath", "$hash  $file", [System.Text.Encoding]::Ascii)
}

function Save-TopLevelFiles([string]$directory) {
    $files = Get-ChildItem $directory | %{ $_.FullName }

    $versionSpecificDirectory = New-Item (Join-Path $directory (Read-CurrentVersionVsix)) -Type Directory

    Move-Item $files $versionSpecificDirectory.FullName
    Save-MD5 $versionSpecificDirectory "ghfvs.msi"
    Save-MD5 $versionSpecificDirectory "GitHub.VisualStudio.vsix"

    Add-Type -assembly "system.io.compression.filesystem"
    $destination = Join-path -path $directory -ChildPath "ghfvs-$($versionSpecificDirectory.name).zip"
    Run-Command -Fatal { [io.compression.zipfile]::CreateFromDirectory($versionSpecificDirectory.fullname, $destination) }
    return "ghfvs-$($versionSpecificDirectory.name).zip"
}

function Upload-Symbols {
    Write-Host "Generating symbols..."

    $symbols = Create-TempDirectory

    $symstore = Join-Path $scriptsDirectory "Debugging Tools for Windows\symstore.exe"
    $buildDirectory = Join-Path $rootDirectory build\$configuration
    Run-Command -Quiet -Fatal { & $symstore add /r /f "$buildDirectory\*.*" /t "GitHub for Visual Studio" /s $symbols }

    Write-Output "Uploading symbols to S3..."
    Run-Command -Quiet -Fatal { Upload-DirectoryToS3 $symbols -S3Bucket $bucketName -KeyPrefix  $symbolsPrefix -Lowercase }

    Remove-Item -Recurse $symbols
}

function Upload-Vsix([string]$directory) {
    Write-Output "Uploading extension to S3..."
    # We don't allow the top-level files to be cached to keep caching proxies from hiding our updates.
    #Run-Command -Quiet -Fatal { Upload-DirectoryToS3 $directory -S3Bucket $bucketName -KeyPrefix $keyPrefix -AllowCachingUnless { $_.Directory.FullName -eq $directory } }
    Run-Command -Fatal { Upload-DirectoryToS3 $directory -S3Bucket $bucketName -KeyPrefix $keyPrefix -AllowCachingUnless { $false } }
}

#if ($NoPush -and $ReleaseChannel -eq "production") {
#    Die "-NoPush cannot be used for production deployments."
#}

Run-Command -Fatal { Require-CleanWorkTree "deploy" -WarnOnly:$Force }

Require-HeadIsPushedToOrigin

Run-Command -Fatal {
    if ($NewVersion) {
        if ($NewVersion -ne "None") {
            Bump-Version $NewVersion -ReleaseChannel:$ReleaseChannel -Branch:$Branch -NoPush:$NoPush
        }
    } else {
        Bump-Version -ReleaseChannel:$ReleaseChannel -Branch:$Branch -NoPush:$NoPush
    }
}

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
    $zipfile = Save-TopLevelFiles $tempDirectory

    Write-Output "Ready at ${tempDirectory}"

    Upload-Symbols
    Upload-Vsix $tempDirectory

    Remove-Item -Recurse $tempDirectory

    Write-Output "Finished deploying GitHub for Visual Studio to ${bucketName}/${keyPrefix}"
    Announce-DeployCompleted
}
