using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Emotion.Primitives
{
    /// <summary>
    /// A plane facing the Z axis, with the origin in the middle.
    /// </summary>
    public class Plane3D : Transform3D
    {
        public Color Tint = Color.White;

        public Plane3D()
        {

        }

        public void Render(RenderComposer c)
        {
            c.PushModelMatrix(_scaleMatrix * _rotationMatrix * _translationMatrix);

            Span<VertexData> vertices = c.RenderStream.GetStreamMemory(4, BatchMode.Quad, null);
            vertices[0].Vertex = new Vector3(-0.5f, -0.5f, 0);
            vertices[1].Vertex = new Vector3(0.5f, -0.5f, 0);
            vertices[2].Vertex = new Vector3(0.5f, 0.5f, 0);
            vertices[3].Vertex = new Vector3(-0.5f, 0.5f, 0);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = Tint.ToUint();
                vertices[i].UV = Vector2.Zero;
            }

            c.PopModelMatrix();
        }
    }
}
