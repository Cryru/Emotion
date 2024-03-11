#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.Time;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.Utility;

#endregion

namespace Emotion.ExecTest;

public class Program
{
    private static void Main(string[] args)
    {
        if (CommandLineParser.FindArgument(args, "tests", out string _))
        {
            MainTests(args);
            return;
        }

        var config = new Configurator
        {
            DebugMode = true
        };

        Engine.Setup(config);
        Engine.SceneManager.SetScene(new TestScene3D());

        Engine.Run();
    }

    private static void MainTests(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true
        };

        TestExecutor.ExecuteTests(args, config);
    }
}

public class TestScene3D : World3DBaseScene<Map3D>
{
    Texture t;

    public override Task LoadAsync()
    {
        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam3D;

        //int width = 64;
        //int height = 64;

        //int gridSize = 16;
        //int smallerGrid = 2;

        //float colorEven = 0.42f;
        //float colorOdd = 0.85f;

        //float smallColorEvenMod = 0.83f;
        //float smallColorOddMod = 1f;

        //Vector3[] pixels = new Vector3[width * height];
        //for (int x = 0; x < width; x++)
        //{
        //    for (int y = 0; y < height; y++)
        //    {
        //        int gridCoordX = (int) ((float) x / gridSize);
        //        int gridCoordY = (int) ((float) y / gridSize);

        //        bool evenX = gridCoordX % 2 == 0;
        //        bool evenY = gridCoordY % 2 == 0;

        //        float color = evenX ?
        //            (evenY ? colorEven : colorOdd)
        //            :
        //            (evenY ? colorOdd : colorEven);

        //        if (x >= 1 && y >= 1)
        //        {
        //            int smallGridX = (int)((float)(x - 1) / smallerGrid);
        //            int smallGridY = (int)((float)(y - 1) / smallerGrid);

        //            bool smallEvenX = smallGridX % 2 == 0;
        //            bool smallEvenY = smallGridY % 2 == 0;

        //            if (smallEvenY)
        //            {
        //                float smallGridModifier = smallEvenX ? smallColorEvenMod : smallColorOddMod;
        //                color *= smallGridModifier;
        //            }
        //        }
                

        //        pixels[y * width + x] = new Vector3(color, color, color);
        //    }
        //}

        //GLThread.ExecuteGLThread(() =>
        //{
        //    byte[] pixelsToByte = new byte[width * height * 3];
        //    for (int i = 0; i < pixels.Length; i++)
        //    {
        //        var pixel = pixels[i];
        //        pixelsToByte[i * 3 + 0] = (byte) (pixel.X * 255);
        //        pixelsToByte[i * 3 + 1] = (byte) (pixel.Y * 255);
        //        pixelsToByte[i * 3 + 2] = (byte) (pixel.Z * 255);
        //    }
        //    t = new Texture();
        //    t.Upload(new Vector2(width, height), pixelsToByte, OpenGL.PixelFormat.Rgb, OpenGL.InternalFormat.Rgb);
        //});

        _editor.EnterEditor();
        return Task.CompletedTask;
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        //composer.RenderSprite(Vector3.Zero, t.Size * 10f, t);
        composer.ClearDepth();
        composer.SetUseViewMatrix(true);

        base.Draw(composer);
    }
}

public class TestScene2D : World2DBaseScene<Map2D>
{
    public override Task LoadAsync()
    {
        _editor.EnterEditor();
        return Task.CompletedTask;
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        composer.ClearDepth();
        composer.SetUseViewMatrix(true);

        base.Draw(composer);
    }
}