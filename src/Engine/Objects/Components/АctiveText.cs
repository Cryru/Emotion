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
        #region "Variables"
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
                if(text != "") ProcessText();
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
        #endregion
        //Private variables.
        #region "Private"
        private string text;
        private SpriteFont font;
        private List<string> textLines;
        private List<int> ts_spacingWord;
        private List<int> ts_spacingTab;
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
            if (attachedObject.HasComponent<ActiveTexture>())
            {
                attachedObject.Component<ActiveTexture>().Active = false;
            }
        }
        /// <summary>
        /// Is run every frame outside of an ink binding.
        /// </summary>
        public override void DrawFree()
        {
            if (attachedObject.HasComponent<ActiveTexture>())
            {
                attachedObject.Component<ActiveTexture>().BeginTargetDraw();
            }
            else
            {
                return;
            }

            //The position of the current letter, in the overall text as they are written in the effects list.
            int pointerPosition = 0;

            //The default values.
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

            Color color = Color.White;
            if (attachedObject.HasComponent<ActiveTexture>())
            {
                color = attachedObject.Component<ActiveTexture>().Tint;
            }

            Context.ink.Start();

            //Run through all lines.
            for (int l = 0; l < textLines.Count; l++)
            {
                //Run through all letters.
                for (int p = 0; p < textLines[l].Length; p++)
                {
                        //Get the data of the current character, if past the length create an empty dummy.
                        CharData current;
                        CharData emptyOP = new CharData("", color);
                        if (p < textLines[l].Length) current = new CharData(textLines[l][p].ToString(), color);
                        else current = new CharData("", color);

                        //check if any tags begin here
                        for (int tag = 0; tag < textTags.Count; tag++)
                        {
                            if (textTags[tag] == null) break;

                            if (textTags[tag].Start == pointerPosition)
                            {
                                textTags[tag].Active = true;
                            }
                        }

                        //apply effect stack
                        Tag[] activeTags = textTags.Where(x => x != null).Where(x => x.Active == true).ToArray();
                        for (int effect = 0; effect < activeTags.Length; effect++)
                        {
                            activeTags[effect].onDuration(current);
                        }

                        //check if any tags end here
                        for (int tag = 0; tag < textTags.Count; tag++)
                        {
                            if (textTags[tag] == null) break;

                            if (textTags[tag].Start == pointerPosition)
                            {
                                textTags[tag].onStart(textTags[tag].Empty ? emptyOP : current);
                            }
                            if (textTags[tag].End == pointerPosition)
                            {
                                textTags[tag].Active = false;
                                textTags[tag].onEnd(textTags[tag].Empty ? emptyOP : current);
                            }
                        }


                    //Add tab space.
                    if (p == 0)
                    {
                        Xoffset += ts_spacingTab[l];
                    }

                    //If the first letter on the line is a space then we don't draw it.
                    if (!(p == 0 && (emptyOP.Content + current.Content) == " "))
                    {
                        //Draw the letter.
                        Context.ink.DrawString(Font, emptyOP.Content + current.Content, new Vector2(X + Xoffset, Y + Yoffset), current.Color);

                        //Add to the Xoffset, if justification then add the line's offset to the offset too.
                        if ((emptyOP.Content + current.Content) == " ")
                            Xoffset += (int)Font.MeasureString(emptyOP.Content + current.Content.ToString()).X + ts_spacingWord[l];
                        else
                            Xoffset += (int)Font.MeasureString(emptyOP.Content + current.Content.ToString()).X;
                    }

                    //Increment the pointer position.
                    pointerPosition++;
                }
                //Reset offsets.
                Xoffset = 0;
                Yoffset += (int)Font.MeasureString(" ").Y;
            }
            Context.ink.End();
            attachedObject.Component<ActiveTexture>().EndTargetDraw();
        }
        #endregion
        //Private functions.
        #region "Internal Functions"
        private void ProcessRenderData()
        {
            //Determine the width of the object.
            float Width = Settings.WWidth;
            if (attachedObject.HasComponent<Transform>())
            {
                Width = attachedObject.Component<Transform>().Width;
            }

            //Determine how to wrap the text to fit the bounds.
            textLines = determineBounds(Width);
            TextStyleCalculate(Width);
        }
        #region "Process Render Data"
        /// <summary>
        /// 
        /// </summary>
        private List<string> determineBounds(float Width)
        {
            List<string> textLines = new List<string>();

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
                    int nextSpace = processedString.IndexOf(' ', i + 1) - i;
                    //Check if the next location of the next space is not too far away.
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

            return textLines;
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
        private void TextStyleCalculate(float Width)
        {
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
                        if(depth == 0)
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
                if(endTag != null)
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
