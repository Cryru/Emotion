#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Utility;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Tools.Windows
{
    public class GpuTextureViewer : ImGuiWindow
    {
        private Texture _selectedTexture;

        public GpuTextureViewer() : base("Texture Viewer")
        {
        }

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            List<Texture> textures = Texture.AllTextures;

            ImGui.BeginChild("Texture List", new Vector2(500, 600), true);
            ImGui.BeginListBox("", new Vector2(490, 580));
            for (var i = 0; i < textures.Count; i++)
            {
                Texture texture = textures[i];
                if (ImGui.Button($"{texture.Pointer} - {texture.CreationStack.Substring(0, Math.Min(100, texture.CreationStack.Length))}")) _selectedTexture = texture;
            }

            ImGui.EndListBox();
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("Texture", new Vector2(600, 600), true, ImGuiWindowFlags.HorizontalScrollbar);
            if (_selectedTexture != null)
            {
                ImGui.Text(_selectedTexture.CreationStack);
                int byteSize = (int) (_selectedTexture.Size.X * _selectedTexture.Size.Y) * Gl.PixelTypeToByteCount(_selectedTexture.PixelType) *
                               Gl.PixelFormatToComponentCount(_selectedTexture.PixelFormat);
                ImGui.Text(
                    $"Dimensions: {_selectedTexture.Size}, Format: {_selectedTexture.PixelFormat}, Memory Usage: {Helpers.FormatByteAmountAsString(byteSize)}, Smooth: {_selectedTexture.Smooth}");
                Tuple<Vector2, Vector2> uv = _selectedTexture.GetImGuiUV();
                ImGui.Image(new IntPtr(_selectedTexture.Pointer), _selectedTexture.Size, uv.Item1, uv.Item2);
            }

            ImGui.EndChild();
        }
    }
}