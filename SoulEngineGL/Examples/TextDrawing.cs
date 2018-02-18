// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine;
using Soul.Engine.Components;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

#endregion

namespace Soul
{
    public class TextDrawing : Scene
    {
        public static void Main()
        {
            Core.Setup(new TextDrawing());
        }

        protected override void Setup()
        {
            string loremIpsum = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry\'s standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
            string loremIpsumWithTags = "Lorem Ipsum is <color=#687384>sim<color=#bf13ab>p</>ly</> dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry\'s standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

            Entity textTest = new Entity("textTest");
            textTest.AttachComponent<RenderData>();
            textTest.GetComponent<RenderData>().Tint = new Color(0, 0, 0, 100);
            textTest.AttachComponent<TextData>();
            textTest.GetComponent<TextData>().Text = loremIpsum;
            textTest.Position = new Vector2(50, 50);
            textTest.Size = new Vector2(200, 100);
            AddEntity(textTest);

            Entity textTestColor = new Entity("textTestColor");
            textTestColor.AttachComponent<RenderData>();
            textTestColor.GetComponent<RenderData>().Tint = new Color(0, 0, 0, 100);
            textTestColor.AttachComponent<TextData>();
            textTestColor.GetComponent<TextData>().Text = loremIpsumWithTags;
            textTestColor.Position = new Vector2(50, 160);
            textTestColor.Size = new Vector2(200, 100);
            AddEntity(textTestColor);
        }

        protected override void Update()
        {
        }
    }
}