Set-StrictMode -Version Latest

New-Module -ScriptBlock {
    function Get-DirectoryBuildPropsPath {
        Join-Path $rootDirectory Directory.Build.Props
    }

    function Get-DirectoryBuildProps {
        $xmlLines = Get-Content (Get-DirectoryBuildPropsPath) -encoding UTF8
        [xml] $xmlLines
    }

    function Write-DirectoryBuildProps([System.Version]$version) {

        $document = Get-DirectoryBuildProps

        $numberOfReplacements = 0
        $document.Project.PropertyGroup.Version = $version.ToString()

        $document.Save((Get-DirectoryBuildPropsPath))
    }

    Export-ModuleMember -Function Write-DirectoryBuildProps
}