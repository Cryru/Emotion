// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Primitives;
using OpenTK;
using Soul;
using Soul.Engine;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

#endregion

namespace Examples.Basic
{
    public class ShapeTest : Scene
    {
        public static void Main(string[] args)
        {
            // Start the engine.
            Core.Setup(new ShapeTest());
        }

        protected override void Setup()
        {
            Vector2[] customPoly =
            {
                new Vector2(9, -9),
                new Vector2(15, 7),
                new Vector2(2, 16),
                new Vector2(-6, 19),
                new Vector2(-19, 5),
                new Vector2(-20, -4),
                new Vector2(-11, -14),
                new Vector2(-4, -15),
                new Vector2(4, -15)
            };

            // Normal shapes

            Entity circle = new Entity("circle");
            circle.AttachComponent<Transform>();
            circle.GetComponent<Transform>().Position = new Vector2(50, 50);
            circle.GetComponent<Transform>().Size = new Vector2(50, 50);
            circle.AttachComponent<RenderData>();
            circle.GetComponent<RenderData>().ApplyTemplate_Circle();

            AddEntity(circle);

            Entity line = new Entity("line");
            line.AttachComponent<Transform>();
            line.GetComponent<Transform>().Position = new Vector2(0, 0);
            line.GetComponent<Transform>().Size = new Vector2(1, 1);
            line.AttachComponent<RenderData>();
            line.GetComponent<RenderData>().SetPointCount(2);
            line.GetComponent<RenderData>().SetPoint(0, new Vector2(150, 50));
            line.GetComponent<RenderData>().SetPoint(1, new Vector2(200, 50));

            AddEntity(line);

            Entity rect = new Entity("rect");
            rect.AttachComponent<Transform>();
            rect.GetComponent<Transform>().Position = new Vector2(250, 50);
            rect.GetComponent<Transform>().Size = new Vector2(50, 50);
            rect.AttachComponent<RenderData>();
            rect.GetComponent<RenderData>().ApplyTemplate_Rectangle();

            AddEntity(rect);

            Entity tri = new Entity("tri");
            tri.AttachComponent<Transform>();
            tri.GetComponent<Transform>().Position = new Vector2(350, 50);
            tri.GetComponent<Transform>().Size = new Vector2(50, 50);
            tri.AttachComponent<RenderData>();
            tri.GetComponent<RenderData>().ApplyTemplate_Triangle();

            AddEntity(tri);

            Entity poly = new Entity("poly");
            poly.AttachComponent<Transform>();
            poly.GetComponent<Transform>().Position = new Vector2(450, 50);
            poly.GetComponent<Transform>().Size = new Vector2(1, 1);
            poly.AttachComponent<RenderData>();
            poly.GetComponent<RenderData>().SetVertices(customPoly);

            AddEntity(poly);

            // Spinning shapes.

            Entity spinCircle = new Entity("spinCircle");
            spinCircle.AttachComponent<Transform>();
            spinCircle.GetComponent<Transform>().Position = new Vector2(50, 150);
            spinCircle.GetComponent<Transform>().Size = new Vector2(50, 50);
            spinCircle.AttachComponent<RenderData>();
            spinCircle.GetComponent<RenderData>().ApplyTemplate_Circle();

            AddEntity(spinCircle);

            Entity spinLine = new Entity("spinLine");
            spinLine.AttachComponent<Transform>();
            spinLine.GetComponent<Transform>().Position = new Vector2(0, 0);
            spinLine.GetComponent<Transform>().Size = new Vector2(1, 1);
            spinLine.AttachComponent<RenderData>();
            spinLine.GetComponent<RenderData>().SetPointCount(2);
            spinLine.GetComponent<RenderData>().SetPoint(0, new Vector2(150, 150));
            spinLine.GetComponent<RenderData>().SetPoint(1, new Vector2(200, 150));

            AddEntity(spinLine);

            Entity spinRect = new Entity("spinRect");
            spinRect.AttachComponent<Transform>();
            spinRect.GetComponent<Transform>().Position = new Vector2(250, 150);
            spinRect.GetComponent<Transform>().Size = new Vector2(50, 50);
            spinRect.AttachComponent<RenderData>();
            spinRect.GetComponent<RenderData>().ApplyTemplate_Rectangle();

            AddEntity(spinRect);

            Entity spinTri = new Entity("spinTri");
            spinTri.AttachComponent<Transform>();
            spinTri.GetComponent<Transform>().Position = new Vector2(350, 150);
            spinTri.GetComponent<Transform>().Size = new Vector2(50, 50);
            spinTri.AttachComponent<RenderData>();
            spinTri.GetComponent<RenderData>().ApplyTemplate_Triangle();

            AddEntity(spinTri);

            Entity spinPoly = new Entity("spinPoly");
            spinPoly.AttachComponent<Transform>();
            spinPoly.GetComponent<Transform>().Position = new Vector2(450, 150);
            spinPoly.GetComponent<Transform>().Size = new Vector2(1, 1);
            spinPoly.AttachComponent<RenderData>();
            spinPoly.GetComponent<RenderData>().SetVertices(customPoly);

            AddEntity(spinPoly);

            // Spin the spinning objects with a script.
            Scripting.Expose("spinCircle", spinCircle.GetComponent<Transform>());
            Scripting.Expose("spinLine", spinLine.GetComponent<Transform>());
            Scripting.Expose("spinRect", spinRect.GetComponent<Transform>());
            Scripting.Expose("spinTri", spinTri.GetComponent<Transform>());
            Scripting.Expose("spinPoly", spinPoly.GetComponent<Transform>());
            Scripting.Register("" +
                               " spinCircle.Rotation += 0.1; " +
                               " spinLine.Rotation += 0.1; " +
                               " spinRect.Rotation += 0.1; " +
                               " spinTri.Rotation += 0.1; " +
                               " spinPoly.Rotation += 0.1; " +
                               "");

            // Colored shapes.

            Entity colorCircle = new Entity("colorCircle");
            colorCircle.AttachComponent<Transform>();
            colorCircle.GetComponent<Transform>().Position = new Vector2(50, 250);
            colorCircle.GetComponent<Transform>().Size = new Vector2(50, 50);
            colorCircle.AttachComponent<RenderData>();
            colorCircle.GetComponent<RenderData>().ApplyTemplate_Circle();
            colorCircle.GetComponent<RenderData>().Color =
                new Color(
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255)
                );

