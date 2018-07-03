$scriptsDirectory = Split-Path $MyInvocation.MyCommand.Path
$rootDirectory = Split-Path ($scriptsDirectory)

function Die([string]$message, [object[]]$output) {
    if ($output) {
        Write-Output $output
        $message += ". See output above."
    }
    Throw (New-Object -TypeName ScriptException -ArgumentList $message)
}

if (Test-Path "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe") {
    $msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
}
elseif (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe") {
    $msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
}
else {
    Die("No suitable msbuild.exe found.")
}

$git = (Get-Command 'git.exe').Path
if (!$git) {
  $git = Join-Path $rootDirectory 'PortableGit\cmd\git.exe'
}
if (!$git) {
  throw "Couldn't find installed an git.exe"
}

$nuget = Join-Path $rootDirectory "tools\nuget\nuget.exe"

function Create-TempDirectory {
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
    New-Item -Type Directory $path
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

    Write-Output "$msbuild $solution /target:$target /property:Configuration=$configuration /p:DeployExtension=false /verbosity:minimal /p:VisualStudioVersion=14.0 $flag1 $flag2"
    Run-Command -Fatal { & $msbuild $solution /target:$target /property:Configuration=$configuration /p:DeployExtension=false /verbosity:minimal /p:VisualStudioVersion=14.0 $flag1 $flag2 }
}

function Push-Changes([string]$branch) {
    Push-Location $rootDirectory

    Write-Output "Pushing $Branch to GitHub..."

    Run-Command -Fatal { & $git push origin $branch }

    Pop-Location
}

Add-Type -AssemblyName "System.Core"
Add-Type -TypeDefinition @"
public class ScriptException : System.Exception
{
    public ScriptException(string message) : base(message)
    {
    }
}
"@
