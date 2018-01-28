// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Graphics;
using Breath.Objects;
using Breath.Systems;
using OpenTK;
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
            {
                // If the cache render doesn't exist or is of a different size.
                if (textData.CachedRender == null || textData.CachedRender.InternalTexture.Size != transform.Size)
                {
                    // Clear if any.
                    textData.CachedRender?.Destroy();
                    // Remake it.
                    textData.CachedRender = new Breath.Objects.RenderTarget((int) transform.Width, (int) transform.Height);
                    // The cache was just remade, so we need a new one.
                    needRerender = true;
                }
            }

            // Check if the text data has changed.
            if (textData.HasUpdated || needRerender || true)
            {
                VBO vertVBO = new VBO();
                Vector2[] vertVec = new[]
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
                Vector2[] vecs = new[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, -1),
                    new Vector2(0, -1)
                };
                fullTextureVBO.Upload(vecs);

                // Create a model matrix for the letters.
                //Matrix4 rotation =
                //    Matrix4.CreateTranslation(-(transform.Center.X - transform.X), -(transform.Center.Y - transform.Y), 0) *
                //    Matrix4.CreateRotationZ(transform.Rotation) *
                //    Matrix4.CreateTranslation(transform.Center.X - transform.X, transform.Center.Y - transform.Y, 0);
                //Matrix4 scale = Matrix4.CreateScale(1, -1, 1);

                //Matrix4 rotationAndScale = scale;

                // Render text.
                //textData.CachedRender.Use();

                int x = 0;
                int y = 0;
                char prevChar = (char) 0;

                // For each character.
                foreach (char c in textData.Text)
                {
                    Glyph glyph = textData.Font.GetGlyph(c, (uint)textData.Size);
                    Texture texture = glyph.GlyphTexture;

                    // Get distance between yMax and glyph height.
                    int topOffset = ConvertToPixels(((textData.Font._face.BBox.Top - textData.Font._face.BBox.Bottom) / 64) - glyph.FreeTypeGlyph.Metrics.Height.ToInt32());
                    y = topOffset;
                    x += 20;
                    //401 in Text.cpp SFML
                    

                    //y = (textData.Font._face.BBox.Top >> 6);
                    //x += (int) textData.Font.GetKerning(prevChar, c);
                    //prevChar = c;//(int) (glyph.FreeTypeGlyph.Metrics.Width + glyph.FreeTypeGlyph.Metrics.HorizontalBearingX);

                    Matrix4 scale = Matrix4.CreateScale((int) glyph.FreeTypeGlyph.Metrics.Width, (int) glyph.FreeTypeGlyph.Metrics.Height, 1);
                    Matrix4 translation = Matrix4.CreateTranslation(x, y, 0);
                    Matrix4 currentGlyphMatrix = scale * translation;

                    if (texture != null)
                    {
                        Window.Current.SetModelMatrix(currentGlyphMatrix);

                        Window.Current.SetTextureModelMatrix(Matrix4.CreateScale(1, -1, 1));
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

                    // process typewriter movement here
                }

                //RenderTarget.StopUsing();

                // Set render data texture to the cached text.
                //renderData.Texture = textData.CachedRender.InternalTexture;
            }
        }

        #endregion

        public int ConvertToPixels(float value)
        {
            return (int) value >> 6;
        }
    }
}