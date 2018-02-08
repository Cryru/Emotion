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

            Entity basicText = new Entity("basicText");
            basicText.AttachComponent<Transform>();
            basicText.GetComponent<Transform>().Position = new Vector2(100, 100);
            basicText.GetComponent<Transform>().Size = new Vector2(500, 500);
            basicText.AttachComponent<RenderData>();
            basicText.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            basicText.GetComponent<RenderData>().Color = new Breath.Primitives.Color(255, 0, 0);
            basicText.AttachComponent<TextData>();
            basicText.GetComponent<TextData>().Text = "Test test\nnew_line@test!";
            basicText.GetComponent<TextData>().Font = AssetLoader.GetFont("testFont.ttf");
            basicText.GetComponent<TextData>().Size = 100;
            AddEntity(basicText);
        }

        protected override void Update()
        {
            bool breakpoint = true;
        }
    }
}