#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.AnimationEditorWindows
{
    public class AnchorPlacer : ImGuiWindow
    {
        private AnimationEditor _parent;
        private LookupAnimatedTexture _anim;
        private bool _clickPlace;
        private int _clickPlaceIdx;
        private Vector2 _clickPlaceOffset;

        public AnchorPlacer(AnimationEditor parent, LookupAnimatedTexture anim) : base("Anchor Placer")
        {
            _parent = parent;
            _anim = anim;
        }

        public override void Update()
        {
            if (!_clickPlace) return;
            if (!Engine.InputManager.IsMouseKeyDown(MouseKey.Left)) return;
            Vector2 pos = Engine.Host.MousePosition;
            _anim.Anchors[_clickPlaceIdx] = pos - _clickPlaceOffset;
            _clickPlace = false;
        }

        protected override void RenderContent(RenderComposer composer)
        {
            Vector2 pos = ImGui.GetWindowPos();
            pos.Y += ImGui.GetWindowHeight();
            for (int i = _anim.StartingFrame; i <= _anim.EndingFrame; i++)
            {
                composer.RenderSprite(new Vector3(pos, 0), _anim.Frames[i].Size, Color.White, _anim.Texture, _anim.Frames[i]);
                composer.RenderSprite(new Vector3(pos + _anim.Anchors[i], 1), new Vector2(3, 3), Color.Red);
                ImGui.InputFloat2($"Frame {i}", ref _anim.Anchors[i]);
                ImGui.SameLine();
                if (_clickPlace)
                {
                    ImGui.TextDisabled($"Click Place {i}");
                }
                else
                {
                    if (ImGui.Button($"Click Place {i}"))
                    {
                        _clickPlace = true;
                        _clickPlaceIdx = i;
                        _clickPlaceOffset = pos;
                    }
                }

                pos.X += _anim.Frames[i].Size.X;
            }
        }
    }
}