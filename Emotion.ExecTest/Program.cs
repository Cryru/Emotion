#region Using

using System.Collections;
using Emotion.Common;
using Emotion.ExecTest.CryruDevelopment;
using Emotion.Testing;
using Emotion.Utility;
using Emotion.ExecTest.ExamplesOne;
using System.Threading.Tasks;
#endregion

namespace Emotion.ExecTest;

public class Program
{
    public static Task Main(string[] args)
    {
        if (CommandLineParser.FindArgument(args, "tests", out string _))
        {
            MainTests(args);
            return Task.CompletedTask;
        }

        Engine.Start(new Configurator
        {
            DebugMode = true,
            HostTitle = "Example"
        }, EntryPointAsync);
        return Task.CompletedTask;
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