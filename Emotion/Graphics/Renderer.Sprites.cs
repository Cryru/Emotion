﻿#nullable enable

#region Using

using Emotion.Game.Text;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Batches.SpriteBatcher;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Camera;

#endregion

namespace Emotion.Graphics
{
    // Command routines for sprites (and sprite-like) which use the RenderComposer.
    public sealed partial class RenderComposer
    {
        private bool _spriteBatcherEnabled;
        private RenderSpriteBatch _batcher = new RenderSpriteBatch();

        public void EnableSpriteBatcher(bool on)
        {
            _spriteBatcherEnabled = on;

            if (!on) _batcher.Clear();
        }

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
            if (_spriteBatcherEnabled && _bufferStack.Count == 1)
            {
                Span<VertexData> vertices = _batcher.AddSprite(CurrentState, texture);
                VertexData.SpriteToVertexData(vertices, position, size, color, texture, textureArea, flipX, flipY);
            }
            else
            {
                Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad, texture);
                VertexData.SpriteToVertexData(vertices, position, size, color, texture, textureArea, flipX, flipY);
            }
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Transform transform, Color color, Texture? texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(transform.Position, transform.Size, color, texture, textureArea);
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
            RenderSprite(position, texture.Size, Color.White, texture, textureArea);
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

        public enum RenderLineMode
        {
            Center,
            Inward,
            Outward
        }

        /// <summary>
        /// Render a line made out of quads.
        /// </summary>
        /// <param name="pointOne">The point to start the line.</param>
        /// <param name="pointTwo">The point to end the line at.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line in world units. The line will always be at least 1 pixel thick.</param>
        /// <param name="renderMode">How to treat the points given.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f, RenderLineMode renderMode = RenderLineMode.Center)
        {
            Vector3 direction = Vector3.Normalize(pointTwo - pointOne);

            Vector3 normal;
            if (_camera is Camera2D)
            {
                normal = new Vector3(-direction.Y, direction.X, 0);

                if (thickness < 1f) thickness = 1f;

                // If the size of the line is going to be less than 1 when scaled,
                // snap it to 1 and modulate alpha
                if (CurrentState.ViewMatrix)
                {
                    float scaledThickness = thickness * _camera.CalculatedScale;
                    if (scaledThickness < 1f)
                    {
                        thickness = 1f / _camera.CalculatedScale;
                        color = color * scaledThickness;
                    }
                }
            }
            else if (_camera is Camera3D && CurrentState.ViewMatrix)
            {
                Assert(_camera is Camera3D);

                Vector3 cameraPosition = _camera.Position;
                Vector3 centerOfLine = (pointOne + pointTwo) / 2f;
                Vector3 cameraDirection = Vector3.Normalize(cameraPosition - centerOfLine);

                normal = Vector3.Cross(direction, cameraDirection);
                normal = Vector3.Normalize(normal);
            }
            else
            {
                normal = new Vector3(-direction.Y, direction.X, 0);
            }

            Vector3 delta = normal * (thickness / 2f);
            Vector3 deltaNeg = -delta;

            if (renderMode == RenderLineMode.Inward)
            {
                pointOne += delta;
                pointTwo += delta;
            }
            else if (renderMode == RenderLineMode.Outward)
            {
                pointOne -= delta;
                pointTwo -= delta;
            }

            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad);
            VertexData.WriteDefaultQuadUV(vertices);

            vertices[0].Vertex = pointOne + delta;
            vertices[1].Vertex = pointTwo + delta;
            vertices[2].Vertex = pointTwo + deltaNeg;
            vertices[3].Vertex = pointOne + deltaNeg;

