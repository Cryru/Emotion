#nullable enable

using Emotion.Scenography;
using Emotion.WIPUpdates.One;
using System.Collections;

namespace Emotion.ExecTest.ExamplesOne;

public class ExampleEmpty : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        Map = new GameMap();

        yield break;
    }
}
