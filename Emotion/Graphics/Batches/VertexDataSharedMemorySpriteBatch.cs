#region Using

using System;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Batches
{
    public class VertexDataSharedMemorySpriteBatch : SharedMemorySpriteBatch<VertexData>
    {
        /// <inheritdoc />
        public override Span<VertexData> GetData(Texture texture, out int texturePointer)
        {
            Span<VertexData> data = base.GetData(texture, out texturePointer);
            if (data == null) return null;

            // Set the Tid.
            for (var i = 0; i < data.Length; i++)
            {
                data[i].Tid = texturePointer;
            }

            return data;
        }
    }
}