#region Using

using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionSDF
{
    public class EmotionSDFReference
    {
        public DrawableFontAtlas ReferenceFont;
        public Packing.PackingResumableState PackingState;
        public FrameBuffer? AtlasFramebuffer;
        public List<DrawableFontAtlas> FontsThatUseReference = new();

        public EmotionSDFReference(Font font, int referenceSize, bool pixelFont)
        {
            ReferenceFont = new DrawableFontAtlas(font, referenceSize, pixelFont);
            PackingState = new Packing.PackingResumableState(Vector2.Zero);
        }

        public void CacheToFile(int size, bool pixelFont, string tag)
        {
            FrameBuffer? atlasBuffer = AtlasFramebuffer;
            if (atlasBuffer == null) return;

            var cachedName = $"Player/SDFCache/{ReferenceFont.Font.FullName}-{tag}-{size}-{pixelFont}";
            var cachedRenderName = $"{cachedName}_R8.png";
            var cachedMetaName = $"{cachedName}.xml";

            var serialized = new EmotionSDFReferenceSerialized(this);
            XMLAsset<EmotionSDFReferenceSerialized>.CreateFromContent(serialized).SaveAs(cachedMetaName, false);

            // Sample framebuffer and save it.
            byte[] data = atlasBuffer.Sample(new Rectangle(0, 0, atlasBuffer.Size), PixelFormat.Red);
            byte[] pngData = PngFormat.Encode(data, atlasBuffer.Size, PixelFormat.Red);
            Engine.AssetLoader.Save(pngData, cachedRenderName, false);
        }

        public static EmotionSDFReference? TryLoadReferenceFromFile(DrawableFontAtlas atlas, int size, bool pixelFont, string tag, bool withDepthStencil)
        {
            var cachedName = $"Player/SDFCache/{atlas.Font.FullName}-{tag}-{size}-{pixelFont}";
            var cachedRenderName = $"{cachedName}_R8.png";
            var cachedMetaName = $"{cachedName}.xml";
            bool cachedImageExists = Engine.AssetLoader.Exists(cachedRenderName);
            bool cachedMetaExists = Engine.AssetLoader.Exists(cachedMetaName);

            if (!cachedImageExists || !cachedMetaExists) return null;

            var texture = Engine.AssetLoader.Get<TextureAsset>(cachedRenderName);
            var atlasMeta = Engine.AssetLoader.Get<XMLAsset<EmotionSDFReferenceSerialized>>(cachedMetaName);

            if (texture == null || atlasMeta == null || atlasMeta.Content == null) return null;

            EmotionSDFReferenceSerialized? serialized = atlasMeta.Content;
            var reference = new EmotionSDFReference(atlas.Font, size, pixelFont);
            reference.PackingState = serialized.PackingState;
            foreach ((int glyphCharIdx, Rectangle glyphUV) in serialized.Glyphs)
            {
                var glyphChar = (char) glyphCharIdx;
                var glyphVal = new DrawableGlyph(glyphChar, atlas.Font.CharToGlyph[glyphChar], reference.ReferenceFont.RenderScale);
                glyphVal.GlyphUV = glyphUV;
                reference.ReferenceFont.Glyphs.Add(glyphChar, glyphVal);
            }

            GLThread.ExecuteGLThreadAsync(() =>
            {
                reference.AtlasFramebuffer = new FrameBuffer(reference.PackingState.Size).WithColor(true, InternalFormat.Red, PixelFormat.Red);
                if (withDepthStencil) reference.AtlasFramebuffer.WithDepthStencil();
                reference.AtlasFramebuffer.ColorAttachment.Smooth = true;

                RenderComposer composer = Engine.Renderer;
                RenderState prevState = composer.CurrentState.Clone();
                composer.PushModelMatrix(Matrix4x4.Identity, false);
                composer.SetState(composer.BlitState);
                composer.RenderToAndClear(reference.AtlasFramebuffer);

                // Draw it upside down as the atlas buffer is upside down.
                composer.RenderSprite(Vector3.Zero, texture.Texture.Size, Color.White, texture.Texture, null, false, true);

                composer.RenderTo(null);
                composer.PopModelMatrix();
                composer.SetState(prevState);

                // Free assets from memory.
                Engine.AssetLoader.Destroy(cachedRenderName);
                Engine.AssetLoader.Destroy(cachedMetaName);
            });

            return reference;
        }

        public class EmotionSDFReferenceSerialized
        {
            public Dictionary<int, Rectangle> Glyphs;
            public Packing.PackingResumableState PackingState;

            public EmotionSDFReferenceSerialized(EmotionSDFReference reference)
            {
                Glyphs = new();
                foreach ((char glyphChar, DrawableGlyph? glyphVal) in reference.ReferenceFont.Glyphs)
                {
                    Glyphs.Add(glyphChar, glyphVal.GlyphUV);
                }

                PackingState = reference.PackingState;
            }

            // Serialization constructor
            protected EmotionSDFReferenceSerialized()
            {
                Glyphs = null!;
                PackingState = null!;
            }
        }
    }
}