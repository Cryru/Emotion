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
    public class AnchorPlacer : ImGuiWindow
    {
        private AnimationEditor _parent;
        private AnimatedTexture _anim;
        private bool _mirrorAnchors;

        public AnchorPlacer(AnimationEditor parent, AnimatedTexture anim) : base("Anchor Placer")
        {
            _parent = parent;
            _anim = anim;
        }

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            Vector2[] anchorArray = _anim.Anchors;
            if (_parent.AnimController != null)
            {
                ImGui.Checkbox("Assign Mirror Anchors", ref _mirrorAnchors);
                if (_mirrorAnchors)
                {
                    if (_parent.AnimController.MirrorXAnchors == null) _parent.AnimController.MirrorXAnchors = new Vector2[_anim.Anchors.Length];
                    anchorArray = _mirrorAnchors ? _parent.AnimController.MirrorXAnchors : anchorArray;
                }

                if (_mirrorAnchors)
                {
                    ImGui.SameLine();
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