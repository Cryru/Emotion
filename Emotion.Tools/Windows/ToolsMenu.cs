#region Using

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

        protected override void RenderContent()
        {
            if (ImGui.Button("Animation Editor")) Parent.AddWindow(new AnimationEditor());

            if (ImGui.Button("Performance Monitor")) Parent.AddWindow(new PerformanceMonitor());
        }

        public override void Update()
        {
        }
    }
}