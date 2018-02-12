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

            Entity lineTop = new Entity("lineTop");
            lineTop.AttachComponent<Transform>();
            lineTop.GetComponent<Transform>().Position = new Vector2(100, 100);
            lineTop.GetComponent<Transform>().Size = new Vector2(1, 1);
            lineTop.AttachComponent<RenderData>();
            lineTop.GetComponent<RenderData>().SetPointCount(2);
            lineTop.GetComponent<RenderData>().SetPoint(0, new Vector2(0, 0));
            lineTop.GetComponent<RenderData>().SetPoint(1, new Vector2(500, 0));
            lineTop.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            lineTop.GetComponent<RenderData>().Priority = 2;
            AddEntity(lineTop);

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
            newLine.GetComponent<Transform>().Position = new Vector2(100, 100);
            newLine.GetComponent<Transform>().Size = new Vector2(1, 1);
            newLine.AttachComponent<RenderData>();
            newLine.GetComponent<RenderData>().SetPointCount(2);
            newLine.GetComponent<RenderData>().SetPoint(0, new Vector2(0, 29));
            newLine.GetComponent<RenderData>().SetPoint(1, new Vector2(500, 29));
            newLine.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            newLine.GetComponent<RenderData>().Priority = 2;
            AddEntity(newLine);

            Entity lineRight = new Entity("lineRight");
            lineRight.AttachComponent<Transform>();
            lineRight.GetComponent<Transform>().Position = new Vector2(100, 100);
            lineRight.GetComponent<Transform>().Size = new Vector2(1, 1);
            lineRight.AttachComponent<RenderData>();
            lineRight.GetComponent<RenderData>().SetPointCount(2);
            lineRight.GetComponent<RenderData>().SetPoint(0, new Vector2(500, 0));
            lineRight.GetComponent<RenderData>().SetPoint(1, new Vector2(500, 400));
            lineRight.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            lineRight.GetComponent<RenderData>().Priority = 2;
            AddEntity(lineRight);

            Entity basicText = new Entity("basicText");
            basicText.AttachComponent<Transform>();
            basicText.GetComponent<Transform>().Position = new Vector2(100, 100);
            basicText.GetComponent<Transform>().Size = new Vector2(500, 400);
            basicText.AttachComponent<RenderData>();
            basicText.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            //basicText.GetComponent<RenderData>().Color = new Breath.Primitives.Color(212, 244, 66);
            basicText.AttachComponent<TextData>();
            basicText.GetComponent<TextData>().Text = "Lorem Ipsum is simply dummy text of the printing and typesetting\nindustry. Lorem Ipsum has been the industry's standard dummy\ntext ever since the 1500s, when an unknown printer took a galley\nof type and scrambled it to make a type specimen book.\n";
            basicText.GetComponent<TextData>().Font = AssetLoader.GetFont("testFont.ttf");
            basicText.GetComponent<TextData>().Size = 100;
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