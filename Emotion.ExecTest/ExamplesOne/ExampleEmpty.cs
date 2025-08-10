#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Scenography;
using Emotion.ExecTest.Experiment;
using Emotion.Game.World;
using Emotion.Graphics;
using Emotion.Primitives;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.ExamplesOne;

public class ExampleEmpty : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        Map = new GameMap();

        yield break;
    }

    public override void RenderScene(Renderer c)
    {
        base.RenderScene(c);

        //FontAsset font = FontAsset.GetDefaultBuiltIn();
        //GPURenderTextExperiment.RenderFont(new Vector2(15, 15), "This is a test", font.Font, 15f, Color.White);
    }
}
