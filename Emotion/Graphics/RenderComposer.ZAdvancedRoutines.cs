#region Using

using System;
using System.Numerics;
using Emotion.Game.Text;
using Emotion.Graphics.Command.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Text;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// Advanced sprite routines to be used along with shaders and such.
    /// </summary>
    public sealed partial class RenderComposer
    {
        /// <summary>
        /// Renders a sprite without a texture, but with uploaded UVs.
        /// If no uvRect is provided 0, 1, 1, 0 is uploaded instead.
        /// </summary>
        public void RenderUVRect(Vector3 pos, Vector2 size, Color color, Rectangle? uvRect = null)
        {
            SpriteBatchBase<VertexData> batch = GetBatch();
            Span<VertexData> vertices = batch.GetData(null);

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
    }
}