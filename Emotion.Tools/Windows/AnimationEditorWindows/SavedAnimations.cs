#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.AnimationEditorWindows
{
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
            if (_parent.Open) return;
            CantClose = false;
            Open = false;
        }

        protected override void RenderContent(RenderComposer composer)
        {
            foreach (KeyValuePair<string, AnimatedTextureBase> savedAnimation in _parent.Saved)
            {
                AnimatedTextureBase anim = savedAnimation.Value;
                ImGui.BeginTooltip();
                ImGui.SetTooltip($"Frames {anim.AnimationFrames} Range {anim.StartingFrame} - {anim.EndingFrame} Loop {anim.LoopType}");
                ImGui.Text(savedAnimation.Key);
                int animPreview = Math.Min(5, anim.AnimationFrames);
                for (var i = 0; i <= animPreview; i++)
                {
                    Rectangle frameBounds = anim.GetFrameBounds(i);
                    (Vector2 u1, Vector2 u2) = anim.Texture.GetImGuiUV(frameBounds);
                    ImGui.Image(new IntPtr(anim.Texture.Pointer), frameBounds.Size / 2f, u1, u2);
                    if (i != animPreview) ImGui.SameLine();
                }

                ImGui.EndTooltip();
            }
        }
    }
}