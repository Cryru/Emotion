#region Using

using System;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.HelpWindows
{
    public class YesNoModal : ImGuiModal
    {
        private Action<bool> _callback;
        private string _text;
        private bool _first = true;

        public YesNoModal(Action<bool> callback, string title, string text) : base(title)
        {
            _callback = callback;
            _text = text;
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (_first)
            {
                ImGui.SetKeyboardFocusHere();
                _first = false;
            }

            ImGui.Text(_text);

            if (ImGui.Button("Yes"))
            {
                _callback(true);
                Open = false;
                return;
            }

            ImGui.SameLine();
            if (ImGui.Button("No"))
            {
                _callback(false);
                Open = false;
            }
        }

        public override void Update()
        {
        }
    }
}