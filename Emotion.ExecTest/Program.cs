#region Using

using System;
using System.Collections;
using System.Runtime.InteropServices;
using Emotion.Audio;
using Emotion.Common;
using Emotion.ExecTest.ExamplesOne;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Audio.WAV;
using Emotion.Standard.Reflector;
using Emotion.Testing;
using Emotion.Utility;

#endregion

namespace Emotion.ExecTest;

public class Program
{
    public static void Main(string[] args)
    {
        if (CommandLineParser.FindArgument(args, "tests", out string _))
        {
            MainTests(args);
            return;
        }

        Engine.Start(new Configurator
        {
            DebugMode = true,
            HostTitle = "Example"
        }, EntryPointAsync);
    }

    private static void MainTests(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true
        };

        TestExecutor.ExecuteTests(args, config);
    }

    private static IEnumerator EntryPointAsync()
    {
        //yield return Engine.SceneManager.SetScene(new TestScene());
        yield return Engine.SceneManager.SetScene(new ExampleEmpty());
    }
}