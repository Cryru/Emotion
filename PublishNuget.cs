#!/usr/bin/env dotnet-script

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

public static void PatchVersionInfo(string version)
{
    string hash = Environment.GetEnvironmentVariable("GITHUB_SHA");

    const string metaFile = "./Common/MetaData.cs";
    string metaFileContent = File.ReadAllText(metaFile);
    metaFileContent = new Regex("Version = \\\"0\\.0\\.0\\\"").Replace(metaFileContent, $"Version = \"{version}\"");
    metaFileContent = new Regex("GitHash = \\\"None\\\"").Replace(metaFileContent, $"GitHash = \"{hash}\"");
    File.WriteAllText(metaFile, metaFileContent);

    const string nuSpec = "./Emotion.nuspec";
    string nuSpecContent = File.ReadAllText(nuSpec);
    nuSpecContent = new Regex("version>1\\.0\\.0</version>").Replace(nuSpecContent, $"version>{version}</version>");
    File.WriteAllText(nuSpec, nuSpecContent);
}

public static void PatchGitHubToken(string token)
{
    const string nugetConfigContent = "./_nuget.config";
    const string nugetConfig = "./nuget.config";
    string nugetConfigContent = File.ReadAllText(nugetConfigContent);
    nugetConfigContent = new Regex("GITHUB_TOKEN").Replace(nugetConfigContent, token);
    File.WriteAllText(nugetConfig, nugetConfigContent);
}

public static void RunCmd(string command)
{
    Process cmd = new Process();
    cmd.StartInfo.FileName = "cmd.exe";
    cmd.StartInfo.Arguments = $"/C {command}";
    cmd.StartInfo.UseShellExecute = false;
    cmd.Start();
    cmd.WaitForExit();
}

public static void Main()
{
    Console.WriteLine("Publish script started.");

    string version = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER");
    Console.WriteLine($"Version is {version}");
    Directory.SetCurrentDirectory("./Emotion");
    PatchVersionInfo($"1.0.{version}");

    Console.WriteLine($"Packing...");
    RunCmd($"dotnet pack --configuration Debug");

    string[] packages = Directory.GetFiles("./bin/Debug/", "*.nupkg");
    if (packages.Length == 0)
    {
        Console.WriteLine("No package generated.");
        return;
    }
    string apiKey = Environment.GetEnvironmentVariable("NUGET_KEY");
    string githubKey = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(githubKey))
    {
        Console.WriteLine("API key or GitHub key not found :(");
        return;
    }

    Console.WriteLine($"Publishing package [{packages[0]}]...");
    RunCmd($"dotnet nuget push {packages[0]} -k {apiKey} --source \"microsoft\"");
    PatchGitHubToken(githubKey);
    RunCmd($"dotnet nuget push {packages[0]} --source \"github\"");
    Console.WriteLine("Script complete!");
}

Main();