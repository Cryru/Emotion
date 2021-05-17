#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Emotion.Common;
using Emotion.Game;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.XML;
using Emotion.Tools.Windows.AnimationEditorWindows;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.Utility;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Tools.Windows
{
    public class AnimationEditor : ImGuiWindow
    {
        public int Scale = 1;
        private bool _playing = true;
        private AnimationNode _overlayAnimation;

        private string _saveName = "";

        // Standard only.
        private Vector2 _frameSize = Vector2.Zero;
        private Vector2 _spacing = Vector2.Zero;

        // Lookup only.
        private AnchorPlacingWindow _anchorPlacerWindow;
        private FrameOrderWindow _orderWindow;
        public bool Mirrored;

        // Files
        private TextureAsset _spriteSheetTexture;
        public AnimatedTexture Animation;
        public AnimationController AnimController;

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
            if (Animation == null)
            {
                ImGui.Text("How are the frames contained in your spritesheet?");

                if (ImGui.Button("Grid"))
                {
                    var win = new AnimationCreateFrom(this, (fs, s) => { Animation = new AnimatedTexture(_spriteSheetTexture, fs, s, AnimationLoopType.Normal, 1000); },
                        (r, c) => { Animation = new AnimatedTexture(_spriteSheetTexture, c, r, AnimationLoopType.Normal, 1000); });
                    Parent.AddWindow(win);
                }

                ImGui.SameLine();

                if (ImGui.Button("Auto Detect Frames"))
                {
                    Rectangle[] frames = AutoDetectFrames(_spriteSheetTexture.Texture);
                    Animation = new AnimatedTexture(_spriteSheetTexture, frames, AnimationLoopType.Normal, 1000);
                }

                return;
            }

            ImGui.Text($"Current File: {_saveName ?? "None"}");
            ImGui.Text($"Texture File: {_spriteSheetTexture.Name} / Resolution: {_spriteSheetTexture.Texture.Size}");

            if (ImGui.Button("Reload Image")) LoadSpriteSheetFile(FileExplorer<TextureAsset>.ExplorerLoadAsset(_spriteSheetTexture.Name));
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
                ImGui.SameLine();
                if (ImGui.Button("<"))
                    Animation.ForceSetFrame(Animation.GetPrevFrameIdx(out bool _, out bool reverse), reverse);
                ImGui.SameLine();
                if (ImGui.Button(">"))
                    Animation.ForceSetFrame(Animation.GetNextFrameIdx(out bool _, out bool reverse), reverse);
            }

            ImGui.Text($"Display Scale: {Scale}");
            ImGui.SameLine();
            if (ImGui.Button("-")) Scale -= 1;
            ImGui.SameLine();
            if (ImGui.Button("+")) Scale += 1;
            ImGui.SameLine();
            if (ImGui.Button($"Mirror (Currently: {(Mirrored ? "Mirrored" : "Not Mirrored")})")) Mirrored = !Mirrored;

            if (ImGui.Button("Place Anchor Points"))
                if (_anchorPlacerWindow == null || !_anchorPlacerWindow.Open)
                    Parent.AddWindow(_anchorPlacerWindow = new AnchorPlacingWindow(this, Animation));

            ImGui.SameLine();
            if (ImGui.Button("Order Frames"))
                if (_orderWindow == null || !_orderWindow.Open)
                    Parent.AddWindow(_orderWindow = new FrameOrderWindow(this, Animation));

            ImGui.SameLine();
            if (ImGui.Button("Redetect Frames"))
            {
                Rectangle[] previousFrames = Animation.Frames;
                Rectangle[] frames = AutoDetectFrames(_spriteSheetTexture.Texture);

                // Try to maintain the old order.
                for (var i = 0; i < frames.Length; i++)
                {
                    for (var old = 0; old < previousFrames.Length; old++)
                    {
                        if (frames[i] != previousFrames[old] || i == old || old >= frames.Length - 1) continue;
                        Rectangle temp = frames[i];
                        frames[i] = frames[old];
                        frames[old] = temp;
                        break;
                    }
                }

                Animation.Frames = frames;
            }

            ImGui.Text($"Current Frame: {Animation.CurrentFrameIndex - Animation.StartingFrame + 1}/{Animation.AnimationFrames + 1} (Total: {Animation.TotalFrames})");
            ImGui.Text($"Current Anchor: {(Animation.Anchors.Length > 0 ? Animation.Anchors[Animation.CurrentFrameIndex].ToString() : "Unknown")}");

            for (var i = 0; i <= Animation.TotalFrames; i++)
            {
                if (i != 0 && i % 10 != 0) ImGui.SameLine(0, 5);

                bool current = Animation.CurrentFrameIndex == i;

                Rectangle frameBounds = Animation.GetFrameBounds(i);
                (Vector2 u1, Vector2 u2) = Animation.Texture.GetImGuiUV(frameBounds);

                ImGui.Image(new IntPtr(Animation.Texture.Pointer), frameBounds.Size / 2f, u1, u2, Vector4.One,
                    current ? new Vector4(1, 0, 0, 1) : new Vector4(0, 0, 0, 1));
            }

            RenderCurrentAnimationSettings();

            RenderSaveSection(composer);
            RenderAnimation(composer);
        }

        private void RenderSaveSection(RenderComposer composer)
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
                if (ImGui.Button("SaveToFile"))
                {
                    string saveName = _saveName.ToLower();
                    if (!saveName.Contains(".anim")) saveName += ".anim";

                    // Fixups
                    if (AnimController?.MirrorXAnchors != null)
                    {
                        var emptyMirrorAnchors = true;
                        for (var i = 0; i < AnimController.MirrorXAnchors.Length; i++)
                        {
                            if (AnimController.MirrorXAnchors[i] == Vector2.Zero) continue;
                            emptyMirrorAnchors = false;
                            break;
                        }

                        if (emptyMirrorAnchors) AnimController.MirrorXAnchors = null;
                    }

                    try
                    {
                        string saveData;
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (AnimController != null)
                            saveData = XMLFormat.To(AnimController);
                        else
                            saveData = XMLFormat.To(Animation);

                        Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(saveData), saveName);
                    }
                    catch (Exception ex)
                    {
                        Engine.Log.Error(ex);
                    }
                }

                if (ImGui.Button("Save Packed Texture"))
                {
                    string saveName = _saveName.ToLower();
                    if (!saveName.Contains(".png")) saveName += ".png";
                    Rectangle[] frames = AnimController != null ? AnimController.AnimTex.Frames : Animation.Frames;
                    var preBinnedFrames = new Rectangle[frames.Length];
                    Array.Copy(frames, preBinnedFrames, frames.Length);
                    Texture spriteSheetTexture = Animation.Texture;

                    var spacing = 2;
                    for (var i = 0; i < frames.Length; i++)
                    {
                        Rectangle frame = frames[i];
                        frames[i] = frame.Inflate(spacing, spacing);
                    }

                    Vector2 totalSize = Binning.FitRectangles(frames, true);
                    FrameBuffer texture = new FrameBuffer(totalSize).WithColor();
                    composer.RenderTo(texture);
                    for (var i = 0; i < frames.Length; i++)
                    {
                        composer.RenderSprite(frames[i].Deflate(spacing, spacing), Color.White, spriteSheetTexture, preBinnedFrames[i]);
                    }

                    composer.RenderTo(null);

                    byte[] pixelsDownload = texture.Sample(new Rectangle(0, 0, totalSize), PixelFormat.Rgba);
                    ImageUtil.FlipImageY(pixelsDownload, (int) totalSize.Y);
                    byte[] pngFile = PngFormat.Encode(pixelsDownload, totalSize, PixelFormat.Rgba);
                    Engine.AssetLoader.Save(pngFile, saveName);
                }
            }
        }

        private void RenderAnimation(RenderComposer composer)
        {
            if (_overlayAnimation != null)
            {
                Rectangle uv = Animation.Frames[_overlayAnimation.StartingFrame];
                Vector2 anchor = Animation.Anchors[_overlayAnimation.StartingFrame];
                composer.RenderSprite(new Vector3(new Vector2(100, 100) + anchor * Scale, 1), uv.Size * Scale, Color.Red, Animation.Texture, uv);
            }

            var offset = new Vector2(100, 100);

            // Animation has a controller.
            if (AnimController != null)
            {
                AnimController.GetCurrentFrameData(out Texture texture, out Rectangle uv, out Vector2 anchor, Mirrored);
                composer.RenderSprite(new Vector3(offset + anchor * Scale, 1), uv.Size * Scale, Color.White, texture, uv, Mirrored);
                return;
            }

            // Animation is just a texture.
            if (Animation.Anchors.Length > 0) offset += Animation.Anchors[Animation.CurrentFrameIndex] * Scale;
            composer.RenderSprite(new Vector3(offset, 1), Animation.CurrentFrame.Size * Scale, Color.White, Animation.Texture, Animation.CurrentFrame, Mirrored);
        }

        private void RenderCurrentAnimationSettings()
        {
            if (AnimController == null)
            {
                if (ImGui.Button("Create Controller"))
                {
                    AnimController = new AnimationController(Animation);
                    AnimController.AddAnimation(new AnimationNode("Default")
                    {
                        EndingFrame = Animation.EndingFrame,
                        StartingFrame = Animation.StartingFrame,
                        LoopType = Animation.LoopType
                    });
                }

                int frameTime = Animation.TimeBetweenFrames;
                if (ImGui.InputInt("MS Between Frames", ref frameTime))
                {
                    Animation.TimeBetweenFrames = frameTime;
                    Animation.Reset();
                }

                ImGui.Text("Starting and ending frames are 0 indexed and inclusive.");

                int startingFrame = Animation.StartingFrame;
                if (ImGui.InputInt("Starting Frame", ref startingFrame))
                {
                    Animation.StartingFrame = startingFrame;
                    Animation.Reset();
                }

                int endingFrame = Animation.EndingFrame;
                if (ImGui.InputInt("Ending Frame", ref endingFrame))
                {
                    Animation.EndingFrame = endingFrame;
                    Animation.Reset();
                }

                var loopType = (int) Animation.LoopType;
                if (ImGui.Combo("Loop Type", ref loopType, string.Join('\0', Enum.GetNames(typeof(AnimationLoopType)))))
                {
                    Animation.LoopType = (AnimationLoopType) loopType;
                    Animation.Reset();
                }
            }
            else
            {
                ImGui.Text("Animations");
                ImGui.PushID("animList");
                foreach (AnimationNode n in AnimController.Animations.Values)
                {
                    bool activeOverlay = _overlayAnimation == n;
                    ImGui.PushID(n.Name);
                    if (n == AnimController.CurrentAnimation)
                    {
                        ImGui.Text(n.Name);
                    }
                    else
                    {
                        if (ImGui.Button(n.Name)) AnimController.SetAnimation(n.Name);
                    }

                    ImGui.SameLine();
                    ImGui.Text($"({n.StartingFrame}-{n.EndingFrame})");
                    ImGui.SameLine();
                    if (ImGui.Checkbox("Overlay", ref activeOverlay)) _overlayAnimation = activeOverlay ? n : null;
                    ImGui.PopID();
                }

                bool enableShortcuts = !ImGui.IsAnyItemActive();
                if (ImGui.Button("(C)reate") || enableShortcuts && Engine.Host.IsKeyDown(Key.C) && AnimController.Animations.All(x => x.Key != "NewAnim"))
                {
                    var newNode = new AnimationNode("NewAnim");
                    AnimController.AddAnimation(newNode);
                    AnimController.SetAnimation(newNode.Name);
                }

                ImGui.SameLine();
                if (AnimController.CurrentAnimation == null)
                {
                    ImGui.PopID();
                    return;
                }

                if (ImGui.Button("Remove")) AnimController.RemoveAnimation(AnimController.CurrentAnimation.Name);
                ImGui.SameLine();
                if (ImGui.Button("(R)ename") || enableShortcuts && Engine.Host.IsKeyDown(Key.R))
                {
                    var newName = new StringInputModal(s =>
                        {
                            AnimController.CurrentAnimation.Name = s;
                            AnimController.Reindex();
                        },
                        $"New name for {AnimController.CurrentAnimation.Name}");
                    Parent.AddWindow(newName);
                }

                ImGui.PopID();

                AnimationNode cur = AnimController.CurrentAnimation;
                var modified = false;

                int frameTime = cur.TimeBetweenFrames;
                if (ImGui.InputInt("MS Between Frames", ref frameTime))
                {
                    cur.TimeBetweenFrames = frameTime;
                    modified = true;
                }

                ImGui.Text("Starting and ending frames are 0 indexed and inclusive.");

                int startingFrame = cur.StartingFrame;
                if (ImGui.InputInt("Starting Frame", ref startingFrame))
                {
                    cur.StartingFrame = startingFrame;
                    modified = true;
                }

                int endingFrame = cur.EndingFrame;
                if (ImGui.InputInt("Ending Frame", ref endingFrame))
                {
                    cur.EndingFrame = endingFrame;
                    modified = true;
                }

                var loopType = (int) cur.LoopType;
                if (ImGui.Combo("Loop Type", ref loopType, string.Join('\0', Enum.GetNames(typeof(AnimationLoopType)))))
                {
                    cur.LoopType = (AnimationLoopType) loopType;
                    modified = true;
                }

                if (modified) AnimController.SetAnimation(cur.Name, true);
            }
        }

        public override void Update()
        {
            if (_playing)
                Animation?.Update(Engine.DeltaTime);
        }

        #region IO

        private void LoadSpriteSheetFile(TextureAsset f)
        {
            if (f?.Texture == null) return;
            _spriteSheetTexture?.Dispose();
            _spriteSheetTexture = f;

            Animation = null;
            AnimController = null;
            _saveName = "";
        }

        private void LoadAnimatedTexture(XMLAsset<AnimatedTexture> f)
        {
            if (f?.Content == null) return;

            AnimatedTexture anim = f.Content;
            _spriteSheetTexture = anim.TextureAsset;
            Animation = anim;
            AnimController = null;
            _saveName = f.Name;
        }

        private void LoadAnimationController(XMLAsset<AnimationController> f)
        {
            if (f?.Content == null) return;
            AnimController = f.Content;
            _spriteSheetTexture = AnimController.AnimTex.TextureAsset;
            Animation = AnimController.AnimTex;
            _saveName = f.Name;
            AnimController.SetAnimation(AnimController.Animations.First().Key);
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

            // Convert to A1 from BGRA8.
            for (int i = 0, w = 0; i < pixels.Length; i += 4, w++)
            {
                if (pixels[i + 3] > 10)
                    pixels[w] = 1;
                else
                    pixels[w] = 0;
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

            return boxes.Where(x => x.Width > 1 && x.Height > 1).OrderBy(x => Math.Round((x.Y + x.Height / 2) / 100f)).ThenBy(x => Math.Round((x.X + x.Width / 2) / 100f)).ToArray();
        }

        #endregion
    }
}