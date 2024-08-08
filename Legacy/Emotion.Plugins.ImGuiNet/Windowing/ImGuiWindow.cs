#region Using

using System.Numerics;
using Emotion.Graphics;
using ImGuiNET;

#endregion

namespace Emotion.Plugins.ImGuiNet.Windowing
{
    public abstract class ImGuiWindow
    {
        public string Title { get; set; }

        public bool Open = true;
        public bool MenuBar = false;
        public bool CantClose = false;

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public WindowManager Parent { get; set; }

        protected ImGuiWindow(string title = "Untitled")
        {
            Title = title;
        }

        public virtual void OnOpen()
        {
        }

        public virtual void Render(Vector2 spawnOffset, RenderComposer composer)
        {
            ImGui.SetNextWindowPos(spawnOffset, ImGuiCond.Appearing);

            var flags = ImGuiWindowFlags.AlwaysAutoResize;
            if (MenuBar) flags |= ImGuiWindowFlags.MenuBar;

            if (CantClose)
                ImGui.Begin(Title, flags);
            else
                ImGui.Begin(Title, ref Open, flags);
            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowSize();
            RenderContent(composer);
            ImGui.End();
        }

        public abstract void Update();
        protected abstract void RenderContent(RenderComposer composer);

        public virtual void Dispose()
        {
        }
    }
}