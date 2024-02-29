#region Using

using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Editor.EditorWindows;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Testing;
using Emotion.UI;
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

        //if (CommandLineParser.FindArgument(args, "3d", out string _))
        //    Engine.SceneManager.SetScene(new TestScene3D());
        //else
        //    Engine.SceneManager.SetScene(new TestScene2D());

        Engine.SceneManager.SetScene(new LayoutEngineTestScene());

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

public class LayoutEngineTestScene : IScene
{
    private UIController _ui;

    public void Load()
    {
        _ui = new UIController();
        _ui.UseNewLayoutSystem = true;

        var listParent = new UIBaseWindow();
        //listParent.LayoutMode = LayoutMode.HorizontalList;
        listParent.UseNewLayoutSystem = true;
        listParent.ListSpacing = new Vector2(1, 1);
        _ui.AddChild(listParent);

        listParent.AddChild(new UIBaseWindow()
        {
            MinSize = new Vector2(20, 20),
            AnchorAndParentAnchor = UIAnchor.TopCenter
        });

        listParent.AddChild(new UIBaseWindow()
        {
            MinSize = new Vector2(20, 20),
            AnchorAndParentAnchor = UIAnchor.BottomCenter
            //Margins = new Rectangle(20, 0, 0, 0)
        });

        listParent.AddChild(new UIBaseWindow()
        {
            MinSize = new Vector2(20, 20),
            //FillY = false
            //Margins = new Rectangle(40, 20, 0, 0)
        });

        //listParent.AddChild(new UIBaseWindow()
        //{
        //    MinSize = new Vector2(20, 20)
        //});

        UIController.Debug_RenderLayoutEngine = listParent;
    }

    public void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.SetDepthTest(false);
        _ui.Render(composer);
    }

    public void Update()
    {
        _ui.InvalidateLayout();
        _ui.Update();
    }

    public void Unload()
    {

    }
}

public class TestScene3D : World3DBaseScene<Map3D>
{
    public override Task LoadAsync()
    {
        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam3D;

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

public class TestObject : GameObject2D
{
    public float TestFloat;
    public int TestInt;
}

public class TestScene2D : World2DBaseScene<Map2D>
{
    public override Task LoadAsync()
    {
        _editor.EnterEditor();

        _editor.ChangeSceneMap(new Map2D(new Vector2(10)));

        var obj = new TestObject();
        _editor.CurrentMap.AddObject(obj);

        var newPanel = new ObjectEditor(obj);
        _editor.UIController.AddChild(newPanel);

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