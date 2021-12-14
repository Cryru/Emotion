#region Using

using Emotion.Graphics;
using ImGuiNET;

#endregion

namespace Emotion.Tools.DevUI
{
    public abstract class ImGuiBaseModal : ImGuiBaseWindow
    {
        private bool _popupOpen;

        protected ImGuiBaseModal(string title) : base(title)
        {

        }

        protected override bool RenderInternal(RenderComposer c)
        {
            if (!_popupOpen)
            {
                ImGui.OpenPopup(Title);
                _popupOpen = true;
            }

            var open = true;
            if (ImGui.BeginPopupModal(Title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                RenderImGui();
                ImGui.EndPopup();
            }

            if (!open) Parent?.RemoveChild(this);

            return true;
        }
    }
}