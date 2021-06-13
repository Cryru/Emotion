#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Network.Dbg;
using Emotion.Network.Game;
using Emotion.Network.Infrastructure;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Tools.Windows;
using ImGuiNET;

#endregion

namespace Emotion.Network
{
    public class DebugWindow : ImGuiWindow
    {
        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            var scene = (NetworkScene) Engine.SceneManager.Current;
            if (scene == null) return;

            ImGui.Text("According to Client:");
            ImGui.Text($"Time: {scene.Time:0}\nServerTime: {scene.ServerTime}\nDelta: {scene.TimeDelta:0} ({scene.MaxDelta})");

            ImGui.Text("");

            ImGui.Text("According to Server:");
            ImGui.Text($"Time: {GameServer.Server.Scene.Time:0}");

            ImGui.Text("");

            ImGui.Text("Debug Settings:");
            ImGui.SliderInt("Simulated Lag", ref Program.DbgClientLag, 0, 2000);
        }
    }

    public class Program
    {
        public static int DbgClientLag = 60;

        private static void Main(string[] args)
        {
            var conf = new Configurator
            {
                DebugMode = true,
                RenderSize = new Vector2(960, 540),
            };
            conf.AddPlugin(new ImGuiNetPlugin());
            ToolsMenu.CustomTools.Add("Network", wm => { wm.AddWindow(new DebugWindow()); });
            Engine.Setup(conf);

            var server = new TestGameServer();
            server.Run();

            var client = new TestGameClient("P1");
            client.Run();
            client.JoinGame(server);

            Engine.Run();
        }
    }
}