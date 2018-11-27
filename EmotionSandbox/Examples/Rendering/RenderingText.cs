// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Game.Text;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using EmotionSandbox.Examples.Generic;

#endregion

namespace EmotionSandbox.Examples.Rendering
{
    public class RenderingText : Layer
    {
        private RichText _richText;
        private TypewriterRichText _twRichText;

        public static void Main()
        {
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new RenderingText(), "Emotion Rendering - Text", 1);
            // Start the context.
            Context.Run();
        }

        public override void Load()
        {
            Context.AssetLoader.Get<Font>("ExampleFont.ttf").GetFontAtlas(20);

            _richText = new RichText(new Vector3(10, 200, 0), new Vector2(300, 100), Context.AssetLoader.Get<Font>("ExampleFont.ttf").GetFontAtlas(20));
            _richText.SetText("Hello, I am rich text. I can do things like <color=255-0-0>t</><color=0-255-0>hi</><color=0-0-255>s</>!");
            _richText.Z = 1;

            _twRichText = new TypewriterRichText(new Vector3(10, 300, 0), new Vector2(300, 500), Context.AssetLoader.Get<Font>("ExampleFont.ttf").GetFontAtlas(20));
            _twRichText.SetText("Hello, I am a rich text extension which performs a <color=255-0-0>typewriter</> effect!");
            _twRichText.SetTypewriterEffect(1000);
            _twRichText.Z = 1;
        }

        public override void Update(float fr)
        {
            _twRichText.Update(fr);
            if (!_twRichText.EffectFinished) return;
            _twRichText.SetText(_twRichText.Text);
            _twRichText.SetTypewriterEffect(1000);
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), Context.Settings.RenderSize, Color.CornflowerBlue);

            // Render the text.
            renderer.RenderString(Context.AssetLoader.Get<Font>("ExampleFont.ttf"), 20, "Hello, I am simple text rendered by the renderer.", new Vector3(0, 0, 1), Color.White);
            renderer.RenderString(Context.AssetLoader.Get<Font>("ExampleFont.ttf"), 25, "Hello, I am simple text rendered by the renderer.", new Vector3(0, 25, 1), Color.White);
            renderer.RenderString(Context.AssetLoader.Get<Font>("ExampleFont.ttf"), 30, "Hello, I am simple text rendered by the renderer.", new Vector3(0, 60, 1), Color.White);
            renderer.RenderString(Context.AssetLoader.Get<Font>("ExampleFont.ttf"), 35, "Hello, I am simple text rendered by the renderer.", new Vector3(0, 100, 1), Color.White);

            // Render the RichText object.
            renderer.Render(_richText);
            // Render an outline around it so it's clear that wrapping occurs.
            renderer.RenderOutline(_richText.Position, _richText.Size, Color.Red);

            // Render the typewriter object.
            renderer.Render(_twRichText);
        }

        public override void Unload()
        {
        }
    }
}