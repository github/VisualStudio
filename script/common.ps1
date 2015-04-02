$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$rootDirectory = Split-Path $scriptsDirectory
$gitHubDirectory = Join-Path $rootDirectory src\GitHub.VisualStudio
$msbuild = "$(get-content env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

$git = (Get-Command 'git.exe').Path
if (!$git) {
  $git = Join-Path $rootDirectory 'PortableGit\cmd\git.exe'
}
if (!$git) {
  throw "Couldn't find installed an git.exe"
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

function Get-CsprojPath {
    Join-Path $gitHubDirectory GitHub.VisualStudio.csproj
}

function Get-CsprojXml {
    $xmlLines = Get-Content (Get-CsprojPath)
    # If we don't explicitly join the lines with CRLF, comments in the XML will
    # end up with LF line-endings, which will make Git spew a warning when we
    # try to commit the version bump.
    $xmlText = $xmlLines -join [System.Environment]::NewLine

    [xml] $xmlText
}

function Read-CurrentVersion {
    $element = Get-ApplicationVersionElement (Get-CsprojXml)
    [System.Version] $element.InnerText
}