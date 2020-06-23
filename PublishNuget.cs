using System;
using System.Diagnostics;
using System.IO;

public static void RunCmd(string command)
{
    Process cmd = new Process();
    cmd.StartInfo.FileName = "cmd.exe";
    cmd.StartInfo.Arguments = $"/C {command}";
    cmd.StartInfo.UseShellExecute = false;
    cmd.Start();
    cmd.WaitForExit();
}

Console.WriteLine("Publish script started.");
string version = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER");
Console.WriteLine($"Version is {version}");
string apiKey = Environment.GetEnvironmentVariable("NUGET_KEY");
if(string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("API key not found :(");
    return;
}
Directory.SetCurrentDirectory("./Emotion");

Console.WriteLine($"Packing...");
RunCmd($"dotnet pack --configuration Release -p:PackageVersion=1.0.{version}");

string[] packages = Directory.GetFiles("./bin/Release/", "*.nupkg");
if(packages.Length == 0)
{
    Console.WriteLine("No package generated.");
    return;
}
Console.WriteLine($"Publishing package [{packages[0]}]...");
RunCmd($"dotnet nuget push {packages[0]} -k {apiKey} -s https://api.nuget.org/v3/index.json");
Console.WriteLine("Script complete!");