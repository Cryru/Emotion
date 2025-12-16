#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Text.LayoutEngineTypes;
using Emotion.Standard.Parsers.OpenType;
using System.Runtime.InteropServices;
using static NewStbTrueTypeSharp.StbTrueType;

#endregion

namespace Emotion.Graphics.Text;

public enum GlyphHeightMeasurement : byte
{
    FullHeight,
    CharacterHeight
}

[DontSerialize]
public class TextLayouter
{
    #region Config

    /// <summary>
    /// Whether to resolve tags in the text, such as <color></color>
    /// Text selection APIs /should/ handle these fine.
    /// </summary>
    public bool ResolveTags { get; init; }

    /// <summary>
    /// Whether the selection API should work.
    /// </summary>
    public bool SelectionAPI { get; init; }

    #endregion

    #region Calculated Metrics

    /// <summary>
    /// The size of the text as layouted last by a call to RunLayout
    /// </summary>
    public IntVector2 Calculated_TotalSize { get; protected set; }

    #endregion

    #region Last Layout Data

    // Change checkers
    private int _lastLayoutTextHash = 0;
    private int _lastLayoutTextSize = 0;
    private int _lastLayoutFontHash = 0;
    private int? _lastLayoutTextWrap = null;
    private GlyphHeightMeasurement _lastLayoutHeightMode = GlyphHeightMeasurement.FullHeight;

    // Layout results
    private List<TagDefinition> _tagDefinitions = new();
    private List<TextBlock> _textBlocks = new();
    private float _lastLayoutTextHeight;
    private float _lastLayoutRenderOffsetY;
    private Font? _lastLayoutFont;
    private List<char> _lastLayoutText = new();

    #endregion

    public TextLayouter(bool resolveTags = true, bool selectionAPI = false)
    {
        ResolveTags = resolveTags;
        SelectionAPI = selectionAPI;
    }

    public void RunLayout(
        ReadOnlySpan<char> text,
        int textSize,
        Font? font,
        int? textWrapWidth = null,
        GlyphHeightMeasurement heightMode = GlyphHeightMeasurement.FullHeight
    )
    {
        // Verify that needs to rerun!
        bool needRerun = false;
        int fontHash = font?.FontHash ?? 0;

        int newTextHash = text.GetStableHashCode();
        if (newTextHash != _lastLayoutTextHash)
            needRerun = true;
        else if (textSize != _lastLayoutTextSize)
            needRerun = true;
        else if (fontHash != _lastLayoutFontHash)
            needRerun = true;
        else if (textWrapWidth != _lastLayoutTextWrap)
            needRerun = true;
        else if (heightMode != _lastLayoutHeightMode)
            needRerun = true;

        if (!needRerun) return;

        _lastLayoutTextHash = newTextHash;
        _lastLayoutTextSize = textSize;
        _lastLayoutFontHash = fontHash;
        _lastLayoutTextWrap = textWrapWidth;
        _lastLayoutHeightMode = heightMode;

        // Reset data
        _textBlocks.Clear();
        _lastLayoutText.Clear();

        if (text.IsEmpty || font == null)
        {
            Calculated_TotalSize = IntVector2.Zero;
            _lastLayoutTextHeight = 0;
            _lastLayoutFont = font;
            return;
        }

        _lastLayoutText.AddRange(text);
        Span<char> str = CollectionsMarshal.AsSpan(_lastLayoutText);

        // 1. Extract layout/draw command tags.
        if (ResolveTags)
        {
            _tagDefinitions.Clear();
            int tagNameStart = -1;
            var currentBlock = new TextBlock(0);
            for (int cIdx = 0; cIdx < str.Length; cIdx++)
            {
                char c = str[cIdx];

                // Inside tag name and it is being closed
                if (tagNameStart != -1 && c == '>')
                {
                    // + and - are for skipping the brackets
                    int tagStart = tagNameStart + 1;
                    int tagLength = cIdx - tagNameStart - 1;

                    var tagDefine = new TagDefinition(tagStart, tagLength);
                    bool isValidTag = ProcessTag(str, ref tagDefine);

                    if (isValidTag)
                    {
                        // Close current block.
                        currentBlock.Length = tagNameStart - currentBlock.StartIndex;
                        _textBlocks.Add(currentBlock);

                        // Add current define
                        _tagDefinitions.Add(tagDefine);

                        tagNameStart = -1;
                        currentBlock = new TextBlock(cIdx + 1);
                    }
                }

                if (c == '<')
                {
                    // New tag starts
                    tagNameStart = cIdx;
                }
            }

            // Close final block
            if (currentBlock.Length == 0)
            {
                currentBlock.Length = str.Length - currentBlock.StartIndex;
                _textBlocks.Add(currentBlock);
            }
        }
        else
        {
            _textBlocks.Add(new TextBlock(0, str.Length));
        }

        // 2. Break up blocks on user specified newline characters.
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];
            ReadOnlySpan<char> blockString = block.GetBlockString(str);

