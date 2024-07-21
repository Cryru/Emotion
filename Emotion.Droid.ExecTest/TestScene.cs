#region Using

using System.Collections;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.ThreeDee.Editor;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.WIPUpdates.One;

#endregion

namespace Emotion.Droid.ExecTest;

public class TestScene : Scene
{
    protected override IEnumerator LoadSceneRoutineAsync()
    {
        
        //throw new System.Exception("haa");
        yield break;
    }

    protected override void RenderScene(RenderComposer c)
    {
        
        c.SetUseViewMatrix(false);
        c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.PrettyGreen);
        c.ClearDepth();
        c.SetUseViewMatrix(true);
    }

    protected override void UpdateScene(float dt)
    {
        if (!EngineEditor.IsOpen) EngineEditor.OpenEditor();
    }
}