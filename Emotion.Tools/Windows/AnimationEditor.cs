#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Tools.Windows.AnimationEditorWindows;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Tools.Windows
{
    public class AnimationEditor : ImGuiWindow
    {
        public int Scale = 1;
        private bool _playing = true;

        private string _saveName = "";

        // Standard only.
        private Vector2 _frameSize = Vector2.Zero;
        private Vector2 _spacing = Vector2.Zero;

        // Lookup only.
        private AnchorPlacer _anchorPlacerWindow;
        private FrameOrderWindow _orderWindow;
        private bool _mirrored;

        // Files
        private TextureAsset _spriteSheetTexture;
        private AnimatedTexture _animation;
        protected AnimationController _animController;

        public AnimationEditor() : base("Animation Editor")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("New From Image"))
            {
                var explorer = new FileExplorer<TextureAsset>(LoadSpriteSheetFile);
                Parent.AddWindow(explorer);
            }

            ImGui.SameLine();

            if (ImGui.Button("Open AnimatedTexture"))
            {
                var explorer = new FileExplorer<XMLAsset<AnimatedTexture>>(LoadAnimatedTexture);
                Parent.AddWindow(explorer);
            }

            ImGui.SameLine();

            if (ImGui.Button("Open AnimationController"))
            {
                var explorer = new FileExplorer<XMLAsset<AnimationController>>(LoadAnimationController);
                Parent.AddWindow(explorer);
            }

            if (_spriteSheetTexture == null) return;
            if (_animation == null)
            {
                ImGui.Text("How are the frames contained in your spritesheet?");

                if (ImGui.Button("Grid"))
                {
                    var win = new GridSettingsWindow(this, (fs, s) => { _animation = new AnimatedTexture(_spriteSheetTexture, fs, s, AnimationLoopType.Normal, 1000); },
                        (r, c) => { _animation = new AnimatedTexture(_spriteSheetTexture, c, r, AnimationLoopType.Normal, 1000); });
                    Parent.AddWindow(win);
                }

                ImGui.SameLine();

                if (ImGui.Button("Auto Detect Frames"))
                {
                    Rectangle[] frames = AutoDetectFrames(_spriteSheetTexture.Texture);
                    _animation = new AnimatedTexture(_spriteSheetTexture, frames, AnimationLoopType.Normal, 1000);
                }

                return;
            }

            ImGui.Text($"Current File: {_saveName ?? "None"}");
            ImGui.Text($"Texture File: {_spriteSheetTexture.Name} / Resolution: {_spriteSheetTexture.Texture.Size}");

            if (ImGui.Button("Reload Image")) LoadSpriteSheetFile(FileExplorer<TextureAsset>.ExplorerLoadAsset(_spriteSheetTexture.Name));
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

            ImGui.InputInt("Display Scale", ref Scale);
            ImGui.SameLine();
            if (ImGui.Button("Mirror")) _mirrored = !_mirrored;

            if (ImGui.Button("Place Anchor Points"))
                if (_anchorPlacerWindow == null || !_anchorPlacerWindow.Open)
                    Parent.AddWindow(_anchorPlacerWindow = new AnchorPlacer(this, _animation));

            if (ImGui.Button("Order Frames"))
                if (_orderWindow == null || !_orderWindow.Open)
                    Parent.AddWindow(_orderWindow = new FrameOrderWindow(this, _animation));

            ImGui.Text($"Current Frame: {_animation.CurrentFrameIndex + 1}/{_animation.AnimationFrames + 1}");
            ImGui.Text($"Current Anchor: {(_animation.Anchors.Length > 0 ? _animation.Anchors[_animation.CurrentFrameIndex].ToString() : "Unknown")}");

            for (var i = 0; i <= _animation.TotalFrames; i++)
            {
                if (i != 0 && i % 5 != 0) ImGui.SameLine(0, 5);

                bool current = _animation.CurrentFrameIndex == i;

                Rectangle frameBounds = _animation.GetFrameBounds(i);
                (Vector2 u1, Vector2 u2) = _animation.Texture.GetImGuiUV(frameBounds);

                ImGui.Image(new IntPtr(_animation.Texture.Pointer), frameBounds.Size / 2f, u1, u2, Vector4.One,
                    current ? new Vector4(1, 0, 0, 1) : Vector4.Zero);
            }

            RenderCurrentAnimationSettings();

            RenderSaveSection();
            RenderAnimation(composer);
        }

        private void RenderSaveSection()
        {
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
                ImGui.SameLine();
                if (!ImGui.Button("SaveToFile")) return;
                string saveName = _saveName;
                if (!saveName.Contains(".anim")) saveName += ".anim";
                if (!saveName.Contains("Player/")) saveName = $"Player/{saveName}";

                try
                {
                    string saveData;
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (_animController != null)
                        saveData = XMLFormat.To(_animController);
                    else
                        saveData = XMLFormat.To(_animation);

                    Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(saveData), saveName);
                }
                catch (Exception ex)
                {
                    Engine.Log.Error(ex);
                }
            }
        }

        private void RenderAnimation(RenderComposer composer)
        {
            var offset = new Vector2(100, 100);
            if (_animation.Anchors.Length > 0) offset += _animation.Anchors[_animation.CurrentFrameIndex] * Scale;
            composer.RenderSprite(new Vector3(offset, 1), _animation.CurrentFrame.Size * Scale, Color.White, _animation.Texture, _animation.CurrentFrame, _mirrored);
        }

        private void RenderCurrentAnimationSettings()
        {
            if (_animController == null)
            {
                if (ImGui.Button("Create Controller"))
                {
                    _animController = new AnimationController(_animation);
                    _animController.AddAnimation(new AnimationNode("Default")
                    {
                        EndingFrame = _animation.EndingFrame,
                        StartingFrame = _animation.StartingFrame,
                        LoopType = _animation.LoopType
                    });
                }

                int frameTime = _animation.TimeBetweenFrames;
                ImGui.InputInt("MS Between Frames", ref frameTime);
                if (frameTime != _animation.TimeBetweenFrames) _animation.TimeBetweenFrames = frameTime;

                int startingFrame = _animation.StartingFrame;
                ImGui.InputInt("Starting Frame", ref startingFrame);
                if (startingFrame != _animation.StartingFrame) _animation.StartingFrame = startingFrame;

                int endingFrame = _animation.EndingFrame;
                ImGui.InputInt("Ending Frame", ref endingFrame);
                if (endingFrame != _animation.EndingFrame) _animation.EndingFrame = endingFrame;

                var loopType = (int) _animation.LoopType;
                ImGui.Combo("Loop Type", ref loopType, string.Join('\0', Enum.GetNames(typeof(AnimationLoopType))));
                if ((AnimationLoopType) loopType != _animation.LoopType) _animation.LoopType = (AnimationLoopType) loopType;
            }
            else
            {
                ImGui.Text("Animations");
                ImGui.PushID("animList");
                foreach (AnimationNode n in _animController.Animations.Values)
                {
                    if (n == _animController.CurrentAnimation)
                    {
                        ImGui.Text(n.Name);
                    }
                    else
                    {
                        if (ImGui.Button(n.Name)) _animController.SetAnimation(n.Name);
                    }
                }

                if (ImGui.Button("Create") && _animController.Animations.All(x => x.Key != "NewAnim"))
                {
                    var newNode = new AnimationNode("NewAnim");
                    _animController.AddAnimation(newNode);
                }

                ImGui.SameLine();
                if (_animController.CurrentAnimation == null)
                {
                    ImGui.PopID();
                    return;
                }

                if (ImGui.Button("Remove")) _animController.RemoveAnimation(_animController.CurrentAnimation.Name);
                ImGui.SameLine();
                if (ImGui.Button("Rename"))
                {
                    var newName = new StringInputModal(s =>
                        {
                            _animController.CurrentAnimation.Name = s;
                            _animController.Reindex();
                        },
                        $"New name for {_animController.CurrentAnimation.Name}");
                    Parent.AddWindow(newName);
                }

                ImGui.PopID();

                AnimationNode cur = _animController.CurrentAnimation;
                var modified = false;

                int frameTime = cur.TimeBetweenFrames;
                ImGui.InputInt("MS Between Frames", ref frameTime);
                if (frameTime != cur.TimeBetweenFrames)
                {
                    cur.TimeBetweenFrames = frameTime;
                    modified = true;
                }

                int startingFrame = cur.StartingFrame;
                ImGui.InputInt("Starting Frame", ref startingFrame);
                if (startingFrame != cur.StartingFrame)
                {
                    cur.StartingFrame = startingFrame;
                    modified = true;
                }

                int endingFrame = cur.EndingFrame;
                ImGui.InputInt("Ending Frame", ref endingFrame);
                if (endingFrame != cur.EndingFrame)
                {
                    cur.EndingFrame = endingFrame;
                    modified = true;
                }

                var loopType = (int) cur.LoopType;
                ImGui.Combo("Loop Type", ref loopType, string.Join('\0', Enum.GetNames(typeof(AnimationLoopType))));
                if ((AnimationLoopType) loopType != cur.LoopType)
                {
                    cur.LoopType = (AnimationLoopType) loopType;
                    modified = true;
                }

                if (modified) _animController.SetAnimation(cur.Name);
            }
        }

        public override void Update()
        {
            if (_playing)
                _animation?.Update(Engine.DeltaTime);
        }

        #region IO

        private void LoadSpriteSheetFile(TextureAsset f)
        {
            if (f?.Texture == null) return;
            _spriteSheetTexture?.Dispose();
            _spriteSheetTexture = f;

            _animation = null;
            _animController = null;
        }

        private void LoadAnimatedTexture(XMLAsset<AnimatedTexture> f)
        {
            if (f?.Content == null) return;

            AnimatedTexture anim = f.Content;
            _spriteSheetTexture = anim.TextureAsset;
            _animation = anim;
            _animController = null;
            _saveName = f.Name;
        }

        private void LoadAnimationController(XMLAsset<AnimationController> f)
        {
            if (f?.Content == null) return;
            _animController = f.Content;
            _spriteSheetTexture = _animController.AnimTex.TextureAsset;
            _animation = _animController.AnimTex;
            _saveName = f.Name;
        }

        #endregion

        #region Helpers

        private static unsafe Rectangle[] AutoDetectFrames(Texture tex)
        {
            var pixels = new byte[(int) (tex.Size.X * tex.Size.Y * 4)];

            fixed (void* p = &pixels[0])
            {
                Texture.EnsureBound(tex.Pointer);
                Gl.GetTexImage(TextureTarget.Texture2d, 0, PixelFormat.Rgba, PixelType.UnsignedByte, new IntPtr(p));
            }

            // Convert to 1 bit.
            for (int i = 0, w = 0; i < pixels.Length; i += 4, w++)
            {
                pixels[w] = (byte) (pixels[i + 3] > 10 ? 1 : 0);
            }

            Array.Resize(ref pixels, pixels.Length / 4);

            var boxes = new List<Rectangle>();

            // First pass - identify box start positions.
            for (var y = 0; y < tex.Size.Y; y++)
            {
                for (var x = 0; x < tex.Size.X; x++)
                {
                    byte current = pixels[(int) (y * tex.Size.X + x)];

                    // Check if the current one is filled.
                    if (current != 1) continue;
                    // Start a box.
                    var start = new Vector2(x, y);
                    var size = new Vector2();
                    var width = 0;
                    // Find the next non full. This is the width.
                    for (int yy = y; yy < tex.Size.Y; yy++)
                    {
                        for (int xx = x; xx < tex.Size.X; xx++)
                        {
                            byte curLook = pixels[(int) (yy * tex.Size.X + xx)];
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
                    for (int yy = y; yy < tex.Size.Y; yy++)
                    {
                        byte curLook = pixels[(int) (yy * tex.Size.X + x)];
                        if (curLook == 0) break;
                        heightLeft++;
                    }

                    // Now go down from the end until we find a non-full.
                    var heightRight = 0;
                    for (int yy = y; yy < tex.Size.Y; yy++)
                    {
                        byte curLook = pixels[(int) (yy * tex.Size.X + (x + size.X - 1))];
                        if (curLook == 0) break;
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

                    if (!boxes[i].IntersectsInclusive(boxes[j])) continue;
                    boxes[i] = Rectangle.Union(boxes[i], boxes[j]);
                    boxes.RemoveAt(j);
                    goto restart;
                }
            }

            return boxes.OrderBy(x => Math.Round((x.Y + x.Height / 2) / 100f)).ThenBy(x => Math.Round((x.X + x.Width / 2) / 100f)).ToArray();
        }

        #endregion
    }
}