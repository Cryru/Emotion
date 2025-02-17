#nullable enable

#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Graphics
{
    public sealed partial class RenderComposer
    {
        private List<Vector3>? _triangles;
        private List<Mesh>? _spheres;
        private List<(Vector3, Vector3)>? _lines;

        private static Color _defaultDbgObjectColor = Color.Green * 0.5f;

        public void DbgAddTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if (!Engine.Configuration.DebugMode) return;
            _triangles ??= new();
            _triangles.Add(p1);
            _triangles.Add(p2);
            _triangles.Add(p3);
        }

        public void DbgAddPoint(Vector3 p, float radius = 1f, Color? color = null)
        {
            if (!Engine.Configuration.DebugMode) return;

            color ??= _defaultDbgObjectColor;
            var meshGen = new SphereMeshGenerator();
            _spheres ??= new();
            _spheres.Add(meshGen.GenerateMesh().TransformMeshVertices(
                Matrix4x4.CreateScale(radius) * Matrix4x4.CreateTranslation(p)
            ).ColorMeshVertices(color.Value));
        }

        public void DbgAddLine(Vector3 p, Vector3 p2, bool relative = false)
        {
            if (!Engine.Configuration.DebugMode) return;

            _lines ??= new();
            _lines.Add((p, relative ? p2 + p : p2));
        }

        public void DbgClear()
        {
            _triangles?.Clear();
            _spheres?.Clear();
            _lines?.Clear();
        }

        public void RenderDebugObjects()
        {
            SetUseViewMatrix(true);

            if (_triangles != null && _triangles.Count != 0)
            {
                Span<VertexData> memory = RenderStream.GetStreamMemory((uint) _triangles.Count, BatchMode.SequentialTriangles);
                for (var i = 0; i < memory.Length; i++)
                {
                    memory[i].Vertex = _triangles[i];
                    memory[i].Color = _defaultDbgObjectColor.ToUint();
                    memory[i].UV = Vector2.Zero;
                }
            }

            if (_spheres != null && _spheres.Count > 0)
                for (var i = 0; i < _spheres.Count; i++)
                {
                    _spheres[i].Render(this);
                }

            if (_lines != null && _lines.Count > 0)
                for (int i = 0; i < _lines.Count; i++)
                {
                    var line = _lines[i];
                    RenderLine(line.Item1, line.Item2, _defaultDbgObjectColor, 1);
                }
        }
    }
}