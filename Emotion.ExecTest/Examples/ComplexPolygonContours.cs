#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.AStar;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest.Examples
{
    /// <summary>
    /// Examples for rendering complex polygons using contours.
    /// Kinda like what canvas2D does.
    /// </summary>
    public class ComplexPolygonContours : IScene
    {
        private Polygon[] _testPolygon = new Polygon[5];

        public void Load()
        {
            // Split your paths into separate polygon instances.
            var poly = new List<Vector2>();
            poly.Add(new Vector2(808, 0));
            poly.Add(new Vector2(100, 0));
            poly.Add(new Vector2(100, 1456));
            poly.Add(new Vector2(808, 1456));
            _testPolygon[0] = new Polygon(poly);

            poly.Clear();
            poly.Add(new Vector2(754, 84));
            poly.Add(new Vector2(754, 1371));
            poly.Add(new Vector2(480, 728));
            _testPolygon[1] = new Polygon(poly);

            poly.Clear();
            poly.Add(new Vector2(154, 1359));
            poly.Add(new Vector2(154, 96));
            poly.Add(new Vector2(422, 728));
            _testPolygon[2] = new Polygon(poly);

            poly.Clear();
            poly.Add(new Vector2(194, 54));
            poly.Add(new Vector2(709, 54));
            poly.Add(new Vector2(451, 660));
            _testPolygon[3] = new Polygon(poly);

            poly.Clear();
            poly.Add(new Vector2(451, 796));
            poly.Add(new Vector2(709, 1402));
            poly.Add(new Vector2(194, 1402));
            _testPolygon[4] = new Polygon(poly);
        }

        public void Update()
        {
        }

        public void Draw(RenderComposer composer)
        {
            // The vertices were taken from a font, and the polygon is huge.
            composer.PushModelMatrix(Matrix4x4.CreateScale(0.1f, 0.1f, 1f));

            composer.SetStencilTest(true);
            composer.StencilWindingStart();
            composer.ToggleRenderColor(false);

            // Draw all of them together. Winding will take care of the overlap.
            var accum = Rectangle.Empty;
            for (var i = 0; i < _testPolygon.Length; i++)
            {
                composer.RenderVertices(_testPolygon[i].Vertices, Color.White);
                accum = accum.Union(_testPolygon[i].Bounds2D);
            }

            composer.StencilWindingEnd();
            composer.ToggleRenderColor(true);

            // You need one quad which covers the bounds of all the polygons.
            // In this case I added the bounding boxes, but you could just draw a screen sized quad.
            composer.RenderSprite(accum, Color.Red);

            composer.SetStencilTest(false);

            composer.PopModelMatrix();
        }

        public void Unload()
        {
        }
    }
}