# Get files in the folder.
Set-Variable -Name files -Value (Get-ChildItem -Path ./bin/Debug-GLES -Recurse -File -Exclude "EmotionCore.dll")

[string[]] $result = @()

# Add header.
$result += "<?xml version=`"1.0`" encoding=`"utf-8`"?>"
$result += "<Project ToolsVersion=`"4.0`" xmlns=`"http://schemas.microsoft.com/developer/msbuild/2003`">"
$result += "	<ItemGroup>"

# Go through all files.
Foreach($file in $files)
{
 # Get the relative path of the file.
 $currentFilePath = (Resolve-Path -Path ($file).PSPath -Relative).Replace(".\", "")
 $targetsItem = "		<None Include=`"`$(MSBuildThisFileDirectory)" + $currentFilePath + "`">
			<Link>" + $currentFilePath + "</Link>
			<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
		</None>";
 $result += $targetsItem
}

$result += "	</ItemGroup>"
$result += "</Project>"

echo $result

$result | Out-File -FilePath "Emotion.targets"

nuget pack .\EmotionCore.nuspec -Version $($env:APPVEYOR_BUILD_VERSION)
