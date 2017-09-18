using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raya.Graphics.Primitives;
using Soul.Engine;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Objects;

namespace Soul.Examples.Basic
{
    public class ShapeTest : Scene
    {

        public static void Main(string[] args)
        {
            // Start the engine.
            Core.Start(new ShapeTest(), "shapeTest");
        }

        public override void Initialize()
        {
            // Shapes.

            GameObject circle = new GameObject();
            circle.AddChild("circle", new BasicShape());
            circle.GetChild<BasicShape>("circle").Type = ShapeType.Circle;
            circle.Position = new Vector2(50, 50);
            circle.Size = new Vector2(50, 50);

            AddChild("circle", circle);

            GameObject line = new GameObject();
            line.AddChild("line", new BasicShape());
            line.GetChild<BasicShape>("line").Type = ShapeType.Line;
            line.Position = new Vector2(150, 50);
            line.Size = new Vector2(200, 50);

            AddChild("line", line);

            GameObject rect = new GameObject();
            rect.AddChild("rect", new BasicShape());
            rect.GetChild<BasicShape>("rect").Type = ShapeType.Rectangle;
            rect.Position = new Vector2(250, 50);
            rect.Size = new Vector2(50, 50);

            AddChild("rect", rect);

            GameObject tri = new GameObject();
            tri.AddChild("tri", new BasicShape());
            tri.GetChild<BasicShape>("tri").Type = ShapeType.Triangle;
            tri.Position = new Vector2(350, 50);
            tri.Size = new Vector2(50, 50);

            AddChild("tri", tri);

            GameObject poly = new GameObject();
            poly.AddChild("poly", new BasicShape());
            poly.GetChild<BasicShape>("poly").Type = ShapeType.Polygon;
            Vector2[] vert = { new Vector2(9, -9), new Vector2(15, 7), new Vector2(2, 16), new Vector2(-6, 19), new Vector2(-19, 5), new Vector2(-20, -4), new Vector2(-11, -14), new Vector2(-4, -15), new Vector2(4, -15) };
            poly.GetChild<BasicShape>().PolygonVertices = vert;
            poly.Position = new Vector2(450, 50);

            AddChild("poly", poly);

            // Spinning shapes.

            GameObject spinCircle = new GameObject();
            spinCircle.Position = new Vector2(50, 150);
            spinCircle.Size = new Vector2(50, 50);
            spinCircle.AddChild("spinCircle", new BasicShape(ShapeType.Circle));

            AddChild("spinCircle", spinCircle);

            GameObject spinLine = new GameObject();
            spinLine.Position = new Vector2(150, 150);
            spinLine.Size = new Vector2(200, 150);
            spinLine.AddChild("spinLine", new BasicShape(ShapeType.Line));

            AddChild("spinLine", spinLine);

            GameObject spinRect = new GameObject();
            spinRect.Position = new Vector2(250, 150);
            spinRect.Size = new Vector2(50, 50);
            spinRect.AddChild("spinRect", new BasicShape(ShapeType.Rectangle));

            AddChild("spinRect", spinRect);

            GameObject spinTri = new GameObject();
            spinTri.Position = new Vector2(350, 150);
            spinTri.Size = new Vector2(50, 50);
            spinTri.AddChild("spinTri", new BasicShape(ShapeType.Triangle));

            AddChild("spinTri", spinTri);

            GameObject spinPoly = new GameObject();
            spinPoly.Position = new Vector2(450, 150);
            spinPoly.AddChild("spinPoly", new BasicShape(ShapeType.Polygon, vert));

            AddChild("spinPoly", spinPoly);

            // Spin the spinning objects with a script.
            ScriptEngine.Expose("spinCircle", spinCircle);
            ScriptEngine.Expose("spinLine", spinLine);
            ScriptEngine.Expose("spinRect", spinRect);
            ScriptEngine.Expose("spinTri", spinTri);
            ScriptEngine.Expose("spinPoly", spinPoly);
            ScriptEngine.RunScript("register(function() {" +
                                        " spinCircle.Rotation += 0.1; " +
                                        " spinLine.Rotation += 0.1; " +
                                        " spinRect.Rotation += 0.1; " +
                                        " spinTri.Rotation += 0.1; " +
                                        " spinPoly.Rotation += 0.1; " +
                                        "});");

            // Outlined shapes.

            GameObject outlineCircle = new GameObject();
            outlineCircle.AddChild("outlineCircle", new BasicShape());
            outlineCircle.GetChild<BasicShape>("outlineCircle").Type = ShapeType.Circle;
            outlineCircle.Position = new Vector2(50, 250);
            outlineCircle.Size = new Vector2(50, 50);
            outlineCircle.GetChild<BasicShape>().OutlineColor = Raya.Graphics.Color.Black;
            outlineCircle.GetChild<BasicShape>().OutlineThickness = 2;

            AddChild("outlineCircle", outlineCircle);

            GameObject outlineLine = new GameObject();
            outlineLine.AddChild("outlineLine", new BasicShape());
            outlineLine.GetChild<BasicShape>("outlineLine").Type = ShapeType.Line;
            outlineLine.Position = new Vector2(150, 250);
            outlineLine.Size = new Vector2(200, 250);
            outlineLine.GetChild<BasicShape>().OutlineColor = Raya.Graphics.Color.Black;
            outlineLine.GetChild<BasicShape>().OutlineThickness = 2;

            AddChild("outlineLine", outlineLine);

            GameObject outlineRect = new GameObject();
            outlineRect.AddChild("outlineRect", new BasicShape());
            outlineRect.GetChild<BasicShape>("outlineRect").Type = ShapeType.Rectangle;
            outlineRect.Position = new Vector2(250, 250);
            outlineRect.Size = new Vector2(50, 50);
            outlineRect.GetChild<BasicShape>().OutlineColor = Raya.Graphics.Color.Black;
            outlineRect.GetChild<BasicShape>().OutlineThickness = 2;

            AddChild("outlineRect", outlineRect);

            GameObject outlineTri = new GameObject();
            outlineTri.AddChild("outlineTri", new BasicShape());
            outlineTri.GetChild<BasicShape>("outlineTri").Type = ShapeType.Triangle;
            outlineTri.Position = new Vector2(350, 250);
            outlineTri.Size = new Vector2(50, 50);
            outlineTri.GetChild<BasicShape>().OutlineColor = Raya.Graphics.Color.Black;
            outlineTri.GetChild<BasicShape>().OutlineThickness = 2;

            AddChild("outlineTri", outlineTri);

            GameObject outlinePoly = new GameObject();
            outlinePoly.AddChild("outlinePoly", new BasicShape());
            outlinePoly.GetChild<BasicShape>("outlinePoly").Type = ShapeType.Polygon;
            outlinePoly.GetChild<BasicShape>().PolygonVertices = vert;
            outlinePoly.Position = new Vector2(450, 250);
            outlinePoly.GetChild<BasicShape>().OutlineColor = Raya.Graphics.Color.Black;
            outlinePoly.GetChild<BasicShape>().OutlineThickness = 2;

            AddChild("outlinePoly", outlinePoly);

            // Color changing shapes

            GameObject colorCircle = new GameObject();
            colorCircle.AddChild("colorCircle", new BasicShape());
            colorCircle.GetChild<BasicShape>("colorCircle").Type = ShapeType.Circle;
            colorCircle.GetChild<BasicShape>("colorCircle").Color =
                new Raya.Graphics.Color(Functions.GenerateRandomNumber(0, 255), Functions.GenerateRandomNumber(0, 255),
                    Functions.GenerateRandomNumber(0, 255));
            colorCircle.Position = new Vector2(50, 350);
            colorCircle.Size = new Vector2(50, 50);

            AddChild("colorCircle", colorCircle);

            GameObject colorLine = new GameObject();
            colorLine.AddChild("colorLine", new BasicShape());
            colorLine.GetChild<BasicShape>("colorLine").Type = ShapeType.Line;
            colorLine.GetChild<BasicShape>("colorLine").Color =
                new Raya.Graphics.Color(Functions.GenerateRandomNumber(0, 255), Functions.GenerateRandomNumber(0, 255),
                    Functions.GenerateRandomNumber(0, 255));
            colorLine.Position = new Vector2(150, 350);
            colorLine.Size = new Vector2(200, 350);

            AddChild("colorLine", colorLine);

            GameObject colorRect = new GameObject();
            colorRect.AddChild("colorRect", new BasicShape());
            colorRect.GetChild<BasicShape>("colorRect").Type = ShapeType.Rectangle;
            colorRect.GetChild<BasicShape>("colorRect").Color =
                new Raya.Graphics.Color(Functions.GenerateRandomNumber(0, 255), Functions.GenerateRandomNumber(0, 255),
                    Functions.GenerateRandomNumber(0, 255));
            colorRect.Position = new Vector2(250, 350);
            colorRect.Size = new Vector2(50, 50);

            AddChild("colorRect", colorRect);

            GameObject colorTri = new GameObject();
            colorTri.AddChild("colorTri", new BasicShape());
            colorTri.GetChild<BasicShape>("colorTri").Type = ShapeType.Triangle;
            colorTri.GetChild<BasicShape>("colorTri").Color =
                new Raya.Graphics.Color(Functions.GenerateRandomNumber(0, 255), Functions.GenerateRandomNumber(0, 255),
                    Functions.GenerateRandomNumber(0, 255));
            colorTri.Position = new Vector2(350, 350);
            colorTri.Size = new Vector2(50, 50);

            AddChild("colorTri", colorTri);

            GameObject colorPoly = new GameObject();
            colorPoly.AddChild("colorPoly", new BasicShape());
            colorPoly.GetChild<BasicShape>("colorPoly").Type = ShapeType.Polygon;
            colorPoly.GetChild<BasicShape>("colorPoly").Color =
                new Raya.Graphics.Color(Functions.GenerateRandomNumber(0, 255), Functions.GenerateRandomNumber(0, 255),
                    Functions.GenerateRandomNumber(0, 255));
            colorPoly.GetChild<BasicShape>().PolygonVertices = vert;
            colorPoly.Position = new Vector2(450, 350);

            AddChild("colorPoly", colorPoly);

            // Change the shape colors with a script.
            ScriptEngine.Expose("colorCircle", colorCircle);
            ScriptEngine.Expose("colorLine", colorLine);
            ScriptEngine.Expose("colorRect", colorRect);
            ScriptEngine.Expose("colorTri", colorTri);
            ScriptEngine.Expose("colorPoly", colorPoly);
            ScriptEngine.Expose("ChangeColor", (Action<GameObject>)ChangeColor);
            ScriptEngine.RunScript("register(function() {" +
                                        " ChangeColor(colorCircle); " +
                                        " ChangeColor(colorLine); " +
                                        " ChangeColor(colorRect); " +
                                        " ChangeColor(colorTri); " +
                                        " ChangeColor(colorPoly); " +
                                        "});");
        }

        public override void Update()
        {

        }

        private void ChangeColor(GameObject obj)
        {
            obj.GetChild<BasicShape>().Color = new Raya.Graphics.Color(obj.GetChild<BasicShape>().Color.R + 1, obj.GetChild<BasicShape>().Color.G + 1, obj.GetChild<BasicShape>().Color.B + 1);
        }
    }
}
