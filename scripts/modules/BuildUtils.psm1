Set-StrictMode -Version Latest

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