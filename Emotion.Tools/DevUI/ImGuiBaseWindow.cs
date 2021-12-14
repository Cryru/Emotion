#region Using

using System.Numerics;
using Emotion.Graphics;
using Emotion.UI;
using ImGuiNET;

#endregion

namespace Emotion.Tools.DevUI
{
    public abstract class ImGuiBaseWindow : UIBaseWindow
    {
        public string Title;
        protected ImGuiWindowFlags _windowFlags;
        protected ToolsWindow _toolsRoot;

        protected ImGuiBaseWindow(string title = "Untitled")
        {
            Title = title;
            _windowFlags = ImGuiWindowFlags.AlwaysAutoResize;
            InputTransparent = false;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
            _toolsRoot = (ToolsWindow) Controller!.GetWindowById("ToolsRoot");
        }

        protected void GetImGuiMetrics()
        {
            Vector2 vec2 = ImGui.GetWindowPos();
            X = vec2.X;
            Y = vec2.Y;
            Size = ImGui.GetWindowSize();
        }

        protected abstract void RenderImGui();

        protected override bool RenderInternal(RenderComposer c)
        {
            var open = true;
            ImGui.Begin(Title, ref open, _windowFlags);
            GetImGuiMetrics();

            if (!open)
            {
                Parent?.RemoveChild(this);
                return false;
            }

            RenderImGui();
            ImGui.End();
            return true;
        }
    }
}