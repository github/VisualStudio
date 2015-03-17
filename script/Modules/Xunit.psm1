Set-StrictMode -Version Latest

function Start-XUnit(
	[string]$project,
	[string]$rootDirectory,
	[ValidateSet('Debug', 'Release')]
	[string]$configuration = "Release") {
	Write-Verbose "Running $project..."
	Start-Job -ScriptBlock {
		$project = $using:project
		$rootDirectory = $using:rootDirectory
		$configuration = $using:configuration

		Set-Location -Path $rootDirectory
		$consoleRunner = Join-Path (Join-Path $rootDirectory lib\xunit) xunit.console.x86.exe
		$outputPath = [System.IO.Path]::GetTempFileName() + $project + ".output"
		$errorPath = [System.IO.Path]::GetTempFileName() + $project + ".errors"
        $dll = (Join-Path $rootDirectory "$project\bin\$configuration\$project.dll")
        $xml = Join-Path $rootDirectory "nunit-$project.xml"
        $args = $dll, "-noshadow", "-xml", $xml
        [object[]] $output = "$consoleRunner " + ($args -join " ")
		$process = Start-Process -PassThru -NoNewWindow -RedirectStandardError $errorPath -RedirectStandardOutput $outputPath $consoleRunner ($args | %{ "`"$_`"" })
		Wait-Process -InputObject $process -Timeout 360 -ErrorAction SilentlyContinue
		
		if ($process.HasExited) {
			if ($process.ExitCode -ne 0) {
				$output += Get-Content $errorPath
				$output += ""
			}
			$output += (Get-Content $outputPath)[-1]
			$exitCode = $process.ExitCode
		}
		else {
			$output += "Tests timed out. Backtrace:"

			if (!$using:PSScriptRoot) {
			    Write-Host "Shit gone cray in Xunit.psm1. using:PSScriptRoot is null."
				exit 99
			}

			$scriptsDirectory = Split-Path $using:PSScriptRoot
			$cdb = Join-Path $scriptsDirectory "Debugging Tools for Windows\cdb.exe"

			$ProcessId = $process.Id
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

			$output += $output[$start..$end]
			$exitCode = 9999
		}

		try {
			Stop-Process -InputObject $process
		}
		catch [Exception] {
		    Write-Host $_

			try {
				Start-Sleep -s 10
				Stop-Process -InputObject $process
			}
			catch [Exception] {
				Write-Host "Successfully stopped process after waiting 10 seconds"
			}
		}

		try {
			Remove-Item $outputPath
			Remove-Item $errorPath
		}
		catch [Exception] {
		    Write-Host $_

			Start-Sleep -s 10
			try {
				Remove-Item $outputPath
				Remove-Item $errorPath
			}
			catch [Exception] {
				Write-Host "Successfully cleaned up after waiting 10 seconds"			
			}
		}
		$result = New-Object System.Object
		$result | Add-Member -Type NoteProperty -Name Output -Value $output
		$result | Add-Member -Type NoteProperty -Name ExitCode -Value $exitCode
		$result  
	}
}
