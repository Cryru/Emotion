#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// Advanced routines, and routines using base functions to be used along with shaders and such.
    /// </summary>
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
        /// The top right position of the imaginary rectangle which encompasses the circle. Can be modified
        /// with "useCenter"
        /// </param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="useCenter">Whether the position should instead be the center of the circle.</param>
        /// <param name="circleDetail">How detailed the circle should be.</param>
        public void RenderCircle(Vector3 position, float radius, Color color, bool useCenter = false, int circleDetail = 30)
        {
            var vertsNeeded = (uint) ((circleDetail + 1) * 3);
            RenderBatch<VertexData> batch = GetBatch(BatchMode.SequentialTriangles, vertsNeeded);
            Span<VertexData> vertices = batch.GetData(vertsNeeded, vertsNeeded);
            Debug.Assert(vertices != null);
            var vertexIdx = 0;

            Vector3 posOffset = useCenter ? new Vector3(position.X - radius, position.Y - radius, position.Z) : position;

            float pX = 0;
            float pY = 0;
            float fX = 0;
            float fY = 0;

            // Generate triangles.
            for (uint i = 0; i < circleDetail; i++)
            {
                var angle = (float) (i * 2 * Math.PI / circleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius + pX, radius + pY, 0);
                vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius + x, radius + y, 0);
                vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius, radius, 0);

                pX = x;
                pY = y;

                if (i == circleDetail - 1)
                {
                    vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius + pX, radius + pY, 0);
                    vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius + fX, radius + fY, 0);
                    vertices[vertexIdx++].Vertex = posOffset + new Vector3(radius, radius, 0);
                }

                if (i != 0) continue;
                fX = x;
                fY = y;
            }

            uint c = color.ToUint();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = c;
                vertices[i].Tid = -1;
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
            RenderBatch<VertexData> batch = GetBatch();
            Span<VertexData> vertices = batch.GetData(4, 6);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderFrameBuffer(FrameBuffer buffer, Vector2? renderSizeOverwrite = null, Vector3? pos = null)
        {
            RenderSprite(pos ?? Vector3.Zero, renderSizeOverwrite ?? buffer.Size, Color.White, buffer.ColorAttachment, new Rectangle(0, 0, buffer.Size));
        }
    }
}