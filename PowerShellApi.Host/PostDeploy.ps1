$path = (Get-Item -Path ".\" -Verbose)
$manifest = [xml] (gc (Join-Path -Path $path -ChildPath ETW/PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.man))
$manifest.instrumentationManifest.instrumentation.events.provider
$manifestDll = Join-Path -Path $path -ChildPath "ETW\PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.dll"
$manifestDll
$manifest.instrumentationManifest.instrumentation.events.provider.SetAttribute("resourceFileName",$manifestDll)
$manifest.instrumentationManifest.instrumentation.events.provider.SetAttribute("messageFileName", $manifestDll)
$manifest.Save((Join-Path -Path $path -ChildPath ETW/PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.man))
& wevtutil.exe um ETW/PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.man
& wevtutil.exe im ETW/PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.man /rf:"ETW/PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.dll" /mf:"ETW/PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.dll" /pf:"ETW/PowerShellRestApi.DDCloud-PowerShellRestApi.etwManifest.dll"