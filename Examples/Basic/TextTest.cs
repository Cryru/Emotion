// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Diagnostics;
using OpenTK;
using Soul.Engine;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Graphics.Text;
using Soul.Engine.Scenography;
using Soul.Engine.Modules;
using Breath.Objects;

#endregion

namespace Examples.Basic
{
    public class TextTest : Scene
    {
        public static void Main()
        {
            Core.Setup(new TextTest());
        }

        protected override void Setup()
        {
            AssetLoader.LoadFont("testFont.ttf");

            Entity line = new Entity("line");
            line.AttachComponent<Transform>();
            line.GetComponent<Transform>().Position = new Vector2(100, 100);
            line.GetComponent<Transform>().Size = new Vector2(1, 1);
            line.AttachComponent<RenderData>();
            line.GetComponent<RenderData>().SetPointCount(2);
            line.GetComponent<RenderData>().SetPoint(0, new Vector2(0, 0));
            line.GetComponent<RenderData>().SetPoint(1, new Vector2(800, 0));
            line.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            line.GetComponent<RenderData>().Priority = 2;
            AddEntity(line);

            Entity lineLeft = new Entity("lineLeft");
            lineLeft.AttachComponent<Transform>();
            lineLeft.GetComponent<Transform>().Position = new Vector2(100, 100);
            lineLeft.GetComponent<Transform>().Size = new Vector2(1, 1);
            lineLeft.AttachComponent<RenderData>();
            lineLeft.GetComponent<RenderData>().SetPointCount(2);
            lineLeft.GetComponent<RenderData>().SetPoint(0, new Vector2(0, 0));
            lineLeft.GetComponent<RenderData>().SetPoint(1, new Vector2(0, 400));
            lineLeft.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            lineLeft.GetComponent<RenderData>().Priority = 2;
            AddEntity(lineLeft);

            Entity newLine = new Entity("newLine");
            newLine.AttachComponent<Transform>();
            newLine.GetComponent<Transform>().Position = new Vector2(0, 0);
            newLine.GetComponent<Transform>().Size = new Vector2(1, 1);
            newLine.AttachComponent<RenderData>();
            newLine.GetComponent<RenderData>().SetPointCount(2);
            newLine.GetComponent<RenderData>().SetPoint(0, new Vector2(1, 122));
            newLine.GetComponent<RenderData>().SetPoint(1, new Vector2(400, 122));
            newLine.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            newLine.GetComponent<RenderData>().Priority = 2;
            AddEntity(newLine);

            Entity lineBottom = new Entity("lineBottom");
            lineBottom.AttachComponent<Transform>();
            lineBottom.GetComponent<Transform>().Position = new Vector2(0, 0);
            lineBottom.GetComponent<Transform>().Size = new Vector2(1, 1);
            lineBottom.AttachComponent<RenderData>();
            lineBottom.GetComponent<RenderData>().SetPointCount(2);
            lineBottom.GetComponent<RenderData>().SetPoint(0, new Vector2(1, 400));
            lineBottom.GetComponent<RenderData>().SetPoint(1, new Vector2(400, 400));
            lineBottom.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            lineBottom.GetComponent<RenderData>().Priority = 2;
            AddEntity(lineBottom);

            Entity basicText = new Entity("basicText");
            basicText.AttachComponent<Transform>();
            basicText.GetComponent<Transform>().Position = new Vector2(100, 100);
            basicText.GetComponent<Transform>().Size = new Vector2(900, 400);
            basicText.AttachComponent<RenderData>();
            basicText.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            basicText.GetComponent<RenderData>().Color = new Breath.Primitives.Color(212, 244, 66);
            basicText.AttachComponent<TextData>();
            basicText.GetComponent<TextData>().Text = "Abcdefg\nhijklmnoAB";
            basicText.GetComponent<TextData>().Font = AssetLoader.GetFont("testFont.ttf");
            basicText.GetComponent<TextData>().Size = 50;
            AddEntity(basicText);
        }

        protected override void Update()
        {
            bool breakpoint = true;

            if (Input.KeyPressed(OpenTK.Input.Key.A))
            {
                GetEntity("basicText").GetComponent<TextData>().Text += "A";
            }
        }
    }
}