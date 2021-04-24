#region Using

using System.Numerics;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.AnimationEditorWindows
{
    public class AnchorPlacingWindow : ImGuiWindow
    {
        private AnimationEditor _parent;
        private AnimatedTexture _anim;

        public AnchorPlacingWindow(AnimationEditor parent, AnimatedTexture anim) : base("Anchor Placer")
        {
            _parent = parent;
            _anim = anim;

            // Ensure anchor array length is correct.
            // It's possible for more frames to be added after initial initialization.
            Vector2[] anchorArray = _anim.Anchors;
            if (anchorArray.Length <= _anim.TotalFrames)
            {
                System.Array.Resize(ref anchorArray, _anim.TotalFrames + 1);
                _anim.Anchors = anchorArray;
            }
            anchorArray = _parent.AnimController.MirrorXAnchors;
            if (anchorArray.Length <= _anim.TotalFrames)
            {
                System.Array.Resize(ref anchorArray, _anim.TotalFrames + 1);
                _parent.AnimController.MirrorXAnchors = anchorArray;
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

            Vector2 pos = ImGui.GetWindowPos();
            pos.Y += ImGui.GetWindowHeight();
            for (int i = _anim.StartingFrame; i <= _anim.EndingFrame; i++)
            {
                composer.RenderSprite(new Vector3(pos, 0), _anim.Frames[i].Size, Color.White, _anim.Texture, _anim.Frames[i]);
                composer.RenderSprite(new Vector3(pos + anchorArray[i], 1), new Vector2(3, 3), Color.Red);
                ImGui.InputFloat2($"Frame {i} ({_anim.Frames[i]})", ref anchorArray[i]);
                pos.X += _anim.Frames[i].Size.X;
            }
        }
    }
}