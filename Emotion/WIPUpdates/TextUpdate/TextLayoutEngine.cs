#nullable enable

#region Using

using System.Runtime.InteropServices;
using Emotion.Game.Text;
using Emotion.Graphics.Text;
using Emotion.Utility;
using Emotion.WIPUpdates.TextUpdate;
using Emotion.WIPUpdates.TextUpdate.LayoutEngineTypes;

#endregion

namespace Emotion.UI;

public class TextLayoutEngine
{
    public static bool TestNewRenderer = false; // WIP

    /// <summary>
    /// The size of the text as layouted last by a call to Run().
    /// </summary>
    public Vector2 TextSize { get; protected set; }

    /// <summary>
    /// The height of the text as if the height mode is FullHeight.
    /// Used to determine total bounds where text will be rendered.
    /// </summary>
    public float TextRenderHeight;

    /// <summary>
    /// Whether to resolve tags in the text, such as <color></color>
    /// Text selection APIs /should/ handle these fine.
    /// </summary>
    public bool ResolveTags = true;

    private bool _wrapText = false;

    private string _text = string.Empty;
    private float _wrapWidth;
    private GlyphHeightMeasurement _heightMode;

    private DrawableFontAtlas _defaultAtlas;
    private bool _dirty = true;

    private List<TagDefinition> _tagDefinitions = new List<TagDefinition>();
    private List<TextBlock> _textBlocks = new List<TextBlock>();

    /// <summary>
    /// The render offset of the text. This is applied by default when calling Render, but can be skipped by calling RenderNoOffset
    /// </summary>
    public Vector3 LayoutRenderOffset { get; protected set; } = Vector3.Zero;

    public bool NeedsToReRun(string text, float? wrapWidth, DrawableFontAtlas defaultAtlas)
    {
        if (_wrapWidth != wrapWidth) return true;
        if (text != _text) return true;
        if (defaultAtlas != _defaultAtlas) return true;
        return false;
    }

    public void InitializeLayout(string text, GlyphHeightMeasurement heightMode = GlyphHeightMeasurement.FullHeight)
    {
        if (_text == text && _heightMode == heightMode) return;

        _text = text ?? string.Empty;
        _heightMode = heightMode;
        _dirty = true;
    }

    public void SetWrap(float? wrapWidth)
    {
        if (wrapWidth == null)
        {
            if (!_wrapText) return;

            _wrapText = false;
        }
        else
        {
            if (_wrapText && _wrapWidth == wrapWidth.Value) return;

            _wrapWidth = wrapWidth.Value;
            _wrapText = true;
        }

        _dirty = true;
    }

    public void SetDefaultAtlas(DrawableFontAtlas defaultAtlas)
    {
        if (_defaultAtlas == defaultAtlas) return;

        _defaultAtlas = defaultAtlas;
        _dirty = true;
    }

