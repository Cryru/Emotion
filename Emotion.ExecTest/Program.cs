#region Using

using Emotion.Core;
using Emotion.ExecTest.ExamplesOne;
using Emotion.Standard;
using Emotion.Testing;
using System.Collections;
using System.Threading.Tasks;

#endregion

namespace Emotion.ExecTest;

public class Program
{
    public static Task Main(string[] args)
    {
        Engine.Start(
            new Configurator {
                DebugMode = true,
                HostTitle = "Example"
            },
            EntryPointAsync
        );

        return Task.CompletedTask;
    }

    private static IEnumerator EntryPointAsync()
    {
        yield return Engine.SceneManager.SetScene(new ExampleEmpty());
    }
}