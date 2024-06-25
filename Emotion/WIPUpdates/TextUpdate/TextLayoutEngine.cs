#region Using

using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Emotion.Game.Text;
using Emotion.Graphics.Text;
using Emotion.Utility;
using Emotion.WIPUpdates.TextUpdate;
using static Emotion.UI.TextLayoutEngine;
using static Emotion.UI.UIBaseWindow;

#endregion

#nullable enable

namespace Emotion.UI;

public class TextLayoutEngine
{
    public static bool TestNewRenderer = false; // WIP

    /// <summary>
    /// The size of the text as layouted last by a call to Run().
    /// </summary>
    public Vector2 TextSize { get; protected set; }

    /// <summary>
    /// Whether to resolve tags in the text, such as <color></color>
    /// </summary>
    public bool ResolveTags = true;

    /// <summary>
    /// Whether to resolve tags in the text that generate content themselves.
    /// Requires ResolveTags to be true. If set to false only layout and tags that
    /// do not add text will be resolved. Text selection APIs do not support content tags.
    /// </summary>
    public bool ResolveContentTags = true;

    private bool _wrapText = false;

    private string _text = string.Empty;
    private float _wrapWidth;
    private GlyphHeightMeasurement _heightMode;

    private DrawableFontAtlas _defaultAtlas;
    private bool _dirty = true;

    private List<TagDefinition> _tagDefinitions = new List<TagDefinition>();
    private List<TextBlock> _textBlocks = new List<TextBlock>();

    public void InitializeLayout(string text, GlyphHeightMeasurement heightMode = GlyphHeightMeasurement.FullHeight)
    {
        if (_text == text && _heightMode == heightMode) return;

        _text = text;
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
                    var tagDefine = new TagDefinition(tagNameStart + 1, cIdx - tagNameStart - 1); // + and - are for skipping the brackets
                    _tagDefinitions.Add(tagDefine);

                    tagNameStart = -1;
                    currentBlock = new TextBlock(cIdx + 1);
                }

