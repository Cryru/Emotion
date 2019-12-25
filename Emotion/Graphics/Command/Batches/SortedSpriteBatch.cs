#region Using

using System;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Graphics.Command.Batches
{
    /// <summary>
    /// Sorts batched sprites by their Z position.
    /// Used for semi-opaque sprites.
    /// </summary>
    public class SortedSpriteBatch : SpriteBatch
    {
        public override unsafe void Process(RenderComposer composer)
        {
            var data = new Span<VertexDataSprite>((void*) _batchedVertices, _mappedTo / 4);

            // Temp sort until https://github.com/dotnet/corefx/issues/15329 reaches us.
            QuickSort(data, 0, data.Length - 1);

            base.Process(composer);
        }

        #region Temp Sorting

        private static void QuickSort(Span<VertexDataSprite> iInput, int start, int end)
        {
            while (true)
            {
                if (start >= end) return;
                int pivot = Partition(iInput, start, end);
                QuickSort(iInput, start, pivot - 1);
                start = pivot + 1;
            }
        }

        private static int Partition(Span<VertexDataSprite> iInput, int start, int end)
        {
            VertexDataSprite pivot = iInput[end];
            int pIndex = start;

            for (int i = start; i < end; i++)
            {
                if (!(iInput[i].FirstVertex.Vertex.Z <= pivot.FirstVertex.Vertex.Z)) continue;
                VertexDataSprite temp = iInput[i];
                iInput[i] = iInput[pIndex];
                iInput[pIndex] = temp;
                pIndex++;
            }

            VertexDataSprite anotherTemp = iInput[pIndex];
            iInput[pIndex] = iInput[end];
            iInput[end] = anotherTemp;
            return pIndex;
        }

        #endregion
    }
}