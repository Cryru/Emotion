#nullable enable

namespace Emotion.Testing;

public class ProxyRenderTestingScene : TestingScene
{
    public Action<Renderer>? ToRender;

    protected override void TestDraw(Renderer c)
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
