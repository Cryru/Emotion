using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Utility;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using Emotion.Standard.XML;
using System.Collections;
using Emotion.Game.Time.Routines;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Standard.Image.PNG;

namespace Emotion.AutomationMechanism;

public static class EmaSystem
{
    public static string[] Args;
    public static bool ConsoleMode = false;

    private static Queue<string[]> _commandBuffer = new();

    public static void Main(string[]? args)
    {
        if (args.Length == 0)
            ConsoleMode = true;
        else
            _commandBuffer.Enqueue(args ?? Array.Empty<string>());

        Configurator config = new Configurator();
        config.HiddenWindow = true;
        config.DebugMode = true;
        config.Logger = new NetIOAsyncLogger(true, "Logs");
        config.Logger.FilterAddSourceToShow("EMA");
        Engine.Setup(config);
        Engine.CoroutineManager.StartCoroutine(RunCommandsRoutine());
        Engine.Run();
    }

    private static IEnumerator RunCommandsRoutine()
    {
        while (true)
        {
            if (_commandBuffer.TryDequeue(out string[]? commands))
            {
                Args = commands;

                // Check if first argument is a script name.
                string firstArg = Args[0];
                string implicitScriptPath = Path.Join("Scripts", $"{firstArg}.cs");
                string? filePath = null;
                if (File.Exists(implicitScriptPath))
                {
                    filePath = implicitScriptPath;
                }
                else if (CommandLineParser.FindArgument(Args, "exec=", out string execScriptPath))
                {
                    if (File.Exists(execScriptPath))
                        filePath = execScriptPath;
                }

                if (filePath == null)
                    Engine.Log.Info($"Unknown command, or script not found - {string.Join(' ', Args)}", "EMA");
                else
                    yield return new TaskRoutineWaiter(ExecuteScriptFromPath(filePath));
            }
            else if (ConsoleMode)
            {
                Engine.Log.Info("Input Command:", "EMA");
                string? input = Console.ReadLine();

                string[] args = Array.Empty<string>();
                if (input != null) args = input.Split(" ");
                if (args.Length == 0)
                    Engine.Log.Info("Empty Command", "EMA");
                else
                    _commandBuffer.Enqueue(args);
            }
            else
            {
                Engine.Quit();
            }

            yield return null;
        }
    }

    public static async Task ExecuteScriptFromPath(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        Engine.Log.Info($"Executing script: {fileName}", "EMA");

        string fileContent = File.ReadAllText(filePath);
        await ExecuteScript(fileContent);
    }

    public static async Task ExecuteScript(string text)
    {
        try
        {
            var options = ScriptOptions.Default;
            options = options.AddReferences(Helpers.AssociatedAssemblies);
            for (var i = 0; i < Helpers.AssociatedAssemblies.Length; i++)
            {
                Assembly? assembly = Helpers.AssociatedAssemblies[i];
                Type[] allTypes = assembly.GetTypes();
                for (var j = 0; j < allTypes.Length; j++)
                {
                    var type = allTypes[j];
                    if (type.Namespace == null) continue;
                    options = options.AddImports(type.Namespace);
                }
            }

            object result = await CSharpScript.EvaluateAsync(text, options);
            Engine.Log.Info("Output:", "EMA");
            Engine.Log.Info(XMLFormat.To(result), "EMA");
        }
        catch (Exception e)
        {
            Engine.Log.Error(e.Message, "EMA");
        }
    }

    public static bool GetGameProjectFolder(out string? projectFolder)
    {
        string[] inputArgs = Args;
        Emotion.Utility.CommandLineParser.FindArgument(inputArgs, "project=", out projectFolder);
        projectFolder ??= inputArgs.Length > 1 ? inputArgs[1] : null;
        if (projectFolder == null)
        {
            Engine.Log.Error("No project name specified", "EMA");
            return false;
        }
        return true;
    }
}