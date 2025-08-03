#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Game.World;
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
