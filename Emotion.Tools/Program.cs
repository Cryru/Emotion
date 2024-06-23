#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text.RasterizationNew;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.UI;
using OpenGL;
using static StbTrueTypeSharp.StbTrueType;

#endregion

namespace Emotion.Tools
{
    public class Program : World2DBaseScene<Map2D>
    {
        private UIController _ui;

        private static void Main()
        {
            var config = new Configurator
            {
                DebugMode = true,
                HostSize = new Vector2(1280, 720)
            };
            ToolsManager.ConfigureForTools(config);

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new Program());
            Engine.Run();
        }

        public void Update()
        {
            _ui.Update();
        }

        private Texture _t;

        public override unsafe Task LoadAsync()
        {
            _ui = new UIController();
            var testBackground = new UISolidColor
            {
                InputTransparent = false,
                WindowColor = Color.CornflowerBlue
            };
            _ui.AddChild(testBackground);
            ToolsManager.AddToolBoxWindow(_ui);


            OtherAsset ass = Engine.AssetLoader.Get<OtherAsset>(FontAsset.DefaultBuiltInFontName, false);
            stbtt_fontinfo fontInfo = new stbtt_fontinfo();
            var cnt = ass.Content.Span;
            fixed (byte* ptr = &cnt[0])
            {
                stbtt_InitFont(fontInfo, ptr, 0);
            }

            var c = 'T';

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);

            float xpos = 0;
            float m_scale = 28 / (float) ascent;

            int advance;
            int lsb;
            stbtt_GetCodepointHMetrics(fontInfo, c, &advance, &lsb);

            int w, h, x0, y0, x1, y1;
            float x_shift = xpos - MathF.Floor(xpos);
            stbtt_GetCodepointBitmapBoxSubpixel(fontInfo, c, m_scale, m_scale, x_shift, 0, &x0, &y0, &x1, &y1);

            w = x1 - x0;
            h = y1 - y0;

            if (xpos + lsb * m_scale < 0) xpos -= lsb * m_scale;

            var glyph_pitch = w;
            var glyphIndex = stbtt_FindGlyphIndex(fontInfo, c);
            byte[] pbm = new byte[w * h];

            fixed (byte* pPbm = &pbm[0])
            {
                stbtt_MakeGlyphBitmapSubpixel(fontInfo, pPbm, w, h, glyph_pitch, m_scale, m_scale, x_shift, 0, glyphIndex);
            }

            SoftwareGlyphRasterizer.RenderGlyph(fontInfo, pbm, w, h, glyph_pitch, m_scale, m_scale, x_shift, 0, glyphIndex);

            int startx = 0;
            int starty = 0;

            // clamp horizontally
            //int left = (int) MathF.Floor(xpos + lsb * m_scale);
            //if (left < 0) {
            //	startx -= left;
            //	left = 0;
            //}
            //if (left + w > destWidth) {
            //	w = destWidth - left;
            //}

            //// clamp vertically
            //int top = m_baseline + y0;
            //if (top < 0) {
            //	starty -= top;
            //	top = 0;
            //}
            //if (top + h >(int)destHeight) {
            //	h = destHeight - top;
            //}

            xpos += m_scale * advance;

            GLThread.ExecuteGLThread(() =>
            {
                _t = new Texture();
                _t.Upload(new Vector2(w, h), pbm, PixelFormat.Red, InternalFormat.Red, PixelType.UnsignedByte);
            });

            _font = fontInfo;

            return Task.CompletedTask;
        }

        private stbtt_fontinfo _font;

        public override void Unload()
        {
        }

        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            _ui.Render(composer);

            composer.RenderSprite(new Vector3(100, 100, 0), _t.Size, Color.White, _t);

            //var font = Engine.AssetLoader.Get<FontAsset>("SourceCodePro-Regular.ttf");
            //composer.RenderString(new Vector3(100, 100, 0), Color.White, "T", font.GetAtlas(18));
        }
    }
}