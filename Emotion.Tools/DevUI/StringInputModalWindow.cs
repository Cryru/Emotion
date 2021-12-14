#region Using

using System;
using ImGuiNET;

#endregion

namespace Emotion.Tools.DevUI
{
    public class StringInputModalWindow : ImGuiBaseModal
    {
        private Action<string> _callback;
        private string _text = "";
        private bool _first = true;
        private int _minChars;

        /// <summary>
        /// Create a new string input modal window.
        /// </summary>
        /// <param name="callback">The callback to call with the string.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="minChars">Minimum amount of characters.</param>
        public StringInputModalWindow(Action<string> callback, string title, int minChars = 1) : base(title)
        {
            _callback = callback;
            _minChars = minChars;
        }

        protected override void RenderImGui()
        {
            if (_first)
            {
                ImGui.SetKeyboardFocusHere();
                _first = false;
            }

            bool enterPressed = ImGui.InputText("Text", ref _text, 100, ImGuiInputTextFlags.EnterReturnsTrue);
            if (!ImGui.Button("Done") && !enterPressed) return;
            if (_text.Length < _minChars) return;
            _callback(_text);
            Parent?.RemoveChild(this);
        }
    }
}