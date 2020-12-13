#region Using

using System;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.AnimationEditorWindows
{
    public class AnimationCreateFrom : ImGuiModal
    {
        private AnimationEditor _parent;

        private Vector2 _frameSize = Vector2.Zero;
        private Vector2 _spacing = Vector2.Zero;
        private Action<Vector2, Vector2> _callback;

        private int _row;
        private int _columns;
        private Action<int, int> _callbackRc;

        public AnimationCreateFrom(AnimationEditor parent, Action<Vector2, Vector2> callback, Action<int, int> callbackRc) : base("Animation, Create From:")
        {
            _parent = parent;
            _callback = callback;
            _callbackRc = callbackRc;
        }

        public override void Update()
        {
            if (_parent.Open) return;
            Open = false;
        }

        protected override void RenderContent(RenderComposer composer)
        {
            ImGui.InputFloat2("Frame Size", ref _frameSize);
            ImGui.InputFloat2("Spacing", ref _spacing);
            if (ImGui.Button("Create From FrameSize"))
            {
                _callback(_frameSize, _spacing);
                Open = false;
            }

            ImGui.InputInt("Rows", ref _row);
            ImGui.InputInt("Columns", ref _columns);
            if (ImGui.Button("Create From Row/Columns"))
            {
                _callbackRc(_row, _columns);
                Open = false;
            }
        }
    }
}