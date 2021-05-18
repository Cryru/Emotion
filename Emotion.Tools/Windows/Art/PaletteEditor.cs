#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Emotion.Common;
using Emotion.Game.Effects;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.Utility;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Tools.Windows.Art
{
    public class PaletteEditor : ImGuiWindow
    {
        private int _editingColor;
        private Vector4 _editCol = Vector4.Zero;
        private bool _updatePreview;

        private string _fileName;

        private Texture _previewTexture;

        private PaletteDescription _description;
        private Palette _selectedPalette;
        private string _newPaletteName = "";

        private TextureAsset _baseTexture;
        private Palette _defaultPalette;
        private byte[] _paletteMap;

        public PaletteEditor()
        {
            Title = "Palette Editor";
        }

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
                    _fileName = Path.GetFileNameWithoutExtension(f.Name) + "Palette.xml";

                    var description = new PaletteDescription
                    {
                        BaseAsset = _fileName
                    };

                    Reset();

                    _baseTexture = f;
                    _description = description;
                });
                Parent.AddWindow(explorer);
            }

            if (ImGui.Button("Choose Palette File"))
            {
                var explorer = new FileExplorer<PaletteAsset>(f =>
                {
                    _fileName = f.Name;

                    Reset();

                    _baseTexture = f.BaseTexture;
                    _description = f.Content;
                });
                Parent.AddWindow(explorer);
            }

            if (_description == null) return;

            // Extract the palette map and the default palette.
            if (_defaultPalette == null)
            {
                _defaultPalette = new Palette
                {
                    Name = "Default"
                };
                var pixelData = new byte[(int) (_baseTexture.Texture.Size.X * _baseTexture.Texture.Size.Y * 4)];
                PixelFormat pixelFormat = _baseTexture.Texture.PixelFormat;
                unsafe
                {
                    fixed (void* p = &pixelData[0])
                    {
                        Texture.EnsureBound(_baseTexture.Texture.Pointer);
                        Gl.GetTexImage(TextureTarget.Texture2d, 0, pixelFormat, PixelType.UnsignedByte, new IntPtr(p));
                    }
                }

                _paletteMap = PaletteBaseTexture.GeneratePaletteMap(pixelData, pixelFormat, out List<Color> defaultCMap);
                _defaultPalette.Colors = defaultCMap.ToArray();

                ref Palette[] pals = ref _description.Palettes;
                Array.Resize(ref pals, _description.Palettes.Length + 1);
                pals[^1] = _defaultPalette;

                _selectedPalette = _defaultPalette;
            }

            if (ImGui.Button("Save"))
            {
                string xml = XMLFormat.To(_description);
                Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(xml), $"Assets/{_fileName}");
            }

            ImGui.Text(_fileName);
            ImGui.Text("Palettes");

            foreach (Palette p in _description.Palettes.Where(p => ImGui.Button(p.Name)))
            {
                _selectedPalette = p;
                _updatePreview = true;
            }

            ImGui.InputText("New", ref _newPaletteName, 20);
            ImGui.SameLine();
            if (ImGui.Button("Create") && !string.IsNullOrEmpty(_newPaletteName))
            {
                _selectedPalette = new Palette
                {
                    Name = _newPaletteName,
                    Colors = _description.GetPalette("Default").Colors.ToArray()
                };
                _newPaletteName = "";

                ref Palette[] pals = ref _description.Palettes;
                Array.Resize(ref pals, _description.Palettes.Length + 1);
                pals[^1] = _selectedPalette;
                _updatePreview = true;
            }

            ImGui.SameLine();
            if (ImGui.Button("Remove") && _selectedPalette != null && _selectedPalette != _defaultPalette)
            {
                var pals = new List<Palette>();
                pals.AddRange(_description.Palettes);
                pals.Remove(_selectedPalette);
                _description.Palettes = pals.ToArray();
                _selectedPalette = null;
            }

            ImGui.Text("Default");
            Tuple<Vector2, Vector2> uvs = _baseTexture.Texture.GetImGuiUV(new Rectangle(0, 0, _baseTexture.Texture.Size));
            ImGui.Image(new IntPtr(_baseTexture.Texture.Pointer), _baseTexture.Texture.Size, uvs.Item1, uvs.Item2);
            ImGui.Text($"Colors: {_defaultPalette.Colors.Length}");

            PaletteColorDisplay(_defaultPalette);

            if (_selectedPalette == null) return;

            if (_updatePreview)
            {
                CreateTextureFromMap(ref _previewTexture, _selectedPalette);
                _updatePreview = false;
            }

            ImGui.Text($"Preview of {_selectedPalette.Name}");
            ImGui.Image(new IntPtr(_previewTexture.Pointer), _previewTexture.Size);

            PaletteColorDisplay(_selectedPalette);

            if (_editingColor != -1)
            {
                ImGui.End();
                var openAlways = true;
                ImGui.Begin("Color Editor", ref openAlways, ImGuiWindowFlags.AlwaysAutoResize);

                if (ImGui.ColorPicker4($"Color Edit - {_editingColor}", ref _editCol))
                {
                    _selectedPalette.Colors[_editingColor] = new Color(_editCol);
                    _updatePreview = true;
                }

                ImGui.Text($"Difference - {(_defaultPalette.Colors[_editingColor].ToVec4() - _editCol).ToVec3()}");

                if (ImGui.Button("Reset Color"))
                {
                    _selectedPalette.Colors[_editingColor] = _defaultPalette.Colors[_editingColor];
                    _editCol = _selectedPalette.Colors[_editingColor].ToVec4();
                    _updatePreview = true;
                }

                ImGui.SameLine();
                if (ImGui.Button("Reset All"))
                {
                    _selectedPalette.Colors = _defaultPalette.Colors.ToArray();
                    _updatePreview = true;
                    _editCol = _selectedPalette.Colors[_editingColor].ToVec4();
                }

                ImGui.SameLine();
                if (ImGui.Button("RGB Pivot from Current"))
                {
                    Color defaultC = _defaultPalette.Colors[_editingColor];
                    Vector4 diff = defaultC.ToVec4() - _editCol;

                    for (var i = 0; i < _selectedPalette.Colors.Length; i++)
                    {
                        if (i == _editingColor) continue;

                        Vector4 defaultThis = _defaultPalette.Colors[i].ToVec4();

                        Vector4 def = defaultThis;
                        def -= diff;
                        _selectedPalette.Colors[i] = new Color(def);
                    }

                    _updatePreview = true;
                }

                ImGui.End();
            }
        }

        private void CreateTextureFromMap(ref Texture texture, Palette palette)
        {
            texture ??= new Texture(_baseTexture.Texture.Size, PixelFormat.Rgba);

            var pixels = new byte[(int) (_baseTexture.Texture.Size.X * _baseTexture.Texture.Size.Y) * 4];
            Span<Color> pixelsAsColor = MemoryMarshal.Cast<byte, Color>(pixels);
            for (var i = 0; i < pixelsAsColor.Length; i++)
            {
                int index = _paletteMap[i];
                Color c = palette.Colors[index];
                pixelsAsColor[i] = c;
            }
            texture.Upload(_baseTexture.Texture.Size, pixels);
        }

        private void PaletteColorDisplay(Palette palette)
        {
            for (var i = 0; i < palette.Colors.Length; i++)
            {
                Vector4 col = palette.Colors[i].ToVec4();
                if (ImGui.ColorButton($"{palette.Name} {i}", col))
                {
                    _editingColor = i;
                    _editCol = col;
                }

                if ((i == 0 || i % 5 != 0) && palette.Colors.Length - 1 != i) ImGui.SameLine();
            }
        }

        public void Reset()
        {
            _defaultPalette = null;
            _selectedPalette = null;
            _editingColor = -1;
            _updatePreview = true;
        }
    }
}