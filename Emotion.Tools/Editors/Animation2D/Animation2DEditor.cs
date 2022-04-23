#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.Animation2D;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet;
using Emotion.Primitives;
using Emotion.Tools.DevUI;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.UI;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Editors.Animation2D
{
    public class Animation2DEditor : PresetGenericEditorWindow<SpriteAnimatorData>
    {
        private Texture _currentAssetTexture;
        private FrameBuffer _textureFb;
        private FontAsset _debugFont;
        private bool _showFrameIdx = true;
        private int _zoomLevel = 1;
        private int _selectedFrame = -1;
        private string _selectedAnimation = "Default";
        private SpriteAnimator _controller;

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

            SpriteAnimatorData currentFileContext = _currentAsset.Content!;
            ImGui.BeginChild("Info", new Vector2(halfScreenWidth, -1), true, ImGuiWindowFlags.HorizontalScrollbar);

            if (currentFileContext.AssetFile == null)
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "No spritesheet texture set!");
                if (ImGui.Button("Set Grid Texture")) OpenTexture(true, currentFileContext);
                ImGui.SameLine();
                if (ImGui.Button("Set Unordered Texture")) OpenTexture(false, currentFileContext);
            }
            else
            {
                ImGui.Text($"Texture: {currentFileContext.AssetFile ?? "None"}");

                if (currentFileContext.Animations != null)
                {
                    ImGui.Text("Animations:");
                    ImGui.BeginChild("Animations", new Vector2(-1, 200), true);
                    Dictionary<string, SpriteAnimationData> anims = currentFileContext.Animations;
                    string removeKey = null;
                    foreach ((string animId, SpriteAnimationData _) in anims)
                    {
                        if (ImGui.Button(animId))
                        {
                            _selectedAnimation = animId;
                            _controller.SetAnimation(_selectedAnimation);
                        }

                        ImGui.SameLine();
                        ImGui.PushID(animId);
                        if (anims.Count != 1)
                        {
                            if (ImGui.SmallButton("Remove")) removeKey = animId;
                            ImGui.SameLine();
                        }

                        if (ImGui.SmallButton("Rename"))
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
                                    anims.Add(_selectedAnimation, selNode);
                                },
                                $"New name for {_selectedAnimation}");
                            _toolsRoot.AddChild(newName);
                        }

                        ImGui.PopID();
                    }

                    if (removeKey != null)
                    {
                        anims.Remove(removeKey);
                        _selectedAnimation = anims.First().Key;
                        _controller.SetAnimation(_selectedAnimation);
                    }

                    ImGui.EndChild();

                    bool enableShortcuts = !ImGui.IsAnyItemActive();
                    bool createNew = ImGui.Button("(C)reate") || enableShortcuts && Engine.Host.IsKeyDown(Key.C);
                    if (createNew)
                    {
                        var newName = "NewAnim";
                        string newNameCur = newName;
                        var loops = 1;
                        while (anims.ContainsKey(newNameCur))
                        {
                            newNameCur = $"{newName} ({loops})";
                            loops++;
                        }

                        var newAnim = new SpriteAnimationData(newNameCur);
                        anims.Add(newNameCur, newAnim);
                        _selectedAnimation = newNameCur;
                        _controller.SetAnimation(_selectedAnimation);
                    }


                    if (_selectedAnimation != null && anims.ContainsKey(_selectedAnimation))
                    {
                        var selAnim = anims[_selectedAnimation];
                    }
                }
            }

            ImGui.EndChild();
            ImGui.SameLine();

            ImGui.BeginChild("Preview", new Vector2(halfScreenWidth, -1), true);
            ImGui.BeginTabBar("TabBar");

            if (ImGui.BeginTabItem("Texture"))
            {
                ISpriteAnimationFrameSource frameSource = currentFileContext.FrameSource;
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

                    if (arraySource != null)
                    {
                        Vector2 mPos = ImGui.GetMousePos();
                        mPos -= winPos;
                        if (ImGui.IsWindowFocused() && Engine.Host.IsKeyDown(Key.MouseKeyRight))
                        {
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
                                }
                            }
                        }
                    }
                }

                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.EndChild();

            //ImGui.BeginGroup();
            //ImGui.BeginTabBar("TabBar");
            //ImGui.EndTabBar();
            //ImGui.EndGroup();
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
                SpriteAnimatorData currentFileContext = _currentAsset.Content!;
                ISpriteAnimationFrameSource frameSource = currentFileContext.FrameSource;
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

            return true;
        }

        #region IO

        protected override bool OnFileLoaded(XMLAsset<SpriteAnimatorData> file)
        {
            _zoomLevel = 1;
            _selectedFrame = -1;
            _selectedAnimation = file.Content.Animations.First().Key;

            var assetFileLoaded = Engine.AssetLoader.Get<TextureAsset>(file.Content.AssetFile, false);
            _currentAssetTexture = assetFileLoaded?.Texture ?? null;
            _controller = new SpriteAnimator(file.Content);
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

        protected override bool OnFileSaving()
        {
            return true;
        }

        #endregion

        private void OpenTexture(bool gridMode, SpriteAnimatorData data)
        {
            var explorer = new FileExplorer<TextureAsset>(f =>
            {
                if (f.Texture == null) return;
                ISpriteAnimationFrameSource source;
                if (gridMode)
                    source = new SpriteGridFrameSource();
                else
                    source = new SpriteArrayFrameSource(f.Texture);

                data.AssetFile = f.Name;
                data.FrameSource = source;

                data.Animations = new Dictionary<string, SpriteAnimationData>();
                var defaultAnimation = new SpriteAnimationData("Default");
                data.Animations.Add("Default", defaultAnimation);
                _currentAssetTexture = f.Texture;
                _controller = new SpriteAnimator(data);

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
    }
}