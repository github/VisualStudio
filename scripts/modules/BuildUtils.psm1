Set-StrictMode -Version Latest

function Die-WithOutput($exitCode, $output) {
    Write-Output $output
    Write-Output ""
    exit $exitCode
}

function Run-Command([scriptblock]$Command, [switch]$Fatal, [switch]$Quiet) {
    $output = ""
    if ($Quiet) {
        $output = & $Command 2>&1
    } else {
        & $Command
    }

    if (!$Fatal) {
        return
    }

    $exitCode = 0
    if ($LastExitCode -ne 0) {
        $exitCode = $LastExitCode
    } elseif (!$?) {
        $exitCode = 1
    } else {
        return
    }

    $error = "Error executing command ``$Command``."
    if ($output) {
        $error = "$error Output:", $output
    }
    Die-WithOutput $exitCode $error
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