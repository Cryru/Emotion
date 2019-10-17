#region Using

using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class ToolsMenu : ImGuiWindow
    {
        public ToolsMenu() : base("Tools")
        {
            CantClose = true;
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("Animation Editor")) Parent.AddWindow(new AnimationEditor());
            if (ImGui.Button("Map Editor")) Parent.AddWindow(new MapEditor());
            if (ImGui.Button("Audio Editor")) Parent.AddWindow(new AudioEditor());

            if (ImGui.Button("Performance Monitor")) Parent.AddWindow(new PerformanceMonitor());
        }

        public override void Update()
        {
        }
    }
}