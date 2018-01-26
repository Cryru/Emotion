// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using OpenTK;
using Soul.Engine;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Scenography;
using Soul.Engine.Modules;

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

            //Entity basicTexture = Entity.CreateBasicDrawable("basicTexture");
            //basicTexture.GetComponent<Transform>().Position = new Vector2(0, 0);
            //basicTexture.GetComponent<Transform>().Size = new Vector2(500, 500);
            //basicTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("test"));
            //basicTexture.GetComponent<RenderData>().Color = new Breath.Graphics.Color(255, 0, 0);
            //AddEntity(basicTexture);

            AssetLoader.GetFont("testFont.ttf").GetGlyph((char) 'A', 15);
        }

        protected override void Update()
        {
            bool breakpoint = true;
        }
    }
}