            int lineBreakIndex = blockString.IndexOf('\n');
            while (lineBreakIndex != -1)
            {
                int lengthToEnd = block.Length - lineBreakIndex;
                block.Length = lineBreakIndex;
                _textBlocks[i] = block;

                TextBlock newBlock = new TextBlock(block.StartIndex + lineBreakIndex + 1);
                newBlock.Length = lengthToEnd - 1;
                newBlock.Newline = true;
                _textBlocks.Insert(i + 1, newBlock);

                blockString = block.GetBlockString(str);
                lineBreakIndex = blockString.IndexOf('\n');
            }
        }

        // 3. Wrap text blocks, this will break them up in more block.
        bool shouldWrap = _lastLayoutTextWrap != null;
        if (shouldWrap)
        {
            AssertNotNull(_lastLayoutTextWrap);
            int wrapWidth = _lastLayoutTextWrap.Value;
            Span<char> wordBreakCharacters = [' ', '\n', '-'];

            int sizeOnCurrentLine = 0;
            for (int i = 0; i < _textBlocks.Count; i++)
            {
                TextBlock block = _textBlocks[i];
                if (block.Newline) sizeOnCurrentLine = 0;

                ReadOnlySpan<char> blockString = block.GetBlockString(str);
                int stringWidth = MeasureStringWidth(blockString, textSize, font);

                // This block fits.
                if (sizeOnCurrentLine + stringWidth < wrapWidth)
                {
                    sizeOnCurrentLine += stringWidth;
                    continue;
                }

                // Check how much of this block can fit by splitting it with word breaks.
                int nextWordBreakChar = 0;
                int lastFitBreakChar = 0;
                int sectionWidth = 0;
                while (sizeOnCurrentLine + sectionWidth < wrapWidth)
                {
                    lastFitBreakChar = nextWordBreakChar;
                    int nextIndex = blockString.Slice(nextWordBreakChar).IndexOfAny(wordBreakCharacters);
                    if (nextIndex == -1)
                        break;

                    nextWordBreakChar += nextIndex + 1;
                    sectionWidth = MeasureStringWidth(blockString.Slice(0, nextWordBreakChar), textSize, font);
                }

                // Not even a single word break fits.
                if (lastFitBreakChar == 0)
                {
                    // Try to newline this whole block first.
                    if (stringWidth < wrapWidth)
                    {
                        block.Newline = true;
                        sizeOnCurrentLine = stringWidth;
                        _textBlocks[i] = block;
                        continue;
                    }

                    // It is possible the block is huge and just contains no word breaks,
                    // in this case we split it character by character until it fits.
                    sectionWidth = stringWidth;
                    int charactersFit = blockString.Length;
                    while (sizeOnCurrentLine + sectionWidth > wrapWidth)
                    {
                        charactersFit--;
                        if (charactersFit == 0) break;
                        sectionWidth = MeasureStringWidth(blockString.Slice(0, charactersFit), textSize, font);
                    }
                    // If the space doesn't even even one character - set it to at least one.
                    if (charactersFit == 0) charactersFit = 1;
                    lastFitBreakChar = charactersFit;
                }

                int lengthToEnd = block.Length - lastFitBreakChar;
                block.Length = lastFitBreakChar;
                block.Skip = lastFitBreakChar == 1 && blockString[0] == ' ';
                // Special case for when going on a new line and there is a space at the end.
                // For text centering and other purposes we don't want this part of the measured length, so we cut it out.
                // However we don't want to do this if ResolveTags is false, since that most likely means the user wants
                // to retain a mapping of virtual characters to characters. (such as in the case of a text input)
                if (ResolveTags && lastFitBreakChar > 0 && str[block.StartIndex + block.Length - 1] == ' ') block.Length--;
                _textBlocks[i] = block;

                if (lengthToEnd > 0)
                {
                    TextBlock newBlock = new TextBlock(block.StartIndex + lastFitBreakChar);
                    newBlock.Length = lengthToEnd;
                    newBlock.Newline = true;
                    _textBlocks.Insert(i + 1, newBlock);
                }

                sizeOnCurrentLine += stringWidth;

                if (_textBlocks.Count > 10_000) // Infinite loop detection. todo: do better
                {
                    Assert(false, $"Infinite loop at layout of: {str}");
                    break;
                }
            }
        }

        // 4. Break up blocks so that they can be rendered into the atlas
        int atlasWidth = TextRenderer.GetAtlasWidth();
        if (textSize < atlasWidth)
        {
            for (int i = 0; i < _textBlocks.Count; i++)
            {
                TextBlock block = _textBlocks[i];
                if (block.Skip) continue;

                ReadOnlySpan<char> blockString = block.GetBlockString(str);
                int stringWidth = MeasureStringApproximate(blockString, textSize, font);
                block.Width = stringWidth;
                _textBlocks[i] = block;

                // This block fits.
                if (stringWidth < atlasWidth) continue;

                // Try to break at the next space
                Span<char> atlasBreakCharacter = [' '];
                int lastFitBreakChar = blockString.IndexOfAny(atlasBreakCharacter);
                if (lastFitBreakChar == 0) // If starts with a space, get the next one, otherwise this block will be size 0.
                    lastFitBreakChar = blockString.Slice(1).IndexOfAny(atlasBreakCharacter);

                // A single word doesn't fit.
                if (lastFitBreakChar == -1)
                {
                    int charactersThatWillFit = atlasWidth / textSize;

                    int blockLength = block.Length;
                    int blockStart = block.StartIndex;

                    block.Length = charactersThatWillFit;
                    _textBlocks[i] = block;

                    int charsLeft = blockLength - charactersThatWillFit;
                    int newBlockStartOffset = charactersThatWillFit;
                    while (charsLeft > 0)
                    {
                        int charsInNewBlock = Math.Min(charsLeft, charactersThatWillFit);
                        TextBlock newBlock = new TextBlock(blockStart + newBlockStartOffset);
                        newBlock.Length = charsInNewBlock;
                        _textBlocks.Insert(i + 1, newBlock);
                        i++; // We know these blocks won't overflow for sure

                        newBlockStartOffset += charsInNewBlock;
                        charsLeft -= charsInNewBlock;
                    }
                }
                // Cut off at word break
                else
                {
                    int lengthToEnd = block.Length - lastFitBreakChar;
                    block.Length = lastFitBreakChar;
                    _textBlocks[i] = block;

                    TextBlock newBlock = new TextBlock(block.StartIndex + lastFitBreakChar);
                    newBlock.Length = lengthToEnd;
                    _textBlocks.Insert(i + 1, newBlock);

                    i--; // We want to rerun this block as cutting just one word doesn't ensure it will be fine.
                }
            }
        }


        // Get lists as spans for easier working with.
        // Nothing should be added or removed from the lists past this point.
        Span<TextBlock> blocksSpan = CollectionsMarshal.AsSpan(_textBlocks);
        Span<TagDefinition> tagDefSpan = CollectionsMarshal.AsSpan(_tagDefinitions);

        // Last. Assign defined tags to blocks
        for (int i = 0; i < _tagDefinitions.Count; i++)
        {
            ref TagDefinition tag = ref tagDefSpan[i];
            int startIndex = tag.NameStartIdx;

            int endIndex = str.Length;
            int depth = 0;
            for (int ii = i + 1; ii < _tagDefinitions.Count; ii++)
            {
                ref TagDefinition nextTag = ref tagDefSpan[ii];
                if (nextTag.TagType == TextTagType.ClosingTag)
                {
                    if (depth == 0)
                    {
                        endIndex = nextTag.NameStartIdx;
                        break;
                    }
                    depth--;
                }
                else
                {
                    depth++;
                }
            }

            for (int b = 0; b < blocksSpan.Length; b++)
            {
                ref TextBlock block = ref blocksSpan[b];
                if (block.Skip) continue;

                if (startIndex <= block.StartIndex && endIndex >= block.StartIndex + block.Length)
                {
                    ApplyTag(tag, ref block);
                }
            }
        }

        // Assign block bounds and calculate total size
        int lineSpacing = font.GetTextHeight(textSize);
        int totalHeight = lineSpacing;
        int longestLine = 0;
        int currentLine = 0;
        for (int i = 0; i < blocksSpan.Length; i++)
        {
            ref TextBlock block = ref blocksSpan[i];
            if (block.Skip) continue;

            block.X = currentLine;
            block.Y = totalHeight - lineSpacing;
            block.Width = MeasureStringWidth(block.GetBlockString(str), textSize, font);

            if (block.Newline)
            {
                totalHeight += lineSpacing;
                currentLine = 0;
            }
            currentLine += block.Width;
            if (currentLine > longestLine) longestLine = currentLine;
        }

        // Calculcate height mode
        _lastLayoutRenderOffsetY = 0;
        if (heightMode == GlyphHeightMeasurement.CharacterHeight)
        {
            float largestCharacterHeight = 0;
            float total = 0;
            for (int i = 0; i < blocksSpan.Length; i++)
            {
                ref TextBlock block = ref blocksSpan[i];
                if (block.Skip) continue;

                if (block.Newline)
                {
                    total += largestCharacterHeight;
                    largestCharacterHeight = 0;
                }

                ReadOnlySpan<char> blockStr = block.GetBlockString(str);
                for (int c = 0; c < blockStr.Length; c++)
                {
                    float height = GetGlyphHeight(font, textSize, blockStr[c]);
                    largestCharacterHeight = Math.Max(height, largestCharacterHeight);
                }
            }

            float oldHeight = totalHeight;
            totalHeight = (int)(total + largestCharacterHeight);
            float renderOffset = totalHeight - oldHeight;
            _lastLayoutRenderOffsetY = renderOffset / 2f;
        }

        Calculated_TotalSize = new IntVector2(longestLine, totalHeight);
        _lastLayoutTextHeight = lineSpacing;
        _lastLayoutFont = font;

        // If we're going to use the selection API, we'll need extra info
        if (SelectionAPI)
        {

        }
    }

    private bool ProcessTag(ReadOnlySpan<char> text, ref TagDefinition def)
    {
        ReadOnlySpan<char> tagName = def.GetTagName(text);
        if (tagName.SequenceEqual("/"))
        {
            def.TagType = TextTagType.ClosingTag;
            return true;
        }

        if (tagName.StartsWith("color")) // color r g b (?a) or color #htmlColor
        {
            Color? col = TagReadingHelper_ColorArgs(tagName);
            if (col != null)
            {
                def.ExtraData = col.Value;
                def.TagType = TextTagType.Color;
                return true;
            }
        }
        else if (tagName.StartsWith("outline")) // outline r g b (?a) size=s or outline #htmlColor size=s
        {
            Color? col = TagReadingHelper_ColorArgs(tagName);
            if (col == null) return false;

            // todo: add helper function for getting args from tags
            const string sizeArgStr = "size=";
            int sizeSpec = tagName.IndexOf(sizeArgStr);
            if (sizeSpec == -1) return false;

            sizeSpec += sizeArgStr.Length;

            int sizeArgSize = 0;
            for (int i = sizeSpec; i < tagName.Length; i++)
            {
                char c = tagName[i];
                if (c == ' ') break;

                sizeArgSize++;
            }
            ReadOnlySpan<char> sizeArg = tagName.Slice(sizeSpec, sizeArgSize);
            bool parsed = int.TryParse(sizeArg, out int sizeNum);
            if (!parsed) sizeNum = 1;

            def.ExtraData = col.Value;
            def.ExtraData2 = sizeNum;
            def.TagType = TextTagType.Outline;
            return true;
        }
        else if (tagName.SequenceEqual("center"))
        {
            def.TagType = TextTagType.Center;
            return true;
        }
        else if (tagName.SequenceEqual("right"))
        {
            def.TagType = TextTagType.Right;
            return true;
        }

        return false;
    }

    private void ApplyTag(TagDefinition def, ref TextBlock block)
    {
        switch (def.TagType)
        {
            case TextTagType.Color:
                {
                    block.Color = def.ExtraData;
                    block.UseDefaultColor = false;
                    break;
                }
            case TextTagType.Outline:
                {
                    block.TextEffect = TextEffectType.Outline;
                    block.EffectColor = def.ExtraData;
                    block.EffectParam = def.ExtraData2;
                    break;
                }
            case TextTagType.Center:
                {
                    block.SpecialLayout = def.GetTagStartIndexInText() == block.StartIndex ? SpecialLayoutFlag.CenterStart : SpecialLayoutFlag.CenterContinue;
                    break;
                }
            case TextTagType.Right:
                {
                    block.SpecialLayout = def.GetTagStartIndexInText() == block.StartIndex ? SpecialLayoutFlag.RightStart : SpecialLayoutFlag.RightContinue;
                    break;
                }
        }
    }

    private Color? TagReadingHelper_ColorArgs(ReadOnlySpan<char> tagName)
    {
        int afterTagSpec = tagName.IndexOf(" ");
        if (afterTagSpec == -1 || afterTagSpec == tagName.Length - 2) return null;

        ReadOnlySpan<char> tagArgs = tagName.Slice(afterTagSpec + 1);
        if (tagArgs.Length == 0) return null;

        bool isHtmlColor = tagArgs[0] == '#';
        if (isHtmlColor)
        {
            Span<Range> arguments = stackalloc Range[2];
            int ranges = tagArgs.Split(arguments, ' ');

            return new Color(tagArgs[arguments[0]]);
        }
        else
        {
            Span<Range> arguments = stackalloc Range[4];
            int ranges = tagArgs.Split(arguments, ' ');
            if (ranges < 3) return null;

            byte.TryParse(tagArgs[arguments[0]], out byte r);
            byte.TryParse(tagArgs[arguments[1]], out byte g);
            byte.TryParse(tagArgs[arguments[2]], out byte b);

            byte a = 255;
            if (ranges == 4 && !byte.TryParse(tagArgs[arguments[3]], out a)) a = 255;

            return new Color(r, g, b, a);
        }
    }

    #region Layout Helpers

    private static int MeasureStringApproximate(ReadOnlySpan<char> text, int textSize, Font font)
    {
        return text.Length * textSize;
    }

    private static unsafe int MeasureStringWidth(ReadOnlySpan<char> text, int textSize, Font font)
    {
        // Its kind of important for this code to match the code in the TextRenderer

        float scale = font.GetScaleFromFontSize(textSize);

        stbtt_fontinfo stbFont = font.StbFontInfo;
        stbtt_GetFontVMetrics(stbFont, out int ascent, out int descent, out int lineGap);

        float xPos = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            int glyphIdx = font.GetGlyphIndexFromChar(ch);
            stbtt_GetGlyphHMetrics(stbFont, glyphIdx, out int advance, out int lsb);

            xPos += advance * scale;
            if (i < text.Length - 1)
            {
                int nextGlyphIndex = font.GetGlyphIndexFromChar(text[i + 1]);
                int kerning = stbtt_GetCodepointKernAdvance(stbFont, glyphIdx, nextGlyphIndex);
                xPos += kerning * scale;
            }
        }

        return (int)MathF.Ceiling(xPos);
    }

    private static unsafe int GetGlyphHeight(Font font, int textSize, char ch)
    {
        float scale = font.GetScaleFromFontSize(textSize);
        var stbFont = font.StbFontInfo;

        int x0, y0, x1, y1;
        stbtt_GetCodepointBitmapBoxSubpixel(stbFont, ch, scale, scale, 0, 0, &x0, &y0, &x1, &y1);

        int height = y1 - y0;
        return height;
    }

    private static unsafe int GetPositionInBlockString(Vector2 pos, ReadOnlySpan<char> text, Font font, int textSize)
    {
        // This code should match MeasureWidth and the code in the TextRenderer

        float scale = font.GetScaleFromFontSize(textSize);

        stbtt_fontinfo stbFont = font.StbFontInfo;
        stbtt_GetFontVMetrics(stbFont, out int ascent, out int descent, out int lineGap);

        float xPos = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            int glyphIdx = font.GetGlyphIndexFromChar(ch);
            stbtt_GetGlyphHMetrics(stbFont, glyphIdx, out int advance, out int lsb);

            float xShift = xPos - MathF.Floor(xPos);
            int x0, y0, x1, y1;
            stbtt_GetCodepointBitmapBoxSubpixel(stbFont, ch, scale, scale, xShift, 0, &x0, &y0, &x1, &y1);

            int width = x1 - x0;
            int height = y1 - y0;
            Rectangle r = new Primitives.Rectangle(xPos, 0, width, height);
            if (r.Contains(pos))
            {
                return i;
            }

            xPos += advance * scale;
            if (i < text.Length - 1)
            {
                int nextGlyphIndex = font.GetGlyphIndexFromChar(text[i + 1]);
                int kerning = stbtt_GetCodepointKernAdvance(stbFont, glyphIdx, nextGlyphIndex);
                xPos += kerning * scale;
            }
        }

        return -1;
    }

    private static unsafe Rectangle GetBoundOfCharacterInString(int chIdx, ReadOnlySpan<char> text, Font font, int textSize)
    {
        // This code should match MeasureWidth and the code in the TextRenderer

        float scale = font.GetScaleFromFontSize(textSize);

        stbtt_fontinfo stbFont = font.StbFontInfo;
        stbtt_GetFontVMetrics(stbFont, out int ascent, out int descent, out int lineGap);

        float xPos = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            int glyphIdx = font.GetGlyphIndexFromChar(ch);
            stbtt_GetGlyphHMetrics(stbFont, glyphIdx, out int advance, out int lsb);

            float xShift = xPos - MathF.Floor(xPos);
            int x0, y0, x1, y1;
            stbtt_GetCodepointBitmapBoxSubpixel(stbFont, ch, scale, scale, xShift, 0, &x0, &y0, &x1, &y1);

            int width = x1 - x0;
            int height = y1 - y0;
            if (chIdx == i)
            {
                return new Rectangle(xPos, 0, width, height);
            }

            xPos += advance * scale;
            if (i < text.Length - 1)
            {
                int nextGlyphIndex = font.GetGlyphIndexFromChar(text[i + 1]);
                int kerning = stbtt_GetCodepointKernAdvance(stbFont, glyphIdx, nextGlyphIndex);
                xPos += kerning * scale;
            }
        }

        return Rectangle.Empty;
    }

    private Vector2 GetNextGlyphPosition(Vector2 pen, char c, out Vector2 drawPosition)
    {
        drawPosition = Vector2.Zero;
        return Vector2.Zero;
    }

    #endregion

    #region API

    public void RenderLastLayout(Renderer r, Vector3 offset, Color defaultColor, TextEffect? effect = null)
    {
        if (_lastLayoutFont == null || _lastLayoutText.Count == 0) return;

        Span<char> text = CollectionsMarshal.AsSpan(_lastLayoutText);

        Color baseColor = defaultColor;
        Vector3 pen = offset;
        pen.Y += _lastLayoutRenderOffsetY;
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];
            if (block.Skip) continue;
            if (block.Newline)
            {
                pen.X = offset.X;
                pen.Y += _lastLayoutTextHeight;
            }

            Color color = block.UseDefaultColor ? baseColor : block.Color * baseColor.A;

            ReadOnlySpan<char> blockString = block.GetBlockString(text);
            TextRenderer.RenderText(r, pen, color, blockString, _lastLayoutFont, _lastLayoutTextSize, effect);
            pen.X += block.Width;
        }
    }

    #region Selection

    public (int, int, float) GetSelectionIndexFromPosition(Vector2 position)
    {
        if (_textBlocks.Count == 0) return (0, 0, 0);
        AssertNotNull(_lastLayoutFont);

        int closestIndex = -1;
        int insideBound = -1;
        float distToClosest = float.MaxValue;
        float distClosestY = float.MaxValue;

        Span<char> str = CollectionsMarshal.AsSpan(_lastLayoutText);

        int index = 0;
        float blockHeight = _lastLayoutTextHeight;
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];
            Rectangle blockBounds = new Primitives.Rectangle(block.X, block.Y, block.Width, blockHeight);

            bool lastBlockOnLine = false;
            if (i == _textBlocks.Count - 1 || _textBlocks[i + 1].Newline)
                lastBlockOnLine = true;

            if (lastBlockOnLine)
                blockBounds.Width += _lastLayoutTextSize;

            bool leftOfCenter = true;// position.X < bound.Center.X;
            if (!blockBounds.Contains(position)) continue;

            ReadOnlySpan<char> blockStr = block.GetBlockString(str);
            int insideChar = GetPositionInBlockString(position - blockBounds.Position, blockStr, _lastLayoutFont, _lastLayoutTextSize);
            if (insideChar != -1)
            {
                insideBound = index + insideChar;
                distToClosest = 0;
            }
            bool a = true;

            //insideBound = leftOfCenter ? index : index + 1;

            index += block.Length;
        }

        int boundReturn = insideBound;
        if (boundReturn == -1) boundReturn = closestIndex;
        if (boundReturn == -1) boundReturn = 0;
        return (boundReturn, insideBound, distToClosest);

        ////int index = 0;
        //Vector2 pen = new Vector2(0, 0);
        //for (int i = 0; i < _textBlocks.Count; i++)
        //{
        //    TextBlock block = _textBlocks[i];

        //    if (block.Newline)
        //    {
        //        pen.X = 0;
        //        pen.Y += _lastLayoutTextHeight;
        //    }

        //    bool lastBlockOnLine = false;
        //    if (i == _textBlocks.Count - 1 || _textBlocks[i + 1].Newline)
        //        lastBlockOnLine = true;
        //    int loopAdd = lastBlockOnLine ? 1 : 0;

        //    ReadOnlySpan<char> blockString = block.GetBlockString(string.Empty);// _text);
        //    for (int ci = 0; ci < blockString.Length + loopAdd; ci++)
        //    {
        //        bool isExtraLoop = ci == blockString.Length;
        //        char c = isExtraLoop ? ' ' : blockString[ci];

        //        pen = GetNextGlyphPosition(pen, c, out Vector2 _);

        //        Rectangle bound = new Rectangle(pen.X, pen.Y, 0, _lastLayoutTextHeight);

        //        bool leftOfCenter = true;// position.X < bound.Center.X;
        //        if (bound.Contains(position))
        //        {
        //            insideBound = leftOfCenter ? index : index + 1;
        //        }

        //        float dist = Math.Abs(position.X - bound.Center.X);
        //        float distY = Math.Abs(position.Y - bound.Center.Y);
        //        if (distClosestY > distY)
        //        {
        //            closestIndex = leftOfCenter ? index : index + 1;
        //            distClosestY = distY;
        //            distToClosest = dist;
        //        }
        //        if (distClosestY == distY && distToClosest > dist)
        //        {
        //            closestIndex = leftOfCenter ? index : index + 1;
        //            distToClosest = dist;
        //        }

        //        pen.X += 0;
        //        index++;
        //    }
        //}

        //int boundReturn = insideBound;
        //if (boundReturn == -1) boundReturn = closestIndex;
        //if (boundReturn == -1) boundReturn = 0;
        //return (boundReturn, insideBound, distToClosest);
    }

    public Rectangle GetBoundOfSelectionIndex(int selectionIdx)
    {
        if (_textBlocks.Count == 0) return Rectangle.Empty;
        AssertNotNull(_lastLayoutFont);

        int insideBound = -1;

        Span<char> str = CollectionsMarshal.AsSpan(_lastLayoutText);

        int index = 0;
        float blockHeight = _lastLayoutTextHeight;
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];

            bool lastBlockOnLine = false;
            if (i == _textBlocks.Count - 1 || _textBlocks[i + 1].Newline)
                lastBlockOnLine = true;

            ReadOnlySpan<char> blockStr = block.GetBlockString(str);

            int thisBlockStart = index;
            int thisBlockEnd = index + (blockStr.Length - 1);
            if (selectionIdx >= thisBlockStart && selectionIdx < thisBlockEnd)
            {
                Rectangle b = GetBoundOfCharacterInString(selectionIdx - thisBlockStart, blockStr, _lastLayoutFont, _lastLayoutTextSize);
                b.Height = _lastLayoutTextHeight;
                return b;
            }

            for (int ch = 0; ch < blockStr.Length; ch++)
            {
                if (index == selectionIdx)
                {

                }

                index++;
            }
        }

        return Rectangle.Empty;

        //int index = 0;
        //Vector2 pen = new Vector2(0, 0);
        //for (int i = 0; i < _textBlocks.Count; i++)
        //{
        //    TextBlock block = _textBlocks[i];

        //    if (block.Newline)
        //    {
        //        pen.X = 0;
        //        pen.Y += _lastLayoutTextHeight;
        //    }

        //    bool lastBlockOnLine = false;
        //    if (i == _textBlocks.Count - 1 || _textBlocks[i + 1].Newline)
        //        lastBlockOnLine = true;
        //    int loopAdd = lastBlockOnLine ? 1 : 0;

        //    ReadOnlySpan<char> blockString = block.GetBlockString(string.Empty);//_text);
        //    for (int ci = 0; ci < blockString.Length + loopAdd; ci++)
        //    {
        //        char c = ci == blockString.Length ? ' ' : blockString[ci];

        //        pen = GetNextGlyphPosition(pen, c, out Vector2 _);

        //        Rectangle bound = new Rectangle(pen.X, pen.Y, 0, _lastLayoutTextHeight);
        //        if (index == selectionIdx) return bound;

        //        pen.X += 0;
        //        index++;
        //    }
        //}

        //return Rectangle.Empty;
    }

    public int GetSelectionIndexFromStringIndex(int stringIndex)
    {
        static (ForEachResult, int) Functor((TextBlock block, int indexInBlock) blockData, (int charIdx, int lineIdx, int charIdxOnLine) selData, int argStringIndex)
        {
            if (blockData.block.StartIndex + blockData.indexInBlock == argStringIndex) return (ForEachResult.Break, selData.charIdx);
            return (ForEachResult.Continue, -1);
        }
        return ForEachTextBlock(Functor, stringIndex);
    }

    public int GetSelectionIndexMax()
    {
        static (ForEachResult, int) Functor((TextBlock block, int indexInBlock) blockData, (int charIdx, int lineIdx, int charIdxOnLine) selData, int _)
        {
            return (ForEachResult.StoreResultAndContinue, selData.charIdx);
        }
        return ForEachTextBlock(Functor, 0);
    }

    public int GetStringIndexFromSelectionIndex(int selIdx)
    {
        static (ForEachResult, int) Functor((TextBlock block, int indexInBlock) blockData, (int charIdx, int lineIdx, int charIdxOnLine) selData, int argSelectionIdx)
        {
            if (selData.charIdx == argSelectionIdx) return (ForEachResult.Break, blockData.block.StartIndex + blockData.indexInBlock);
            return (ForEachResult.Continue, -1);
        }
        int actualStringIndex = ForEachTextBlock(Functor, selIdx);
        if (actualStringIndex == -1 || actualStringIndex > _lastLayoutText.Count) actualStringIndex = 0;
        return actualStringIndex;
    }

    public (int lineIdx, int charIdxOnLine) GetLineOfSelectedIndex(int selectionIdx)
    {
        static (ForEachResult, (int, int)) Functor((TextBlock block, int indexInBlock) blockData, (int charIdx, int lineIdx, int charIdxOnLine) selData, int argSelectionIdx)
        {
            if (selData.charIdx == argSelectionIdx) return (ForEachResult.Break, (selData.lineIdx, selData.charIdxOnLine));
            return (ForEachResult.Continue, (-1, 0));
        }
        return ForEachTextBlock(Functor, selectionIdx);
    }

    // Used for up/down arrow keys in text input.
    public int GetSelectionIndexOnOtherLine(int selectionIdx, int lineSign)
    {
        (int lineOfCurrentIndex, int charIdxOnLine) = GetLineOfSelectedIndex(selectionIdx);
        int otherLineIndex = lineOfCurrentIndex + lineSign;
        if (otherLineIndex < 0) otherLineIndex = 0;

        static (ForEachResult, int) Functor((TextBlock block, int indexInBlock) blockData, (int charIdx, int lineIdx, int charIdxOnLine) selData, (int selectionIdx, int otherLineIndex) args)
        {
            if (selData.lineIdx == args.otherLineIndex)
            {
                // Exact character on other line.
                if (selData.charIdxOnLine == args.selectionIdx) return (ForEachResult.Break, selData.charIdx);

                // Store fallback position if otherline is shorter.
                if (selData.charIdxOnLine < args.selectionIdx) return (ForEachResult.StoreResultAndContinue, selData.charIdx);
            }

            return (ForEachResult.Continue, -1);
        }
        int selIndexOtherLine = ForEachTextBlock(Functor, (charIdxOnLine, otherLineIndex));
        if (selIndexOtherLine == -1) selIndexOtherLine = selectionIdx;
        return selIndexOtherLine;
    }

    public delegate void ForEachLineBetweenSelectionIndicesDelegateFunc<A1>(Rectangle lineRect, A1 arg1);

    private int GetFirstSelectionIndexOnLine(int lineIndex)
    {
        static (ForEachResult, int) Functor((TextBlock block, int indexInBlock) blockData, (int charIdx, int lineIdx, int charIdxOnLine) selData, int arglineIndex)
        {
            if (selData.lineIdx == arglineIndex) return (ForEachResult.Break, selData.charIdx);
            return (ForEachResult.Continue, -1);
        }
        return ForEachTextBlock(Functor, lineIndex);
    }

    private int GetLastSelectionIndexOnLine(int lineIndex)
    {
        static (ForEachResult, int) Functor((TextBlock block, int indexInBlock) blockData, (int charIdx, int lineIdx, int charIdxOnLine) selData, int arglineIndex)
        {
            if (selData.lineIdx == arglineIndex) return (ForEachResult.StoreResultAndContinue, selData.charIdx);
            if (selData.lineIdx > arglineIndex) return (ForEachResult.BreakAndReturnStoredResult, -1);
            return (ForEachResult.Continue, -1);
        }
        return ForEachTextBlock(Functor, lineIndex);
    }

    public void ForEachLineBetweenSelectionIndices<A1>(int selectionIdxOne, int selectionIdxTwo, ForEachLineBetweenSelectionIndicesDelegateFunc<A1> functor, A1 arg1)
    {
        int startIdx = Math.Min(selectionIdxOne, selectionIdxTwo);
        int endIdx = Math.Max(selectionIdxOne, selectionIdxTwo);

        (int lineStart, int _) = GetLineOfSelectedIndex(startIdx);
        (int lineEnd, int _) = GetLineOfSelectedIndex(endIdx);

        if (lineStart == lineEnd)
        {
            Rectangle lineBoundStart = GetBoundOfSelectionIndex(startIdx);
            Rectangle lineBoundEnd = GetBoundOfSelectionIndex(endIdx);
            Rectangle totalLineBound = Rectangle.FromMinMaxPointsChecked(lineBoundStart.Position, lineBoundEnd.BottomRight);
            functor(totalLineBound, arg1);
            return;
        }

        for (int lineIdx = lineStart; lineIdx <= lineEnd; lineIdx++)
        {
            int selIndexLineStart = GetFirstSelectionIndexOnLine(lineIdx);
            if (selIndexLineStart < startIdx) selIndexLineStart = startIdx;

            int selIndexLineEnd = GetLastSelectionIndexOnLine(lineIdx);
            if (selIndexLineEnd > endIdx) selIndexLineEnd = endIdx;

            Rectangle lineBoundStart = GetBoundOfSelectionIndex(selIndexLineStart);
            Rectangle lineBoundEnd = GetBoundOfSelectionIndex(selIndexLineEnd);
            Rectangle totalLineBound = Rectangle.FromMinMaxPointsChecked(lineBoundStart.Position, lineBoundEnd.BottomRight);
            functor(totalLineBound, arg1);
        }
    }

    #endregion

    private enum ForEachResult
    {
        Continue,
        Break,
        BreakAndReturnStoredResult,
        StoreResultAndContinue
    }

    private delegate (ForEachResult, T) ForEachTextBlockDelegateFunc<T, A1>((TextBlock, int) blockData, (int, int, int) selData, A1 arg1);

    private T ForEachTextBlock<T, A1>(ForEachTextBlockDelegateFunc<T, A1> functor, A1 arg1)
    {
        T returnVal = default!;
        bool isValDefault = true;

        int index = 0;
        int indexOnLine = 0;
        int lineIdx = 0;
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];

            if (block.Newline)
            {
                lineIdx++;
                indexOnLine = 0;
            }

            bool lastBlockOnLine = false;
            if (i == _textBlocks.Count - 1 || _textBlocks[i + 1].Newline)
                lastBlockOnLine = true;
            int loopAdd = lastBlockOnLine ? 1 : 0;

            for (int ci = 0; ci < block.Length + loopAdd; ci++)
            {
                (ForEachResult action, T functionVal) = functor((block, ci), (index, lineIdx, indexOnLine), arg1);
                switch (action)
                {
                    case ForEachResult.Break:
                        return functionVal;
                    case ForEachResult.StoreResultAndContinue:
                    case ForEachResult.Continue when isValDefault:
                        returnVal = functionVal;
                        break;
                    case ForEachResult.BreakAndReturnStoredResult:
                        return returnVal;
                }
                isValDefault = false;

                index++;
                indexOnLine++;
            }
        }

        return returnVal;
    }

    #endregion
}
