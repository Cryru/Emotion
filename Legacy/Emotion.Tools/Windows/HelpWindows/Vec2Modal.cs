#region Using

using System;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.HelpWindows
{
    public class Vec2Modal : ImGuiModal
    {
        private Action<Vector2> _callback;
        private string _text;
        private Vector2 _vector;
        private bool _first = true;

        public Vec2Modal(Action<Vector2> callback, string title, string text, Vector2 defaultValue) : base(title)
        {
            _callback = callback;
            _text = text;
            _vector = defaultValue;
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (_first)
            {
                ImGui.SetKeyboardFocusHere();
                _first = false;
            }

            ImGui.InputFloat2(_text, ref _vector);

            if (ImGui.Button("Confirm"))
            {
                _callback(_vector);
                Open = false;
            }
        }

        public override void Update()
        {
        }
    }
}