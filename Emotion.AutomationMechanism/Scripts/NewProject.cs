using System;
using System.Text;
using System.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.AutomationMechanism;
using System.Collections;
using Emotion.Game.Time.Routines;
using Emotion.Utility;
using WinApi.User32;

public static class TestScript
{
    static IEnumerator Routine()
    {
        string[] inputArgs = EmaSystem.Args;

        Engine.Log.Info("~-~ Create New Project ~-~", "EMA");

        bool anyInputMade = false;

        CommandLineParser.FindArgument(inputArgs, "name=", out string? projectName);
        projectName ??= inputArgs.Length > 1 ? inputArgs[1] : null;
        if (projectName == null)
        {
            anyInputMade = true;
            Engine.Log.Info("Enter project name: ", "EMA");
            string? input = null;
            while (input == null)
            {
                input = Console.ReadLine();
            }
            projectName = input;
        }

        CommandLineParser.FindArgument(inputArgs, "path=", out string? rootFolder);
        rootFolder ??= inputArgs.Length > 2 ? inputArgs[2] : null;
        if (rootFolder == null)
        {
            anyInputMade = true;
            Engine.Log.Info($"Current path is: {Path.GetFullPath(".")}", "EMA");
            Engine.Log.Info("Enter project path: ", "EMA");
            string? input = null;
            while (input == null)
            {
                input = Console.ReadLine();
            }
            rootFolder = input;
        }

        if (anyInputMade)
        {
            Engine.Log.Info($"Is this correct? (y/n)", "EMA");
            Engine.Log.Info($"[Project Name]: {projectName}", "EMA");
            Engine.Log.Info($"[Project Path]: {Path.GetFullPath(rootFolder)}", "EMA");
            string? input = null;
            while (input == null)
            {
                input = Console.ReadLine();
                if (input.ToLowerInvariant() == "y")
                {
                    break;
                }
                else if (input.ToLowerInvariant() == "n")
                {
                    // Reset
                    yield return Routine();
                    yield break;
                }
                else
                {
                    input = null;
                }
            }
            Engine.Log.Info($"Creating project...", "EMA");
        }
        else
        {
            Engine.Log.Info($"[Project Name]: {projectName}", "EMA");
            Engine.Log.Info($"[Project Path]: {Path.GetFullPath(rootFolder)}", "EMA");
            Engine.Log.Info($"Creating project...", "EMA");
        }

        DirectoryInfo directory = Directory.CreateDirectory($"{rootFolder}/{projectName}");
        if (!directory.Exists)
        {
            Engine.Log.Error("Couldn't create folder for project :(", "EMA");
            yield break;
        }

        Engine.Log.Info($"Copying new project template...", "EMA");

        string scriptAssets = Path.Join(".", "Scripts", "CreateProjectAssets");
        scriptAssets = Path.GetFullPath(scriptAssets);

        var dics = Directory.GetDirectories(scriptAssets, "*");
        var files = Directory.GetFiles(scriptAssets, "*", SearchOption.AllDirectories);
        

        yield return null;

        yield return null;
    }

    public static Coroutine Pepegich()
    {
        return Engine.CoroutineManager.StartCoroutine(Routine());
    }
}