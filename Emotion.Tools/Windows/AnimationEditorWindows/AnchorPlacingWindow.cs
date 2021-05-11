#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Utility;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.AnimationEditorWindows
{
    public class AnchorPlacingWindow : ImGuiWindow
    {
        private AnimationEditor _parent;
        private AnimatedTexture _anim;
        private int _anchorSettingFrame;

        // Interactive anchor tool.
        private bool _mouseDown;
        private Vector2 _mouseDownPos;
        private Vector2 _interactiveAnchorStart;

        public AnchorPlacingWindow(AnimationEditor parent, AnimatedTexture anim) : base("Anchor Placer")
        {
            _parent = parent;
            _anim = anim;

            // Ensure anchor array length is correct.
            // It's possible for more frames to be added after initial initialization.
            Vector2[] anchorArray = _anim.Anchors;
            if (anchorArray.Length <= _anim.TotalFrames)
            {
                Array.Resize(ref anchorArray, _anim.TotalFrames + 1);
                _anim.Anchors = anchorArray;
            }

            if (_parent.AnimController != null)
            {
                anchorArray = _parent.AnimController.MirrorXAnchors;
                if (anchorArray != null && anchorArray.Length <= _anim.TotalFrames)
                {
                    Array.Resize(ref anchorArray, _anim.TotalFrames + 1);
                    _parent.AnimController.MirrorXAnchors = anchorArray;
                }
            }
        }

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            Vector2[] anchorArray = _anim.Anchors;

            if (_parent.AnimController != null)
            {
                if (_parent.Mirrored)
                {
                    _parent.AnimController.MirrorXAnchors ??= new Vector2[_anim.Anchors.Length];
                    anchorArray = _parent.Mirrored ? _parent.AnimController.MirrorXAnchors : anchorArray;
                }

                if (_parent.Mirrored)
                {
                    if (ImGui.Button("Copy Non-Mirror Anchors"))
                        for (int i = _anim.StartingFrame; i <= _anim.EndingFrame; i++)
                        {
                            anchorArray[i] = _anim.Anchors[i];
                        }

                    ImGui.SameLine();
                    if (ImGui.Button("Copy Y Axis of Non-Mirror Anchors"))
                        for (int i = _anim.StartingFrame; i <= _anim.EndingFrame; i++)
                        {
                            anchorArray[i].Y = _anim.Anchors[i].Y;
                        }
                }
            }

            bool selected = _anchorSettingFrame == -1;
            if (selected) ImGui.PushStyleColor(ImGuiCol.Button, new Color(255, 0, 0).ToUint());
            if (ImGui.Button("Interactive Move All")) _anchorSettingFrame = -1;
            if (selected) ImGui.PopStyleColor();

            int startFrame = _anim.StartingFrame;
            int endFrame = _anim.EndingFrame;
            for (int i = startFrame; i <= endFrame; i++)
            {
                ImGui.PushID(i);
                ImGui.InputFloat2($"Frame {i} ({_anim.Frames[i]})", ref anchorArray[i]);
                ImGui.SameLine();
                selected = _anchorSettingFrame == i;
                if (selected) ImGui.PushStyleColor(ImGuiCol.Button, new Color(255, 0, 0).ToUint());
                if (ImGui.Button("Interactive Set")) _anchorSettingFrame = i;
                if (selected) ImGui.PopStyleColor();

                ImGui.PopID();
            }

            Vector2 pos = ImGui.GetWindowPos();
            pos.Y += ImGui.GetWindowHeight();
            pos = pos.IntCastRound();

            float scale = _parent.Scale;
            var interactiveRect = Rectangle.Empty;
            if (_anchorSettingFrame != -1)
            {
                _anchorSettingFrame = Maths.Clamp(_anchorSettingFrame, startFrame, endFrame);
                pos.Y += 10 * scale;
                int prevFrame = Math.Max(_anchorSettingFrame - 1, startFrame);
                Vector2 size = _anim.Frames[prevFrame].Size * scale;

                Vector2 prevFramePos = pos + anchorArray[prevFrame] * scale;
                Rectangle inflatedRect = new Rectangle(prevFramePos, size).Inflate(scale, scale);
                composer.RenderSprite(inflatedRect.PositionZ(0), inflatedRect.Size, Color.White);
                composer.RenderSprite(new Vector3(prevFramePos, 0), size, Color.White * 0.5f, _anim.Texture, _anim.Frames[prevFrame], _parent.Mirrored);

                size = _anim.Frames[_anchorSettingFrame].Size * scale;
                interactiveRect = new Rectangle(pos + anchorArray[_anchorSettingFrame] * scale, size);
                composer.RenderSprite(interactiveRect.PositionZ(0), size, Color.White * 0.75f, _anim.Texture, _anim.Frames[_anchorSettingFrame], _parent.Mirrored);
                composer.RenderOutline(interactiveRect.Inflate(scale, scale), Color.Red);
            }
            else
            {
                for (int i = startFrame; i <= endFrame; i++)
                {
                    Vector2 size = _anim.Frames[i].Size * scale;
                    interactiveRect = new Rectangle(pos + anchorArray[i] * scale, size);
                    composer.RenderSprite(interactiveRect.PositionZ(0), size, Color.White * 0.75f, _anim.Texture, _anim.Frames[i], _parent.Mirrored);
                }

                composer.RenderOutline(interactiveRect.Inflate(scale, scale), Color.Red);
            }

            if (!_mouseDown && interactiveRect.Contains(Engine.Host.MousePosition) && Engine.Host.IsKeyDown(Key.MouseKeyLeft))
            {
                _mouseDown = true;
                _mouseDownPos = Engine.Host.MousePosition;
                _interactiveAnchorStart = _anchorSettingFrame == -1 ? Vector2.Zero : anchorArray[_anchorSettingFrame];
            }
            else if (!Engine.Host.IsKeyHeld(Key.MouseKeyLeft))
            {
                _mouseDown = false;
            }

            if (_mouseDown)
            {
                Vector2 movedAmount = Engine.Host.MousePosition - _mouseDownPos;
                Vector2 m = (movedAmount / scale).IntCastRound();
                if (_anchorSettingFrame == -1)
                {
                    for (int i = startFrame; i <= endFrame; i++)
                    {
                        anchorArray[i] = anchorArray[i] - _interactiveAnchorStart;
                        anchorArray[i] = anchorArray[i] + m;
                    }

                    _interactiveAnchorStart = m;
                }
                else
                {
                    anchorArray[_anchorSettingFrame] = _interactiveAnchorStart + m;
                }
            }
        }
    }
}