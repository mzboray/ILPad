properties {
    $binDir = "$PSScriptRoot\bin"
}

Framework "4.5.1"

task default -depends Compile

task Clean {
    if (Test-Path $binDir) {
        rm $binDir -Recurse -Force
	}
}

task Init -depends Clean {
    mkdir $binDir
}

task Compile -depends Init {
	msbuild "ILPad.sln" /t:Rebuild /p:OutDir="$binDir"
}

task Run -depends Compile {
	Start-Process "$binDir\ILPad.exe"
}