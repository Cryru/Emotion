using Emotion.Scenography;
using Emotion.UI;
using Emotion.Utility;
using System.Collections;

namespace Game.Scenes;

public class DefaultScene : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        yield break;
    }

    public override void UpdateScene(float dt)
    {
        base.UpdateScene(dt);
    }

    public override void RenderScene(RenderComposer c)
    {
        base.RenderScene(c);
    }
}
