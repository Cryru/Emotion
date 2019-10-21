#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Standard.Image;
using Emotion.Utility;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Tools.Windows
{
    public class AnimationEditor : ImGuiWindow
    {
        private IAnimatedTexture _animation;

        private Vector2 _frameSize = Vector2.Zero;
        private Vector2 _spacing = Vector2.Zero;
        private int _loopType = 1;
        private int _frameTime = 250;
        private int _startFrame;
        private int _endFrame = -1;

        private int _scale = 1;
        private TextureAsset _file;

        private bool _playing = true;

        public Dictionary<string, IAnimatedTexture> Saved = new Dictionary<string, IAnimatedTexture>();
        private string _saveName = "";
        private SavedAnimations _savedAnimationsWindow;

        private string _type;

        public AnimationEditor() : base("Animation Editor")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (_type == null)
            {
                if (ImGui.Button("Standard")) _type = "standard";
                ImGui.SameLine();
                if (ImGui.Button("Lookup")) _type = "lookup";
            }
            else if (_type == "standard")
            {
                RenderContentStandard(composer);
            }
            else if (_type == "lookup")
            {
                RenderContentLookup(composer);
            }
        }

        private void RenderContentStandard(RenderComposer composer)
        {
            // File selection.
            if (ImGui.Button("Choose SpriteSheet File"))
            {
                var explorer = new FileExplorer<TextureAsset>(LoadSpriteSheetFile);
                Parent.AddWindow(explorer);
            }

            if (ImGui.Button("Choose Animation File"))
            {
                var explorer = new FileExplorer<XMLAsset<AnimatedTextureDescription>>(f =>
                {
                    if (f?.Content == null) return;
                    _animation = f.Content.CreateFrom();
                });
                Parent.AddWindow(explorer);
            }

            // File data.
            ImGui.Text($"Current File: {_file?.Name ?? "None"}");
            if (_file == null) return;
            if (_animation == null) _animation = new AnimatedTexture(_file.Texture, _frameSize, (AnimationLoopType) _loopType, _frameTime);

            if (ImGui.Button("Reload Image")) LoadSpriteSheetFile(FileExplorer<TextureAsset>.ExplorerLoadAsset(_file.Name));
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
            ImGui.Image(new IntPtr(_animation.Texture.Pointer), _animation.CurrentFrame.Size * _scale, uv1, uv2);
            ImGui.Text($"Resolution: {_animation.Texture.Size}");
            ImGui.InputInt("Display Scale", ref _scale);

            // Editors
            if (_animation is AnimatedTexture standardAnim)
            {
                ImGui.InputInt("MS Between Frames", ref _frameTime);
                if (_frameTime != standardAnim.TimeBetweenFrames) _animation.TimeBetweenFrames = _frameTime;
                ImGui.InputFloat2("Frame Size", ref _frameSize);
                if (_frameSize != standardAnim.FrameSize) standardAnim.FrameSize = _frameSize;
                ImGui.InputFloat2("Spacing", ref _spacing);
                if (_spacing != standardAnim.Spacing) standardAnim.Spacing = _spacing;
            }

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
            ImGui.InputText("Name", ref _saveName, 100);
            ImGui.SameLine();
            if (string.IsNullOrEmpty(_saveName))
            {
                ImGui.TextDisabled("Save");
                ImGui.SameLine();
                ImGui.TextDisabled("SaveToFile");
            }
            else
            {
                if (ImGui.Button("Save"))
                {
                    Saved.Add(_saveName, _animation.Copy());
                    _saveName = "";
                    if (_savedAnimationsWindow == null) Parent.AddWindow(_savedAnimationsWindow = new SavedAnimations(this));
                }

                ImGui.SameLine();
                if (ImGui.Button("SaveToFile"))
                {
                    AnimationDescriptionBase description = _animation.GetDescription(_file.Name);
                    var serializer = new XmlSerializer(description.GetType());
                    FileStream stream = File.OpenWrite(Helpers.CrossPlatformPath("./Assets/" + _saveName + ".anim"));
                    serializer.Serialize(stream, description);
                    stream.Flush();
                    stream.Close();
                    _saveName = "";
                }
            }
        }

        private Rectangle[] _lookupRects;

        private void RenderContentLookup(RenderComposer composer)
        {
            // File selection.
            if (ImGui.Button("Choose SpriteSheet File"))
            {
                var explorer = new FileExplorer<TextureAsset>(LoadSpriteSheetFile);
                Parent.AddWindow(explorer);
            }

            if (ImGui.Button("Choose Animation File"))
            {
                var explorer = new FileExplorer<XMLAsset<LookupAnimatedDescription>>(f =>
                {
                    if (f?.Content == null) return;
                    _animation = f.Content.CreateFrom();
                });
                Parent.AddWindow(explorer);
            }

            // File data.
            ImGui.Text($"Current File: {_file?.Name ?? "None"}");
            if (_file == null) return;
            if (_animation == null) _animation = new LookupAnimatedTexture(_file.Texture, null, (AnimationLoopType) _loopType, _frameTime);
            if (ImGui.Button("Reload Image")) LoadSpriteSheetFile(FileExplorer<TextureAsset>.ExplorerLoadAsset(_file.Name));
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
            ImGui.Image(new IntPtr(_animation.Texture.Pointer), _animation.CurrentFrame.Size * _scale, uv1, uv2);

            if (_lookupRects != null)
            {
                Vector2 pos = ImGui.GetWindowPos();
                pos.Y += ImGui.GetWindowHeight();
                composer.RenderSprite(new Vector3(pos, 0), _file.Texture.Size * _scale, Color.White, _file.Texture);
                for (var i = 0; i < _lookupRects.Length; i++)
                {
                    composer.RenderOutline(new Vector3((_lookupRects[i].Location * _scale) + pos, 1) , _lookupRects[i].Size * _scale, Color.Red);
                }
            }

            ImGui.Text($"Resolution: {_animation.Texture.Size}");
            ImGui.InputInt("Display Scale", ref _scale);

            // Editors
            if (_animation is LookupAnimatedTexture lookupAnim)
                if (ImGui.Button("Auto Detect Frames"))
                {
                    _lookupRects = AutoDetectLookup();
                    lookupAnim.Frames = _lookupRects;
                }
                    

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
            ImGui.InputText("Name", ref _saveName, 100);
            ImGui.SameLine();
            if (string.IsNullOrEmpty(_saveName))
            {
                ImGui.TextDisabled("Save");
                ImGui.SameLine();
                ImGui.TextDisabled("SaveToFile");
            }
            else
            {
                if (ImGui.Button("Save"))
                {
                    Saved.Add(_saveName, _animation.Copy());
                    _saveName = "";
                    if (_savedAnimationsWindow == null) Parent.AddWindow(_savedAnimationsWindow = new SavedAnimations(this));
                }

                ImGui.SameLine();
                if (ImGui.Button("SaveToFile"))
                {
                    AnimationDescriptionBase description = _animation.GetDescription(_file.Name);
                    var serializer = new XmlSerializer(description.GetType());
                    FileStream stream = File.OpenWrite(Helpers.CrossPlatformPath("./Assets/" + _saveName + ".anim"));
                    serializer.Serialize(stream, description);
                    stream.Flush();
                    stream.Close();
                    _saveName = "";
                }
            }
        }

        private void LoadSpriteSheetFile(TextureAsset f)
        {
            if (f?.Texture == null) return;
            _file?.Dispose();
            _file = f;
        }

        public override void Update()
        {
            if (_playing)
                _animation?.Update(Engine.DeltaTime);
        }

        private unsafe Rectangle[] AutoDetectLookup()
        {
            var pixels = new byte[(int) (_file.Texture.Size.X * _file.Texture.Size.Y * 4)];

            fixed (void* p = &pixels[0])
            {
                Texture.EnsureBound(_file.Texture.Pointer);
                Gl.GetTexImage(TextureTarget.Texture2d, 0, PixelFormat.Rgba, PixelType.UnsignedByte, new IntPtr(p));
            }

            ImageUtil.FlipImageY(pixels, (int) _file.Texture.Size.X, (int) _file.Texture.Size.Y);
            // Convert to 1 bit.
            for (int i = 0, w = 0; i < pixels.Length; i += 4, w++)
            {
                pixels[w] = (byte) (pixels[i + 3] > 10 ? 1 : 0);
            }
            Array.Resize(ref pixels, pixels.Length / 4);

            var boxes = new List<Rectangle>();

            // First pass - identify box start positions.
            for (var y = 0; y < _file.Texture.Size.Y; y++)
            {
                for (var x = 0; x < _file.Texture.Size.X; x++)
                {
                    byte current = pixels[(int) (y * _file.Texture.Size.X + x)];

                    // Check if the current one is filled.
                    if (current != 1) continue;
                    // Start a box.
                    var start = new Vector2(x, y);
                    var size = new Vector2();
                    var width = 0;
                    // Find the next non full. This is the width.
                    for (int yy = y; yy < _file.Texture.Size.Y; yy++)
                    {
                        for (int xx = x; xx < _file.Texture.Size.X; xx++)
                        {
                            byte curLook = pixels[(int) (yy * _file.Texture.Size.X + xx)];
                            if (curLook == 0)
                            {
                                if (width > size.X) size.X = width;
                                goto step2;
                            }
                            width++;
                        }

                        if (width > size.X) size.X = width;
                        width = 0;
                    }
                    step2:
                    // Now go down from the start until we find a non-full.
                    var heightLeft = 0;
                    for (int yy = y; yy < _file.Texture.Size.Y; yy++)
                    {
                        byte curLook = pixels[(int) (yy * _file.Texture.Size.X + x)];
                        if (curLook == 0)
                        {
                            break;
                        }
                        heightLeft++;
                    }

                    // Now go down from the end until we find a non-full.
                    var heightRight = 0;
                    for (int yy = y; yy < _file.Texture.Size.Y; yy++)
                    {
                        byte curLook = pixels[(int) (yy * _file.Texture.Size.X + (x + size.X - 1))];
                        if (curLook == 0)
                        {
                            break;
                        }
                        heightRight++;
                    }

                    size.Y = MathF.Max(heightLeft, heightRight);
                    boxes.Add(new Rectangle(start, size));
                    x += (int) size.X;
                }
            }

            // Combine all boxes.
            restart:
            for (var i = 0; i < boxes.Count; i++)
            {
                for (var j = 0; j < boxes.Count; j++)
                {
                    if (i == j) continue;

                    if (!boxes[i].Intersects(boxes[j])) continue;
                    boxes[i] = Rectangle.Union(boxes[i], boxes[j]);
                    boxes.RemoveAt(j);
                    goto restart;
                }
            }

            return boxes.ToArray();
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
            foreach (KeyValuePair<string, IAnimatedTexture> savedAnimation in _parent.Saved)
            {
                ImGui.Text(savedAnimation.Key);
                IAnimatedTexture anim = savedAnimation.Value;
                int animPreview = Math.Min(5, anim.AnimationFrames);
                for (var i = 0; i <= animPreview; i++)
                {
                    Rectangle frameBounds = anim.GetFrameBounds(i);
                    (Vector2 u1, Vector2 u2) = anim.Texture.GetImGuiUV(frameBounds);
                    ImGui.Image(new IntPtr(anim.Texture.Pointer), frameBounds.Size / 2f, u1, u2);
                    if (i != animPreview) ImGui.SameLine();
                }
            }
        }
    }
}