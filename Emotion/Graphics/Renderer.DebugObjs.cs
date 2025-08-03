#nullable enable

#region Using

using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Standard.MeshGenerators;

#endregion

namespace Emotion.Graphics;

public sealed partial class Renderer
{
    private List<Vector3>? _triangles;
    private List<Mesh>? _spheres;
    private List<(Vector3, Vector3)>? _lines;
    private List<Cube>? _cubes;

    private static Color _defaultDbgObjectColor = Color.Green * 0.5f;

    public void DbgAddTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        if (!Engine.Configuration.DebugMode) return;
        _triangles ??= new();
        _triangles.Add(p1);
        _triangles.Add(p2);
        _triangles.Add(p3);
    }

    public void DbgAddPoint(Vector3 p, float radius = 0.5f, Color? color = null)
    {
        if (!Engine.Configuration.DebugMode) return;

        color ??= _defaultDbgObjectColor;

        var meshGen = new SphereMeshGenerator();
        Mesh sphereMesh = meshGen.GenerateMesh().TransformMeshVertices(
            Matrix4x4.CreateScale(radius) * Matrix4x4.CreateTranslation(p)
        );
        sphereMesh.Material = new MeshMaterial()
        {
            DiffuseColor = color.Value
        };

        _spheres ??= new();
        _spheres.Add(sphereMesh);
    }

    public void DbgAddCube(Cube cube)
    {
        if (!Engine.Configuration.DebugMode) return;

        _cubes ??= new();
        _cubes.Add(cube);
    }

    public void DbgAddLine(Vector3 p, Vector3 p2, bool relative = false)
    {
        if (!Engine.Configuration.DebugMode) return;

        _lines ??= new();
        _lines.Add((p, relative ? p2 + p : p2));
    }

    public void DbgAddRectangle(Rectangle rect)
    {
        if (!Engine.Configuration.DebugMode) return;

        _lines ??= new();

        Vector2 position = rect.Position;
        Vector2 size = rect.Size;

        Vector3 nn = position.ToVec3();
        Vector3 pn = new Vector3(position.X + size.X, position.Y, 0);
        Vector3 np = new Vector3(position.X, position.Y + size.Y, 0);
        Vector3 pp = new Vector3(position.X + size.X, position.Y + size.Y, 0);

        _lines.Add((nn, pn));
        _lines.Add((pn, pp));
        _lines.Add((pp, np));
        _lines.Add((np, nn));
    }

    public void DbgClear()
    {
        _triangles?.Clear();
        _spheres?.Clear();
        _lines?.Clear();
        _cubes?.Clear();
    }

    public void RenderDebugObjects()
    {
        SetUseViewMatrix(true);

        if (_triangles != null && _triangles.Count != 0)
        {
            Span<VertexData> memory = RenderStream.GetStreamMemory((uint)_triangles.Count, BatchMode.SequentialTriangles);
            for (var i = 0; i < memory.Length; i++)
            {
                memory[i].Vertex = _triangles[i];
                memory[i].Color = _defaultDbgObjectColor.ToUint();
                memory[i].UV = Vector2.Zero;
            }
        }

        if (_spheres != null)
            for (var i = 0; i < _spheres.Count; i++)
            {
                _spheres[i].Render(this);
            }

        if (_lines != null)
            for (int i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                RenderLine(line.Item1, line.Item2, _defaultDbgObjectColor, 1);
            }

        if (_cubes != null)
            for (int i = 0; i < _cubes.Count; i++)
            {
                var cube = _cubes[i];
                cube.RenderOutline(this, _defaultDbgObjectColor, 0.05f);
            }
    }
}