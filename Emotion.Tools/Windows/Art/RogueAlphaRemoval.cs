#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Standard.Image.PNG;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Tools.Windows.Art
{
    public class RogueAlphaRemoval : ImGuiWindow
    {
        private TextureAsset _file;
        private int _rogueAlphaPixels;
        private byte[] _pixelData;
        private byte[] _removedPixelData;
        private Texture _previewTexture;
        private byte _threshold = 10;
        private string _lastPreviewMode = "normal";

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            // File selection.
            if (ImGui.Button("Choose Texture File"))
            {
                var explorer = new FileExplorer<TextureAsset>(f =>
                {
                    _file = f;
                    _pixelData = null;
                });
                Parent.AddWindow(explorer);
            }

            if (_file == null) return;

            _previewTexture ??= new Texture(new Vector2(100, 100), _file.Texture.PixelFormat);
            if (_pixelData == null)
            {
                _pixelData = new byte[(int) (_file.Texture.Size.X * _file.Texture.Size.Y * 4)];

                unsafe
                {
                    fixed (void* p = &_pixelData[0])
                    {
                        Texture.EnsureBound(_file.Texture.Pointer);
                        Gl.GetTexImage(TextureTarget.Texture2d, 0, _file.Texture.PixelFormat, PixelType.UnsignedByte, new IntPtr(p));
                    }
                }

                _removedPixelData = new byte[_pixelData.Length];
                DetectRogueAlpha();
                UpdatePreview("normal");
            }

            ImGui.Image(new IntPtr(_file.Texture.Pointer), _file.Texture.Size);
            int thresholdInput = _threshold;
            ImGui.InputInt("Threshold", ref thresholdInput, 1, 5);
            if (thresholdInput != _threshold)
            {
                _threshold = (byte) thresholdInput;
                DetectRogueAlpha();
                UpdatePreview();
            }

            ImGui.Text($"Detected Rogue Alpha: {_rogueAlphaPixels}/{_file.Texture.Size.X * _file.Texture.Size.Y} pixels");

            ImGui.Text("Preview");
            ImGui.Image(new IntPtr(_previewTexture.Pointer), _previewTexture.Size);

            if (ImGui.Button("Normal Preview")) UpdatePreview("normal");
            ImGui.SameLine();
            if (ImGui.Button("Alpha Preview")) UpdatePreview("alpha");

            if (ImGui.Button("Apply Changes"))
            {
                UpdatePreview("export");
                byte[] pngData = PngFormat.Encode(_removedPixelData,  _previewTexture.Size, _file.Texture.PixelFormat);
                Engine.AssetLoader.Save(pngData, _file.Name);
            }
        }

        private void DetectRogueAlpha()
        {
            if (_pixelData == null) return;

            _rogueAlphaPixels = 0;

            for (var i = 0; i < _pixelData.Length; i += 4)
            {
                byte b = _pixelData[i];
                byte g = _pixelData[i + 1];
                byte r = _pixelData[i + 2];
                byte a = _pixelData[i + 3];

                if (a > _threshold) continue;
                if (b != 0 && g != 0 && r != 0) _rogueAlphaPixels++;
            }
        }

        private void UpdatePreview(string previewMode = null)
        {
            if (_pixelData == null) return;
            if (previewMode == null) previewMode = _lastPreviewMode;
            else _lastPreviewMode = previewMode;

            for (var i = 0; i < _pixelData.Length; i += 4)
            {
                byte b = _pixelData[i];
                byte g = _pixelData[i + 1];
                byte r = _pixelData[i + 2];
                byte a = _pixelData[i + 3];

                switch (previewMode)
                {
                    case "alpha":
                        if (a > _threshold)
                        {
                            _removedPixelData[i] = 0;
                            _removedPixelData[i + 1] = 255;
                            _removedPixelData[i + 2] = 0;
                            _removedPixelData[i + 3] = 255;
                            continue;
                        }

                        if (a == 0)
                        {
                            _removedPixelData[i] = 255;
                            _removedPixelData[i + 1] = 255;
                            _removedPixelData[i + 2] = 255;
                            _removedPixelData[i + 3] = 255;
                            continue;
                        }

                        _removedPixelData[i] = 0;
                        _removedPixelData[i + 1] = 0;
                        _removedPixelData[i + 2] = 255;
                        _removedPixelData[i + 3] = 255;

                        break;
                    case "export":
                        if (a > _threshold)
                        {
                            _removedPixelData[i] = b;
                            _removedPixelData[i + 1] = g;
                            _removedPixelData[i + 2] = r;
                            _removedPixelData[i + 3] = a;
                            continue;
                        }

                        _removedPixelData[i] = 0;
                        _removedPixelData[i + 1] = 0;
                        _removedPixelData[i + 2] = 0;
                        _removedPixelData[i + 3] = 0;
                        break;
                    default:
                        if (a > _threshold)
                        {
                            _removedPixelData[i] = b;
                            _removedPixelData[i + 1] = g;
                            _removedPixelData[i + 2] = r;
                            _removedPixelData[i + 3] = a;
                            continue;
                        }

                        if (a == 0)
                        {
                            _removedPixelData[i] = 255;
                            _removedPixelData[i + 1] = 255;
                            _removedPixelData[i + 2] = 255;
                            _removedPixelData[i + 3] = 255;
                            continue;
                        }

                        _removedPixelData[i] = 0;
                        _removedPixelData[i + 1] = 0;
                        _removedPixelData[i + 2] = 0;
                        _removedPixelData[i + 3] = 0;
                        break;
                }
            }

            _previewTexture.Upload(_file.Texture.Size, _removedPixelData);
        }
    }
}