#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Text;
using Emotion.Standard.Parsers.OpenType;

#endregion

namespace Emotion.Graphics
{
    // Command routines for sprites (and sprite-like) which use the RenderComposer.
    public sealed partial class Renderer
    {
        /// <summary>
        /// Render a (textured) quad to the screen.
        /// </summary>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        /// <param name="flipX">Whether to flip the texture on the x axis.</param>
        /// <param name="flipY">Whether to flip the texture on the y axis.</param>
        public void RenderSprite(Vector3 position, Vector2 size, Color color, Texture? texture = null, Rectangle? textureArea = null, bool flipX = false, bool flipY = false)
        {
            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad, texture);
            VertexData.SpriteToVertexData(vertices, position, size, color, texture, textureArea, flipX, flipY);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Rectangle rect, Color color, Texture? texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(rect.Position.ToVec3(), rect.Size, color, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector3 position, Vector2 size, Texture? texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(position, size, Color.White, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector2 position, Vector2 size, Color color, Texture? texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(position.ToVec3(), size, color, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector3 position, Color color, Texture texture, Rectangle textureArea)
        {
            RenderSprite(position, textureArea.Size, color, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector2 position, Color color, Texture texture, Rectangle textureArea)
        {
            RenderSprite(position.ToVec3(), textureArea.Size, color, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector3 position, Texture texture, Rectangle? textureArea = null)
        {
            RenderSprite(position, textureArea?.Size ?? texture.Size, Color.White, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector3 position, Vector2 size, Color color, TextureAsset texture, Rectangle? textureArea = null, bool flipX = false, bool flipY = false)
        {
            Texture? textureUnderlying = texture?.Texture;
            RenderSprite(position, size, color, textureUnderlying);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector3 position, Vector2 size, TextureAsset texture, Rectangle? textureArea = null, bool flipX = false, bool flipY = false)
        {
            RenderSprite(position, size, Color.White, texture);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector3 position, TextureAsset texture, Rectangle? textureArea = null, bool flipX = false, bool flipY = false)
        {
            Texture? textureUnderlying = texture?.Texture;
            if (textureUnderlying == null) return;
            RenderSprite(position, textureUnderlying.Size, Color.White, textureUnderlying, textureArea, flipX, flipY);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(IntVector2 position, IntVector2 size, Color color, Texture? texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(position.ToVec3(), size.ToVec2(), color, texture, textureArea);
        }

        public void RenderGrid(Vector3 pos, Vector2 size, Vector2 tileSize, Color color, Vector2? offset = null)
        {
            ShaderAsset gridShader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/3DGrid.xml");
            if (!gridShader.Loaded || gridShader.Shader == null) return;

            SetShader(gridShader.Shader);
            gridShader.Shader.SetUniformVector2("squareSize", tileSize);
            gridShader.Shader.SetUniformVector2("cameraPos", offset.HasValue ? (offset.Value / size) : Vector2.Zero);
            gridShader.Shader.SetUniformVector2("totalSize", size);
            gridShader.Shader.SetUniformColor("objectTint", color);
            RenderSprite(pos, size, Color.White);
            SetShader(null);
        }
    }
}