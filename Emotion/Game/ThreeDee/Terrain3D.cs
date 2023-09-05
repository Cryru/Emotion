#region Using

using System;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.ThreeDee
{
    public class Terrain3D
    {
        private static VertexData[] _terrainMesh;
        private static ushort[] _terrainIndices;

        public Terrain3D(int width, int height, int tileSize)
        {
            _terrainMesh = new VertexData[width * height];
            Vector3 pen = Vector3.Zero;
            for (var i = 0; i < _terrainMesh.Length; i++)
            {
                _terrainMesh[i] = new VertexData
                {
                    Vertex = pen * new Vector3(tileSize),
                    Color = (Color.White * 0.60f).ToUint()
                };

                pen.X++;
                //pen.Y = pen.Y + Helpers.GenerateRandomNumber(-100, 100) / 1000f;
                if (pen.X >= width)
                {
                    pen.Y++;
                    pen.X = 0;
                }
            }

            var centerMatrix = Matrix4x4.CreateTranslation(-width / 2 * tileSize, -height / 2 * tileSize, 0);
            for (var i = 0; i < _terrainMesh.Length; i++)
            {
                _terrainMesh[i].Vertex = Vector3.Transform(_terrainMesh[i].Vertex, centerMatrix);
            }

            _terrainIndices = new ushort[(width - 1) * (height - 1) * 6];
            var indicesIndex = 0;
            for (var y = 0; y < width - 1; y++)
            {
                int yIndex = y * width;
                for (var x = 0; x < width - 1; x++)
                {
                    int absIndex = yIndex + x;
                    int lowerLine = (y + 1) * width;
                    int absIndexBelow = lowerLine + x;

                    _terrainIndices[indicesIndex] = (ushort) (absIndex + 0);
                    _terrainIndices[indicesIndex + 1] = (ushort) (absIndex + 1);
                    _terrainIndices[indicesIndex + 2] = (ushort) (absIndexBelow + 1);
                    _terrainIndices[indicesIndex + 3] = (ushort) (absIndexBelow + 1);
                    _terrainIndices[indicesIndex + 4] = (ushort) (absIndexBelow + 0);
                    _terrainIndices[indicesIndex + 5] = (ushort) (absIndex + 0);

                    indicesIndex += 6;
                }
            }
        }

        public void Render(RenderComposer c)
        {
            StreamData<VertexData> memory = c.RenderStream.GetStreamMemory((uint)_terrainMesh.Length, (uint)_terrainIndices.Length, BatchMode.Quad);
            new Span<VertexData>(_terrainMesh).CopyTo(memory.VerticesData);
            new Span<ushort>(_terrainIndices).CopyTo(memory.IndicesData);
            for (var i = 0; i < memory.IndicesData.Length; i++)
            {
                memory.IndicesData[i] += memory.StructIndex;
            }
        }
    }
}