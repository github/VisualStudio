Set-StrictMode -Version Latest

New-Module -ScriptBlock {

    function Validate-Version([System.Version]$version) {
        ($version.Major -ge 0) -and ($version.Minor -ge 0) -and ($version.Build -ge 0)
    }

    function Generate-Version([System.Version]$currentVersion,
        [bool]$BumpMajor, [bool] $BumpMinor,
        [bool]$BumpPatch, [bool] $BumpBuild) {

        if (!(Validate-Version $currentVersion)) {
            Die 1 "Invalid current version $currentVersion"
        }

        if ($BumpMajor) {
            New-Object -TypeName System.Version -ArgumentList ($currentVersion.Major + 1), $currentVersion.Minor, $currentVersion.Build, 0
        } elseif ($BumpMinor) {
            New-Object -TypeName System.Version -ArgumentList $currentVersion.Major, ($currentVersion.Minor + 1), $currentVersion.Build, 0
        } elseif ($BumpPatch) {
            New-Object -TypeName System.Version -ArgumentList $currentVersion.Major, $currentVersion.Minor, ($currentVersion.Build + 1), 0
        } elseif ($BumpBuild) {
            $timestamp = [System.DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
            [System.Version] "$($currentVersion.Major).$($currentVersion.Minor).$($currentVersion.Build).$timestamp"
        }
        else {
            $currentVersion
        }
    }

    function Write-Version([System.Version]$version) {
        Write-VersionVsixManifest $version
        Write-VersionInstaller $version
        Write-VersionSolutionInfo $version
        Set-Content $rootDirectory\build\version $version
    }

    function Commit-Version([System.Version]$version) {

        Write-Host "Committing version bump..."

        Push-Location $rootDirectory

        Run-Command -Fatal { & $git commit --message "Bump version to $version" -- }

        $output = Start-Process $git "commit --all --message ""Bump version to $version""" -wait -NoNewWindow -ErrorAction Continue -PassThru
        if ($output.ExitCode -ne 0) {
            Die 1 "Error committing version bump"
        }

        Pop-Location
    }

    Export-ModuleMember -Function Validate-Version,Write-Version,Commit-Version,Generate-Version
}
