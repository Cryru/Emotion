// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Raya.Input;
using Raya.Primitives;
using Soul.Engine;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Objects;

#endregion

namespace Examples.Basic
{
    public class TextTest : Scene
    {
        public static void Main(string[] args)
        {
            // Start the engine with this scene.
            Core.Start(
                new TextTest(),
                "textTest"
            );
        }

        public override void Initialize()
        {
            AssetLoader.LoadFont("testFont.ttf");

            GameObject textTest = new GameObject();
            AddChild("textTest", textTest);
            textTest.Position = new Vector2(10, 10);
            textTest.Size = new Vector2(200, 100);
            textTest.AddChild("textTest", new Text("testFont.ttf", loremIpsum, 20));
            textTest.AddChild("bg", new BasicShape(ShapeType.Rectangle));
            textTest.GetChild<BasicShape>().Color = Color.Black;
            textTest.GetChild<BasicShape>().Priority = -1;

        }

        public override void Update()
        {
            if (Input.MouseButtonHeld(Mouse.Button.Left))
            {
                GetChild("textTest").Size = Input.MousePosition - GetChild("textTest").Position;
            }
        }

        public static string loremIpsum =
                @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
            ;
    }
}