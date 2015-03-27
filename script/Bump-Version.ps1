<#
.SYNOPSIS
    Bumps the version number of GitHub for Visual Studio
.DESCRIPTION
    By default, just bumps the last component of the version number by one. An
    alternate version number can be specified on the command line.

    The new version number is committed to the local repository and pushed to
    GitHub.
#>

Param(
    # It would be nice to use our Validate-Version function here, but we
    # can't because this Param definition has to come before any other code in the
    # file.
    [ValidateScript({ ($_.Major -ge 0) -and ($_.Minor -ge 0) -and ($_.Build -ge 0) -and ($_.Revision -ge 0) })]
    [System.Version]
    $NewVersion = $null
)

Set-StrictMode -Version Latest

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$rootDirectory = Split-Path $scriptsDirectory
$env:PATH = "$scriptsDirectory;$env:PATH"
$env:PSModulePath = $env:PSModulePath + ";$scriptsDirectory\Modules"

#$git = Get-Command git.cmd
. $scriptsDirectory\common.ps1

#Import-Module (Join-Path $scriptsDirectory Modules\Utilities)

function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Write-Error $message
    exit 1
}

function Validate-Version([System.Version]$version) {
    ($version.Major -ge 0) -and ($version.Minor -ge 0) -and ($version.Build -ge 0) -and ($version.Revision -ge 0)
}

function Bump-Version {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $currentVersion
        ,
        [System.Version]
        $proposedVersion
    )

    if ($proposedVersion) {
        if ($currentVersion -ge $proposedVersion) {
            Die "Proposed version $proposedVersion is not higher than the current version $currentVersion"
        }
        return $proposedVersion
    }

    New-Object -TypeName System.Version -ArgumentList $currentVersion.Major, $currentVersion.Minor, $currentVersion.Build, ($currentVersion.Revision + 1)
}

function Write-VersionVsixManifest {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    $document = Get-VsixManifestXml

    $numberOfReplacements = 0
    $document.PackageManifest.Metadata.Identity.Version = $version.ToString()

    $document.Save((Get-VsixManifestPath))
}

function Write-VersionInstaller{
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    $document = Get-WiXManifestXml

    $numberOfReplacements = 0
    $document.WiX.Product.Version = $version.ToString()

    $document.Save((Get-WiXManifestPath))
}

function Write-VersionAssemblyInfo {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    $assemblyInfo = Join-Path $scriptsDirectory SolutionInfo.cs
	Write-Output $assemblyInfo
    $numberOfReplacements = 0
    $newContent = Get-Content $assemblyInfo | %{
        $regex = "(Assembly(?:File)?Version)\(`"\d+\.\d+\.\d+\.\d+`"\)"
        $newString = $_
        if ($_ -match $regex) {
            $numberOfReplacements++
            $newString = $_ -replace $regex, "`$1(`"$version`")"
        }
        $regex = "(const string Version = )`"\d+\.\d+\.\d+\.\d+`";"
        $newString = $_
        if ($_ -match $regex) {
            $numberOfReplacements++
            $newString = $_ -replace $regex, "`$1`"$version`";"
        }
        $newString
    }

    if ($numberOfReplacements -ne 3) {
        Die "Expected to replace the version number in 3 places in SolutionInfo.cs (AssemblyVersion, AssemblyFileVersion, const string Version) but actually replaced it in $numberOfReplacements"
    }

    $newContent | Set-Content $assemblyInfo
}

function Write-Version {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    Write-VersionVsixManifest $version
	Write-VersionInstaller $version
    Write-VersionAssemblyInfo $version
}

function Commit-VersionBump {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    Write-Output "Committing version bump..."

    Push-Location $rootDirectory

    $output = & $git commit --all --message "Bump version to $version" 2>&1
    if (!$? -or ($LastExitCode -ne 0)) {
        Die "Error committing version bump" $output
    }

    Pop-Location
}

function Push-Changes {
    Push-Location $rootDirectory

    $branch = Get-CheckedOutBranch
    Write-Output "Pushing $branch to GitHub..."

    $output = & $git push origin $branch 2>&1
    if ($LastExitCode -ne 0) {
        Die "Error pushing $branch to GitHub" $output
    }

    Pop-Location
}

Require-CleanWorkTree "bump version"
if (!$? -or ($LastExitCode -ne 0)) {
    exit 1
}

$currentVersion = Read-CurrentVersion
$NewVersion = Bump-Version $currentVersion $NewVersion
Write-Version $NewVersion
Commit-VersionBump $NewVersion
Push-Changes
