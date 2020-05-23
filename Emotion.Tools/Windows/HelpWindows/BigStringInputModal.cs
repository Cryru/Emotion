#region Using

using System;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.HelpWindows
{
    public class BigStringInputModal : ImGuiModal
    {
        private Action<string> _callback;
        private string _text;

        public BigStringInputModal(Action<string> callback, string title, string startingText) : base(title)
        {
            _callback = callback;
            _text = startingText ?? "";
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.InputTextMultiline("Code", ref _text, 10000, new System.Numerics.Vector2(700, 500), ImGuiInputTextFlags.AllowTabInput))
            {
                _callback(_text);
            }
            if (!ImGui.Button("Close")) return;
            Open = false;
        }

        public override void Update()
        {
        }
    }
}