#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.ExecTest.Examples;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.UI;

#endregion

namespace Emotion.Tools
{
    public class Program : IScene
    {
        private UIController _ui;

        private static void Main()
        {
            var config = new Configurator
            {
                DebugMode = true,
                HostSize = new Vector2(1280, 720)
            };
            ToolsManager.ConfigureForTools(config);

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new PhysicsPlayground());
            Engine.Run();
        }

        public void Update()
        {
            _ui.Update();
        }

        public void Load()
        {
            _ui = new UIController();
            var testBackground = new UISolidColor
            {
                InputTransparent = false,
                WindowColor = Color.CornflowerBlue
            };
            _ui.AddChild(testBackground);
            ToolsManager.AddToolBoxWindow(_ui);
        }

        public void Unload()
        {
        }

        public void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            _ui.Render(composer);
        }
    }
}