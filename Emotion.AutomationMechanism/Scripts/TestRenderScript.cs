using System;
using System.Threading.Tasks;
using System.Collections;

var inputArgs = EmaSystem.Args;

public class TestScene : Scene
{
    private FrameBuffer _buff;

    public override Task LoadAsync()
    {
        GLThread.ExecuteGLThread(() =>
        {
            _buff = new FrameBuffer(new Vector2(1024, 1024)).WithColor();
        });
        return Task.CompletedTask;
    }

    public override void Draw(RenderComposer c)
    {
        c.SetUseViewMatrix(false);
        c.RenderToAndClear(_buff);
        c.RenderSprite(Vector3.Zero, _buff.Size, Color.Red);
        c.RenderTo(null);

        var pixels = _buff.Sample(new Rectangle(0, 0, _buff.Size), OpenGL.PixelFormat.Rgba);
        var pngBytes = PngFormat.Encode(pixels, _buff.Size, OpenGL.PixelFormat.Rgba);
        Engine.AssetLoader.Save(pngBytes, "test.png");

        Console.WriteLine("loop");
    }

    public override void Update()
    {

    }
}

IEnumerator Routine()
{
    Engine.Log.Info("Changing scene...", "EMA");
    yield return new TaskRoutineWaiter(Engine.SceneManager.SetScene(new TestScene()));
    Engine.Log.Info("Waiting one frame...", "EMA");
    yield return null;
}

Coroutine coroutine = Engine.CoroutineManager.StartCoroutine(Routine());
while (!coroutine.Finished)
    await Task.Delay(100);