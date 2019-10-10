#region Using

using System.Numerics;
using ImGuiNET;

#endregion

namespace Emotion.Plugins.ImGuiNet.Windowing
{
    public abstract class ImGuiWindow
    {
        public string Title { get; set; }

        public bool Open = true;
        public bool CantClose = false;

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public WindowManager Parent { get; set; }

        public ImGuiWindow(string title = "Untitled")
        {
            Title = title;
        }

        public void Render(Vector2 spawnOffset)
        {
            ImGui.SetNextWindowPos(spawnOffset, ImGuiCond.Appearing);
            if (CantClose)
                ImGui.Begin(Title, ImGuiWindowFlags.AlwaysAutoResize);
            else
                ImGui.Begin(Title, ref Open, ImGuiWindowFlags.AlwaysAutoResize);
            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowSize();
            RenderContent();
            ImGui.End();
        }

        public abstract void Update();
        protected abstract void RenderContent();
    }
}