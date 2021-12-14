#region Using

using System;
using ImGuiNET;

#endregion

namespace Emotion.Tools.DevUI
{
    public class ButtonInputModalWindow : ImGuiBaseModal
    {
        protected Action<int> _callback;
        private string _text;
        private string[] _buttons;
        private bool _first = true;

        public ButtonInputModalWindow(string title, string text, Action<int> callback, params string[] buttons) : base(title)
        {
            _callback = callback;
            _text = text;
            _buttons = buttons;
        }

        protected override void RenderImGui()
        {
            if (_first)
            {
                ImGui.SetKeyboardFocusHere();
                _first = false;
            }

            ImGui.Text(_text);

            for (var i = 0; i < _buttons.Length; i++)
            {
                if (i != 0) ImGui.SameLine();
                if (ImGui.Button(_buttons[i]))
                {
                    _callback?.Invoke(i);
                    Parent?.RemoveChild(this);
                    return;
                }
            }
        }
    }

    public class YesNoModalWindow : ButtonInputModalWindow
    {
        public YesNoModalWindow(string title, string text, Action<bool> callback) : base(title, text, null, "Yes", "No")
        {
            // Wrap callback to boolean.
            _callback = index => { callback(index == 0); };
        }
    }
}