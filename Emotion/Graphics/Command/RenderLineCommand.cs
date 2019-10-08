#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Graphics.Command
{
    public class RenderLineCommand : RenderSpriteCommand
    {
        public Vector3 PointOne;
        public Vector3 PointTwo;
        public float Thickness;

        public RenderLineCommand()
        {
            Vertices[0].Tid = -1;
            Vertices[1].Tid = -1;
            Vertices[2].Tid = -1;
            Vertices[3].Tid = -1;
        }

        public override void Process()
        {
            Vector2 normal = Vector2.Normalize(new Vector2(PointTwo.Y - PointOne.Y, -(PointTwo.X - PointOne.X))) * Thickness;
            float z = Math.Max(PointOne.Z, PointTwo.Z);

            Vertices[0].Vertex = new Vector3(PointOne.X + normal.X, PointOne.Y + normal.Y, z);
            Vertices[1].Vertex = new Vector3(PointTwo.X + normal.X, PointTwo.Y + normal.Y, z);
            Vertices[2].Vertex = new Vector3(PointTwo.X - normal.X, PointTwo.Y - normal.Y, z);
            Vertices[3].Vertex = new Vector3(PointOne.X - normal.X, PointOne.Y - normal.Y, z);

            Vertices[0].Color = Color;
            Vertices[1].Color = Color;
            Vertices[2].Color = Color;
            Vertices[3].Color = Color;
        }

        public override void Execute(RenderComposer composer)
        {
        }
    }
}