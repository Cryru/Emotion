#nullable enable

using Emotion.Core.Systems.Scenography;
using System.Collections;

namespace Emotion.ExecTest.ExamplesOne;

public class ExampleEmpty : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        yield break;
    }
}
