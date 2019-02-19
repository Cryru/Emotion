# Get files in the folder.
Set-Variable -Name files -Value (Get-ChildItem -Path ./bin/Debug-GLES -Recurse -File)

[string[]] $targetsPatch = @()
[string[]] $exceptions = @("System.Numerics.Vectors.dll", "System.Numerics.Vectors.xml", "OpenTK.xml", "EmotionCore.dll", "EmotionCore.xml", "EmotionCore.pdb")
[string[]] $nuSpecPatch = @()

# Add header.
$targetsPatch += "<?xml version=`"1.0`" encoding=`"utf-8`"?>"
$targetsPatch += "<Project ToolsVersion=`"4.0`" xmlns=`"http://schemas.microsoft.com/developer/msbuild/2003`">"
$targetsPatch += "	<ItemGroup>"

# Go through all files.
Foreach ($file in $files) {
    # Get the relative path of the file.
    $currentFilePath = (Resolve-Path -Path ($file).PSPath -Relative).Replace(".\", "").Replace("bin\Debug-GLES\", "")

    # Check if exception.
    if ($exceptions.Contains($currentFilePath)) {continue}

    $targetsItem = "		<None Include=`"`$(MSBuildThisFileDirectory)" + $currentFilePath + "`">
			<Link>" + $currentFilePath + "</Link>
			<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
		</None>";
    $targetsPatch += $targetsItem

    $index = $currentFilePath.LastIndexOf("\");
    $rootFolder = "";
    if ($index -ne -1) {
        $rootFolder = "\" + $currentFilePath.Substring(0, $currentFilePath.LastIndexOf("\"));
    }

    $nuSpecPatch += "<file src=`"bin\Debug-GLES\" + $currentFilePath + "`" target=`"Build" + $rootFolder + "`" />";
}

$targetsPatch += "	</ItemGroup>"
$targetsPatch += "</Project>"

echo $targetsPatch

$targetsPatch | Out-File -FilePath "Emotion.targets"
(Get-Content "EmotionCore.nuspec") -replace '<NugetPack.ps1 />', $nuSpecPatch | Set-Content "EmotionCore.nuspec"

nuget pack .\EmotionCore.nuspec -Version $($env:APPVEYOR_BUILD_VERSION) -Symbols -SymbolPackageFormat snupkg