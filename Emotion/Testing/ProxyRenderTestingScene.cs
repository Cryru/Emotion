#region Using

using System;
using Emotion.Graphics;

#endregion

#nullable enable

namespace Emotion.Testing;

public class ProxyRenderTestingScene : TestingScene
{
    public Action<RenderComposer>? ToRender;

    protected override void TestDraw(RenderComposer c)
    {
        ToRender?.Invoke(c);
    }

    protected override void TestUpdate()
    {

    }

    public override void BetweenEachTest()
    {
        base.BetweenEachTest();

        ToRender = null;
    }
}
