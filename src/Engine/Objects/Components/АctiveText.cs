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
    public class ActiveText : Component
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
                //Check if the text is changed before applying processing.
                if(value != text)
                {
                    text = value;
                    if (text == "") processedString = ""; else ProcessText();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public SpriteFont Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
                if (text != "") ProcessText();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public TextStyle Style;
        /// <summary>
        /// The default text color.
        /// </summary>
        public Color Color = Color.White;
        /// <summary>
        /// Scale the transform component (if any) according to the text's size.
        /// </summary>
        public bool AutoSize = false;
        /// <summary>
        /// The width of the text.
        /// </summary>
        public float Width
        {
            get
            {
                if (attachedObject.HasComponent<Transform>() && !AutoSize)
                {
                    return attachedObject.Component<Transform>().Width;
                }
                else
                {
                    return width;
                }
            }
        }
        /// <summary>
        /// The height of the text.
        /// </summary>
        public float Height
        {
            get
            {
                if (attachedObject.HasComponent<Transform>() && !AutoSize)
                {
                    return attachedObject.Component<Transform>().Height;
                }
                else
                {
                    return height;
                }
            }
        }
        #endregion
        //Private variables.
        #region "Private"
        private string text;
        private SpriteFont font;
        private List<string> textLines = new List<string>();
        private List<int> ts_spacingWord;
        private List<int> ts_spacingTab;
        private float width;
        private float height;
        #endregion
        #region "Processed Text Data"
        /// <summary>
        /// 
        /// </summary>
        private List<Tag> textTags = new List<Tag>();
        /// <summary>
        /// 
        /// </summary>
        public string ProcessedString
        {
            get
            {
                return processedString;
            }
        }
        private string processedString;
        /// <summary>
        /// The text composed to a texture.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                return texture as Texture2D;
            }
        }
        private RenderTarget2D texture;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        public ActiveText()
        {
            Text = "";
            Font = AssetManager.DefaultFont;
            Style = TextStyle.Left;
        }
        /// <summary>
        /// 
        /// </summary>
        public ActiveText(string Text)
        {
            this.Text = Text;
            Font = AssetManager.DefaultFont;
            Style = TextStyle.Left;
        }
        /// <summary>
        /// 
        /// </summary>
        public ActiveText(string Text, SpriteFont Font)
        {
            this.Text = Text;
            this.Font = Font;
            Style = TextStyle.Left;
        }
        /// <summary>
        /// 
        /// </summary>
        public ActiveText(string Text, SpriteFont Font, TextStyle Style)
        {
            this.Text = Text;
            this.Font = Font;
            this.Style = Style;
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// Is run every tick.
        /// </summary>
        public override void Update()
        {
            ProcessRenderData();
        }
        /// <summary>
        /// Is run every frame outside of an ink binding.
        /// </summary>
        public override void Compose()
        {
            //The position of the current letter in the processed tag free text.
            int pointerPosition = 0;

            //Get values for size and position.
            float X = 0;
            float Y = 0;
            if (attachedObject.HasComponent<Transform>())
            {
                X = attachedObject.Component<Transform>().X;
                Y = attachedObject.Component<Transform>().Y;
            }

            //The offsets of the current letter.
            int Xoffset = 0;
            int Yoffset = 0;

            //Start composing on the render target.
            Context.ink.StartRenderTarget(ref texture, (int)Width, (int)Height);

            //Run through all lines.
            for (int l = 0; l < textLines.Count; l++)
            {
                //Run through all letters.
                for (int p = 0; p < textLines[l].Length; p++)
                {
                    //Define data for the current character.
                    CharData current = new CharData(Color);
                    string currentChar = textLines[l][p].ToString();

                    //Check if any tags start here and set them to active, skip empty tags.
                    for (int tag = 0; tag < textTags.Count; tag++)
                    {
                        if (textTags[tag] == null || textTags[tag].Empty) continue;

                        if (textTags[tag].Start == pointerPosition)
                        {
                            textTags[tag].Active = true;
                        }
                    }

                    //Get active tags and apply their duration effect on the character data.
                    Tag[] activeTags = textTags.Where(x => x != null).Where(x => x.Active == true).ToArray();
                    for (int effect = 0; effect < activeTags.Length; effect++)
                    {
                        activeTags[effect].onDuration(current);
                    }

                    //Check if any tags end here, we do this after the duration effect has been applied and then overwrite.
                    for (int tag = 0; tag < textTags.Count; tag++)
                    {
                        if(textTags[tag] == null || textTags[tag].Empty) continue;

                        if(textTags[tag].Start == pointerPosition)
                        {
                            textTags[tag].onStart(current);
                        }

                        if (textTags[tag].End == pointerPosition && textTags[tag].Active == true)
                        {
                            textTags[tag].Active = false;
                            textTags[tag].onEnd(current);
                        }
                    }

                    //Add tab space offset if on the first character on a line.
                    if (p == 0)
                    {
                        Xoffset += ts_spacingTab[l];
                    }

                    //If the first letter on the line is a space then we don't draw it.
                    if (!(p == 0 && (currentChar) == " "))
                    {
                        //Draw the letter with the collected character data.
                        Context.ink.DrawString(Font, currentChar, new Vector2(X + Xoffset, Y + Yoffset), current.Color);

                        //If the current character is a space add word spacing offset.
                        if (currentChar == " ")
                            Xoffset += (int)Font.MeasureString(currentChar).X + ts_spacingWord[l];
                        else
                            Xoffset += (int)Font.MeasureString(currentChar).X;
                    }

                    //Increment the pointer position.
                    pointerPosition++;
                }

                //Starting a new line, reset X offset and increment Y offset.
                Xoffset = 0;
                Yoffset += (int)Font.MeasureString(" ").Y;
            }

            //Stop composing.
            Context.ink.EndRenderTarget();
        }
        #endregion

        #region "Internal Functions"
        private void ProcessRenderData()
        {
            //Clear the text lines.
            textLines.Clear();

            //Determine how to wrap the text to fit the bounds.
            determineBounds();

            //Check if auto sizing.
            if(AutoSize) SetAutoSize();

            //Determine word and tab spacing to display the specified style.
            TextStyleCalculate();
        }
        /// <summary>
        /// Sets the width to the longest line and the height to the number of lines.
        /// </summary>
        private void SetAutoSize()
        {
            //The biggest line.
            int MaxWidth = 0;
            for (int i = 0; i < textLines.Count; i++)
            {
                //Find the width of the current line.
                int CurWidth = (int) stringWidth(textLines[i]);

                //Check if exceeding max.
                if (CurWidth > MaxWidth)
                {
                    MaxWidth = CurWidth;
                }
            }
            width = MaxWidth;

            height = (int)Font.MeasureString(" ").Y * textLines.Count();
        }
        #region "Process Render Data"
        /// <summary>
        /// Splits the text so it fits within its bounds.
        /// </summary>
        private void determineBounds()
        {
            //Check if there is any text to process.
            if (processedString == null || processedString == "") return;

            //Check if auto sizing.
            float Width = this.Width;
            if (AutoSize)
            {
                Width = int.MaxValue;
            }

            //Get all characters.
            List<char> characters = processedString.ToCharArray().ToList();

            //The current line as a string.
            string lineString = "";

            //Loop through all characters.
            for (int i = 0; i < characters.Count; i++)
            {

                //Check if we are at the last character.
                if (i == characters.Count - 1)
                {
                    textLines.Add(lineString + characters[i].ToString());
                    lineString = "";
                    continue;
                }

                //Check if a character is a space.
                if (characters[i] == ' ')
                {
                    //Get the space left on the current line.
                    float spaceOnLine = Width - stringWidth(lineString);
                    //Find the location of the next space.
                    int nextSpace = Math.Min(processedString.IndexOf(' ', i + 1) - i, processedString.IndexOf('\n', i + 1) - i);
                    //Check if there is actually a next space.
                    if (nextSpace > 0)
                    {
                        //Get the text to the next space.
                        string textToNextSpace = string.Join("", characters.GetRange(i + 1, nextSpace));
                        //If the text fits on the current line then go on a new line.
                        if (spaceOnLine <= stringWidth(textToNextSpace))
                        {
                            textLines.Add(lineString + "");
                            lineString = " ";
                            continue;
                        }
                    }
                }

                //If the current character is a new line character go on a new line.
                if (characters[i] == '\n')
                {
                    textLines.Add(lineString);
                    lineString = "";
                    continue;
                }

                //Check if there is still space on the current line.
                if (stringWidth(lineString) + stringWidth(characters[i].ToString()) <= Width)
                {
                    lineString += characters[i].ToString();
                    continue;
                }
                else
                {
                    //If not enough space then go on the next line.
                    textLines.Add(lineString);
                    lineString = "";
                    i--;
                    continue;
                }
            }
        }
        /// <summary>
        /// Returns the width of a string.
        /// </summary>
        /// <param name="line">The string to measure.</param>
        /// <returns>The width of the input string.</returns>
        private float stringWidth(string text)
        {
            return Font.MeasureString(text).X;
        }
        /// <summary>
        /// Calculates the style offsets for the selected text style.
        /// </summary>
        private void TextStyleCalculate()
        {
            //Check if there is any text to process.
            if (processedString == null || processedString == "") return;

            //Clears offsets for the last frame.
            ts_spacingWord = new List<int>();
            ts_spacingTab = new List<int>();

            //Generate offsets depending on the next frame.
            switch (Style)
            {
                case TextStyle.Center: //In this mode we center the text by subtracting the line's width from the total width and dividing it by two.

                    //Go through all lines.
                    for (int l = 0; l < textLines.Count; l++)
                    {
                        string currentLine = textLines[l];
                        //If the first character of a line is a space we don't count it.
                        if (textLines[l].ToCharArray().First() == ' ')
                        {
                            currentLine = textLines[l].Substring(1);
                        }
                        //Add the offset.
                        ts_spacingWord.Add(0);
                        ts_spacingTab.Add((int)(Width - Font.MeasureString(currentLine).X) / 2);
                    }

                    break;
                case TextStyle.Right: //In this mode we center the text by subtracting the line's width from the total width and dividing it by two.

                    //Go through all lines.
                    for (int l = 0; l < textLines.Count; l++)
                    {
                        string currentLine = textLines[l];
                        //If the first character of a line is a space we don't count it.
                        if (textLines[l].ToCharArray().First() == ' ')
                        {
                            currentLine = textLines[l].Substring(1);
                        }
                        //Add the offset.
                        ts_spacingWord.Add(0);
                        ts_spacingTab.Add((int)(Width - Font.MeasureString(currentLine).X));
                    }

                    break;
                case TextStyle.Justified: //In this mode text is stretched to fill the current line as much as possible.
                case TextStyle.JustifiedCenter:

                    List<string> processedTextcleand = new List<string>();

                    //Clean text with starting space.
                    for (int i = 0; i < textLines.Count; i++)
                    {
                        if (textLines[i].Length > 0 && textLines[i][0] == ' ')
                        {
                            processedTextcleand.Add(textLines[i].Substring(1));
                        }
                        else
                        {
                            processedTextcleand.Add(textLines[i]);
                        }
                    }

                    //Calculate the width of all lines.
                    List<float> lineWidth = new List<float>();

                    for (int i = 0; i < processedTextcleand.Count; i++)
                    {
                        lineWidth.Add(Font.MeasureString(processedTextcleand[i]).X);
                    }

                    //Find the longest line.
                    float temp_width = 0;
                    for (int l = 0; l < lineWidth.Count; l++)
                    {
                        if (lineWidth[l] > temp_width)
                        {
                            temp_width = lineWidth[l];
                        }
                    }

                    //Go through all lines and make their offsets big enough to approximate this line.
                    for (int l = 0; l < processedTextcleand.Count; l++)
                    {
                        //Check if the current line is the longest one.
                        if (Font.MeasureString(processedTextcleand[l]).X == temp_width)
                        {
                            ts_spacingWord.Add(0);
                            continue;
                        }
                        //Check for very short lines, or the last line, which should not be justified.
                        if (Font.MeasureString(processedTextcleand[l]).X < temp_width / 3 || l == processedTextcleand.Count - 1)
                        {
                            ts_spacingWord.Add(0);
                            continue;
                        }

                        //Else start incrementing.
                        int temp_offset = 0;
                        while (Font.MeasureString(processedTextcleand[l]).X + (temp_offset * processedTextcleand[l].Count(x => x == ' ')) <= temp_width)
                        {
                            temp_offset++;

                            //Endless loop escape.
                            if (temp_offset > 666)
                            {
                                temp_offset = 0;
                                break;
                            }
                        }
                        if (temp_offset != 0)
                            ts_spacingWord.Add(temp_offset - 1);
                        else
                            ts_spacingWord.Add(temp_offset);
                    }

                    if (Style == TextStyle.JustifiedCenter)
                    {
                        //Center justified text.
                        for (int l = 0; l < processedTextcleand.Count; l++)
                        {
                            float centeringoffet = Width - (Font.MeasureString(processedTextcleand[l]).X + (ts_spacingWord[l] * processedTextcleand[l].Count(x => x == ' ')));

                            //We want to center the first line, while placing the others below it.
                            if (l > 0) ts_spacingTab.Add(ts_spacingTab[l - 1]); else ts_spacingTab.Add((int)centeringoffet / 2);
                        }
                    }
                    else
                    {
                        for (int l = 0; l < processedTextcleand.Count; l++)
                        {
                            ts_spacingTab.Add(0);
                        }
                    }

                    break;

                default:
                    //If invalid or non implemented just add an empty array.
                    for (int l = 0; l < textLines.Count; l++)
                    {
                        ts_spacingTab.Add(0);
                        ts_spacingWord.Add(0);
                    }
                    break;
            }
        }
        #endregion
        private void ProcessText()
        {
            //Reset the tag array.
            textTags.Clear();
            //Clean the text from tags and record some info while doing it.
            List<TagData> tempTagData = new List<TagData>();
            processedString = CleanTags(tempTagData);
            //Process the captured info.
            ProcessTags(tempTagData);
        }
        #region "Process Text Data"
        /// <summary>
        /// Returns a clean string without any tags in it.
        /// </summary>
        /// <param name="tempTagData">Tag data recorded while cleaning.</param>
        /// <returns>A clean string without any tags in it.</returns>
        private string CleanTags(List<TagData> tempTagData)
        {
            //The text being built without tags.
            string tagFreeText = "";
            int charactersFromPreviousTag = 0;

            //Loop through the text looking for tag opening statements.
            for (int pointer = 0; pointer < Text.Length; pointer++)
            {
                //Get the current and previous character.
                char curChar = Text[pointer];
                char prevChar = Text[Math.Max(0, pointer - 1)];

                //Check if opening a tag and the character is not being escaped.
                if (curChar == '<' && prevChar != '\\')
                {
                    //Declare a variable to declare the captured tag information.
                    string tagInfo;

                    //Read the discovered tag and move the point to its end so it doesn't get recorded to the tag free text.
                    pointer = FindTagEnd(pointer, out tagInfo);
                    //Record information that was captured while cleaning.
                    tempTagData.Add(new TagData(tagInfo, tagFreeText.Length, charactersFromPreviousTag));
                    charactersFromPreviousTag = 0;
                    //Continue to the next character, which should be after the tag's closing character.
                    continue;
                }

                //If the current character is an escape symbol that is not being escaped itself do not write it, otherwise write it.
                if (!(curChar == '\\' && prevChar != '\\')) { tagFreeText += curChar; charactersFromPreviousTag++; }
            }

            return tagFreeText;
        }
        /// <summary>
        /// Returns the position of the tag information's end.
        /// </summary>
        /// <param name="tagOpenPosition">The position where the tag was opened.</param>
        /// <param name="tagInfo">The information inside the tag.</param>
        /// <returns>The position where the tag information ends.</returns>
        private int FindTagEnd(int tagOpenPosition, out string tagInfo)
        {
            //Prepare to record tag information.
            tagInfo = "";

            //Search from this position, plus one so we don't capture the opening character, until the end of the text for the tag closing symbol - '>'.
            for (int pointer = tagOpenPosition + 1; pointer < Text.Length; pointer++)
            {
                //Get the current and previous characters.
                char curChar = Text[pointer];
                char prevChar = Text[Math.Max(0, pointer - 1)];

                //Check if closing a tag and the character is not being escaped.
                if (curChar == '>' && prevChar != '\\')
                {
                    //Found it!
                    return pointer;
                }
                else
                {
                    //If not closing then continue recording info.
                    tagInfo += curChar;
                }
            }

            /*
             * If no closing character is found then the tag information spans the rest of the text.
             * This is not intended behaviour, but is the logical one, and is the default error handling here.
            */
            return Text.Length - 1;
        }
        /// <summary>
        /// Processed captured tags during the cleaning session.
        /// </summary>
        /// <param name="capturedData">The tags captured while cleaning.</param>
        private void ProcessTags(List<TagData> capturedData)
        {
            foreach (var tagData in capturedData)
            {
                //Check if skipping this tag.
                if (tagData.Skip == true) continue;

                //Check if the tag information collected contains data in addition to the identifier and separate them if so.
                string identifier = tagData.Information.Contains("=") ? tagData.Information.Split('=')[0] : tagData.Information;
                string data = tagData.Information.Contains("=") ? tagData.Information.Split('=')[1] : "";

                //Check if an ending tag, with no preceding opening tag.
                if (identifier == "/") continue;

                //Try to find this tag's ending tag pair.
                TagData endTag = null;

                /* 
                 * Depth is used to detect and prevent taking another tag's ending pair since we aren't looking for the
                 * first closing tag, but rather the right one.
                 */
                int depth = 0;
                for (int i = capturedData.IndexOf(tagData) + 1; i < capturedData.Count; i++)
                {
                    if (capturedData[i].Information == "/")
                    {
                        if (depth == 0)
                        {
                            endTag = capturedData[i];
                            break;
                        }
                        else
                        {
                            depth--;
                        }
                    }
                    else
                    {
                        depth++;
                    }
                }

                //Check if any is found.
                int endLocation = -1;
                bool empty = false;
                if (endTag != null)
                {
                    //Delete the ending tag the captured data.
                    endTag.Skip = true;
                    empty = endTag.CharactersFromPreviousTag == 0;
                    /*
                     * Assign the ending point to the last collected tag. We subtract one from the actual position because
                     * otherwise we are grabbing the character after the tag into the ending.
                     */
                    endLocation = Math.Max(endTag.StartPosition - 1, 0);
                    endLocation = endLocation < tagData.StartPosition ? tagData.StartPosition : endLocation;
                }

                //All tag data is ready, add it.
                textTags.Add(TagFactory.Build(identifier, data, tagData.StartPosition, endLocation, empty));
            }
        }
        #endregion
        #endregion

        //Other
        #region "Component Interface"
        public override void Draw() { }
        #endregion
    }
}
