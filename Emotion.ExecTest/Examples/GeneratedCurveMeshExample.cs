#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest.Examples
{
    public class GeneratedCurveMeshExample : Scene
    {
        private Vector3 _curveApex = new Vector3(50, -100, 0);

        public override Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public override void Update()
        {
            if (Engine.Host.IsKeyHeld(Key.MouseKeyLeft)) _curveApex = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition).ToVec3();
        }

        public override void Draw(RenderComposer composer)
        {
            // Draw triangle representation.
            Span<VertexData> memory = composer.RenderStream.GetStreamMemory(3, BatchMode.SequentialTriangles);
            memory[0].Vertex = new Vector3(0);
            memory[1].Vertex = new Vector3(100, 0, 0);
            memory[2].Vertex = _curveApex;
            for (var j = 0; j < memory.Length; j++)
            {
                memory[j].UV = Vector2.Zero;
                memory[j].Color = (Color.Red * 0.5f).ToUint();
            }

            // Draw curve.
            Span<VertexData> data = composer.GetStreamedQuadraticCurveMesh(Vector3.Zero, new Vector3(100, 0, 0), _curveApex);
            for (var j = 0; j < data.Length; j++)
            {
                data[j].UV = Vector2.Zero;
                data[j].Color = (Color.Blue * 0.5f).ToUint();
            }
        }
    }
}