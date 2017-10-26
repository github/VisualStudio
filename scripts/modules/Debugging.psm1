Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$rootDirectory = Split-Path (Split-Path (Split-Path $MyInvocation.MyCommand.Path))
$cdb = Join-Path $rootDirectory "tools\Debugging Tools for Windows\cdb.exe"

function Get-DotNetStack([int]$ProcessId) {
    $commands = @(
        ".cordll -ve -u -l",
        ".loadby sos clr",
        "!eestack -ee",
        ".detach",
        "q"
    )

    $Env:_NT_SYMBOL_PATH = "cache*${Env:PROGRAMDATA}\dbg\sym;SRV*http://msdl.microsoft.com/download/symbols;srv*http://windows-symbols.githubapp.com/symbols"
    $output = & $cdb -lines -p $ProcessId -c ($commands -join "; ")
    if ($LastExitCode -ne 0) {
        $output
        throw "Error running cdb"
    }

    $start = ($output | Select-String -List -Pattern "^Thread   0").LineNumber - 1
    $end = ($output | Select-String -List -Pattern "^Detached").LineNumber - 2
    $output[$start..$end]
}
