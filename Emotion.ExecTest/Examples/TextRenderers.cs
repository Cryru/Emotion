#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Tools;
using Emotion.UI;

#endregion

namespace Emotion.ExecTest.Examples
{
    public class TextRenderers : Scene
    {
        private FontAsset _font;

        private UIController _ui;

        public override async Task LoadAsync()
        {
             _font = await Engine.AssetLoader.GetAsync<FontAsset>("debugFont.otf");
            //_font = await Engine.AssetLoader.GetAsync<FontAsset>("ElectricSleepFont.ttf");

            _ui = new UIController();
            ToolsManager.AddToolBoxWindow(_ui);
        }

        public override void Update()
        {
            _ui.Update();
        }

        public override void Draw(RenderComposer composer)
        {
            Color textCol = new Color(32, 32, 32);

            int size = 20;
            string text = "The quick brown fox jumped over the lazy dog.";

            composer.SetUseViewMatrix(false);

            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, new Color(240, 240, 240));

            composer.PushModelMatrix(Matrix4x4.CreateScale(1, 1, 1) * Matrix4x4.CreateTranslation(100, 100, 0));

            FontAsset.GlyphRasterizer = GlyphRasterizer.Emotion;
            composer.RenderLine(new Vector3(0, 0, 0), new Vector3(500, 0, 0), Color.Red);
            composer.RenderString(new Vector3(0, 0, 0), textCol, "Emotion Renderer:\n" + text, _font.GetAtlas(size));

            FontAsset.GlyphRasterizer = GlyphRasterizer.EmotionSDFVer3;
            composer.RenderLine(new Vector3(0, 140, 0), new Vector3(500, 140, 0), Color.Red);
            composer.RenderString(new Vector3(0, 140, 0), textCol, "EmotionSDFVer3:\n" + text, _font.GetAtlas(size));

            FontAsset.GlyphRasterizer = GlyphRasterizer.EmotionSDFVer4;
            composer.RenderLine(new Vector3(0, 300, 0), new Vector3(500, 300, 0), Color.Red);
            composer.RenderString(new Vector3(0, 300, 0), textCol, "EmotionSDFVer4:\n" + text, _font.GetAtlas(size));

            FontAsset.GlyphRasterizer = GlyphRasterizer.StbTrueType;
            composer.RenderLine(new Vector3(0, 450, 0), new Vector3(500, 450, 0), Color.Red);
            composer.RenderString(new Vector3(0, 450, 0), textCol, "StbTrueType:\n" + text, _font.GetAtlas(size));

            composer.PopModelMatrix();

            _ui.Render(composer);

            composer.SetUseViewMatrix(true);
        }
    }
}