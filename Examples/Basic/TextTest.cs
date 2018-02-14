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
using Breath.Primitives;

#endregion

namespace Examples.Basic
{
    public class TextTest : Scene
    {
        private bool _isArial = true;

        public static void Main()
        {
            Core.Setup(new TextTest());
        }

        protected override void Setup()
        {
            AssetLoader.LoadFont("Arial.ttf");
            AssetLoader.LoadFont("ElectricSleep.ttf");

            Entity textSmall = new Entity("textSmall");
            textSmall.AttachComponent<Transform>();
            textSmall.GetComponent<Transform>().Position = new Vector2(50, 30);
            textSmall.GetComponent<Transform>().Size = new Vector2(900, 50);
            textSmall.AttachComponent<RenderData>();
            textSmall.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            textSmall.GetComponent<RenderData>().Color = new Color(86, 123, 137);
            textSmall.AttachComponent<TextData>();
            textSmall.GetComponent<TextData>().Text =
                "Lorem Ipsum is simply dummy text of the printing and typesetting industry.";
            textSmall.GetComponent<TextData>().Font = AssetLoader.GetFont("Arial.ttf");
            textSmall.GetComponent<TextData>().Size = 20;
            AddEntity(textSmall);

            Entity textMedium = new Entity("textMedium");
            textMedium.AttachComponent<Transform>();
            textMedium.GetComponent<Transform>().Position = new Vector2(50, 70);
            textMedium.GetComponent<Transform>().Size = new Vector2(900, 100);
            textMedium.AttachComponent<RenderData>();
            textMedium.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            textMedium.GetComponent<RenderData>().Color = new Breath.Primitives.Color(212, 66, 244);
            textMedium.AttachComponent<TextData>();
            textMedium.GetComponent<TextData>().Text =
                "Lorem Ipsum is simply dummy text of the printing\nand typesetting industry.";
            textMedium.GetComponent<TextData>().Font = AssetLoader.GetFont("Arial.ttf");
            textMedium.GetComponent<TextData>().Size = 40;
            AddEntity(textMedium);

            Entity textLarge = new Entity("textLarge");
            textLarge.AttachComponent<Transform>();
            textLarge.GetComponent<Transform>().Position = new Vector2(50, 170);
            textLarge.GetComponent<Transform>().Size = new Vector2(900, 350);
            textLarge.AttachComponent<RenderData>();
            textLarge.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            textLarge.GetComponent<RenderData>().Color = new Breath.Primitives.Color(212, 244, 66);
            textLarge.AttachComponent<TextData>();
            textLarge.GetComponent<TextData>().Text =
                "Lorem Ipsum is simply\ndummy text of the\nprinting and typesetting\nindustry.";
            textLarge.GetComponent<TextData>().Font = AssetLoader.GetFont("Arial.ttf");
            textLarge.GetComponent<TextData>().Size = 80;
            AddEntity(textLarge);
        }

        protected override void Update()
        {
            bool breakpoint = true;

            if (Input.KeyPressed(OpenTK.Input.Key.Space))
            {
                if (_isArial)
                {
                    GetEntity("textSmall").GetComponent<TextData>().Font = AssetLoader.GetFont("ElectricSleep.ttf");
                    GetEntity("textSmall").GetComponent<TextData>().Text =
                        GetEntity("textSmall").GetComponent<TextData>().Text.ToUpper();

                    GetEntity("textMedium").GetComponent<TextData>().Font = AssetLoader.GetFont("ElectricSleep.ttf");
                    GetEntity("textMedium").GetComponent<TextData>().Text =
                        GetEntity("textMedium").GetComponent<TextData>().Text.ToUpper();

                    GetEntity("textLarge").GetComponent<TextData>().Font = AssetLoader.GetFont("ElectricSleep.ttf");
                    GetEntity("textLarge").GetComponent<TextData>().Text =
                        GetEntity("textLarge").GetComponent<TextData>().Text.ToUpper();
                    _isArial = false;
                }
                else
                {
                    GetEntity("textSmall").GetComponent<TextData>().Font = AssetLoader.GetFont("Arial.ttf");
                    GetEntity("textSmall").GetComponent<TextData>().Text =
                        GetEntity("textSmall").GetComponent<TextData>().Text.ToUpper();

                    GetEntity("textMedium").GetComponent<TextData>().Font = AssetLoader.GetFont("Arial.ttf");
                    GetEntity("textMedium").GetComponent<TextData>().Text =
                        GetEntity("textMedium").GetComponent<TextData>().Text.ToUpper();

                    GetEntity("textLarge").GetComponent<TextData>().Font = AssetLoader.GetFont("Arial.ttf");
                    GetEntity("textLarge").GetComponent<TextData>().Text =
                        GetEntity("textLarge").GetComponent<TextData>().Text.ToUpper();
                    _isArial = true;
                }

            }
        }
    }
}