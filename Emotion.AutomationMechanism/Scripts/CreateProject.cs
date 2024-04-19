using System;
using System.Text;
using System.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.AutomationMechanism;

string scriptAssets = Path.Join(".", "Scripts", "CreateProjectAssets");
scriptAssets = Path.GetFullPath(scriptAssets);
if (!Directory.Exists(scriptAssets))
{
    Engine.Log.Error($"Script assets not found - {scriptAssets}", "EMA");
    return;
}

string[] inputArgs = EmaSystem.Args;
Emotion.Utility.CommandLineParser.FindArgument(inputArgs, "name=", out string? projectName);
projectName ??= inputArgs.Length > 1 ? inputArgs[1] : null;
if (projectName == null)
{
    Engine.Log.Error("No project name specified", "EMA");
    return;
}

Emotion.Utility.CommandLineParser.FindArgument(inputArgs, "path=", out string? rootFolder);
rootFolder ??= inputArgs.Length > 2 ? inputArgs[2] : null;
rootFolder ??= "..";

Engine.Log.Info($"Creating project {projectName} at {Path.GetFullPath(rootFolder)}", "EMA");

DirectoryInfo directory = Directory.CreateDirectory($"{rootFolder}/{projectName}");
if (!directory.Exists)
{
    Engine.Log.Error("Couldn't create folder for project :(", "EMA");
    return;
}

static void CopyFileIfNotExist(string from, string to)
{
    if (!File.Exists(to))
        File.Copy(from, to);
}

// Main folder
{
    string readMeFilePath = Path.Join(directory.FullName, "ReadMe.md");
    if (!File.Exists(readMeFilePath))
    {
        StringBuilder readMeFile = new StringBuilder();
        readMeFile.AppendLine($"# {projectName}");
        File.WriteAllText(readMeFilePath, readMeFile.ToString());
    }

    CopyFileIfNotExist(Path.Join(scriptAssets, ".gitattributes"), Path.Join(directory.FullName, ".gitattributes"));
    CopyFileIfNotExist(Path.Join(scriptAssets, ".gitignore"), Path.Join(directory.FullName, ".gitignore"));
}

// Code folder
{
    DirectoryInfo codeDirectory = Directory.CreateDirectory(Path.Join(directory.FullName, "code"));
    if (!codeDirectory.Exists)
    {
        Engine.Log.Error("Couldn't create folder for code", "CreateProject");
    }
    else
    {
        DirectoryInfo projectDirectory = Directory.CreateDirectory(Path.Join(codeDirectory.FullName, projectName));
        CopyFileIfNotExist(Path.Join(scriptAssets, "GlobalImports.cs"), Path.Join(projectDirectory.FullName, "GlobalImports.cs"));
        CopyFileIfNotExist(Path.Join(scriptAssets, ".editorconfig"), Path.Join(projectDirectory.FullName, ".editorconfig"));

        string slnFilePath = Path.Join(directory.FullName, $"{projectName}.sln");

        if (!File.Exists(slnFilePath))
        {
            StringBuilder solutionFile = new StringBuilder();
            solutionFile.AppendLine($"Microsoft Visual Studio Solution File, Format Version 12.00");
            solutionFile.AppendLine($"VisualStudioVersion = 17.8.34525.116");
            solutionFile.AppendLine($"MinimumVisualStudioVersion = 10.0.40219.1");

            var projectGuid = Guid.NewGuid();
            var projectGuidForBuildConfig = Guid.NewGuid();

            solutionFile.AppendLine($"Project(\"{{{projectGuid}}}\") = \"{projectName}\", \"{projectName}\\{projectName}.csproj\", \"{{{projectGuidForBuildConfig}}}\"");
            solutionFile.AppendLine($"EndProject");

            var emotionGuid = Guid.NewGuid();
            var emotionGuidForBuildConfig = Guid.NewGuid();

            solutionFile.AppendLine($"Project(\"{{{emotionGuid}}}\") = \"Emotion\", \"..\\..\\Emotion\\Emotion\\Emotion.csproj\", \"{{{emotionGuidForBuildConfig}}}\"");
            solutionFile.AppendLine($"EndProject");

            File.WriteAllText(slnFilePath, solutionFile.ToString());
        }
    }
}

// Assets folder
{
    DirectoryInfo assetsDirectory = Directory.CreateDirectory(Path.Join(directory.FullName, "assets"));
    if (!assetsDirectory.Exists)
    {
        Engine.Log.Error("Couldn't create folder for assets", "CreateProject");
    }
    else
    {
        File.Create(Path.Join(assetsDirectory.FullName, "assets go here.txt"));
    }
}

// Create assets and code folder
// Create csproj and set it up
//  Link with Emotion
//  Setup assets folder to auto-copy
//  Disable creation of localization folders
//  maybe setup publishing settings?
// Setup Program.cs with basic code and basic scene (arg 2d or 3d?)
//  Maybe setup a test map to be loaded/written to on startup?

// Open the folder.
var platform = Engine.Host as Win32Platform;
platform?.OpenFolderAndSelectFile(directory.FullName + Path.DirectorySeparatorChar);

return "Project Created!";