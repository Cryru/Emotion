#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.Animation;
using Emotion.Game.Animation2D;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet;
using Emotion.Primitives;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.UI;
using Emotion.Utility;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Editors.Animation2D
{
    public class Animation2DEditor : PresetGenericEditorWindow<AnimatedSprite>
    {
        private Texture _currentAssetTexture;
        private FrameBuffer _textureFb;
        private FontAsset _debugFont;

        private FrameBuffer _animatedPreviewFb;
        private bool _animatedPreviewInvalidated = true;
        private bool _animatedPreviewAnchorMode;

        private SpriteAnimationController _controller;
        private bool _playing = true;

        private bool _showFrameIdx = true;
        private int _zoomLevel = 1;

        // Anim editor.
        private int _selectedFrame = -1;
        private string _selectedAnimation = "Default";
        private int _draggedFrame = -1;
        private int _addFrameInput;

        // Anchor editor.
        private int _frameAnchor;

        public Animation2DEditor() : base("Animation 2D Editor")
        {
            _debugFont = Engine.AssetLoader.Get<FontAsset>("debugFont.otf");
        }

        public override void DetachedFromController(UIController controller)
        {
            if (_textureFb != null) GLThread.ExecuteGLThreadAsync(_textureFb.Dispose);
            base.DetachedFromController(controller);
        }

        protected override void RenderImGui()
        {
            base.RenderImGui();
            float halfScreenWidth = Engine.Renderer.CurrentTarget.Size.X / 2f - 10; // Remove space between the two panels.

            if (_currentAsset == null)
            {
                ImGui.Text("No file loaded. Click 'New' to start!");
                return;
            }

            AnimatedSprite currentFileContext = _currentAsset.Content!;
            ImGui.BeginChild("Left", new Vector2(halfScreenWidth, -1), true, ImGuiWindowFlags.HorizontalScrollbar);

            if (currentFileContext.AssetFile == null)
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "No spritesheet texture set!");
                if (ImGui.Button("Set Grid Texture")) CreateNewFromTexture(true, currentFileContext);
                ImGui.SameLine();
                if (ImGui.Button("Set Unordered Texture")) CreateNewFromTexture(false, currentFileContext);
            }
            else
            {
                ImGui.Text($"Texture: {currentFileContext.AssetFile ?? "None"}");

                if (currentFileContext.Animations != null)
                {
                    ImGui.Text("Animations:");
                    ImGui.BeginChild("Animations", new Vector2(-1, 200), true, ImGuiWindowFlags.NoScrollbar);

                    Dictionary<string, SpriteAnimationData> anims = currentFileContext.Animations;
                    AnimationsList(anims);
                    AnimationControlButtons(anims);

                    if (_selectedAnimation != null && anims.ContainsKey(_selectedAnimation))
                    {
                        SpriteAnimationData selAnim = anims[_selectedAnimation];
                        ImGui.Text($"{selAnim.Name}:");
                        AnimationFrameEditor(anims);
                    }

                    ImGui.EndChild();
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("Right", new Vector2(halfScreenWidth, -1), true);
            ImGui.BeginTabBar("TabBar");

            if (ImGui.BeginTabItem("Frame Order"))
            {
                FrameOrderTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Frame Anchors"))
            {
                FrameAnchorTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Animation Preview"))
            {
                AnimationPreviewTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.EndChild();

            //ImGui.BeginGroup();
            //ImGui.BeginTabBar("TabBar");
            //ImGui.EndTabBar();
            //ImGui.EndGroup();
        }

        protected override bool UpdateInternal()
        {
            if (_playing && _controller != null) _controller.Update(Engine.DeltaTime);
            return base.UpdateInternal();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            var open = true;
            ImGui.SetNextWindowPos(new Vector2(0, 20), ImGuiCond.Always);
            ImGui.SetNextWindowSize(c.CurrentTarget.Size - new Vector2(0, 20));
            ImGui.Begin(Title, ref open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);

            RenderImGui();

            ImGui.End();

            Position = Vector3.Zero;
            Size = c.CurrentTarget.Size;
            if (!open)
            {
                Parent?.RemoveChild(this);
                return false;
            }

            if (_textureFb?.ColorAttachment == null) return true; // Disposed or uninitialized fb
            if (_currentAssetTexture == null) return true;

            _textureFb.ColorAttachment.Smooth = false;
            c.RenderToAndClear(_textureFb);
            c.RenderSprite(Vector3.Zero, _currentAssetTexture.Size, _currentAssetTexture);

            // Render meta overlay on the spritesheet texture.
            if (_currentAsset != null)
            {
                AnimatedSprite currentFileContext = _currentAsset.Content!;
                SpriteAnimationFrameSource frameSource = currentFileContext.FrameSource;
                if (frameSource != null)
                    for (var i = 0; i < frameSource.GetFrameCount(); i++)
                    {
                        Rectangle frameUv = frameSource.GetFrameUV(i);
                        c.RenderOutline(frameUv, _selectedFrame == i ? Color.Green : Color.Red);

                        if (_showFrameIdx && frameSource is SpriteArrayFrameSource)
                        {
                            Vector3 stringPos = frameUv.Position.ToVec3();
                            DrawableFontAtlas atlas = _debugFont.GetAtlas(20);
                            c.RenderString(stringPos + new Vector3(1), Color.Black, i.ToString(), atlas);
                            c.RenderString(stringPos, Color.Red, i.ToString(), atlas);
                        }
                    }
            }

            c.RenderTo(null);

            RenderAnimationPreview(c);

            return true;
        }

        #region IO

        protected void ResetEditors()
        {
            _zoomLevel = 1;
            _selectedFrame = -1;
            _frameAnchor = 0;
            _draggedFrame = -1;
            _addFrameInput = 0;
            _animatedPreviewInvalidated = true;
        }

        protected override bool OnFileLoaded(XMLAsset<AnimatedSprite> file)
        {
            ResetEditors();
            _selectedAnimation = file.Content.Animations.First().Key;

            var assetFileLoaded = Engine.AssetLoader.Get<TextureAsset>(file.Content.AssetFile, false);
            _currentAssetTexture = assetFileLoaded?.Texture;
            _controller = new SpriteAnimationController(file.Content);
            _controller.SetAnimation(_selectedAnimation);

            GLThread.ExecuteGLThreadAsync(() =>
            {
                if (_textureFb == null)
                    _textureFb = new FrameBuffer(_currentAssetTexture.Size).WithColor();
                else
                    _textureFb.Resize(_currentAssetTexture.Size);
            });

            return true;
        }

        private void CreateNewFromTexture(bool gridMode, AnimatedSprite data)
        {
            var explorer = new FileExplorer<TextureAsset>(f =>
            {
                if (f.Texture == null) return;

                SpriteAnimationFrameSource source;
                if (gridMode)
                    source = new SpriteGridFrameSource();
                else
                    source = new SpriteArrayFrameSource(f.Texture);

                data.AssetFile = f.Name;
                data.FrameSource = source;
                data.Animations = new Dictionary<string, SpriteAnimationData>
                {
                    {"Default", new SpriteAnimationData("Default", 0)}
                };

                _currentAssetTexture = f.Texture;
                _controller = new SpriteAnimationController(data);

                ResetEditors();
                _selectedAnimation = "Default";

                GLThread.ExecuteGLThreadAsync(() =>
                {
                    if (_textureFb == null)
                        _textureFb = new FrameBuffer(_currentAssetTexture.Size).WithColor();
                    else
                        _textureFb.Resize(_currentAssetTexture.Size);
                });
            });
            _toolsRoot.AddLegacyWindow(explorer);
        }

        protected override bool OnFileSaving()
        {
            return true;
        }

        #endregion

        #region Editor Parts

        private void AnimationsList(Dictionary<string, SpriteAnimationData> anims)
        {
            ImGui.BeginChild("animScroll", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), false);
            foreach ((string animId, SpriteAnimationData anim) in anims)
            {
                if (_selectedAnimation == animId)
                {
                    EditorHelpers.SelectedButtonTextOnly(animId);
                }
                else if (ImGui.Button(animId))
                {
                    _selectedAnimation = animId;
                    _controller.SetAnimation(_selectedAnimation);
                }

                ImGui.SameLine();
                ImGui.Text($"{anim.FrameIndices.Length} Frames ({anim.TimeBetweenFrames * anim.FrameIndices.Length} Duration)");
            }

            ImGui.EndChild();
        }

        private void AnimationControlButtons(Dictionary<string, SpriteAnimationData> anims)
        {
            Vector2 winSize = ImGui.GetWindowSize();
            ImGui.SetCursorPosY(winSize.Y - ImGui.GetFrameHeightWithSpacing() - (ImGui.GetTextLineHeightWithSpacing() - ImGui.GetTextLineHeight()));
            if (ImGui.Button("Add Animation"))
            {
                const string newName = "NewAnim";
                string newNameCur = newName;
                var loops = 1;
                while (anims.ContainsKey(newNameCur))
                {
                    newNameCur = $"{newName} ({loops})";
                    loops++;
                }

                var newAnim = new SpriteAnimationData(newNameCur, 0);
                anims.Add(newNameCur, newAnim);
                _selectedAnimation = newNameCur;
                _controller.SetAnimation(_selectedAnimation);
            }

            ImGui.SameLine();
            if (ImGui.Button("Remove Selected") && anims.Count != 1)
            {
                anims.Remove(_selectedAnimation);
                _selectedAnimation = anims.First().Key;
                _controller.SetAnimation(_selectedAnimation);
                UnsavedChanges();
            }

            ImGui.SameLine();
            if (ImGui.Button("Rename Selected"))
            {
                var newName = new StringInputModalWindow(s =>
                    {
                        string ogS = s;
                        var loops = 1;
                        while (anims.ContainsKey(s))
                        {
                            s = $"{ogS} ({loops})";
                            loops++;
                        }

                        SpriteAnimationData selNode = anims[_selectedAnimation];
                        selNode.Name = s;
                        anims.Remove(_selectedAnimation);
                        _selectedAnimation = s;
                        anims.Add(s, selNode);
                        UnsavedChanges();
                    },
                    $"New name for {_selectedAnimation}");
                _toolsRoot.AddChild(newName);
            }

            ImGui.EndChild();
        }

        private void AnimationFrameEditor(Dictionary<string, SpriteAnimationData> anims)
        {
            AnimatedSprite currentFileContext = _currentAsset!.Content!;
            SpriteAnimationData selAnim = anims[_selectedAnimation];

            ImGui.BeginChild("Animation", new Vector2(-1, -1), true);
            ImGui.Text("Frames: (Drag and Drop to rearrange)");

            SpriteAnimationFrameSource frameSource = currentFileContext.FrameSource;
            _draggedFrame = EditorHelpers.DragAndDropList(selAnim.FrameIndices, _draggedFrame, true);
            if (selAnim.FrameIndices.Length == 0) EditorHelpers.ButtonSizedHole("");

            ImGui.PushItemWidth(150);
            if (ImGui.InputInt("", ref _addFrameInput)) _addFrameInput = Maths.Clamp(_addFrameInput, 0, frameSource.GetFrameCount() - 1);
            ImGui.SameLine();
            if (ImGui.Button("Add Frame"))
            {
                selAnim.FrameIndices = selAnim.FrameIndices.AddToArray(_addFrameInput);
                if (_addFrameInput < frameSource.GetFrameCount() - 2) _addFrameInput++;
                _controller.Reset();
                UnsavedChanges();
            }

            ImGui.SameLine();
            ImGui.Button("Remove (Drop Here)");
            if (ImGui.BeginDragDropTarget())
            {
                ImGuiPayloadPtr dataPtr = ImGui.AcceptDragDropPayload("UNUSED");
                unsafe
                {
                    if ((IntPtr) dataPtr.NativePtr != IntPtr.Zero && dataPtr.IsDelivery())
                    {
                        int[] indices = selAnim.FrameIndices;
                        for (int i = _draggedFrame; i < indices.Length - 1; i++)
                        {
                            indices[i] = indices[i + 1];
                        }

                        Array.Resize(ref selAnim.FrameIndices, selAnim.FrameIndices.Length - 1);
                        _controller.Reset();
                        _draggedFrame = -1;
                        UnsavedChanges();
                    }
                }

                ImGui.EndDragDropTarget();
            }

            ImGui.PushItemWidth(150);
            ImGui.Text("Duration");
            ImGui.SameLine();
            if (ImGui.InputInt("###DurInput", ref selAnim.TimeBetweenFrames))
            {
                _controller.Reset();
                UnsavedChanges();
            }

            ImGui.SameLine();
            if (selAnim.FrameIndices.Length > 0) ImGui.Text($"Total Duration: {selAnim.TimeBetweenFrames * selAnim.FrameIndices.Length}");

            var loopType = (int) selAnim.LoopType;
            if (ImGui.Combo("Loop Type", ref loopType, string.Join('\0', Enum.GetNames(typeof(AnimationLoopType)))))
            {
                selAnim.LoopType = (AnimationLoopType) loopType;
                _controller.Reset();
                UnsavedChanges();
            }
        }

        private void FrameOrderTab()
        {
            AnimatedSprite currentFileContext = _currentAsset!.Content!;
            SpriteAnimationFrameSource frameSource = currentFileContext.FrameSource;
            var arraySource = frameSource as SpriteArrayFrameSource;

            if (frameSource != null)
            {
                ImGui.Text($"Frames: {currentFileContext.FrameSource.GetFrameCount()}");
                ImGui.SameLine();
                if (arraySource != null) ImGui.Checkbox("Show Frame Indices", ref _showFrameIdx);
                ImGui.SameLine();
                if (ImGui.Button("Re-detect Frames"))
                {
                    currentFileContext.FrameSource = new SpriteArrayFrameSource(_currentAssetTexture);
                    UnsavedChanges();
                }
            }

            ImGui.InputInt("Zoom", ref _zoomLevel);
            ImGui.Text("Right-click on a frame to select it, and then on another frame to swap their positions.");

            ImGui.BeginChild("FramePreview", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar);

            // Array source being unordered allows for frames to be reordered.
            if (_textureFb != null && frameSource != null)
            {
                Vector2 winPos = ImGui.GetCursorScreenPos();

                FrameBufferTexture texture = _textureFb.ColorAttachment;
                (Vector2 uv1, Vector2 uv2) = texture.GetImGuiUV();
                ImGui.Image((IntPtr) texture.Pointer, texture.Size * _zoomLevel, uv1, uv2);

                if (arraySource != null && arraySource.Frames != null && ImGui.IsWindowFocused() && Engine.Host.IsKeyDown(Key.MouseKeyRight))
                {
                    Vector2 mPos = ImGui.GetMousePos();
                    mPos -= winPos;

                    int clickedOn = -1;
                    for (var i = 0; i < frameSource.GetFrameCount(); i++)
                    {
                        Rectangle uv = frameSource.GetFrameUV(i);
                        uv *= _zoomLevel;
                        if (!uv.Contains(mPos)) continue;
                        clickedOn = i;
                        break;
                    }

                    if (clickedOn != -1)
                    {
                        if (_selectedFrame == -1)
                        {
                            _selectedFrame = clickedOn;
                        }
                        else
                        {
                            Rectangle uvSelected = frameSource.GetFrameUV(_selectedFrame);
                            Rectangle uvNew = frameSource.GetFrameUV(clickedOn);
                            arraySource.Frames[_selectedFrame] = uvNew;
                            arraySource.Frames[clickedOn] = uvSelected;
                            _selectedFrame = -1;
                            UnsavedChanges();
                        }
                    }
                }
            }

            ImGui.EndChild();
        }

        private void AnimationPreviewTab()
        {
            _animatedPreviewAnchorMode = false;

            ImGui.InputInt("Zoom", ref _zoomLevel);
            ImGui.Text($"Frame {_controller.CurrentFrameIndex}/{_controller.CurrentAnimationData.FrameIndices.Length}");

            bool paused = !_playing;
            if (ImGui.Button("Play/Pause")) _playing = !_playing;
            if (paused)
            {
                ImGui.SameLine();
                if (ImGui.ArrowButton("Prev Frame", ImGuiDir.Left)) _controller.ForceNextFrame();
                ImGui.SameLine();
                if (ImGui.ArrowButton("Next Frame", ImGuiDir.Right)) _controller.ForcePrevFrame();
            }

            if (_animatedPreviewFb != null)
            {
                FrameBufferTexture texture = _animatedPreviewFb.ColorAttachment;
                (Vector2 uv1, Vector2 uv2) = texture.GetImGuiUV();
                ImGui.Image((IntPtr) texture.Pointer, texture.Size * _zoomLevel, uv1, uv2);
            }
        }

        private void FrameAnchorTab()
        {
            if (_currentAsset == null) return;
            _animatedPreviewAnchorMode = true;

            SpriteAnimationFrameSource frameSource = _currentAsset.Content.FrameSource;
            int frameCount = frameSource.GetFrameCount();

            // Ensure data is of correct length. It's possible for it to be malformed if frames were re-detected or
            // user did something wacky.
            frameSource.FrameOrigins ??= new OriginPosition[frameCount];
            if (frameSource.FrameOrigins.Length != frameCount) Array.Resize(ref frameSource.FrameOrigins, frameCount);

            frameSource.FrameOffsets ??= new Vector2[frameCount];
            if (frameSource.FrameOffsets.Length != frameCount) Array.Resize(ref frameSource.FrameOffsets, frameCount);

            ImGui.InputInt("Zoom", ref _zoomLevel);

            ImGui.Text($"Frame {_frameAnchor}/{frameCount}");
            ImGui.SameLine();
            if (ImGui.ArrowButton("PrevFrame", ImGuiDir.Left))
            {
                _frameAnchor--;
                if (_frameAnchor < 0) _frameAnchor = 0;
            }

            ImGui.SameLine();
            if (ImGui.ArrowButton("NextFrame", ImGuiDir.Right))
            {
                _frameAnchor++;
                if (_frameAnchor > frameCount - 1) _frameAnchor = frameCount - 1;
            }

            var anchorType = (int) frameSource.FrameOrigins[_frameAnchor];
            if (ImGui.Combo("Anchor Type", ref anchorType, string.Join('\0', Enum.GetNames(typeof(OriginPosition)))))
            {
                frameSource.FrameOrigins[_frameAnchor] = (OriginPosition) anchorType;
                _controller.Reset();
                UnsavedChanges();
            }

            ImGui.SameLine();
            if (ImGui.Button("Apply To All"))
            {
                for (var i = 0; i < frameSource.FrameOrigins.Length; i++)
                {
                    frameSource.FrameOrigins[i] = (OriginPosition) anchorType;
                }

                UnsavedChanges();
            }

            if (ImGui.InputFloat2("Additional Offset", ref frameSource.FrameOffsets[_frameAnchor])) UnsavedChanges();

            if (_animatedPreviewFb != null)
            {
                FrameBufferTexture texture = _animatedPreviewFb.ColorAttachment;
                (Vector2 uv1, Vector2 uv2) = texture.GetImGuiUV();
                ImGui.Image((IntPtr) texture.Pointer, texture.Size * _zoomLevel, uv1, uv2);
            }
        }

        private void RenderAnimationPreview(RenderComposer c)
        {
            AnimatedSprite currentFileContext = _currentAsset!.Content!;
            SpriteAnimationFrameSource frameSource = currentFileContext.FrameSource;

            if (_animatedPreviewInvalidated)
            {
                var size = new Vector2();
                for (var i = 0; i < frameSource.GetFrameCount(); i++)
                {
                    Rectangle frameUV = frameSource.GetFrameUV(i);

                    size.X = MathF.Max(size.X, frameUV.Width);
                    size.Y = MathF.Max(size.Y, frameUV.Height);
                }

                if (size.X > size.Y) size.Y = size.X;
                else if (size.Y > size.X) size.X = size.Y;

                size *= 2;

                GLThread.ExecuteGLThreadAsync(() =>
                {
                    if (_animatedPreviewFb == null)
                        _animatedPreviewFb = new FrameBuffer(size).WithColor();
                    else
                        _animatedPreviewFb.Resize(size, true);
                });

                _animatedPreviewInvalidated = false;
            }

            if (_animatedPreviewFb != null)
            {
                c.RenderToAndClear(_animatedPreviewFb);

                Vector2 size = _animatedPreviewFb.Size;
                c.RenderSprite(Vector3.Zero, size, new Color(32, 32, 32));
                c.RenderLine(new Vector2(0, size.Y / 2), new Vector2(size.X, size.Y / 2), Color.White * 0.2f);
                c.RenderLine(new Vector2(size.X / 2, 0), new Vector2(size.X / 2, size.Y), Color.White * 0.2f);

                if (_animatedPreviewAnchorMode)
                {
                    // Draw a shadow of the previous frame.
                    if (_frameAnchor != 0)
                    {
                        _controller.GetRenderDataForFrame(_frameAnchor - 1, out Vector3 renderPosSh, out Texture textureSh, out Rectangle uvSh);
                        renderPosSh = renderPosSh.RoundClosest();
                        c.RenderSprite((size / 2f).RoundClosest().ToVec3() + renderPosSh, uvSh.Size, Color.White * 0.3f, textureSh, uvSh);
                    }

                    _controller.GetRenderDataForFrame(_frameAnchor, out Vector3 renderPos, out Texture texture, out Rectangle uv);
                    renderPos = renderPos.RoundClosest();
                    c.RenderSprite((size / 2f).RoundClosest().ToVec3() + renderPos, uv.Size, Color.White, texture, uv);
                }
                else
                {
                    _controller.GetRenderData(out Vector3 renderPos, out Texture texture, out Rectangle uv);
                    renderPos = renderPos.RoundClosest();
                    c.RenderSprite((size / 2f).RoundClosest().ToVec3() + renderPos, uv.Size, Color.White, texture, uv);
                }

                c.RenderTo(null);
            }
        }

        #endregion
    }
}