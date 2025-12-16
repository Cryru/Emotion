#nullable enable

using Emotion.Core.Systems.JobSystem;
using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Batches;
using Emotion.Standard.Parsers.OpenType;
using static NewStbTrueTypeSharp.StbTrueType;

namespace Emotion.Graphics.Text;

public static partial class TextRenderer
{
    private static unsafe void LBL_RenderText(
        Renderer r, Vector3 pos, Color color,
        ReadOnlySpan<char> blockString, Font font, int textSize, TextEffect? effect = null
    )
    {
        float scale = font.GetScaleFromFontSize(textSize);

        var stbFont = font.StbFontInfo;
        float xPos = 0;
        for (int i = 0; i < blockString.Length; i++)
        {
            char ch = blockString[i];
            Span<char> span = new Span<char>(ref ch);

            int glyphIdx = font.GetGlyphIndexFromChar(ch);
            stbtt_GetGlyphHMetrics(stbFont, glyphIdx, out int advance, out int lsb);

            float xShift = xPos - MathF.Floor(xPos);

            if (effect != null)
            {
                TextEffect effectVal = effect.Value;
                Vector3 renderOffset = -effectVal.GetRenderOffset().ToVec3();

                (Texture, Rectangle)? effectRender = TextRenderer.GetRasterizedLetter(ch, xShift, font, textSize, effect);
                if (effectRender != null)
                {
                    r.RenderSprite(
                        new Vector3(MathF.Floor(pos.X + xPos) + renderOffset.X, pos.Y + renderOffset.Y, pos.Z),
                        effectVal.EffectColor,
                        effectRender.Value.Item1,
                        effectRender.Value.Item2
                    );
                }
            }

            (Texture, Rectangle)? renderData = TextRenderer.GetRasterizedLetter(ch, xShift, font, textSize);
            if (renderData != null)
            {
                r.RenderSprite(
                    new Vector3(MathF.Floor(pos.X + xPos), pos.Y, pos.Z),
                    color,
                    renderData.Value.Item1,
                    renderData.Value.Item2
                );
            }

            xPos += advance * scale;
            if (i < blockString.Length - 1)
            {
                int nextGlyphIndex = font.GetGlyphIndexFromChar(blockString[i + 1]);
                int kerning = stbtt_GetCodepointKernAdvance(stbFont, glyphIdx, nextGlyphIndex);
                xPos += kerning * scale;
            }
        }
    }

    private static (Texture, Rectangle)? GetRasterizedLetter(char ch, float xShift, Font font, int textSize, TextEffect? effect = null)
    {
        if (char.IsWhiteSpace(ch))
            return null;

        // Check if we have it rasterized already
        int stringHash = ch;
        int fontHash = font.FontHash;
        int effectHash = effect?.GetHashCode() ?? 0;
        int finalHash = HashCode.Combine(stringHash, fontHash, textSize, effectHash, (int) (xShift * 10));
        foreach (TextureAtlasSimple<int> atlas in _atlases)
        {
            if (atlas.CheckHas(finalHash, out Rectangle uv, true))
                return (atlas.Texture, uv);
        }

        // Still rendering
        if (_pendingRender.Contains(finalHash))
            return null;

        // Start a job to render
        RenderCharacterAsyncJob renderJob = RenderCharacterAsyncJob.JobPool.Get();
        renderJob.Set(finalHash, ch, font, textSize, effect, xShift);
        Engine.Jobs.Add(renderJob);
        _pendingRender.Add(finalHash);
        return null;
    }

    private static HashSet<int> _pendingRender = new HashSet<int>();

    private class RenderCharacterAsyncJob : ISimpleAsyncJob
    {
        public static ObjectPoolManual<RenderCharacterAsyncJob> JobPool = new ObjectPoolManual<RenderCharacterAsyncJob>(
            static () => new RenderCharacterAsyncJob(),
            static (j) => j.Reset()
        );

        private int _hash;
        private char _ch;
        private Font? _font = null;
        private int _textSize = 0;
        private TextEffect? _effect;
        private float _xShift = 0;
        private RenderTextOutput? _output = null;

        public void Reset()
        {
            _font = null;
            _output = null;
        }

        public void Set(int hash, char ch, Font font, int textSize, TextEffect? effect, float forcexShift = 0)
        {
            _hash = hash;
            _ch = ch;
            _font = font;
            _textSize = textSize;
            _effect = effect;
            _xShift = forcexShift;
        }

        public void Run()
        {
            char ch = _ch;
            Font? font = _font;
            int textSize = _textSize;
            TextEffect? effect = _effect;
            float xShift = _xShift;

            AssertNotNull(font);

            Span<char> letterAsSpan = new Span<char>(ref ch);
            RenderTextOutput output = RasterizeText(letterAsSpan, font, textSize, effect, xShift);
            this._output = output;
           
            GLThread.ExecuteOnGLThreadAsync<RenderCharacterAsyncJob>(Run_GLThreadPart, this);
        }

        private void Run_GLThreadPart(RenderCharacterAsyncJob job)
        {
            // This function is executed on the GL thread so its single threaded
            // -----------------------------
            int finalHash = _hash;
            char ch = _ch;
            Font? font = _font;
            int textSize = _textSize;
            RenderTextOutput? output = _output;

            AssertNotNull(font);
            AssertNotNull(output);

            // Sanity check
            float atlasSize = GetAtlasWidth();
            if (output.Size.X > atlasSize || output.Size.Y > atlasSize)
            {
                Engine.Log.Warning($"Tried to render character larger than the atlas! '{ch}' {font.FullName} @ {textSize}", nameof(TextRenderer), true);
            }
            else
            {
                IntVector2 outputResolution = output.Size;
                Span<byte> outputData = output.GetData();

                // Try to add it to each existing atlas
                (Texture, Rectangle)? atlasData = null;
                TextureAtlasSimple<int>? atlasAddedTo = null;
                foreach (TextureAtlasSimple<int> atlas in _atlases)
                {
                    Rectangle? uv = atlas.TryAdd(finalHash, outputResolution, outputData);
                    if (uv != null)
                    {
                        atlasAddedTo = atlas;
                        atlasData = (atlas.Texture, uv.Value);
                        break;
                    }
                }

                // No space in any existing atlas - create new atlas
                if (atlasData == null)
                {
                    TextureAtlasSimple<int> newAtlas = CreateNewAtlas();
                    Rectangle? uv = newAtlas.TryAdd(finalHash, outputResolution, outputData);
                    AssertNotNull(uv); // New atlas should always succeed!
                    _atlases.Add(newAtlas);

                    atlasData = (newAtlas.Texture, uv.Value);
                    atlasAddedTo = newAtlas;
                }
            }

            // Cleanup
            AssertNotNull(job._output);
            job._output.Done();
            _pendingRender.Remove(job._hash);
            JobPool.Return(job);
            Assert(job._output == null);
        }
    }
}