            AddEntity(colorCircle);

            Entity colorLine = new Entity("colorLine");
            colorLine.AttachComponent<Transform>();
            colorLine.GetComponent<Transform>().Position = new Vector2(0, 0);
            colorLine.GetComponent<Transform>().Size = new Vector2(1, 1);
            colorLine.AttachComponent<RenderData>();
            colorLine.GetComponent<RenderData>().SetPointCount(2);
            colorLine.GetComponent<RenderData>().SetPoint(0, new Vector2(150, 250));
            colorLine.GetComponent<RenderData>().SetPoint(1, new Vector2(200, 250));
            colorLine.GetComponent<RenderData>().Color =
                new Color(
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255)
                );

            AddEntity(colorLine);

            Entity colorRect = new Entity("colorRect");
            colorRect.AttachComponent<Transform>();
            colorRect.GetComponent<Transform>().Position = new Vector2(250, 250);
            colorRect.GetComponent<Transform>().Size = new Vector2(50, 50);
            colorRect.AttachComponent<RenderData>();
            colorRect.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            colorRect.GetComponent<RenderData>().Color =
                new Color(
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255)
                );

            AddEntity(colorRect);

            Entity colorTri = new Entity("colorTri");
            colorTri.AttachComponent<Transform>();
            colorTri.GetComponent<Transform>().Position = new Vector2(350, 250);
            colorTri.GetComponent<Transform>().Size = new Vector2(50, 50);
            colorTri.AttachComponent<RenderData>();
            colorTri.GetComponent<RenderData>().ApplyTemplate_Triangle();
            colorTri.GetComponent<RenderData>().Color =
                new Color(
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255)
                );

            AddEntity(colorTri);

            Entity colorPoly = new Entity("colorPoly");
            colorPoly.AttachComponent<Transform>();
            colorPoly.GetComponent<Transform>().Position = new Vector2(450, 250);
            colorPoly.GetComponent<Transform>().Size = new Vector2(1, 1);
            colorPoly.AttachComponent<RenderData>();
            colorPoly.GetComponent<RenderData>().SetVertices(customPoly);
            colorPoly.GetComponent<RenderData>().Color =
                new Color(
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255),
                    Utilities.GenerateRandomNumber(0, 255)
                );

            AddEntity(colorPoly);

            // Color the objects with a script.
            Scripting.Expose("colorCircle", colorCircle.GetComponent<RenderData>());
            Scripting.Expose("colorLine", colorLine.GetComponent<RenderData>());
            Scripting.Expose("colorRect", colorRect.GetComponent<RenderData>());
            Scripting.Expose("colorTri", colorTri.GetComponent<RenderData>());
            Scripting.Expose("colorPoly", colorPoly.GetComponent<RenderData>());
            Func<Color> genFunc = () => new Color(
                Utilities.GenerateRandomNumber(0, 255),
                Utilities.GenerateRandomNumber(0, 255),
                Utilities.GenerateRandomNumber(0, 255)
            );
            Scripting.Expose("genColor", genFunc);
            Scripting.Register("" +
                               "if(new Date().getMilliseconds() % 16 == 0) {" +
                               "  colorCircle.Color = genColor(); " +
                               "  colorLine.Color = genColor(); " +
                               "  colorRect.Color = genColor(); " +
                               "  colorTri.Color = genColor(); " +
                               "  colorPoly.Color = genColor(); " +
                               "}" +
                               "");

            // Textures Shapes

            AssetLoader.LoadTexture("imageTest.png");

            Entity texturedCircle = new Entity("texturedCircle");
            texturedCircle.AttachComponent<Transform>();
            texturedCircle.GetComponent<Transform>().Position = new Vector2(50, 350);
            texturedCircle.GetComponent<Transform>().Size = new Vector2(50, 50);
            texturedCircle.AttachComponent<RenderData>();
            texturedCircle.GetComponent<RenderData>().ApplyTemplate_Circle();
            texturedCircle.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(texturedCircle);

            Entity texturedLine = new Entity("texturedLine");
            texturedLine.AttachComponent<Transform>();
            texturedLine.GetComponent<Transform>().Position = new Vector2(0, 0);
            texturedLine.GetComponent<Transform>().Size = new Vector2(1, 1);
            texturedLine.AttachComponent<RenderData>();
            texturedLine.GetComponent<RenderData>().SetPointCount(2);
            texturedLine.GetComponent<RenderData>().SetPoint(0, new Vector2(150, 350));
            texturedLine.GetComponent<RenderData>().SetPoint(1, new Vector2(200, 350));
            texturedLine.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(texturedLine);

            Entity texturedRect = new Entity("texturedRect");
            texturedRect.AttachComponent<Transform>();
            texturedRect.GetComponent<Transform>().Position = new Vector2(250, 350);
            texturedRect.GetComponent<Transform>().Size = new Vector2(50, 50);
            texturedRect.AttachComponent<RenderData>();
            texturedRect.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            texturedRect.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(texturedRect);

            Entity texturedTri = new Entity("texturedTri");
            texturedTri.AttachComponent<Transform>();
            texturedTri.GetComponent<Transform>().Position = new Vector2(350, 350);
            texturedTri.GetComponent<Transform>().Size = new Vector2(50, 50);
            texturedTri.AttachComponent<RenderData>();
            texturedTri.GetComponent<RenderData>().ApplyTemplate_Triangle();
            texturedTri.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(texturedTri);

            Entity texturedPoly = new Entity("texturedPoly");
            texturedPoly.AttachComponent<Transform>();
            texturedPoly.GetComponent<Transform>().Position = new Vector2(450, 350);
            texturedPoly.GetComponent<Transform>().Size = new Vector2(1, 1);
            texturedPoly.AttachComponent<RenderData>();
            texturedPoly.GetComponent<RenderData>().SetVertices(customPoly);
            texturedPoly.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(texturedPoly);
        }

        private void ChangeColor(Entity obj)
        {
            obj.GetComponent<RenderData>().Color = new Color(obj.GetComponent<RenderData>().Color.R + 1,
                obj.GetComponent<RenderData>().Color.G + 1, obj.GetComponent<RenderData>().Color.B + 1);
        }
    }
}