#region Using

using System;
using System.Collections.Generic;
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
        private AnimatedTexture _animation;

        private Vector2 _frameSize = Vector2.Zero;
        private Vector2 _spacing = Vector2.Zero;
        private int _loopType = 1;
        private int _frameTime = 250;
        private int _startFrame;
        private int _endFrame = -1;

        private int _scale = 1;
        private TextureAsset _file;

        private bool _playing = true;

        public Dictionary<string, AnimatedTexture> Saved = new Dictionary<string, AnimatedTexture>();
        private string _saveName = "";
        private SavedAnimations _savedAnimationsWindow;

        public AnimationEditor() : base("Animation Editor")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            // File selection.
            if (ImGui.Button("Choose File"))
            {
                var explorer = new FileExplorer<TextureAsset>(LoadFile);
                Parent.AddWindow(explorer);
            }

            // File data.
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

            // Image data and scale.
            (Vector2 uv1, Vector2 uv2) = _animation.Texture.GetImGuiUV(_animation.CurrentFrame);
            ImGui.Image(new IntPtr(_animation.Texture.Pointer),
                _animation.FrameSize == Vector2.Zero ? new Vector2(100, 100) * _scale : _animation.FrameSize * _scale, uv1, uv2);
            ImGui.Text($"Resolution: {_animation.Texture.Size}");
            ImGui.InputInt("Display Scale", ref _scale);

            // Editors
            ImGui.InputInt("MS Between Frames", ref _frameTime);
            if (_frameTime != _animation.TimeBetweenFrames) _animation.TimeBetweenFrames = _frameTime;
            ImGui.InputFloat2("Frame Size", ref _frameSize);
            if (_frameSize != _animation.FrameSize) _animation.FrameSize = _frameSize;
            ImGui.InputFloat2("Spacing", ref _spacing);
            if (_spacing != _animation.Spacing) _animation.Spacing = _spacing;
            ImGui.InputInt("Starting Frame", ref _startFrame);
            if (_startFrame != _animation.StartingFrame) _animation.StartingFrame = _startFrame;
            ImGui.InputInt("Ending Frame", ref _endFrame);
            if (_endFrame != _animation.EndingFrame) _animation.EndingFrame = _endFrame;
            ImGui.Combo("Loop Type", ref _loopType, string.Join('\0', Enum.GetNames(typeof(AnimationLoopType))));
            if ((AnimationLoopType) _loopType != _animation.LoopType) _animation.LoopType = (AnimationLoopType) _loopType;

            // Frames.
            ImGui.Text($"Current Frame: {_animation.CurrentFrameIndex + 1}/{_animation.AnimationFrames + 1}");

            for (var i = 0; i < _animation.TotalFrames; i++)
            {
                if (i != 0 && i % 5 != 0) ImGui.SameLine(0, 5);

                bool current = _animation.CurrentFrameIndex == i;

                Rectangle frameBounds = _animation.GetFrameBounds(i);
                (Vector2 u1, Vector2 u2) = _animation.Texture.GetImGuiUV(frameBounds);

                ImGui.Image(new IntPtr(_animation.Texture.Pointer), _frameSize / 2f, u1, u2, Vector4.One,
                    current ? new Vector4(1, 0, 0, 1) : Vector4.Zero);
            }

            // Saving
            ImGui.InputText("Save Name", ref _saveName, 100);
            ImGui.SameLine();
            if (string.IsNullOrEmpty(_saveName))
            {
                ImGui.TextDisabled("Save");
            }
            else
            {
                if (ImGui.Button("Save"))
                {
                    Saved.Add(_saveName, new AnimatedTexture(_animation));
                    _saveName = "";
                    if (_savedAnimationsWindow == null)
                    {
                        Parent.AddWindow(_savedAnimationsWindow = new SavedAnimations(this));
                    }
                }
            }
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

    public class SavedAnimations : ImGuiWindow
    {
        private AnimationEditor _parent;

        public SavedAnimations(AnimationEditor parent) : base("Saved Animations")
        {
            _parent = parent;
            CantClose = true;
        }

        public override void Update()
        {
            if (!_parent.Open)
            {
                CantClose = false;
                Open = false;
            }
        }

        protected override void RenderContent(RenderComposer composer)
        {
            foreach (KeyValuePair<string, AnimatedTexture> savedAnimation in  _parent.Saved)
            {
                ImGui.Text(savedAnimation.Key);
                AnimatedTexture anim = savedAnimation.Value;
                int animPreview = Math.Min(5, anim.AnimationFrames);
                for (var i = 0; i <= animPreview; i++)
                {
                    Rectangle frameBounds = anim.GetFrameBounds(i);
                    (Vector2 u1, Vector2 u2) = anim.Texture.GetImGuiUV(frameBounds);
                    ImGui.Image(new IntPtr(anim.Texture.Pointer), anim.FrameSize / 2f, u1, u2);
                    if(i != animPreview) ImGui.SameLine();
                }

            }
        }
    }
}