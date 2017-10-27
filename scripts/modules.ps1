Add-Type -AssemblyName "System.Core"
Add-Type -TypeDefinition @"
public class ScriptException : System.Exception
{
    public ScriptException(string message) : base(message)
    {
    }
}
"@

New-Module -ScriptBlock {
    $scriptsDirectory = $PSScriptRoot
    $rootDirectory = Split-Path ($scriptsDirectory)
    $nuget = Join-Path $rootDirectory "tools\nuget\nuget.exe"
    Export-ModuleMember -Variable scriptsDirectory,rootDirectory,nuget
}

New-Module -ScriptBlock {
    function Die([int]$exitCode, [string]$message, [object[]]$output) {
        $host.SetShouldExit($exitCode)
        if ($output) {
            Write-Output $output
            $message += ". See output above."
        }
        Throw (New-Object -TypeName ScriptException -ArgumentList $message)
    }


    function Run-Command([scriptblock]$Command, [switch]$Fatal, [switch]$Quiet, [int]$Timeout = 0) {
        $script = $Command

        $output = ""

        $exitCode = 0
        $errorStr = "failed"

        if ($Timeout -gt 0) {
            $script = [ScriptBlock]::Create({Set-Location $using:PWD;}.ToString() + $Command.ToString() + ' 2>&1 | %{ "$_" }' + {; if ($LastExitCode -ne 0) { throw "Failed" }}.ToString())
            & $scriptsDirectory\clearerror.cmd
            $job = Start-Job -InitializationScript { Set-Location $PSScriptRoot } -ScriptBlock $script
            $state = Wait-Job -Timeout $Timeout $job
            if (!$state) {
                $exitCode = 2
                $errorStr = "timed out"
            } else {
                $output = Receive-Job $job
                if ($job.State -eq 'Failed') {
                    $exitCode = 1
                }
            }
        } else {

            if ($Quiet) {
                $output = & $command 2>&1 | %{ "$_" }
            } else {
                & $command
            }
        }

        if (!$? -and $LastExitCode -ne 0) {
            $exitCode = $LastExitCode
        } elseif ($? -and $LastExitCode -ne 0) {
            $exitCode = $LastExitCode
        }

        if ($exitCode -ne 0) {
            if (!$Fatal) {
                Write-Output "``$Command`` $errorStr" $output
            } else {
                Die $exitCode "``$Command`` $errorStr" $output
            }
        }
        $output
    }

    function Create-TempDirectory {
        $path = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
        New-Item -Type Directory $path
    }

    Export-ModuleMember -Function Die,Run-Command,Create-TempDirectory
}

New-Module -ScriptBlock {
    function Find-MSBuild() {
        if (Test-Path "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe") {
            $msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
        }
        elseif (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe") {
            $msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
        }
        else {
            Die("No suitable msbuild.exe found.")
        }
        $msbuild
    }

    function Build-Solution([string]$solution,[string]$target,[string]$configuration, [bool]$ForVSInstaller) {
        Run-Command -Fatal { & $nuget restore $solution -NonInteractive -Verbosity detailed }
        $flag1 = ""
        $flag2 = ""
        if ($ForVSInstaller) {
            $flag1 = "/p:IsProductComponent=true"
            $flag2 = "/p:TargetVsixContainer=$rootDirectory\build\vsinstaller\GitHub.VisualStudio.vsix"
            new-item -Path $rootDirectory\build\vsinstaller -ItemType Directory -Force | Out-Null
        }

        $msbuild = Find-MSBuild

        Write-Output "$msbuild $solution /target:$target /property:Configuration=$configuration /p:DeployExtension=false /verbosity:minimal /p:VisualStudioVersion=14.0 $flag1 $flag2"
        Run-Command -Fatal { & $msbuild $solution /target:$target /property:Configuration=$configuration /p:DeployExtension=false /verbosity:minimal /p:VisualStudioVersion=14.0 $flag1 $flag2 }
    }

    Export-ModuleMember -Function Find-MSBuild,Build-Solution
}

New-Module -ScriptBlock {
    function Find-Git() {
        $git = (Get-Command 'git.exe').Path
        if (!$git) {
          $git = Join-Path $rootDirectory 'PortableGit\cmd\git.exe'
        }
        if (!$git) {
          Die("Couldn't find installed an git.exe")
        }
        $git
    }

    function Push-Changes([string]$branch) {
        Push-Location $rootDirectory

        Write-Output "Pushing $Branch to GitHub..."

        Run-Command -Fatal { & $git push origin $branch }

        Pop-Location
    }

    function Update-Submodules {
        Write-Output "Updating submodules..."
        Write-Output ""

        Run-Command -Fatal { git submodule init }
        Run-Command -Fatal { git submodule sync }
        Run-Command -Fatal { git submodule update --recursive --force }
    }

    function Clean-WorkingTree {
        Write-Output "Cleaning work tree..."
        Write-Output ""

        Run-Command -Fatal { git clean -xdf }
        Run-Command -Fatal { git submodule foreach git clean -xdf }
    }

    $git = Find-Git
    Export-ModuleMember -Function Find-Git,Push-Changes,Update-Submodules,Clean-WorkingTree
}