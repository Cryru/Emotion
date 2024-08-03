#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Testing;
using Emotion.Utility;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using WinApi.User32;

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

        Engine.Start(new Configurator
        {
            DebugMode = true,
            HostTitle = "Example"
        }, EntryPointAsync);
    }

    private static void MainTests(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true
        };

        //TestExecutor.ExecuteTests(args, config);
    }

    private static IEnumerator EntryPointAsync()
    {
        yield return Engine.SceneManager.SetScene(new TestScene());

    }
}

public class TestObject : MapObject
{
    private Vector2 _inputDirection;

    public TestObject()
    {
        Size = new Vector2(20);

        Engine.Host.OnKey.AddListener(KeyInput, KeyListenerType.Game);
    }

    protected bool KeyInput(Key key, Platform.Input.KeyState status)
    {
        Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
        if (keyAxisPart != Vector2.Zero)
        {
            if (status == Platform.Input.KeyState.Down)
                _inputDirection += keyAxisPart;
            else if (status == Platform.Input.KeyState.Up)
                _inputDirection -= keyAxisPart;

            return false;
        }

        return true;
    }

    public override void Update(float dt)
    {
        Position2 += _inputDirection * 0.1f * dt;
    }

    public override void Render(RenderComposer c)
    {
        c.RenderSprite(Position, Size, Color.Red);
    }
}

public class TestScene : Scene
{
    protected override IEnumerator LoadSceneRoutineAsync()
    {
        // Fixes deadlock
        Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");

        Map = new GameMap();
        //Map.AddAndInitObject(new MapObjectSprite()
        //{
        //    EntityFile = "Test/Character.em2"
        //});

        Map.AddAndInitObject(new TestObject());

        //throw new System.Exception("haa");
        yield break;
    }

    public override void RenderScene(RenderComposer c)
    {
        c.SetUseViewMatrix(false);
        c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.PrettyGreen);
        c.ClearDepth();
        c.SetUseViewMatrix(true);

        c.RenderCircle(Vector3.Zero, 20, Color.White, true);
        base.RenderScene(c);
    }
}

//public class TestScene3D : World3DBaseScene<Map3D>
//{
//    public override Task LoadAsync()
//    {
//        var cam3D = new Camera3D(new Vector3(100));
//        cam3D.LookAtPoint(Vector3.Zero);
//        Engine.Renderer.Camera = cam3D;

//        _editor.EnterEditor();
//        return Task.CompletedTask;
//    }

//    public override void Draw(RenderComposer composer)
//    {
//        composer.SetUseViewMatrix(false);
//        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
//        composer.ClearDepth();
//        composer.SetUseViewMatrix(true);

//        base.Draw(composer);
//    }
//}

//public class TestScene2D : World2DBaseScene<Map2D>
//{
//    public override Task LoadAsync()
//    {
//        _editor.EnterEditor();
//        return Task.CompletedTask;
//    }

//    public override void Draw(RenderComposer composer)
//    {
//        composer.SetUseViewMatrix(false);
//        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
//        composer.ClearDepth();
//        composer.SetUseViewMatrix(true);

//        base.Draw(composer);
//    }
//}