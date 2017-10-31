Add-Type -AssemblyName "System.Core"
Add-Type -TypeDefinition @"
public class ScriptException : System.Exception
{
    public int ExitCode { get; private set; }
    public ScriptException(string message, int exitCode) : base(message)
    {
        this.ExitCode = exitCode;
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
        #$host.SetShouldExit($exitCode)
        if ($output) {
            Write-Host $output
            $message += ". See output above."
        }
        $hash = @{
            Message = $message
            ExitCode = $exitCode
            Output = $output
        }
        Throw (New-Object -TypeName ScriptException -ArgumentList $message,$exitCode)
        #throw $message
    }


    function Run-Command([scriptblock]$Command, [switch]$Fatal, [switch]$Quiet) {
        $output = ""

        $exitCode = 0

        if ($Quiet) {
            $output = & $command 2>&1 | %{ "$_" }
        } else {
            & $command
        }

        if (!$? -and $LastExitCode -ne 0) {
            $exitCode = $LastExitCode
        } elseif ($? -and $LastExitCode -ne 0) {
            $exitCode = $LastExitCode
        }

        if ($exitCode -ne 0) {
            if (!$Fatal) {
                Write-Host "``$Command`` failed" $output
            } else {
                Die $exitCode "``$Command`` failed" $output
            }
        }
        $output
    }

    function Run-Process([int]$Timeout, [string]$Command, [string[]]$Arguments, [switch]$Fatal = $false)
    {
        $args = ($Arguments | %{ "`"$_`"" })
        [object[]] $output = "$Command " + $args
        $exitCode = 0
        $outputPath = [System.IO.Path]::GetTempFileName()
        $process = Start-Process -PassThru -NoNewWindow -RedirectStandardOutput $outputPath $Command ($args | %{ "`"$_`"" })
        Wait-Process -InputObject $process -Timeout $Timeout -ErrorAction SilentlyContinue
        if ($process.HasExited) {
            $output += Get-Content $outputPath
            $exitCode = $process.ExitCode
        } else {
            $output += "Tests timed out. Backtrace:"
            $output += Get-DotNetStack $process.Id
            $exitCode = 9999
        }
        Stop-Process -InputObject $process
        Remove-Item $outputPath
        if ($exitCode -ne 0) {
            if (!$Fatal) {
                Write-Host "``$Command`` failed" $output
            } else {
                Die $exitCode "``$Command`` failed" $output
            }
        }
        $output
    }

    <#
    function Run-Job([string[]]$Command, [switch]$Fatal, [switch]$Quiet, [int]$Timeout = 0) {
        #Write-Host "ARGS $Command"
        $cmd = $Command

        $output = ""

        $exitCode = 0
        $errorStr = "failed"
        set-psdebug -trace 2
        #$script = [ScriptBlock]::Create($Command.ToString() + ' 2>&1 | %{ "$_" }' + {; if ($LastExitCode -ne 0) throw "Failed" }}.ToString())
        #$script = [ScriptBlock]::Create("& `$args[0] " + ' 2>&1 | %{ "$_" }' + {; if ($LastExitCode -ne 0) { throw "Failed" }}.ToString())
        #$script = { & $args[0] $args[1] 2>&1 | %{ "$_" } ; if ($LastExitCode -ne 0) { throw "Failed" }}
        & $scriptsDirectory\clearerror.cmd
        $script = { & (@$args[0]) }
        $job = Start-Job -ScriptBlock $script -ArgumentList (,$cmd)
        $state = Wait-Job -Timeout $Timeout $job
        if (!$state) {
            $exitCode = 2
            $errorStr = "timed out"
        } else {
            $output = Receive-Job $job
            Write-Host $output
            if ($job.State -eq 'Failed') {
                $exitCode = 1
            }
        }

        if (!$? -and $LastExitCode -ne 0) {
            $exitCode = $LastExitCode
        } elseif ($? -and $LastExitCode -ne 0) {
            $exitCode = $LastExitCode
        }

        if ($exitCode -ne 0) {
            if (!$Fatal) {
                Write-Host "``$Command`` $errorStr" $output
            } else {
                Die $exitCode "``$Command`` $errorStr" $output
            }
        }
        $output
    }
    #>

    function Create-TempDirectory {
        $path = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
        New-Item -Type Directory $path
    }

    Export-ModuleMember -Function Die,Run-Command,Run-Process,Create-TempDirectory
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

    function Build-Solution([string]$solution, [string]$target, [string]$configuration, [switch]$ForVSInstaller, [bool]$Deploy = $false) {
        Run-Command -Fatal { & $nuget restore $solution -NonInteractive -Verbosity detailed }
        $flag1 = ""
        $flag2 = ""
        if ($ForVSInstaller) {
            $flag1 = "/p:IsProductComponent=true"
            $flag2 = "/p:TargetVsixContainer=$rootDirectory\build\vsinstaller\GitHub.VisualStudio.vsix"
            new-item -Path $rootDirectory\build\vsinstaller -ItemType Directory -Force | Out-Null
        } elseif (!$Deploy) {
            $configuration += "WithoutVsix"
            $flag1 = "/p:Package=Skip"
        }

        $msbuild = Find-MSBuild

        Write-Host "$msbuild $solution /target:$target /property:Configuration=$configuration /p:DeployExtension=false /verbosity:minimal /p:VisualStudioVersion=14.0 $flag1 $flag2"
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

        Write-Host "Pushing $Branch to GitHub..."

        Run-Command -Fatal { & $git push origin $branch }

        Pop-Location
    }

    function Update-Submodules {
        Write-Host "Updating submodules..."
        Write-Host ""

        Run-Command -Fatal { git submodule init }
        Run-Command -Fatal { git submodule sync }
        Run-Command -Fatal { git submodule update --recursive --force }
    }

    function Clean-WorkingTree {
        Write-Host "Cleaning work tree..."
        Write-Host ""

        Run-Command -Fatal { git clean -xdf }
        Run-Command -Fatal { git submodule foreach git clean -xdf }
    }

    function Get-HeadSha {
        Run-Command -Quiet { & $git rev-parse HEAD }
    }

    $git = Find-Git
    Export-ModuleMember -Function Find-Git,Push-Changes,Update-Submodules,Clean-WorkingTree,Get-HeadSha
}

New-Module -ScriptBlock {
    function Write-Manifest([string]$directory) {
        Add-Type -Path (Join-Path $rootDirectory packages\Newtonsoft.Json.6.0.8\lib\net35\Newtonsoft.Json.dll)

        $manifest = @{
            NewestExtension = @{
                Version = [string](Read-CurrentVersionVsix)
                Commit = [string](Get-HeadSha)
            }
        }

        $manifestPath = Join-Path $directory manifest
        [Newtonsoft.Json.JsonConvert]::SerializeObject($manifest) | Out-File $manifestPath -Encoding UTF8
    }

    Export-ModuleMember -Function Write-Manifest
}