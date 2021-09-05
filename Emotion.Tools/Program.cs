#region Using

using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Tools.Windows;
using ImGuiNET;

#endregion

namespace Emotion.Tools
{
    public class ToolsBoot
    {
        public static void ConfigureForTools(Configurator config)
        {
            if (config.Plugins.Any(x => x.GetType() == typeof(ImGuiNetPlugin))) return;
            config.AddPlugin(new ImGuiNetPlugin());
        }
    }

    internal class Program : IScene
    {
        private static void Main()
        {
            var config = new Configurator
            {
                DebugMode = true,
                HostSize = new Vector2(1280, 720)
            };
            ToolsBoot.ConfigureForTools(config);

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new Program());
            Engine.Run();
        }

        public void Update()
        {

        }

        public void Load()
        {

        }

        public void Unload()
        {

        }

        public void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.RenderToolsMenu();
        }
    }
}