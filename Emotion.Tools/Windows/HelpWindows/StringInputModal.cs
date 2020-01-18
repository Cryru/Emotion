#region Using

using System;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.HelpWindows
{
    public class StringInputModal : ImGuiModal
    {
        private Action<string> _callback;
        private string _text = "";

        /// <summary>
        /// Create a new string input modal window.
        /// </summary>
        /// <param name="callback">The callback to call with the string.</param>
        /// <param name="title">The title of the dialog.</param>
        public StringInputModal(Action<string> callback, string title) : base(title)
        {
            _callback = callback;
        }

        protected override void RenderContent(RenderComposer composer)
        {
            ImGui.InputText("Text: ", ref _text, 100);
            if (!ImGui.Button("Done")) return;
            _callback(_text);
            Open = false;
        }

        public override void Update()
        {
        }
    }
}