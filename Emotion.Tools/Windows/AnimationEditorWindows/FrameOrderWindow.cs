#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.AnimationEditorWindows
{
    public class FrameOrderWindow : ImGuiWindow
    {
        private AnimationEditor _parent;
        private LookupAnimatedTexture _anim;
        private FontAsset _font;

        private Vector2 _lastOffset;
        private int _holdingIdx = -1;

        public FrameOrderWindow(AnimationEditor parent, LookupAnimatedTexture anim) : base("Frame Order")
        {
            _parent = parent;
            _anim = anim;
            _font = Engine.AssetLoader.Get<FontAsset>("debugFont.otf");
        }

        public override void Update()
        {
            if(ImGuiNetPlugin.Focused) return;

            if (Engine.InputManager.IsMouseKeyDown(Platform.Input.MouseKey.Right))
            {
                _holdingIdx = -1;
            }
            
            if (!Engine.InputManager.IsMouseKeyDown(Platform.Input.MouseKey.Left)) return;
            Vector2 mousePos = Engine.Host.MousePosition;
            for (var i = 0; i < _anim.Frames.Length; i++)
            {
                Rectangle clickCapture = _anim.Frames[i];
                clickCapture.Location += _lastOffset;
                if (!clickCapture.Intersects(mousePos)) continue;
                if (_holdingIdx == -1)
                {
                    _holdingIdx = i;
                    break;
                }

                Rectangle transfer = _anim.Frames[i];
                _anim.Frames[i] = _anim.Frames[_holdingIdx];
                _anim.Frames[_holdingIdx] = transfer;
                _holdingIdx = -1;
                break;
            }
        }

        protected override void RenderContent(RenderComposer composer)
        {
            Vector2 offset = ImGui.GetWindowPos();
            offset.Y += ImGui.GetWindowHeight();
            _lastOffset = offset;
            composer.RenderSprite(new Vector3(offset, 0), _anim.Texture.Size, Color.White, _anim.Texture);
            for (var i = 0; i < _anim.Frames.Length; i++)
            {
                composer.RenderOutline(new Vector3(offset + _anim.Frames[i].Position, 1), _anim.Frames[i].Size, Color.Red);
                composer.RenderString(new Vector3(offset + _anim.Frames[i].Position, 1), Color.Red, i.ToString(), _font.GetAtlas(15));
            }

            ImGui.Text(_holdingIdx == -1 ? "Click on a rectangle to change it's position." : $"Select new position for frame {_holdingIdx}!");
        }
    }
}