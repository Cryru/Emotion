#region Using

using Emotion.Core;
using Emotion.ExecTest.ExamplesOne;
using System.Collections;
using System.Threading.Tasks;

#endregion

namespace Emotion.ExecTest;

public class Program
{
    public static Task Main(string[] args)
    {
        return Engine.Start(
            new Configurator
            {
                DebugMode = true,
                HostTitle = "Example"
            },
            EntryPointAsync
        );
    }

    private static IEnumerator EntryPointAsync()
    {
        yield return Engine.SceneManager.SetScene(new ExampleEmpty());
    }
}