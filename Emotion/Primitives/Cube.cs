#nullable enable

using Emotion.Graphics;
using Emotion.Graphics.Text.EmotionRenderer;

namespace Emotion.Primitives;

/// <summary>
/// Represents an AABB.
/// </summary>
public struct Cube
{
	public Vector3 Origin;
	public Vector3 HalfExtents;

	public Cube(Vector3 center, Vector3 halfExtent)
	{
		Origin = center;
		HalfExtents = halfExtent;
	}

	public Vector3[] GetVertices()
	{
		var vertices = new Vector3[8];
		vertices[0] = new Vector3(Origin.X - HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z - HalfExtents.Z);
		vertices[1] = new Vector3(Origin.X + HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z - HalfExtents.Z);
		vertices[2] = new Vector3(Origin.X - HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z - HalfExtents.Z);
		vertices[3] = new Vector3(Origin.X + HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z - HalfExtents.Z);
		vertices[4] = new Vector3(Origin.X - HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z + HalfExtents.Z);
		vertices[5] = new Vector3(Origin.X + HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z + HalfExtents.Z);
		vertices[6] = new Vector3(Origin.X - HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z + HalfExtents.Z);
		vertices[7] = new Vector3(Origin.X + HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z + HalfExtents.Z);
		return vertices;
	}

	public Cube Transform(Matrix4x4 mat)
	{
		var first = true;
		var min = new Vector3(0);
		var max = new Vector3(0);

		Vector3[] vertices = GetVertices();
		for (var i = 0; i < vertices.Length; i++)
		{
			Vector3 vertex = Vector3.Transform(vertices[i], mat);
			if (first)
			{
				min = vertex;
				max = vertex;
				first = false;
			}
			else
			{
				// Find the minimum and maximum extents of the vertices
				min = Vector3.Min(min, vertex);
				max = Vector3.Max(max, vertex);
			}
		}

		Vector3 center = (min + max) / 2f;

		Vector3 halfExtent = (max - min) / 2f;
		return new Cube(center, halfExtent);
	}

	private static int[][] _outlineEdges = new int[][]
	{
		new int[] { 0, 1 }, // Bottom face
		new int[] { 1, 3 },
		new int[] { 3, 2 },
		new int[] { 2, 0 },
		new int[] { 4, 5 }, // Top face
		new int[] { 5, 7 },
		new int[] { 7, 6 },
		new int[] { 6, 4 },
		new int[] { 0, 4 }, // Connecting lines
		new int[] { 1, 5 },
		new int[] { 2, 6 },
		new int[] { 3, 7 }
	};

	public void RenderOutline(RenderComposer c, Color? color = null)
	{
		Vector3[] vertices = GetVertices();
		foreach (int[] edge in _outlineEdges)
		{
			c.RenderLine(vertices[edge[0]], vertices[edge[1]], color ?? Color.White, 1, false);
		}
	}
}