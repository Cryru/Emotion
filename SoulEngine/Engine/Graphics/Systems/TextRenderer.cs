// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
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
        #region Properties

        private static GlProgram _textRenderingProgram;

        #endregion

        #region Shaders

        /// <summary>
        /// The default vertex shader.
        /// </summary>
        private static string _vertexShader = @"
        #version 330 core
        
        layout(location = 0) in vec2 position;
        layout(location = 1) in vec4 vertColor;
        layout(location = 2) in vec2 vertexUV;
        
        out vec4 fragColor;
        out vec4 UV;
        
        // The model view projection matrix passed for this execution of the shader.
        uniform mat4 mvp;
        
        // The model matrix for the texture UV.
        uniform mat4 textureModel;
        
        void main() {
        	// Multiply the position by the mvp matrix.
            gl_Position = mvp * vec4(position, 1, 1);
        
        	// Pass data onto the fragment shader.
        	fragColor = vertColor;
        	UV = textureModel * vec4(vertexUV, 1, 1);
        }
        ";

        /// <summary>
        /// The text rendering fragment shader - allows us to color letters.
        /// </summary>
        private static string _fragmentShader = @"
        #version 330 core
        
        in vec4 fragColor;
        in vec4 UV;
        
        layout(location = 0) out vec4 color;
        
        uniform sampler2D textureSampler;
        
        void main(){
          color = vec4(fragColor.r, fragColor.g, fragColor.b, texture(textureSampler, UV.xy).a);
        }
        ";

        #endregion

        static TextRenderer()
        {
            // Setup program.
            _textRenderingProgram = new GlProgram();

            Shader defaultVertex = new Shader(OpenTK.Graphics.OpenGL.ShaderType.VertexShader, _vertexShader);
            Shader textFragment = new Shader(OpenTK.Graphics.OpenGL.ShaderType.FragmentShader, _fragmentShader);

            _textRenderingProgram.AttachShader(defaultVertex);
            _textRenderingProgram.AttachShader(textFragment);
            _textRenderingProgram.Link();

            _textRenderingProgram.DetachShader(defaultVertex);
            _textRenderingProgram.DetachShader(textFragment);

            defaultVertex.Destroy();
            textFragment.Destroy();
        }

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
            // Get components.
            TextData textData = entity.GetComponent<TextData>();
            Transform transform = entity.GetComponent<Transform>();

            // Check if a font is selected.
            if (textData.Font == null) return;

            // Transfer last tick's render needs.
            bool needRender = textData.NeedRender;

            // Check if transform size has changed or we need to create a cached render.
            if (transform.HasUpdated || textData.CachedRender == null)
            {
                if (textData.CachedRender == null || textData.CachedRender.Texture.Size != transform.Size)
                {
                    // Clear if any.
                    textData.CachedRender?.Destroy();
                    // Remake it.
                    textData.CachedRender = new RenderTarget((int)transform.Width, (int)transform.Height);
                    // The cache was just remade, so we need to render.
                    needRender = true;
                }
            }

            // If the text data has updated we definitely need to render.
            if (textData.HasUpdated) needRender = true;

            textData.NeedRender = needRender;
        }

        protected override void Draw(Entity entity)
        {
            // Get components.
            RenderData renderData = entity.GetComponent<RenderData>();
            TextData textData = entity.GetComponent<TextData>();

            // Check if we can and need to render.
            if (!textData.NeedRender || textData.Font == null || textData.CachedRender == null) return;

            // Use the text rendering program.
            Core.BreathWin.SetProgram(_textRenderingProgram);
            Core.BreathWin.SetTextureModelMatrix(Matrix4.Identity);

            // Render text.
            Core.BreathWin.DrawOnTarget(textData.CachedRender);

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
                        y += glyph.NewLineSpace;
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
                Matrix4 scale = Matrix4.CreateScale(glyph.Width, glyph.Height, 1);
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
                     (int)Math.Ceiling(textData.Font.GetKerning(prevChar, c));

                // Assign previous character.
                prevChar = c;
            }

            Core.BreathWin.DrawNormally();
            Core.BreathWin.SetDefaultProgram();

            // Set render data texture to the cached text.
            renderData.ApplyTexture(textData.CachedRender.Texture);

            // Set need render to false as we just did.
            textData.NeedRender = false;
        }

        #endregion
    }
}