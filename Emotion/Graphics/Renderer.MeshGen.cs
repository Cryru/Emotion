#region Using

using System;
using System.Numerics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Graphics
{
    public sealed partial class RenderComposer
    {
        /// <summary>
        /// Generate a quadratic curve mesh within the render stream. Returns the memory gotten.
        /// The number of vertices generated is resolution param + 1.
        /// </summary>
        /// <param name="start">Curve start.</param>
        /// <param name="end">Curve end.</param>
        /// <param name="ctrlP1">Control point.</param>
        /// <param name="resolution">Mesh detail.</param>
        /// <returns>RenderStream memory with a generated triangle fan quadratic curve mesh.</returns>
        public Span<VertexData> GetStreamedQuadraticCurveMesh(Vector3 start, Vector3 end, Vector3 ctrlP1, int resolution = 12)
        {
            float incr = 1.0f / (resolution - 1); // Points are denser around curve, so t values work fine.
#if false
            for (var i = 0; i < resolution; i++)
            {
                float length = i == resolution - 1 ? 1.0f : incr * i;
                Vector3 sC = Vector3.Lerp(start, ctrlP1, length);
                Vector3 cE = Vector3.Lerp(ctrlP1, end, length);
                Vector3 curveP = Vector3.Lerp(sC, cE, length);
                RenderCircle(curveP, 2, Emotion.Primitives.Color.Pink, true);
            }
#endif

            Span<VertexData> memory = RenderStream.GetStreamMemory((uint)(resolution + 1), BatchMode.TriangleFan);
            memory[0].Vertex = Vector3.Lerp(start, end, 0.5f);

            for (var i = 0; i < resolution; i++)
            {
                float length = i == resolution - 1 ? 1.0f : incr * i;

                // De Casteljau's algorithm
                Vector3 sC = Vector3.Lerp(start, ctrlP1, length);
                Vector3 cE = Vector3.Lerp(ctrlP1, end, length);
                Vector3 curveP = Vector3.Lerp(sC, cE, length);
                memory[i + 1].Vertex = curveP;
            }

            return memory;
        }

        /// <summary>
        /// Generate a cubic curve mesh within the render stream. Returns the memory gotten.
        /// </summary>
        /// <param name="start">Curve start.</param>
        /// <param name="end">Curve end.</param>
        /// <param name="ctrlP1">Control point.</param>
        /// <param name="ctrlP2">Second control point.</param>
        /// <param name="resolution">Mesh detail.</param>
        /// <returns>RenderStream memory with a generated triangle fan cubic curve mesh.</returns>
        public Span<VertexData> GetStreamedCubicCurveMesh(Vector3 start, Vector3 end, Vector3 ctrlP1, Vector3 ctrlP2, int resolution = 12)
        {
            Span<VertexData> memory = RenderStream.GetStreamMemory((uint)(resolution + 1), BatchMode.TriangleFan);
            memory[0].Vertex = Vector3.Lerp(start, end, 0.5f);

            float incr = 1.0f / (resolution - 1);
            for (var i = 0; i < resolution; i++)
            {
                float length = i == resolution - 1 ? 1.0f : incr * i;
                Vector3 sC1 = Vector3.Lerp(start, ctrlP1, length);
                Vector3 c1C2 = Vector3.Lerp(ctrlP1, ctrlP2, length);
                Vector3 c2E = Vector3.Lerp(ctrlP2, end, length);

                Vector3 sC1C1C2 = Vector3.Lerp(sC1, c1C2, length);
                Vector3 c1C2C2E = Vector3.Lerp(c1C2, c2E, length);

                Vector3 curveP = Vector3.Lerp(sC1C1C2, c1C2C2E, length);
                memory[i + 1].Vertex = curveP;
            }

            return memory;
        }
    }
}