    public void Run()
    {
        if (!_dirty) return;
        _dirty = true;
        _textBlocks.Clear();

        // 1. Extract layout/draw command tags.
        if (ResolveTags)
        {
            _tagDefinitions.Clear();
            int tagNameStart = -1;
            TextBlock currentBlock = new TextBlock(0);
            for (int cIdx = 0; cIdx < _text.Length; cIdx++)
            {
                char c = _text[cIdx];

                // Inside tag name and it is being closed
                if (tagNameStart != -1 && c == '>')
                {
                    // + and - are for skipping the brackets
                    int tagStart = tagNameStart + 1;
                    int tagLength = cIdx - tagNameStart - 1;

                    TagDefinition tagDefine = new TagDefinition(tagStart, tagLength);
                    bool isValidTag = ProcessTag(ref tagDefine);

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
                currentBlock.Length = _text.Length - currentBlock.StartIndex;
                _textBlocks.Add(currentBlock);
            }
        }
        else
        {
            _textBlocks.Add(new TextBlock(0, _text.Length));
        }

        // 2. Break up blocks on user specified newline characters.
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];
            ReadOnlySpan<char> blockString = block.GetBlockString(_text);

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

                blockString = block.GetBlockString(_text);
                lineBreakIndex = blockString.IndexOf('\n');
            }
        }

        // 3. Wrap text blocks, this will break them up in more block.
        if (_wrapText)
        {
            Span<char> wordBreakCharacters = [' ', '\n', '-'];

            float sizeOnCurrentLine = 0;
            for (int i = 0; i < _textBlocks.Count; i++)
            {
                TextBlock block = _textBlocks[i];
                if (block.Newline) sizeOnCurrentLine = 0;

                ReadOnlySpan<char> blockString = block.GetBlockString(_text);
                float stringWidth = MeasureStringWidth(blockString);

                // This block fits.
                if (sizeOnCurrentLine + stringWidth < _wrapWidth)
                {
                    sizeOnCurrentLine += stringWidth;
                    continue;
                }

                // Check how much of this block can fit by splitting it with word breaks.
                int nextWordBreakChar = 0;
                int lastFitBreakChar = 0;
                float sectionWidth = 0;
                while (sizeOnCurrentLine + sectionWidth < _wrapWidth)
                {
                    lastFitBreakChar = nextWordBreakChar;
                    int nextIndex = blockString.Slice(nextWordBreakChar).IndexOfAny(wordBreakCharacters);
                    if (nextIndex == -1)
                        break;

                    nextWordBreakChar += nextIndex + 1;
                    sectionWidth = MeasureStringWidth(blockString.Slice(0, nextWordBreakChar));
                }

                // Not even a single word break fits.
                if (lastFitBreakChar == 0)
                {
                    // Try to newline this whole block first.
                    if (stringWidth < _wrapWidth)
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
                    while (sizeOnCurrentLine + sectionWidth > _wrapWidth)
                    {
                        charactersFit--;
                        if (charactersFit == 0) break;
                        sectionWidth = MeasureStringWidth(blockString.Slice(0, charactersFit));
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
                if (ResolveTags && lastFitBreakChar > 0 && _text[block.StartIndex + block.Length - 1] == ' ') block.Length--;
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
                    Assert(false, $"Infinite loop at layout of: {_text}");
                    break;
                }
            }
        }

        // Get lists as spans for easier working with.
        // Nothing should be added or removed from the lists past this point.
        Span<TextBlock> blocksSpan = CollectionsMarshal.AsSpan(_textBlocks);
        Span<TagDefinition> tagDefSpan = CollectionsMarshal.AsSpan(_tagDefinitions);

        // Last. Combine tags and assign blocks to definitions.
        for (int i = 0; i < _tagDefinitions.Count; i++)
        {
            ref TagDefinition tag = ref tagDefSpan[i];
            int startIndex = tag.NameStartIdx;

            int endIndex = _text.Length;
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

            for (int b = 0; b < _textBlocks.Count; b++)
            {
                ref TextBlock block = ref blocksSpan[b];
                if (block.Skip) continue;

                if (startIndex <= block.StartIndex && endIndex >= block.StartIndex + block.Length)
                {
                    ApplyTag(tag, ref block);
                }
            }
        }

        TextSize = MeasureString();
    }

    private TextLayouter _layouter; // temporarily used for text layout render

    public void Render(RenderComposer c, Vector3 offset, Color baseColor, FontEffect defaultEffect = FontEffect.None, float defaultEffectAmount = 0f, Color? defaultEffectColor = null)
    {
        RenderWithNoLayoutOffset(c, offset + LayoutRenderOffset, baseColor, defaultEffect, defaultEffectAmount, defaultEffectColor);
    }

    public void RenderWithNoLayoutOffset(RenderComposer c, Vector3 offset, Color baseColor, FontEffect defaultEffect = FontEffect.None, float defaultEffectAmount = 0f, Color? defaultEffectColor = null)
    {
        if (_defaultAtlas == null) return;
        AssertNotNull(_defaultAtlas);

        _layouter ??= new TextLayouter(_defaultAtlas);
        _layouter.SetAtlas(_defaultAtlas);
        TextLayouter layouter = _layouter;

        var reUsableVector = new Vector3();
        _defaultAtlas.SetupDrawing(c, _text, defaultEffect, defaultEffectAmount, defaultEffectColor); // outline layout tag doesnt work because of this

        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock currentBlock = _textBlocks[i];
            if (currentBlock.Skip) continue;

            if (TestNewRenderer)
            {
                TextRenderEngine.CacheEntries(_text, currentBlock);
                TextRenderEngine.RenderBlock(c, _text, currentBlock, _defaultAtlas.Font);
            }

            Color color = currentBlock.UseDefaultColor ? baseColor : currentBlock.Color * baseColor.A;

            bool blockEffect = currentBlock.TextEffect != FontEffect.None;
            FontEffect effect = blockEffect ? currentBlock.TextEffect : defaultEffect;
            Color effectColor = blockEffect ? currentBlock.EffectColor : defaultEffectColor ?? Color.White;
            float effectAmount = blockEffect ? currentBlock.EffectParam : defaultEffectAmount;

            if (currentBlock.Newline)
            {
                layouter.NewLine();
            }

            if (currentBlock.SpecialLayout == SpecialLayoutFlag.CenterStart)
            {
                float combinedBlockWidth = MeasureStringWidth(currentBlock.GetBlockString(_text));
                for (int ii = i + 1; ii < _textBlocks.Count; ii++)
                {
                    TextBlock otherBlock = _textBlocks[ii];
                    if (otherBlock.Skip) continue;
                    if (otherBlock.SpecialLayout != SpecialLayoutFlag.CenterContinue) break;
                    if (otherBlock.Newline) break;

                    combinedBlockWidth += MeasureStringWidth(otherBlock.GetBlockString(_text));
                }

                // todo: calculate blocks positions outside of rendering
                // maybe also render params such as color?
                float textWidth = TextSize.X;
                float center = textWidth / 2f - combinedBlockWidth / 2f;

                var currentPenLoc = layouter.GetPenLocation();
                layouter.AddToPen(new Vector2(center - currentPenLoc.X, 0));
            }
            else if (currentBlock.SpecialLayout == SpecialLayoutFlag.RightStart)
            {
                float combinedBlockWidth = MeasureStringWidth(currentBlock.GetBlockString(_text));
                for (int ii = i + 1; ii < _textBlocks.Count; ii++)
                {
                    TextBlock otherBlock = _textBlocks[ii];
                    if (otherBlock.Skip) continue;
                    if (otherBlock.SpecialLayout != SpecialLayoutFlag.RightContinue) break;
                    if (otherBlock.Newline) break;

                    combinedBlockWidth += MeasureStringWidth(otherBlock.GetBlockString(_text));
                }

                // todo: calculate blocks positions outside of rendering
                // maybe also render params such as color?
                float textWidth = TextSize.X;
                float center = textWidth - combinedBlockWidth;

                var currentPenLoc = layouter.GetPenLocation();
                layouter.AddToPen(new Vector2(center - currentPenLoc.X, 0));
            }

            ReadOnlySpan<char> text = currentBlock.GetBlockString(_text);
            foreach (char ch in text)
            {
                Vector2 gPos = layouter.AddLetter(ch, out DrawableGlyph g);
                if (g == null || g.GlyphUV == Rectangle.Empty) continue;

                reUsableVector.X = gPos.X;
                reUsableVector.Y = gPos.Y;
                _defaultAtlas.DrawGlyph(c, g, offset + reUsableVector, color);
            }

            // c.RenderString(offset, color, text, _defaultAtlas, layouter, effect, effectAmount, effectColor);
        }

        _defaultAtlas.FinishDrawing(c);

        RenderTest(c, offset);
    }

    private bool ProcessTag(ref TagDefinition def)
    {
        ReadOnlySpan<char> tagName = def.GetTagName(_text);
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
                    block.TextEffect = FontEffect.Outline;
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

    private float MeasureStringWidth(ReadOnlySpan<char> text)
    {
        text = text.TrimEnd();

        float sizeSoFar = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            Vector2 pos = GetNextGlyphPosition(new Vector2(sizeSoFar, 0), c, out Vector2 _, out DrawableGlyph? g);
            sizeSoFar = pos.X;
            if (g == null) continue;

            sizeSoFar += g.XAdvance;
        }
        return sizeSoFar;
    }

    private Vector2 GetNextGlyphPosition(Vector2 pen, char c, out Vector2 drawPosition, out DrawableGlyph? g)
    {
        var _atlas = _defaultAtlas;
        bool _hasZeroGlyph = false;

        var result = new Vector2(pen.X, pen.Y);
        drawPosition = Vector2.Zero;
        g = null;

        if (c == '\n')
        {
            result.X = 0;
            result.Y += MathF.Round(_atlas.FontHeight);
        }
        else
        {
            if (!_atlas.Glyphs.TryGetValue(c, out g))
            {
                if (!_hasZeroGlyph) return result;
                g = _atlas.Glyphs[(char)0];
            }

            drawPosition = result;
            drawPosition.X += g.XBearing;
        }

        return result;
    }

    private Vector2 MeasureString()
    {
        Vector3 layoutRenderOffset = Vector3.Zero;

        float lineSpacing = _defaultAtlas.FontHeight;

        Vector2 sizeSoFar = new Vector2(0, 0);
        float largestLine = 0;

        float currentLineWithoutDescent = 0;
        float smallestCharacterHeight = lineSpacing;
        float largestCharacterHeight = 0;

        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];

            // If jumping on a new line, check if the current line is the largest so far.
            if (block.Newline)
            {
                if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
                sizeSoFar.X = 0;
                sizeSoFar.Y += lineSpacing;

                currentLineWithoutDescent = 0;
                smallestCharacterHeight = lineSpacing;
                largestCharacterHeight = 0;
            }

            ReadOnlySpan<char> blockString = block.GetBlockString(_text);
            for (int ci = 0; ci < blockString.Length; ci++)
            {
                char c = blockString[ci];

                Vector2 pos = GetNextGlyphPosition(sizeSoFar, c, out Vector2 _, out DrawableGlyph? g);
                sizeSoFar = pos;
                if (g == null) continue;

                sizeSoFar.X += g.XAdvance;

                // Record these metrics to use for height mode
                var characterHeightWithoutDescent = g.Height + (_defaultAtlas.Ascent - g.Height) + g.Descent;
                if (characterHeightWithoutDescent > currentLineWithoutDescent) currentLineWithoutDescent = characterHeightWithoutDescent;
                if (g.Height > largestCharacterHeight) largestCharacterHeight = g.Height;
                if (g.Height > 0 && g.Height < smallestCharacterHeight) smallestCharacterHeight = g.Height;
            }
        }

        if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
        if (largestLine != 0) sizeSoFar.X = largestLine;

        // As if FullHeight
        TextRenderHeight = sizeSoFar.Y + lineSpacing;

        switch (_heightMode)
        {
            case GlyphHeightMeasurement.FullHeight:
                sizeSoFar.Y += lineSpacing;
                break;
            case GlyphHeightMeasurement.NoDescent:
                sizeSoFar.Y += MathF.Ceiling(currentLineWithoutDescent);
                break;
            case GlyphHeightMeasurement.UnderflowLittle:
                float halfWay = Maths.Lerp(smallestCharacterHeight, largestCharacterHeight, 0.5f);
                sizeSoFar.Y += MathF.Ceiling(halfWay);
                layoutRenderOffset.Y = -MathF.Ceiling(_defaultAtlas.Ascent - halfWay);
                break;
            case GlyphHeightMeasurement.NoMinY:
                sizeSoFar.Y += MathF.Ceiling(largestCharacterHeight);
                layoutRenderOffset.Y = -MathF.Ceiling(_defaultAtlas.Ascent - largestCharacterHeight);
                break;
            case GlyphHeightMeasurement.Underflow:
                sizeSoFar.Y += MathF.Ceiling(smallestCharacterHeight);
                layoutRenderOffset.Y = -MathF.Ceiling(_defaultAtlas.Ascent - smallestCharacterHeight);
                break;
        }

        LayoutRenderOffset = layoutRenderOffset;

        return sizeSoFar;
    }

    #endregion

    #region API

    public void RenderTest(RenderComposer c, Vector3 offset)
    {
        Vector2 position = Engine.Host.MousePosition - offset.ToVec2();

        (int index, int insideBound, float dist) = GetSelectionIndexFromPosition(position);
        var boundRect = GetBoundOfSelectionIndex(index);
        //c.RenderSprite(offset + boundRect.Position.ToVec3(), boundRect.Size, Color.Red);
        //c.RenderString(offset - new Vector3(0, 50, 0), Color.Cyan, $"{index}, inside: {insideBound}, dist: {dist}", _defaultAtlas);

    }

    #region Selection

    public (int, int, float) GetSelectionIndexFromPosition(Vector2 position)
    {
        int closestIndex = -1;
        int insideBound = -1;
        float distToClosest = float.MaxValue;
        float distClosestY = float.MaxValue;

        int index = 0;
        Vector2 pen = new Vector2(0, 0);
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];

            if (block.Newline)
            {
                pen.X = 0;
                pen.Y += _defaultAtlas.FontHeight;
            }

            bool lastBlockOnLine = false;
            if (i == _textBlocks.Count - 1 || _textBlocks[i + 1].Newline)
                lastBlockOnLine = true;
            int loopAdd = lastBlockOnLine ? 1 : 0;

            ReadOnlySpan<char> blockString = block.GetBlockString(_text);
            for (int ci = 0; ci < blockString.Length + loopAdd; ci++)
            {
                bool isExtraLoop = ci == blockString.Length;
                char c = isExtraLoop ? ' ' : blockString[ci];

                pen = GetNextGlyphPosition(pen, c, out Vector2 _, out DrawableGlyph g);
                if (g == null) continue;

                Rectangle bound = new Rectangle(pen.X, pen.Y, g.XAdvance, _defaultAtlas.FontHeight);

                bool leftOfCenter = true;// position.X < bound.Center.X;
                if (bound.Contains(position))
                {
                    insideBound = leftOfCenter ? index : index + 1;
                }

                float dist = Math.Abs(position.X - bound.Center.X);
                float distY = Math.Abs(position.Y - bound.Center.Y);
                if (distClosestY > distY)
                {
                    closestIndex = leftOfCenter ? index : index + 1;
                    distClosestY = distY;
                    distToClosest = dist;
                }
                if (distClosestY == distY && distToClosest > dist)
                {
                    closestIndex = leftOfCenter ? index : index + 1;
                    distToClosest = dist;
                }

                pen.X += g.XAdvance;
                index++;
            }
        }

        int boundReturn = insideBound;
        if (boundReturn == -1) boundReturn = closestIndex;
        if (boundReturn == -1) boundReturn = 0;
        return (boundReturn, insideBound, distToClosest);
    }

    public Rectangle GetBoundOfSelectionIndex(int selectionIdx)
    {
        int index = 0;
        Vector2 pen = new Vector2(0, 0);
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];

            if (block.Newline)
            {
                pen.X = 0;
                pen.Y += _defaultAtlas.FontHeight;
            }

            bool lastBlockOnLine = false;
            if (i == _textBlocks.Count - 1 || _textBlocks[i + 1].Newline)
                lastBlockOnLine = true;
            int loopAdd = lastBlockOnLine ? 1 : 0;

            ReadOnlySpan<char> blockString = block.GetBlockString(_text);
            for (int ci = 0; ci < blockString.Length + loopAdd; ci++)
            {
                char c = ci == blockString.Length ? ' ' : blockString[ci];

                pen = GetNextGlyphPosition(pen, c, out Vector2 _, out DrawableGlyph g);
                if (g == null) continue;

                Rectangle bound = new Rectangle(pen.X, pen.Y, g.XAdvance, _defaultAtlas.FontHeight);
                if (index == selectionIdx) return bound;

                pen.X += g.XAdvance;
                index++;
            }
        }

        return Rectangle.Empty;
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
        if (actualStringIndex == -1 || actualStringIndex > _text.Length) actualStringIndex = 0;
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
