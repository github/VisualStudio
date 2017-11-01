Set-StrictMode -Version Latest

New-Module -ScriptBlock {

    function Get-SolutionInfoPath {
        Join-Path $rootDirectory src\common\SolutionInfo.cs
    }

    function Read-VersionSolutionInfo {
        $file = Get-SolutionInfoPath
        $currentVersion = Get-Content $file | %{
            $newString = $_
            $regex = "const string Version = `"(\d+\.\d+\.\d+\.\d+)`";"
            if ($_ -match $regex) {
                $version = $matches[1]
            }
            $version
        }

        if (!Validate-Version $currentVersion) {
            Die 1 "Invalid currentVersion $currentVersion"
        }

        [System.Version] $currentVersion
    }

    function Write-VersionSolutionInfo([System.Version]$version) {
        $file = Get-SolutionInfoPath
        $numberOfReplacements = 0
        $newContent = Get-Content $file | %{
            $newString = $_
            $regex = "(string Version = )`"\d+\.\d+\.\d+\.\d+`";"
            if ($_ -match $regex) {
                $numberOfReplacements++
                $newString = $_ -replace $regex, "`$1`"$version`";"
            }
            $newString
        }

        if ($numberOfReplacements -ne 1) {
            Die 1 "Expected to replace the version number in 1 place in SolutionInfo.cs (Version) but actually replaced it in $numberOfReplacements"
        }

        $newContent | Set-Content $file
    }

    Export-ModuleMember -Function Get-SolutionInfoPath,Read-VersionSolutionInfo,Write-VersionSolutionInfo
}