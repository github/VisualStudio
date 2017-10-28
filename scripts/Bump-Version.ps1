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
    [ValidateScript({ ($_.Major -ge 0) -and ($_.Minor -ge 0) -and ($_.Build -ge 0) })]
    [System.Version]
    $NewVersion = $null
    ,
    [switch]
    $BumpMajor = $false
    ,
    [switch]
    $BumpMinor = $false
    ,
    [switch]
    $BumpPatch = $false
    ,
    [string]
    $Branch
    ,
    [switch]
    $Push = $false
	,
    [switch]
    $Force = $false
)

Set-StrictMode -Version Latest

$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$modulesPath = (Join-Path $scriptsDirectory Modules)
$env:PATH = "$scriptsDirectory;$modulesPath;$env:PATH"
$env:PSModulePath = $env:PSModulePath + ";$modulesPath"
Import-Module "$modulesPath\wix"
Import-Module "$modulesPath\vsix"

$rootDirectory = Split-Path ($scriptsDirectory)
$sourceDirectory = Join-Path $rootDirectory src\GitHub.VisualStudio

. $scriptsDirectory\common.ps1

function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Write-Error $message
    exit 1
}

function Validate-Version([System.Version]$version) {
    ($version.Major -ge 0) -and ($version.Minor -ge 0) -and ($version.Build -ge 0)
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

    if ($BumpMajor) {
        New-Object -TypeName System.Version -ArgumentList ($currentVersion.Major + 1), $currentVersion.Minor, $currentVersion.Build, 0
    } elseif ($BumpMinor) {
        New-Object -TypeName System.Version -ArgumentList $currentVersion.Major, ($currentVersion.Minor + 1), $currentVersion.Build, 0
    } elseif ($BumpPatch) {
        New-Object -TypeName System.Version -ArgumentList $currentVersion.Major, $currentVersion.Minor, ($currentVersion.Build + 1), 0
    }
}

function Read-CurrentVersion {
    $element = Get-ApplicationVersionElement (Get-CsprojXml)
    [System.Version] $element.InnerText
}

function Get-CsprojPath {
    Join-Path $sourceDirectory GitHub.VisualStudio.csproj
}

function Get-CsprojXml {
    $xmlLines = Get-Content (Get-CsprojPath)
    # If we don't explicitly join the lines with CRLF, comments in the XML will
    # end up with LF line-endings, which will make Git spew a warning when we
    # try to commit the version bump.
    $xmlText = $xmlLines -join [System.Environment]::NewLine

    [xml] $xmlText
}

function Get-ApplicationVersionElement([xml]$csproj) {
    $xmlns = New-Object Xml.XmlNamespaceManager($csproj.NameTable)
    $xmlns.AddNamespace("vs", $csproj.Project.xmlns)
    $nodeList = $csproj.SelectNodes("/vs:Project/vs:PropertyGroup/vs:ApplicationVersion", $xmlns)
    if ($nodeList.Count -ne 1) {
        throw ("Expected only one ApplicationVersion element, but found {0}" -f $nodeList.Count)
    }

    $nodeList.Item(0)
}

function Write-VersionCsproj {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    $document = Get-CsprojXml disk
    $element = Get-ApplicationVersionElement $document
    $element.InnerText = $version.ToString()
    $document.Save((Get-CsprojPath))
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

    $content = @"
<?xml version="1.0" encoding="utf-8"?>
<Include>
  <?define VersionNumber="$version" ?>
</Include>
"@

    $file = Get-WiXVersionFile
    $content | Set-Content $file
}

function Write-VersionAssemblyInfo {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    $assemblyInfo = Join-Path $rootDirectory "src\common\SolutionInfo.cs"
	Write-Output $assemblyInfo
    $numberOfReplacements = 0
    $newContent = Get-Content $assemblyInfo | %{
        $newString = $_
        $regex = "(const string Version = )`"\d+\.\d+\.\d+\.\d+`";"
        if ($_ -match $regex) {
            $numberOfReplacements++
            $newString = $_ -replace $regex, "`$1`"$version`";"
        }
        $newString
    }

    if ($numberOfReplacements -ne 1) {
        Die "Expected to replace the version number in 1 place in SolutionInfo.cs (AssemblyVersion, AssemblyFileVersion, const string Version) but actually replaced it in $numberOfReplacements"
    }

    $newContent | Set-Content $assemblyInfo
}

function Write-Version {
    Param(
        [ValidateScript({ Validate-Version $_ })]
        [System.Version]
        $version
    )

    Write-VersionCsproj $version
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

    $output = Start-Process $git "commit --all --message ""Bump version to $version""" -wait -NoNewWindow -ErrorAction Continue -PassThru
    if ($output.ExitCode -ne 0) {
        Die "Error committing version bump"
    }

    Pop-Location
}

if (!$Force){
    Require-CleanWorkTree "bump version"
}

if (!$? -or ($LastExitCode -ne 0)) {
    exit 1
}


if (!$BumpMajor -and !$BumpMinor -and !$BumpPatch -and ($NewVersion -eq $null -or !(Validate-Version($NewVersion)))) {
    Die "You need to indicate which part of the version to update via -BumpMajor/-BumpMinor/-BumpPatch flags or a custom version via -NewVersion"
}

$currentVersion = Read-CurrentVersion
$NewVersion = Bump-Version $currentVersion $NewVersion
Write-Version $NewVersion
Commit-VersionBump $NewVersion

if ($Push) {
    Push-Changes $Branch
}
