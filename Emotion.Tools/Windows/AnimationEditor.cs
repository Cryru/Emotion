#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class AnimationEditor : ImGuiWindow
    {
        private static AnimatedTexture _animation;

        private static Vector2 _frameSize = Vector2.Zero;
        private static int _loopType = 1;
        private static int _frameTime = 250;

        private static int _scale = 1;
        private static TextureAsset _file;

        private static bool _playing = true;

        public AnimationEditor() : base("Animation Editor")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("Choose File"))
            {
                var explorer = new FileExplorer<TextureAsset>(LoadFile);
                Parent.AddWindow(explorer);
            }

            ImGui.Text($"Current File: {_file?.Name ?? "None"}");
            if (_file == null || _animation == null) return;
            if (ImGui.Button("Reload")) LoadFile(FileExplorer<TextureAsset>.ExplorerLoadAsset(_file.Name));
            ImGui.SameLine();
            if (_playing)
            {
                if (ImGui.Button("Pause"))
                    _playing = false;
            }
            else
            {
                if (ImGui.Button("Play"))
                    _playing = true;
            }

            if (_file == null || _animation == null) return;

            (Vector2 uv1, Vector2 uv2) = _animation.Texture.GetImGuiUV(_animation.CurrentFrame);
            ImGui.Image(new IntPtr(_animation.Texture.Pointer),
                _animation.FrameSize == Vector2.Zero ? new Vector2(100, 100) * _scale : _animation.FrameSize * _scale, uv1, uv2);

            ImGui.Text($"Resolution: {_animation.Texture.Size}");
            ImGui.InputInt("MS Between Frames", ref _frameTime);
            if (_frameTime != _animation.TimeBetweenFrames) _animation.TimeBetweenFrames = _frameTime;

            ImGui.InputFloat2("Frame Size", ref _frameSize);
            if (_frameSize != _animation.FrameSize) _animation.FrameSize = _frameSize;
            ImGui.Combo("Loop Type", ref _loopType, string.Join('\0', Enum.GetNames(typeof(AnimationLoopType))));
            if ((AnimationLoopType) _loopType != _animation.LoopType) _animation.LoopType = (AnimationLoopType) _loopType;

            ImGui.Text($"Current Frame: {_animation.CurrentFrameIndex + 1}/{_animation.AnimationFrames + 1}");

            for (var i = 0; i < _animation.TotalFrames; i++)
            {
                if (i != 0 && i % 5 != 0) ImGui.SameLine(0, 5);

                bool current = _animation.CurrentFrameIndex == i;

                Rectangle frameBounds = _animation.GetFrameBounds(i);
                (Vector2 u1, Vector2 u2) = _animation.Texture.GetImGuiUV(frameBounds);

                ImGui.Image(new IntPtr(_animation.Texture.Pointer), new Vector2(50, 50), u1, u2, Vector4.One,
                    current ? new Vector4(1, 0, 0, 1) : Vector4.Zero);
            }

            ImGui.InputInt("Display Scale", ref _scale);
        }

        private void LoadFile(TextureAsset f)
        {
            if (f?.Texture == null) return;
            _file?.Dispose();
            _file = f;
            _animation = new AnimatedTexture(f.Texture, _frameSize, (AnimationLoopType) _loopType, _frameTime);
        }

        public override void Update()
        {
            if (_playing)
                _animation?.Update(Engine.DeltaTime);
        }
    }
}