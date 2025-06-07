using Emotion.Utility;
using Game.Scenes;

namespace Game;

public static class Program
{
    public static void Main()
    {
        var config = new Configurator()
        {
#if DEBUG
            DebugMode = true
#endif
        };

        Engine.Start(config, EntryRoutineAsync);
    }

    private static IEnumerator EntryRoutineAsync()
    {
        yield return Engine.SceneManager.SetScene(new DefaultScene());
    }
}