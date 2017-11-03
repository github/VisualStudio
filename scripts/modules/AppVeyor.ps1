Set-StrictMode -Version Latest

New-Module -ScriptBlock {

    function Get-AppVeyorPath {
        Join-Path $rootDirectory appveyor.yml
    }

    function Read-VersionAppVeyor {
        $file = Get-AppVeyorPath
        $currentVersion = Get-Content $file | %{
            $regex = "`^version: '(\d+\.\d+\.\d+)\.`{build`}'`$"
            if ($_ -match $regex) {
                $matches[1]
            }
        }
        [System.Version] $currentVersion
    }

    function Write-VersionAppVeyor([System.Version]$version) {
        $file = Get-AppVeyorPath
        $numberOfReplacements = 0
        $newContent = Get-Content $file | %{
            $newString = $_
            $regex = "version: '(\d+\.\d+\.\d+)"
            if ($newString -match $regex) {
                $numberOfReplacements++
                $newString = $newString -replace $regex, "version: '$($version.Major).$($version.Minor).$($version.Build)"
            }
            $newString
        }

        if ($numberOfReplacements -ne 1) {
            Die 1 "Expected to replace the version number in 1 place in appveyor.yml (version) but actually replaced it in $numberOfReplacements"
        }

        $newContent | Set-Content $file
    }

    Export-ModuleMember -Function Get-AppVeyorPath,Read-VersionAppVeyor,Write-VersionAppVeyor
}