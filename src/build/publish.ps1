param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\IPioneerReceiverControl.Rx\bin\Release\netstandard2.0\IPioneerReceiverControl.Rx.dll')).Version.ToString(3)
	}
else {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\IPioneerReceiverControl.Rx\bin\Release\netstandard2.0\IPioneerReceiverControl.Rx.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

c:\tools\nuget\Nuget.exe push ".\NuGet\PioneerReceiverControl.Rx.$version.symbols.nupkg" -Source https://www.nuget.org