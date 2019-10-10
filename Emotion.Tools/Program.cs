#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Tools.Windows;
using ImGuiNET;

#endregion

namespace Emotion.Tools
{
    internal class Program
    {
        #region UI Logic

        private static WindowManager _manager = new WindowManager();

        #endregion

        private static void Main()
        {
            Engine.Setup(new Configurator().AddPlugin(new ImGuiNetPlugin()).SetDebug(true).SetHostSettings(new Vector2(1280, 720)));
            Engine.DebugDrawAction = DebugDrawAction;
            Engine.DebugUpdateAction = DebugUpdateAction;
            Engine.Run();
        }

        private static bool init = true;

        private static void DebugUpdateAction()
        {
            if (init)
            {
                DebugInit();
                init = false;
            }

            _manager.Update();
        }

        private static void DebugInit()
        {
            _manager.AddWindow(new ToolsMenu());
        }

        private static void DebugDrawAction(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);

            ImGui.NewFrame();

            _manager.Render();

            ImGuiNetPlugin.RenderUI(composer);
        }
    }
}