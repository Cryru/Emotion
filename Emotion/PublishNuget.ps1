$fileName = dotnet pack --configuration Release |  Select-String nupkg
$fileName -Replace "^[^']'", "" -Replace "'[^']$", ""
dotnet nuget push $fileName --source "github"