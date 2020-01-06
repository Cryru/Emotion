#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using ImGuiNET;

#endregion

namespace Emotion.Plugins.ImGuiNet.Windowing
{
    public abstract class ImGuiModal : ImGuiWindow
    {
        private bool _popupOpen = false;

        protected ImGuiModal(string title = "Untitled") : base(title)
        {

        }

        public override void Render(Vector2 spawnOffset, RenderComposer composer)
        {
            if (!_popupOpen)
            {
                ImGui.OpenPopup(Title);
                _popupOpen = true;
            }

            if (!ImGui.BeginPopupModal(Title, ref Open, ImGuiWindowFlags.AlwaysAutoResize)) return;
            Position = ImGui.GetWindowPos();
            Size = ImGui.GetWindowSize();
            RenderContent(composer);
            ImGui.EndPopup();
        }
    }
}