            uint c = color.ToUint();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = c;
            }
        }

        /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float, RenderLineMode)" />
        public void RenderLine(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
        {
            RenderLine(pointOne.ToVec3(), pointTwo.ToVec3(), color, thickness);
        }

        /// <summary>
        /// Render a line, from a line segment.
        /// </summary>
        /// <param name="segment">The line segment to render.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void RenderLine(ref LineSegment segment, Color color, float thickness = 1f)
        {
            RenderLine(segment.Start, segment.End, color, thickness);
        }

        public void RenderLine(LineSegment segment, Color color, float thickness = 1f)
        {
            RenderLine(segment.Start, segment.End, color, thickness);
        }

        /// <summary>
        /// Render a line with an arrow at the end.
        /// </summary>
        /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float, RenderLineMode)" />
        public void RenderArrow(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f)
        {
            RenderLine(pointOne, pointTwo, color, thickness);

            Vector3 diff = pointTwo - pointOne;
            const float maxArrowHeadLength = 10;
            float length = Math.Min(diff.Length() / 2, maxArrowHeadLength);
            float width = length / 2;

            Vector3 direction = Vector3.Normalize(diff);
            var normal = new Vector3(-direction.Y, direction.X, 0);
            Vector3 lengthDelta = length * direction;
            Vector3 delta = width * normal;
            Vector3 arrowPointOne = pointTwo - lengthDelta + delta;
            Vector3 arrowPointTwo = pointTwo - lengthDelta - delta;

            RenderLine(pointTwo, arrowPointOne, color, thickness);
            RenderLine(pointTwo, arrowPointTwo, color, thickness);
        }

        /// <inheritdoc cref="RenderArrow(Vector3, Vector3, Color, float)" />
        public void RenderArrow(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
        {
            RenderArrow(pointOne.ToVec3(), pointTwo.ToVec3(), color, thickness);
        }

        /// <summary>
        /// Render a rectangle outline.
        /// </summary>
        /// <param name="position">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the lines.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void RenderRectOutline(Vector3 position, Vector2 size, Color color, float thickness = 1)
        {
            Vector3 nn = position;
            Vector3 pn = new Vector3(position.X + size.X, position.Y, position.Z);
            Vector3 np = new Vector3(position.X, position.Y + size.Y, position.Z);
            Vector3 pp = new Vector3(position.X + size.X, position.Y + size.Y, position.Z);

            RenderLine(nn, pn, color, thickness, RenderLineMode.Inward);
            RenderLine(pn, pp, color, thickness, RenderLineMode.Inward);
            RenderLine(pp, np, color, thickness, RenderLineMode.Inward);
            RenderLine(np, nn, color, thickness, RenderLineMode.Inward);
        }

        /// <inheritdoc cref="RenderRectOutline(Vector3, Vector2, Color, float)" />
        public void RenderRectOutline(Vector2 position, Vector2 size, Color color, float thickness = 1)
        {
            RenderRectOutline(position.ToVec3(), size, color, thickness);
        }

        /// <inheritdoc cref="RenderRectOutline(Vector3, Vector2, Color, float)" />
        public void RenderRectOutline(Rectangle rect, Color color, float thickness = 1)
        {
            RenderRectOutline(rect.Position.ToVec3(), rect.Size, color, thickness);
        }

        /// <summary>
        /// Render a string from an atlas.
        /// </summary>
        /// <param name="position">The top left position of where to start drawing the string.</param>
        /// <param name="color">The text color.</param>
        /// <param name="text">The text itself.</param>
        /// <param name="atlas">The font atlas to use.</param>
        /// <param name="layouter">The layouter to use.</param>
        /// <param name="effect">Effect to apply</param>
        /// <param name="effectAmount">The effect amount.</param>
        /// <param name="effectColor">The effect color.</param>
        public void RenderString(
            Vector3 position, Color color, string text, DrawableFontAtlas atlas, TextLayouter? layouter = null,
            FontEffect effect = FontEffect.None, float effectAmount = 0f, Color? effectColor = null)
        {
            layouter ??= new TextLayouter(atlas);

            atlas.SetupDrawing(this, text, effect, effectAmount, effectColor);

            var reUsableVector = new Vector3();
            foreach (char c in text)
            {
                Vector2 gPos = layouter.AddLetter(c, out DrawableGlyph g);
                if (g == null || g.GlyphUV == Rectangle.Empty) continue;

                reUsableVector.X = gPos.X;
                reUsableVector.Y = gPos.Y;
                atlas.DrawGlyph(this, g, position + reUsableVector, color);
            }

            atlas.FinishDrawing(this);
        }

        public void RenderGrid(Vector3 pos, Vector2 size, Vector2 tileSize, Color color, Vector2? offset = null)
        {
            ShaderAsset gridShader = Engine.AssetLoader.ONE_Get<ShaderAsset>("Shaders/3DGrid.xml");
            if (gridShader.Loaded && gridShader.Shader != null)
            {
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
}