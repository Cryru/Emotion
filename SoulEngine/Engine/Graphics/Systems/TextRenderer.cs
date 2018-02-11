// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Primitives;
using Breath.Objects;
using Breath.Systems;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Graphics.Text;

#endregion

namespace Soul.Engine.Graphics.Systems
{
    internal class TextRenderer : SystemBase
    {
        #region System API

        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(RenderData), typeof(TextData), typeof(Transform)};
        }

        protected internal override void Setup()
        {
            // Set priority to before the render system.
            Priority = 7;

            // Set intent to draw.
            Draws = true;
        }

        protected override void Update(Entity entity)
        {
            // Get components.
            TextData textData = entity.GetComponent<TextData>();
            Transform transform = entity.GetComponent<Transform>();

            // Check if a font is selected.
            if (textData.Font == null) return;

            // Transfer last ticks rerender needs.
            bool needRerender = textData.NeedRerender;

            // Check if transform size has changed or we need to create a cached render.
            if (transform.HasUpdated || textData.CachedRender == null)
            {
                if (textData.CachedRender == null || textData.CachedRender.Texture.Size != transform.Size)
                {
                    // Clear if any.
                    textData.CachedRender?.Destroy();
                    // Remake it.
                    textData.CachedRender = new RenderTarget((int) transform.Width, (int) transform.Height);
                    // The cache was just remade, so we need to rerender.
                    needRerender = true;
                }
            }

            // If the text data has updated we definitely need to rerender.
            if(textData.HasUpdated) needRerender = true;

            textData.NeedRerender = needRerender;
        }

        protected override void Draw(Entity entity)
        {
            // Get components.
            RenderData renderData = entity.GetComponent<RenderData>();
            TextData textData = entity.GetComponent<TextData>();

            // Check if we can and need to render.
            if (!textData.NeedRerender || textData.Font == null || textData.CachedRender == null) return;

            // Render text.
            Core.BreathWin.DrawOnTarget(textData.CachedRender);

            // Rendering metrics.
            int x = 0;
            int y = 0;
            char prevChar = (char) 0;

            // For each character.
            foreach (char c in textData.Text)
            {
                Glyph glyph = textData.Font.GetGlyph(c, (uint) textData.Size);
                Texture texture = glyph.GlyphTexture;

                // Special character handling.
                switch (c)
                {
                    case '\n':
                        // New line.
                        y += glyph.Height + glyph.TopOffset;
                        x = 0;
                        continue;
                    case '\r':
                        // Windows new line.
                        continue;
                    case '\t':
                        // Tab.
                        x += glyph.Width * 2;
                        continue;
                }

                // Add the top offset.
                int topOffset = glyph.TopOffset + y;
                x += glyph.BearingX;

                // Generate matrices.
                Matrix4 scale = Matrix4.CreateScale((int) glyph.Width, (int) glyph.Height, 1);
                Matrix4 translation = Matrix4.CreateTranslation(x, topOffset, 0);

                // If there is a texture draw it.
                if (texture != null)
                {
                    Core.BreathWin.SetModelMatrix(scale * translation);

                    Core.BreathWin.SetTexture(texture);

                    RenderData.RectangleVBO.EnableShaderAttribute(2, 2); // texture
                    renderData.ColorVBO.EnableShaderAttribute(1, 4); // color
                    RenderData.RectangleVBO.EnableShaderAttribute(0, 2); // vert
                    RenderData.RectangleVBO.Draw();
                    RenderData.RectangleVBO.DisableShaderAttribute(0);
                    renderData.ColorVBO.DisableShaderAttribute(1);
                    RenderData.RectangleVBO.DisableShaderAttribute(2);

                    texture.StopUsing();
                    Core.BreathWin.SetModelMatrix(Matrix4.Identity);
                }

                // Advance the x position by the glyph advance and add kerning. (Kerning might be broken.)
                x += (glyph.Advance - glyph.BearingX) +
                     (int) Math.Ceiling(textData.Font.GetKerning(prevChar, c));

                // Assign previous character.
                prevChar = c;
            }

            Core.BreathWin.DrawNormally();

            // Set render data texture to the cached text.
            renderData.ApplyTexture(textData.CachedRender.Texture);

            // Set needs rerender to false.
            textData.NeedRerender = false;
        }

        #endregion
    }
}