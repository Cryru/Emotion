using Emotion.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Emotion.UI.TextLayoutEngine;
using static NewStbTrueTypeSharp.StbTrueType;

namespace Emotion.WIPUpdates.TextUpdate;

public class TextRenderEntry
{
    public string SourceString;
    public int Start;
    public int Length;

    public bool Dirty = true;

    public TextRenderEntry(string srcString, int start, int length)
    {
        SourceString = srcString;
        Start = start;
        Length = length;
    }

    public ReadOnlySpan<char> GetString()
    {
        return SourceString.AsSpan(Start, Length);
    }

    public override string ToString()
    {
        return GetString().ToString();
    }
}

public static class TextRenderEngine
{
    public static List<TextRenderEntry> CachedEntries  = new List<TextRenderEntry>(64);
    public static FrameBuffer Atlas;

    public static void Init()
    {
        Atlas = new FrameBuffer(new Vector2(4096)).WithColor(true, OpenGL.InternalFormat.R8, OpenGL.PixelFormat.Red);
    }

    public static void CacheEntries(string sourceString, TextBlock textBlock)
    {
        if (Atlas == null) Init(); // todo

        // todo: make sure effects and font match :P
        Span<Range> wordRanges = stackalloc Range[1024];

        ReadOnlySpan<char> str = textBlock.GetBlockString(sourceString);
        int wordCount = str.Split(wordRanges, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < wordCount; i++)
        {
            Range word = wordRanges[i];
            ReadOnlySpan<char> wordStr = str[word];

            // todo
            bool present = false;
            for (int j = 0; j < CachedEntries.Count; j++)
            {
                TextRenderEntry cachedEntry = CachedEntries[j];
                ReadOnlySpan<char> cachedWordStr = cachedEntry.GetString();
                if (wordStr.SequenceEqual(cachedWordStr))
                {
                    present = true;
                    break;
                }
            }

            if (!present)
            {
                (int start, int length) = word.GetOffsetAndLength(str.Length);
                TextRenderEntry entry = new TextRenderEntry(sourceString, start, length);
                CachedEntries.Add(entry);
            }
        }
    }

    public static void RenderBlock(string sourceString, TextBlock block, FontAsset font)
    {
        OtherAsset fontBytes = Engine.AssetLoader.Get<OtherAsset>(font.Name, false);
        stbtt_fontinfo stbFont = CreateFont(fontBytes.Content.ToArray(), 0);
        
        
    }
}