                if (c == '<')
                {
                    // Close current tag.
                    currentBlock.Length = cIdx - currentBlock.StartIndex;
                    _textBlocks.Add(currentBlock);

                    // New tag starts
                    tagNameStart = cIdx;
                }
            }

            if (tagNameStart != -1)
            {
                currentBlock = new TextBlock(tagNameStart);
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
                _textBlocks[i] = block;

                TextBlock newBlock = new TextBlock(block.StartIndex + lastFitBreakChar);
                newBlock.Length = lengthToEnd;
                newBlock.Newline = true;
                _textBlocks.Insert(i + 1, newBlock);

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
            for (int ii = i + 1; ii < _tagDefinitions.Count; ii++)
            {
                ref TagDefinition nextTag = ref tagDefSpan[ii];
                var name = nextTag.GetTagName(_text);
                if (name.SequenceEqual("/"))
                {
                    endIndex = nextTag.NameStartIdx;
                    break;
                }
            }

            for (int b = 0; b < _textBlocks.Count; b++)
            {
                ref TextBlock block = ref blocksSpan[b];
                if (block.Skip) continue;
                
                if (startIndex <= block.StartIndex && endIndex >= block.StartIndex + block.Length)
                {
                    ProcessTag(tag, ref block);
                }
            }
        }

        TextSize = MeasureString();
    }

    public void Render(RenderComposer c, Vector3 offset, Color baseColor, FontEffect defaultEffect = FontEffect.None, float defaultEffectAmount = 0f, Color? defaultEffectColor = null)
    {
        Vector3 pen = Vector3.Zero;
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock currentBlock = _textBlocks[i];
            if (currentBlock.Skip) continue;

            if (TestNewRenderer)
            {
                TextRenderEngine.CacheEntries(_text, currentBlock);
                TextRenderEngine.RenderBlock(c, _text, currentBlock, _defaultAtlas.Font);
            }

            Color color = currentBlock.UseDefaultColor ? baseColor : currentBlock.Color;

            bool blockEffect = currentBlock.TextEffect != FontEffect.None;
            FontEffect effect = blockEffect ? currentBlock.TextEffect : defaultEffect;
            Color effectColor = blockEffect ? currentBlock.EffectColor : defaultEffectColor ?? Color.White;
            float effectAmount = blockEffect ? currentBlock.EffectParam : defaultEffectAmount;

            TextLayouter layouter = new TextLayouter(_defaultAtlas); // todo

            if (currentBlock.Newline)
            {
                layouter.NewLine();
                pen.X = 0;
            }

            if (currentBlock.CenterLayout == 1) // starting
            {
                float combinedBlockWidth = MeasureStringWidth(currentBlock.GetBlockString(_text));
                for (int ii = i + 1; ii < _textBlocks.Count; ii++)
                {
                    TextBlock otherBlock = _textBlocks[ii];
                    if (otherBlock.Skip) continue;
                    if (otherBlock.CenterLayout != 2) break;
                    if (otherBlock.Newline) break;

                    combinedBlockWidth += MeasureStringWidth(otherBlock.GetBlockString(_text));
                }

                // todo: calculate blocks positions outside of rendering
                // maybe also render params such as color?
                float textWidth = TextSize.X;
                float center = textWidth / 2f - combinedBlockWidth / 2f;
                pen.X = center;
            }

            c.RenderString(offset + pen, color, currentBlock.GetBlockString(_text).ToString(), _defaultAtlas, layouter, effect, effectAmount, effectColor);

            pen = pen + layouter.GetPenLocation().ToVec3();
        }

        RenderTest(c, offset);
    }

    private void ProcessTag(TagDefinition def, ref TextBlock block)
    {
        var tagName = def.GetTagName(_text);
        if (tagName.StartsWith("color")) // color r g b (?a) or color #htmlColor
        {
            LayoutTag_Color(def, ref block);
            return;
        }
        else if (tagName.StartsWith("outline")) // outline r g b (?a) size=s or outline #htmlColor size=s
        {
            LayoutTag_Outline(def, ref block);
            return;
        }
        else if (tagName.StartsWith("center"))
        {
            block.CenterLayout = (byte) (def.GetTagStartIndexInText() == block.StartIndex ? 1 : 2);
            return;
        }
    }

    private Color? TagReadingHelper_ColorArgs(TagDefinition tag)
    {
        ReadOnlySpan<char> tagName = tag.GetTagName(_text);
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

    private void LayoutTag_Color(TagDefinition tag, ref TextBlock block)
    {
        Color? col = TagReadingHelper_ColorArgs(tag);
        if (col != null)
        {
            block.Color = col.Value;
            block.UseDefaultColor = false;
        }
    }

    private void LayoutTag_Outline(TagDefinition tag, ref TextBlock block)
    {
        Color? col = TagReadingHelper_ColorArgs(tag);
        if (col != null)
        {
            block.EffectColor = col.Value;
            block.TextEffect = FontEffect.Outline;
        }
        else
        {
            return;
        }

        // todo: add helper function for getting args from tags
        const string sizeArgStr = "size=";
        ReadOnlySpan<char> tagName = tag.GetTagName(_text);
        int sizeSpec = tagName.IndexOf(sizeArgStr);
        if (sizeSpec != -1)
        {
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
            block.EffectParam = sizeNum;
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
            Vector2 pos = GetNextGlyphPosition(new Vector2(sizeSoFar, 0), c, out Vector2 _, out DrawableGlyph g);
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
        float lineSpacing = _defaultAtlas.FontHeight; // todo: height modes

        Vector2 sizeSoFar = new Vector2(0, 0);
        float largestLine = 0;
        float tallestOnLine = 0;
        for (int i = 0; i < _textBlocks.Count; i++)
        {
            TextBlock block = _textBlocks[i];

            // If jumping on a new line, check if the current line is the largest so far.
            if (block.Newline)
            {
                if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
                sizeSoFar.X = 0;
                sizeSoFar.Y += lineSpacing;
                tallestOnLine = 0;
            }

            ReadOnlySpan<char> blockString = block.GetBlockString(_text);
            for (int ci = 0; ci < blockString.Length; ci++)
            {
                char c = blockString[ci];

                Vector2 pos = GetNextGlyphPosition(sizeSoFar, c, out Vector2 _, out DrawableGlyph g);
                sizeSoFar = pos;
                if (g == null) continue;

                sizeSoFar.X += g.XAdvance;
                //float verticalSize = g.Height;// + (_defaultAtlas.Ascent - g.Height) + g.Descent;
                //if (verticalSize > tallestOnLine) tallestOnLine = verticalSize;
            }
        }

        sizeSoFar.Y += lineSpacing;

        if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
        if (largestLine != 0) sizeSoFar.X = largestLine;

        sizeSoFar.Y = MathF.Max(sizeSoFar.Y, _defaultAtlas.FontHeight);

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

    public enum ForEachResult
    {
        Continue,
        Break,
        BreakAndReturnStoredResult,
        StoreResultAndContinue
    }

    public delegate (ForEachResult, T) ForEachTextBlockDelegateFunc<T, A1>((TextBlock, int) blockData, (int, int, int) selData, A1 arg1);

    public T ForEachTextBlock<T, A1>(ForEachTextBlockDelegateFunc<T, A1> functor, A1 arg1)
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

    private struct TagDefinition
    {
        public int NameStartIdx;
        public int NameLength;
        public bool Skip;

        public TagDefinition(int nameStartIdx, int nameLength)
        {
            NameStartIdx = nameStartIdx;
            NameLength = nameLength;
            Skip = false;
        }

        public ReadOnlySpan<char> GetTagName(string totalText)
        {
            return totalText.AsSpan().Slice(NameStartIdx, NameLength);
        }

        public int GetTagStartIndexInText()
        {
            return NameStartIdx + NameLength + 1; // "name>" + 1 because of the bracket
        }
    }

    public struct TextBlock
    {
        public int StartIndex;
        public int Length;
        public bool Skip;

        public Color Color;
        public bool UseDefaultColor;

        public FontEffect TextEffect;
        public Color EffectColor;
        public int EffectParam;

        public bool Newline;
        public byte CenterLayout;

        public TextBlock(int startIndex)
        {
            StartIndex = startIndex;
            UseDefaultColor = true;
        }

        public TextBlock(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
            UseDefaultColor = true;
        }

        public ReadOnlySpan<char> GetBlockString(string totalText)
        {
            return totalText.AsSpan().Slice(StartIndex, Length);
        }
    }
}
