#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.SpriteStack;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Tools.Windows;
using ImGuiNET;

#endregion

namespace Emotion.ExecTest.Examples
{
    public class Viewer3D : IScene
    {
        // Viewer
        private SpriteStackTexture _texture;
        private SpriteStackModel _model;
        private WindowManager _wm;
        private Vector3 _rotation = new Vector3(45, 0, 45);
        private float _scale = 1f;

        // Editor
        private int[] _frameSize = new int[2];
        private int _selectedFrame = -1;

        public void Load()
        {
            var fileName = "test.png";
            _frameSize[0] = 32;
            _frameSize[1] = 32;

            void LoadModel()
            {
                _texture = Engine.AssetLoader.Get<SpriteStackTexture>(fileName, false);
                _model = _texture.GetSpriteStackModel(new Vector2(_frameSize[0], _frameSize[1]));
            }

            Engine.Renderer.FarZ = 1000;
            _wm = new WindowManager();
            _wm.AddWindow(new ProxyEditor("3D Viewer Controls", () => { }, c =>
            {
                if (ImGui.Button("Reload")) LoadModel();

                ImGui.DragFloat("Scale", ref _scale);
                ImGui.DragFloat3("Rotation", ref _rotation);

                ImGui.InputInt2("Frame Size", ref _frameSize[0]);

                if (ImGui.BeginCombo("Frame", _selectedFrame.ToString()))
                {
                    for (var i = 0; i < _model.Frames.Length; i++)
                    {
                        if (ImGui.Selectable(i.ToString())) _selectedFrame = i;

                        ImGui.SameLine();
                        Tuple<Vector2, Vector2> textureUv = _texture.Texture.GetImGuiUV(new Rectangle(i * _frameSize[0], 0, _frameSize[0], _frameSize[1]));
                        ImGui.Image((IntPtr) _texture.Texture.Pointer, new Vector2(_frameSize[0], _frameSize[1]), textureUv.Item1, textureUv.Item2);
                    }

                    ImGui.EndCombo();
                }

                if (_selectedFrame != -1)
                {
                    Tuple<Vector2, Vector2> selectedTextureUv = _texture.Texture.GetImGuiUV(new Rectangle(_selectedFrame * _frameSize[0], 0, _frameSize[0], _frameSize[1]));
                    ImGui.Image((IntPtr) _texture.Texture.Pointer, new Vector2(200 * _frameSize[0] / _frameSize[1], 200), selectedTextureUv.Item1, selectedTextureUv.Item2);
                }
            }));

            LoadModel();
        }

        public void Update()
        {
        }

        public void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(composer.CurrentTarget.Viewport, Color.CornflowerBlue);
            composer.ClearDepth();
            composer.SetUseViewMatrix(true);

            _model.RotationDeg = _rotation;
            _model.Scale = _scale;
            _model.Render(composer);

            ImGui.NewFrame();
            _wm.Render(composer);
            composer.RenderUI();
        }

        public void Unload()
        {
        }
    }
}