// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Graphics;
using Breath.Objects;
using Breath.Systems;
using OpenTK;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Enums;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Graphics.Text;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Graphics.Systems
{
    internal class TextRenderer : SystemBase
    {
        #region System API

        protected internal override Type[] GetRequirements()
        {
            return new[] { typeof(RenderData), typeof(TextData), typeof(Transform) };
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
        }

        protected override void Draw(Entity entity)
        {
            // Get components.
            RenderData renderData = entity.GetComponent<RenderData>();
            TextData textData = entity.GetComponent<TextData>();
            Transform transform = entity.GetComponent<Transform>();

            // Check if a font is selected.
            if (textData.Font == null) return;

            // If there is no cached render we for sure have to render.
            bool needRerender = textData.CachedRender == null;

            // Check if transform size has changed.
            if (transform.HasUpdated || needRerender)
                if (textData.CachedRender == null || textData.CachedRender.InternalTexture.Size != transform.Size)
                {
                    // Clear if any.
                    textData.CachedRender?.Destroy();
                    // Remake it.
                    textData.CachedRender = new RenderTarget((int)transform.Width, (int)transform.Height);
                    // The cache was just remade, so we need a new one.
                    needRerender = true;
                }

            // Check if the text data has changed.
            if (textData.HasUpdated || needRerender || true)
            {
                VBO vertVBO = new VBO();
                Vector2[] vertVec =
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                };
                vertVBO.Upload(vertVec);

                VBO whiteColorVBO = new VBO();
                Color whiteCol = Color.White;
                whiteColorVBO.Upload(whiteCol.ToVertexArray(4));

                VBO fullTextureVBO = new VBO();
                Vector2[] vecs =
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                };
                fullTextureVBO.Upload(vecs);

                // Render text.
                textData.CachedRender.Use();

                // Rendering metrics.
                int x = 0;
                int y = 0;
                char prevChar = (char)0;

                // For each character.
                foreach (char c in textData.Text)
                {
                    Glyph glyph = textData.Font.GetGlyph(c, (uint)textData.Size);
                    Texture texture = glyph.GlyphTexture;

                    // Special character handling.
                    switch (c)
                    {
                        case '\n':
                            // New line.
                            y += textData.Font._face.Height / 64;
                            x = 0;
                            continue;
                        case '\r':
                            // Windows new line.
                            continue;
                        case '\t':
                            // Tab.
                            x += glyph.FreeTypeGlyph.Metrics.HorizontalAdvance.ToInt32() * 2;
                            continue;
                    }

                    // Set the top offset.
                    int topOffset = y + textData.Font._face.Height / 64 - glyph.Top;

                    // Generate matrices.
                    Matrix4 scale = Matrix4.CreateScale((int)glyph.FreeTypeGlyph.Metrics.Width,
                        (int)glyph.FreeTypeGlyph.Metrics.Height, 1);
                    Matrix4 translation = Matrix4.CreateTranslation(x, topOffset, 0);
                    Matrix4 currentGlyphMatrix = scale * translation;

                    // If there is a texture draw it.
                    if (texture != null)
                    {
                        Window.Current.SetModelMatrix(currentGlyphMatrix);

                        Window.Current.SetTextureModelMatrix(Matrix4.CreateScale(1, 1, 1));
                        Window.Current.SetTexture(texture);

                        fullTextureVBO.EnableShaderAttribute(2, 2); // texture
                        renderData.ColorVBO.EnableShaderAttribute(1, 4); // color
                        vertVBO.EnableShaderAttribute(0, 2); // vert
                        vertVBO.Draw();
                        vertVBO.DisableShaderAttribute(0);
                        renderData.ColorVBO.DisableShaderAttribute(1);
                        fullTextureVBO.DisableShaderAttribute(2);

                        Window.Current.StopUsingTexture();

                        Window.Current.SetModelMatrix(Matrix4.Identity);
                    }

                    // Advance the width by the glyph advance and add kerning.
                    x += glyph.FreeTypeGlyph.Metrics.HorizontalAdvance.ToInt32() + (int)Math.Ceiling(textData.Font.GetKerning(prevChar, c));

                    // Assign previous character.
                    prevChar = c;
                }

                RenderTarget.StopUsing();

                // Set render data texture to the cached text.
                renderData.ApplyTexture(textData.CachedRender.InternalTexture);
            }
        }

        #endregion
    }
}