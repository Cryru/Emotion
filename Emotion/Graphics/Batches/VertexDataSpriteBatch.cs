#region Using

using System;
using System.Diagnostics;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Batches
{
    public class VertexDataSpriteBatch : SpriteBatch<VertexData>
    {
        /// <inheritdoc />
        public VertexDataSpriteBatch(bool ownGraphicsMemory = false) : base(ownGraphicsMemory)
        {
        }

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

        /// <inheritdoc />
        public override unsafe void RemapTextures()
        {
            var binding = new uint[_textureBinding.Length];
            Array.Copy(_textureBinding, 0, binding, 0, _textureBinding.Length);

            _textureSlotUtilization = 0;
            var data = new Span<VertexData>((void*) _batchedVertices, _mappedTo);
            for (var i = 0; i < data.Length; i++)
            {
                ref VertexData cur = ref data[i];

                // Check if any texture.
                if (cur.Tid == -1) continue;

                Debug.Assert(cur.Tid >= 0 && cur.Tid < binding.Length);

                // Get texture pointer.
                uint oldBindingPtr = binding[(int) cur.Tid];

                // Find within new binding.
                int newIdx = -1;
                for (var b = 0; b < _textureSlotUtilization; b++)
                {
                    if (_textureBinding[b] == oldBindingPtr) newIdx = b;
                }

                // If not found - add it.
                if (newIdx == -1)
                {
                    _textureBinding[_textureSlotUtilization] = oldBindingPtr;
                    newIdx = _textureSlotUtilization;
                    _textureSlotUtilization++;

                    Debug.Assert(_textureSlotUtilization < _textureBinding.Length);
                }

                // Amend vertex.
                cur.Tid = newIdx;
            }
        }
    }
}