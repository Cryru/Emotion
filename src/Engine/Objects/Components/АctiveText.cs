using Microsoft.Xna.Framework;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Events;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Enums;
using SoulEngine.Objects.Components.Helpers;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Location, Size, and Rotation.
    /// </summary>
    public class ActiveText : DrawComponent
    {
        #region "Declarations"
        //Main variables.
        #region "Primary"
        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                GenerateCache();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public SpriteFont Font;
        /// <summary>
        /// 
        /// </summary>
        public TextStyle Style;
        /// <summary>
        /// The default text color.
        /// </summary>
        public Color Color = Color.White;
        /// <summary>
        /// The scrolling of the text.
        /// </summary>
        public Vector2 Scroll = new Vector2(0, 0);
        #region "Bounds"
        /// <summary>
        /// The width of the text, based on the transform component's width if attached, and the screen's width if not.
        /// </summary>
        public int Width
        {
            get
            {
                if (AutoWidth)
                {
                    if (width != 0) return width; else return Settings.Width;
                }
                else return attachedObject.Width;
            }
        }
        /// <summary>
        /// The height of the text.
        /// </summary>
        public int Height
        {
            get
            {
                if (AutoHeight)
                {
                    if (height != 0) return height; else return Settings.Height;
                }
                else return attachedObject.Height;
            }
        }
        /// <summary>
        /// If set to true the height of the text texture will be according to the text inside rather than the transform.
        /// </summary>
        public bool AutoHeight = false;
        /// <summary>
        /// If set to true the width of the text texture will be according to the text inside rather than the transform.
        /// </summary>
        public bool AutoWidth = false;
        /// <summary>
        /// The character to draw up to. Used for scrolling and cutting off text without messing with formatting.
        /// </summary>
        public int DrawLimit
        {
            get
            {
                if (drawlimit == -1)
                    drawlimit = Text.Length;

                return drawlimit;
            }
            set
            {
                if (value == -1)
                    drawlimit = Text.Length;
                else
                    drawlimit = value;
            }
        }
        /// <summary>
        /// The height of the text.
        /// </summary>
        public int TextHeight
        {
            get
            {
                return height;
            }
        }
        /// <summary>
        /// The width of the text.
        /// </summary>
        public int TextWidth
        {
            get
            {
                return width;
            }
        }
        #endregion
        #endregion
        //Private variables.
        #region "Private"
        private int width = 0;
        private int height = 0;
        private int drawlimit = -1;
        private List<TextLine> linesCache;
        private string text = "";
        #endregion
        #region "Processed Text Data"
        private RenderTarget2D texture;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        public override Component Initialize()
        {
            text = "";
            Font = AssetManager.DefaultFont;
            Style = TextStyle.Left;
            Priority = 1;

            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        public Component Initialize(string Text)
        {
            text = Text;
            Font = AssetManager.DefaultFont;
            Style = TextStyle.Left;
            Priority = 1;

            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Font"></param>
        public Component Initialize(string Text, SpriteFont Font)
        {
            text = Text;
            this.Font = Font;
            Style = TextStyle.Left;
            Priority = 1;

            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Font"></param>
        /// <param name="Style"></param>
        public Component Initialize(string Text, SpriteFont Font, TextStyle Style)
        {
            text = Text;
            this.Font = Font;
            this.Style = Style;
            Priority = 1;

            return this;
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// Composes the text texture based on cached data.
        /// </summary>
        public override void Compose()
        {
            //Check if no cache.
            if (linesCache == null) return;

            //Start composing on the render target.
            Context.ink.StartRenderTarget(ref texture, Width, Height);

            //Apply scrolling.
            float offsetX = Scroll.X;
            float offsetY = Scroll.Y;

            int currentChar = 0;
            float firstLineJustifiedCenterOffset = 0;

            //Each line.
            for (int y = 0; y < linesCache.Count; y++)
            {
                //Determine beginning X offset based on the selected style.
                int wordSpacing = 0;
                int spaceLeftOnLine = Width - stringWidth(linesCache[y].ToString());
                //---------------------------------------------------------------------------------------
                switch (Style)
                {
                    case TextStyle.Right:
                        offsetX = linesCache[y].SpaceLeft;
                        break;
                    case TextStyle.Center:
                        offsetX = linesCache[y].SpaceLeft / 2;
                        break;
                    case TextStyle.JustifiedCenter:
                    case TextStyle.Justified:
                        float spaces = linesCache[y].Chars.Select(x => x.Content).Count(x => x == " ");
                        float currentBoost = wordSpacing * spaces;
                        float nextBoost = (wordSpacing + 1) * spaces;
                        //If manually going to a new line then don't apply justification.
                        if (!linesCache[y].Manual && y != linesCache.Count - 1)
                        {
                            if (spaces != 0)
                            {
                                /*
                                 * Check if there is space on the current line when we apply word spacing,
                                 * if there will be enough space after we add one to the word spacing.
                                */
                                while (linesCache[y].SpaceLeft - currentBoost >= 0 && linesCache[y].SpaceLeft - nextBoost >= 0)
                                {
                                    wordSpacing += 1;
                                    currentBoost = wordSpacing * spaces;
                                    nextBoost = (wordSpacing + 1) * spaces;
                                }
                            }
                        }

                        //If this is the first line set the first line justification.
                        if (y == 0) firstLineJustifiedCenterOffset = (linesCache[0].SpaceLeft - currentBoost) / 2;
                        if (Style == TextStyle.JustifiedCenter) offsetX = firstLineJustifiedCenterOffset; else offsetX = Scroll.X;
                        break;
                    default:
                        offsetX = Scroll.X;
                        break;
                }
                //---------------------------------------------------------------------------------------

                //Each character.
                for (int x = 0; x < linesCache[y].Chars.Count; x++)
                {
                    //Check if trying to draw past limit.
                    if (currentChar == drawlimit) break;

                    //Each character effect.
                    for (int t = 0; t < linesCache[y].Chars[x].Tags.Count; t++)
                    {
                        linesCache[y].Chars[x].Tags[t].Effect(linesCache[y].Chars[x], new DrawData(offsetX, offsetY));
                    }
                    
                    //Draw the character.
                    Context.ink.DrawString(Font, linesCache[y].Chars[x].Content, new Vector2(offsetX, offsetY), linesCache[y].Chars[x].Color * 1f);

                    //Increment character counter and add its width to the offset.
                    currentChar++;
                    offsetX += stringWidth(linesCache[y].Chars[x].Content);

                    if (linesCache[y].Chars[x].Content == " ") offsetX += wordSpacing;
                }

                //Move the Y offset to draw on a new line.
                offsetY += stringHeight();
            }

            //Stop composing.
            Context.ink.EndRenderTarget();

            //Assign the rendertarget to the draw texture.
            Texture = texture;
        }
        /// <summary>
        /// Caches data.
        /// </summary>
        public void GenerateCache()
        {
            //Reset cache.
            linesCache = new List<TextLine>();

            //The space left on the current line.
            int spaceOnLine = AutoWidth ? Settings.Width : Width;

            //The current line.
            TextLine currentLine = new TextLine();

            //The tags in effect.
            List<Tag> tagStack = new List<Tag>();

            //Read through the text.
            for (int i = 0; i < Text.Length; i++)
            {
                //Get the current character.
                CharData current = new CharData(Text[i].ToString(), Color, Font);

                //Check if opening a tag.
                if (current.Content == "<")
                {
                    //Read the tag info.
                    string tagInfo;
                    i = ReadTag(i, out tagInfo);
                    //Process the collected tag.
                    ProcessTagStack(tagStack, tagInfo);
                    continue;
                }

                //Apply effects.
                for (int e = 0; e < tagStack.Count; e++)
                {
                    current.Tags.Add(tagStack[e]);
                }

                //Define a trigger for forcing a new line.
                bool newLine = false;

                //If the current character is space and it isn't the last character...
                if (current.Content == " " && i != Text.Length - 1)
                {
                    //Get the text between this space and the next.
                    string textBetweenCurrentCharAndNextSpace = "";
                    int locationOfNextSpace = Text.IndexOf(' ', i + 1);
                    if (locationOfNextSpace != -1)
                        textBetweenCurrentCharAndNextSpace = Text.Substring(i + 1, locationOfNextSpace - i - 1);
                    else
                        textBetweenCurrentCharAndNextSpace = Text.Substring(i + 1);

                    //Check if manual new line symbol is present between this and the next space.
                    if (textBetweenCurrentCharAndNextSpace.IndexOf('\n') != -1)
                    {
                        textBetweenCurrentCharAndNextSpace = textBetweenCurrentCharAndNextSpace.Substring(0, textBetweenCurrentCharAndNextSpace.IndexOf('\n'));
                    }


                    //Check if there is no space on the line for the next word, in which case force a new line.
                    if (spaceOnLine - stringWidth(" " + textBetweenCurrentCharAndNextSpace) <= 0)
                    {
                        newLine = true;
                    }
                }

                //If the character is not a space and there is not enough space on the next line or it's a new line character, set offsets to new line.
                if ((current.Content != " " && spaceOnLine - stringWidth(current.Content) <= 0) || current.Content == "\n" || newLine)
                {
                    //NEW LINE
                    currentLine.Manual = current.Content == "\n";
                    currentLine.SpaceLeft = spaceOnLine;
                    linesCache.Add(currentLine);
                    spaceOnLine = AutoWidth ? Settings.Width : Width;
                    currentLine = new TextLine();
                }

                //Update the offset if not going to a new line directed from a space to prevent trailing spaces.
                if (!newLine)
                {
                    //Add the character to the current line.
                    currentLine.Chars.Add(current);
                    spaceOnLine = AutoWidth ? Settings.Width - stringWidth(currentLine.ToString()) : Width - stringWidth(currentLine.ToString());
                }
            }

            //Check if any characters are left to be rendered.
            if (currentLine.Chars.Count > 0)
            {
                currentLine.SpaceLeft = spaceOnLine;
                linesCache.Add(currentLine);
            }

            //Determine text sizes from cached data.
            if (linesCache.Count > 0)
            {
                height = linesCache.Count * stringHeight();
                width = stringWidth(linesCache.OrderBy(x => x.SpaceLeft).First().ToString());
            }
        }
        public override void Draw()
        {
            base.Draw(Width, Height);
        }
        public override void Update()
        {

        }
        #endregion

        #region "Scroll Functions"
        /// <summary>
        /// Scrolls the text to the bottom.
        /// </summary>
        public void ScrollBottom()
        {
            Scroll = new Vector2(Scroll.X, Height - TextHeight);
        }
        /// <summary>
        /// Scrolls the text one line up.
        /// </summary>
        public void ScrollLineUp()
        {
            Scroll.Y += stringHeight();
        }
        /// <summary>
        /// Scrolls the text one line down.
        /// </summary>
        public void ScrollLineDown()
        {
           Scroll.Y -= stringHeight();
        }
        #endregion

        #region "Internal Functions"
        /// <summary>
        /// Returns the width of a string.
        /// </summary>
        /// <param name="line">The string to measure.</param>
        /// <returns>The width of the input string.</returns>
        private int stringWidth(string text)
        {
            return (int) Math.Ceiling(Font.MeasureString(text).X);
        }

        private int stringHeight(string text = " ")
        {
            return (int) Math.Ceiling(Font.MeasureString(text).Y);
        }

        /// <summary>
        /// Read the tag.
        /// </summary>
        /// <param name="Position">The position of the tag opening character.</param>
        /// <param name="TagInformation">The collected tag's information.</param>
        /// <returns>The position of the tag closing character</returns>
        private int ReadTag(int Position, out string TagInformation)
        {
            TagInformation = "";

            //Read through the text from the position of the opening character.
            for (int i = Position + 1; i < Text.Length; i++)
            {
                //If the character is the closing tag return the position to continue reading after it. 
                //It's incremented by one by the 'continue' statement.
                if (Text[i] == '>') return i;

                //Add the character to the tag's information.
                TagInformation += Text[i];
            }

            //If a closing character is not found then everything up to the text's ending is considered part of the tag.
            return Text.Length - 1;
        }

        /// <summary>
        /// Updates the tag stack with new tag information.
        /// </summary>
        /// <param name="TagStack">The collected tags,</param>
        /// <param name="TagInformation">The tag information to process. Shouldn't include the opening and closing characters.</param>
        private void ProcessTagStack(List<Tag> TagStack, string TagInformation)
        {
            //Check if the tag information collected contains data in addition to the identifier and separate them if so.
            string identifier = TagInformation.Contains("=") ? TagInformation.Split('=')[0] : TagInformation;
            string data = TagInformation.Contains("=") ? TagInformation.Split('=')[1] : "";

            //Check if ending tag, if yes remove the last tag.
            if (identifier == "/")
            {
                if (TagStack.Count > 0) TagStack.RemoveAt(TagStack.Count - 1);
            }
            else
            {
                Tag temp = TagFactory.Build(identifier.ToLower(), data);
                if(temp != null) TagStack.Add(temp);
            }
                
        }
        #endregion
    }
}
