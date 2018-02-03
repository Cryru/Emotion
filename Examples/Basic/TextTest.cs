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
            Font f = AssetLoader.GetFont("testFont.ttf");

            RenderTarget renderTarget = new RenderTarget(200, 200);
            //renderTarget.Use();

            Glyph g = f.GetGlyph((char) 'A', 200);

            //Entity basicTexture = Entity.CreateBasicDrawable("basicTexture");
            //basicTexture.GetComponent<Transform>().Position = new Vector2(0, 0);
            //basicTexture.GetComponent<Transform>().Size = new Vector2(500, 500);
            //basicTexture.GetComponent<RenderData>().ApplyTexture(g.GlyphTexture);
            //basicTexture.GetComponent<RenderData>().Color = new Breath.Graphics.Color(255, 0, 0);
            //AddEntity(basicTexture);

            Entity basicText = new Entity("basicText");
            basicText.AttachComponent<Transform>();
            basicText.GetComponent<Transform>().Position = new Vector2(100, 100);
            basicText.GetComponent<Transform>().Size = new Vector2(400, 200);
            basicText.AttachComponent<RenderData>();
            basicText.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            basicText.GetComponent<RenderData>().Color = new Breath.Graphics.Color(255, 0, 0);
            basicText.GetComponent<RenderData>().Enabled = false;
            basicText.AttachComponent<TextData>();
            basicText.GetComponent<TextData>().Text = "Hel\tlo sir!\nHow art thou?hhweifhewofhoewfhoewfihweoifhwoeifhoewfhoiwefhweoifhoewioefwoifhowehfi\nwihwehofewfweohf";
            basicText.GetComponent<TextData>().Font = AssetLoader.GetFont("testFont.ttf");
            basicText.GetComponent<TextData>().Size = 50;
            AddEntity(basicText);
        }

        protected override void Update()
        {
            bool breakpoint = true;
        }
    }
}