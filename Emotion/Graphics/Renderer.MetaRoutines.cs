#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    // Advanced routines, and routines using base functions to be used along with shaders and such.
    public sealed partial class RenderComposer
    {
        /// <summary>
        /// Render a circle outline.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary rectangle which encompasses the circle. Can be modified
        /// with "useCenter"
        /// </param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="useCenter">Whether the position should instead be the center of the circle.</param>
        /// <param name="circleDetail">How detailed the circle should be.</param>
        public void RenderCircleOutline(Vector3 position, float radius, Color color, bool useCenter = false, int circleDetail = 30)
        {
            Vector3 posOffset = useCenter ? new Vector3(position.X - radius, position.Y - radius, position.Z) : position;

            float fX = 0;
            float fY = 0;
            float pX = 0;
            float pY = 0;

            // Generate points.
            for (uint i = 0; i < circleDetail; i++)
            {
                var angle = (float) (i * 2 * Math.PI / circleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                if (i == 0)
                {
                    fX = x;
                    fY = y;
                }
                else
                {
                    RenderLine(posOffset + new Vector3(radius + pX, radius + pY, 0), posOffset + new Vector3(radius + x, radius + y, 0), color);
                }

                pX = x;
                pY = y;

                if (i == circleDetail - 1) RenderLine(posOffset + new Vector3(radius + x, radius + y, 0), posOffset + new Vector3(radius + fX, radius + fY, 0), color);
            }
        }

        /// <summary>
        /// Render a circle.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary square which encompasses the circle, or the center.
        /// </param>
        /// <param name="radius">The radius.</param>
        /// <param name="color">The color.</param>
        /// <param name="positionIsCenter">Whether the position should instead be the center of the circle.</param>
        /// <param name="detail">How many triangles to generate. The more the smoother.</param>
        public void RenderCircle(Vector3 position, float radius, Color color, bool positionIsCenter = false, int detail = 30)
        {
            RenderEllipse(position, new Vector2(radius), color, positionIsCenter, detail);
        }

        /// <summary>
        /// Render an ellipse.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary rectangle which encompasses the ellipse. Can be modified with "useCenter"
        /// </param>
        /// <param name="radius">The radius.</param>
        /// <param name="color">The color.</param>
        /// <param name="positionIsCenter">Whether the position should instead be the center of the circle.</param>
        /// <param name="detail">How many triangles to generate. The more the smoother.</param>
        /// <param name="colorMiddle">A separate color just for the vertices in the center of the ellipse.</param>
        public void RenderEllipse(Vector3 position, Vector2 radius, Color color, bool positionIsCenter = false, int detail = 30, Color? colorMiddle = null)
        {
            var vertsNeeded = (uint) ((detail + 1) * 3);
            Span<VertexData> vertices = RenderStream.GetStreamMemory(vertsNeeded, BatchMode.SequentialTriangles);
            Debug.Assert(vertices != null);
            var vertexIdx = 0;

            Vector3 posOffset = positionIsCenter ? new Vector3(position.X - radius.X, position.Y - radius.Y, position.Z) : position;

            float pX = 0;
            float pY = 0;
            float fX = 0;
            float fY = 0;

            // Generate triangles.
            for (uint i = 0; i < detail; i++)
            {
                var angle = (float) (i * 2 * Math.PI / detail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius.X;
                float y = (float) Math.Sin(angle) * radius.Y;

                vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius.X + pX, radius.Y + pY, 0);
                vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius.X + x, radius.Y + y, 0);
                vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius.X, radius.Y, 0);

                pX = x;
                pY = y;

                if (i == detail - 1)
                {
                    vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius.X + pX, radius.Y + pY, 0);
                    vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius.X + fX, radius.Y + fY, 0);
                    vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius.X, radius.Y, 0);
                }

                if (i != 0) continue;
                fX = x;
                fY = y;
            }

            uint c = color.ToUint();
            uint cM = colorMiddle?.ToUint() ?? c;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = i % 3 == 2 ? cM : c;
                vertices[i].UV = Vector2.Zero;
            }
        }

        /// <summary>
        /// Render to a frame buffer, but clear all its attachments first.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to render to. If set to null will revert to the previous buffer, in which case the
        /// buffer will not be cleared.
        /// </param>
        public void RenderToAndClear(FrameBuffer buffer)
        {
            RenderTo(buffer);
            ClearFrameBuffer();
        }

        /// <summary>
        /// Renders a sprite without a texture, but with uploaded UVs.
        /// If no uvRect is provided 0, 1, 1, 0 is uploaded instead.
        /// </summary>
        public void RenderUVRect(Vector3 pos, Vector2 size, Color color, Rectangle? uvRect = null)
        {
            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad, Texture.NoTexture);

            vertices[0].Vertex = pos;
            vertices[1].Vertex = new Vector3(pos.X + size.X, pos.Y, pos.Z);
            vertices[2].Vertex = new Vector3(pos.X + size.X, pos.Y + size.Y, pos.Z);
            vertices[3].Vertex = new Vector3(pos.X, pos.Y + size.Y, pos.Z);

            uint c = color.ToUint();
            vertices[0].Color = c;
            vertices[1].Color = c;
            vertices[2].Color = c;
            vertices[3].Color = c;

            Rectangle uv = uvRect ?? new Rectangle(0, 1, 1, 0);

            vertices[0].UV = new Vector2(uv.X, uv.Y);
            vertices[1].UV = new Vector2(uv.Width, uv.Y);
            vertices[2].UV = new Vector2(uv.Width, uv.Height);
            vertices[3].UV = new Vector2(uv.X, uv.Height);
        }

        /// <summary>
        /// Render a framebuffer's color attachment to the currently bound buffer.
        /// If you're "blitting" make sure the view matrix is disabled.
        /// </summary>
        /// <param name="buffer">The buffer to render.</param>
        /// <param name="renderSizeOverwrite">
        /// By default the rendered quad will be the same size as the framebuffer. You can change
        /// that using this parameter.
        /// </param>
        /// <param name="pos">The position to render to. By default this is 0,0</param>
        /// <param name="uv">The part of the framebuffer to render. By default this is the whole thing.</param>
        /// <param name="attachment">The buffer attachment to render. ColorAttachment by default.</param>
        /// <param name="color">The color to render in. White by default.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderFrameBuffer(FrameBuffer buffer, Vector2? renderSizeOverwrite = null, Vector3? pos = null, Rectangle? uv = null, Texture attachment = null, Color? color = null)
        {
            // todo: deprecate along with overload.
            RenderSprite(pos ?? Vector3.Zero, renderSizeOverwrite ?? buffer.Size, color ?? Color.White, attachment ?? buffer.ColorAttachment, uv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderFrameBuffer(FrameBuffer buffer, Color? color)
        {
            RenderFrameBuffer(buffer, null, null, null, null, color);
        }

        public void RenderRoundedRect(Vector3 pos, Vector2 size, Color c, float radius = 7f)
        {
            float mn = MathF.Min(size.X, size.Y) / 2.0f;
            radius = MathF.Min(mn, radius);

            float z = pos.Z;
            var r = new Rectangle(pos.X, pos.Y, size.X, size.Y);
            Rectangle inner = r.Clone().Deflate(radius, radius);

            RenderCircle(inner.TopLeft.ToVec3(z), radius, c, true);
            RenderCircle(inner.TopRight.ToVec3(z), radius, c, true);
            RenderCircle(inner.BottomLeft.ToVec3(z), radius, c, true);
            RenderCircle(inner.BottomRight.ToVec3(z), radius, c, true);

            RenderSprite(new Vector3(r.X + radius, r.Y, z), new Vector2(r.Width - radius * 2, r.Height), c);
            RenderSprite(new Vector3(r.X, r.Y + radius, z), new Vector2(r.Width, r.Height - radius * 2), c);
        }

        public void RenderRoundedRectSdf(Vector3 pos, Vector2 size, Color c, float radius = 7f)
        {
            var roundedRectShader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/RoundedRectangle.xml");

            SetShader(roundedRectShader.Shader);
            roundedRectShader.Shader.SetUniformVector2("RectSize", size);
            roundedRectShader.Shader.SetUniformFloat("RadiusPixels", radius * 2);
            RenderUVRect(pos, size, c);
            SetShader();
        }
    }
}