#region Using

using System;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Command.Batches
{
    public class VertexDataShaderMemorySpriteBatch : SharedMemorySpriteBatch<VertexData>
    {
        /// <inheritdoc />
        public override Span<VertexData> GetData(Texture texture, out int texturePointer)
        {
            Span<VertexData> data = base.GetData(texture, out texturePointer);

            // Set the Tid.
            for (var i = 0; i < data.Length; i++)
            {
                data[i].Tid = texturePointer;
            }

            return data;
        }
    }
}