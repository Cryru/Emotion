// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Objects;
using Breath.Primitives;
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
        #region Properties

        /// <summary>
        /// The scale to render text at.
        /// </summary>
        private const int FontScale = 1;

        /// <summary>
        /// Default white VBO. Text is rendered white and then colored via the render data.
        /// </summary>
        private static VBO _whiteColorVBO;

        /// <summary>
        /// The GL Program used for text rendering.
        /// </summary>
        private static GlProgram _textRenderingProgram;

        /// <summary>
        /// The default vertex shader.
        /// </summary>
        private const string VertexShader = @"#version 330 core
        
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
        }";

        /// <summary>
        /// The text rendering fragment shader - allows us to color letters.
        /// </summary>
        private const string FragmentShader = @"#version 330 core
        
        in vec4 fragColor;
        in vec4 UV;
        
        layout(location = 0) out vec4 color;
        
        uniform sampler2D textureSampler;
        
        void main(){
          float a = texture(textureSampler, UV.xy).a;

    
          color = vec4(1, 1, 1, a);
        }";

        #endregion

        static TextRenderer()
        {
            // Setup white color VBO.
            _whiteColorVBO = new VBO();
            _whiteColorVBO.Upload(Color.White.ToVertexArray(4));

            // Setup program.
            _textRenderingProgram = new GlProgram();

            Shader defaultVertex = new Shader(ShaderType.VertexShader, VertexShader);
            Shader textFragment = new Shader(ShaderType.FragmentShader, FragmentShader);

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
                    textData.CachedRender = new RenderTarget((int)transform.Width * FontScale, (int)transform.Height * FontScale);
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

            // Render text.
            Core.BreathWin.DrawOnTarget(textData.CachedRender);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Rendering metrics.
            int x = 0;
            int y = 0;
            char prevChar = (char)0;

            // For each character.
            foreach (char c in textData.Text)
            {
                Glyph glyph = textData.Font.GetGlyph(c, (uint) (textData.Size * FontScale));
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

                // If there is a texture draw it.
                if (texture != null)
                {
                    // Generate matrices.
                    Matrix4 scale = Matrix4.CreateScale(texture.Width, texture.Height, 1);
                    Matrix4 translation = Matrix4.CreateTranslation(x, topOffset, 0);

                    Core.BreathWin.Draw(RenderData.RectangleVBO, _whiteColorVBO, RenderData.RectangleVBO, texture,
                        scale * translation);
                }

                // Advance the x position by the glyph advance and add kerning. (Kerning might be broken.)
                x += (glyph.Advance - glyph.BearingX) +
                     (int)Math.Ceiling(textData.Font.GetKerning(prevChar, c));

                // Assign previous character.
                prevChar = c;
            }

            // Restore default drawing.
            Core.BreathWin.DrawOnTheScreen();
            Core.BreathWin.SetDefaultProgram();

            // Set render data texture to the cached text.
            renderData.ApplyTexture(textData.CachedRender.Texture);

            // Set need render to false as we just did.
            textData.NeedRender = false;
        }

        #endregion
    }
}