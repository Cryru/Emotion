using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An object used to render text.
    /// </summary>
    class TextObject : ObjectBase
    {
        #region "Declarations"
        //Text Properties.
        public string Text = ""; //The text of the object.
        private List<string> processedText = new List<string>(); //The text that has been processed and offloaded to the lists.
        private List<string> procEffects = new List<string>(); //The effects for the processed symbols.
        public SpriteFont Font = null; //A list of fonts to use, can be switched using a tag.

        //Effects.
        public bool Outline = false; //Whether to outline the text.
        public Color outlineColor = Color.Black; //The color of the outline.
        public int outlineSize = 1; //The size of the outline.

        //Render Modes
        public enum RenderMode
        {
            Left,
            Center,
            Right,
            Justified
        }
        public RenderMode TextStyle = RenderMode.Left; //The way to render the text.
        private List<int> ts_spaceX = new List<int>(); //The offset for space between letters. Each line corresponds to a line within the processedtext.

        //Background
        public bool Background = false;
        public Texture backgroundImage;
        public Color backgroundColor = Color.White;
        public float backgroundOpacity = 1f;
        private ObjectBase bgObject;
        public int backgroundMargin = 5;

        //Other.
        public bool noEffects = false; //Whether to skip rendering effect tags.
        public bool TagRemoval = true; //Whether to skip removing the tags. This does nothing if noEffects is false.
        public bool autoSizeX = false; //Whether to automatically set the width of the object to the text's size.
        public bool autoSizeY = false; //Whether to automatically set the height of the object based on the text's size.
        #endregion

        //Initializer
        public TextObject(SpriteFont font = null, string _Text = "Lorem Ipsum", int Width = 100, int Height = 100)
        {
            //Check if a font has been assigned.
            if(font == null)
            {
                Font = Core.fontDebug;
            }
            else
            {
                Font = font;
            }
            //Assign the text.
            Text = _Text;

            //Default Width/Height
            this.Width = Width;
            this.Height = Height;
        }
        public void SetupBackground(Texture image, Color Color, float Opacity)
        {
            Background = true;
            bgObject = new ObjectBase(image);
            bgObject.Color = Color;
            bgObject.Opacity = Opacity;
        }

        //Is called every frame to draw the object.
        public override void Draw()
        {
            //Process the text.
            Process();
            Render();
        }
        public void FixNewLine(ref string txt)
        {
            //Fix new line endings.
            txt = txt.Replace("\\r", "\r");
            txt = txt.Replace("\\n", "\n");
            txt = txt.Replace("\\R", "\r");
            txt = txt.Replace("\\N", "\n");
            txt = txt.Replace("\r\n", "\n");
            txt = txt.Replace("\r", "\r\n");
            txt = txt.Replace("\r\n", "<- !<NEWLINE>! ->");
            txt = txt.Replace("<- !<NEWLINE>! ->", "\n");
        }
        public void Process()
        {
            //Clear old variables.
            procEffects.Clear();
            processedText.Clear();

            string stringInProcess = "";

            //Find the tags.
            stringInProcess = FindTags();
            if (noEffects)
            {
                if(TagRemoval == false) stringInProcess = Text;
                procEffects.Clear();
            }

            //Fix new line in text.
            FixNewLine(ref stringInProcess);

            //Calculate the lines and sort them in the processedText list. This also includes part of the auto height and width code.
            CalculateLines(stringInProcess);

            //Fit the width based on the longest line if auto width is on.
            if (autoSizeX)
            {
                AutoWidth();
            }

            //Fit the height based on the amount of lines.
            if (autoSizeY)
            {
                AutoHeight();
            }

            //Cut lines that will not be shown due to height.
            CutLines();

            //Calculate the offsets for the selected style.
            TextStyleCalculate();
        }
        public void Render()
        {
            //Render a background if enabled.
            if(Background)
            {
                if (bgObject == null) bgObject = new ObjectBase(backgroundImage);
                bgObject.Bounds = new Rectangle(new Point(Bounds.X - backgroundMargin, Bounds.Y - backgroundMargin), 
                    new Point(Bounds.Width + (backgroundMargin * 2), Bounds.Height + (backgroundMargin * 2)));
                bgObject.Image = backgroundImage;
                bgObject.Color = backgroundColor;
                bgObject.Opacity = backgroundOpacity;
                bgObject.Draw();
            }
            //Render the text with the effects stack.
            RenderText();
        }
        #region "Processing"
        private void CalculateLines(string stringInProcess)
        {
            //Get all characters.
            List<char> characters = stringInProcess.ToCharArray().ToList();

            string lineString = ""; //The current line as a string.

            //Check if auto sizing width.
            if (autoSizeX)
            {
                Width = int.MaxValue;
            }

            //Loop through all characters.
            for (int i = 0; i < characters.Count; i++)
            {

                //Check if we are at the last character.
                if (i == characters.Count - 1)
                {
                    processedText.Add(lineString + characters[i].ToString());
                    lineString = "";
                    continue;
                }

                //Check if a character is a space.
                if(characters[i] == ' ')
                {
                    //Get the space left on the current line.
                    float spaceOnLine = Width - lineWidth(lineString);
                    //Find the location of the next space.
                    int nextSpace = spaceToNextSpace(characters, i) - i;
                    //Check if the next location of the next space is not too far away.
                    if(nextSpace > 0)
                    {
                        //Get the text to the next space.
                        string textToNextSpace = string.Join("", characters.GetRange(i + 1, nextSpace));
                        //If the text fits on the current line then go on a new line.
                        if (spaceOnLine <= lineWidth(textToNextSpace))
                        {
                            processedText.Add(lineString + "");
                            lineString = " ";
                            continue;
                        }
                    }
                }

                //If the current character is a new line character go on a new line.
                if (characters[i] == '\n')
                {
                    processedText.Add(lineString);
                    lineString = "";
                    continue;
                }

                //Check if there is still space on the current line.
                if (lineWidth(lineString) + lineWidth(characters[i].ToString()) <= Width)
                {
                    lineString += characters[i].ToString();
                    continue;
                }
                else
                {
                    //If not enough space then go on the next line.
                    processedText.Add(lineString);
                    lineString = "";
                    i--;
                    continue;
                }
            }
        }
        private float lineWidth(string line)
        {
            return Font.MeasureString(line).X;
        }
        private int spaceToNextSpace(List<char> characters, int cur)
        {
            //Loop from the current character to the ending.
            for (int i = cur + 1; i < characters.Count; i++)
            {
                //If we find a space return the location of it.
                if(characters[i] == ' ')
                {
                    return i;
                }
            }
            //If none found return -1;
            return -1;
        }
        private void AutoWidth()
        {
            //The biggest line.
            int MaxWidth = 0;
            for (int i = 0; i < processedText.Count; i++)
            {
                //Find the width of the current line.
                int CurWidth = (int)Font.MeasureString(processedText[i]).X;

                //Check if exceeding max.
                if (CurWidth > MaxWidth)
                {
                    MaxWidth = CurWidth;
                }
            }
            Width = MaxWidth;
        }
        private void AutoHeight()
        {
            Height = (int)Font.MeasureString(" ").Y * processedText.Count();
        }
        private void CutLines()
        {
            //Remove extra lines.
            int linesCount = (int) Math.Round(Height / Font.MeasureString(" ").Y);
            processedText.RemoveRange(Math.Min(linesCount, processedText.Count), processedText.Count - Math.Min(linesCount, processedText.Count));
        }
        //Decides on the offsets of the text based on the text style.
        private void TextStyleCalculate()
        {
            ts_spaceX.Clear();
            switch(TextStyle)
            {
                case RenderMode.Center: //In this mode we center the text by subtracting the line's width from the total width and dividing it by two.

                    //Go through all lines.
                    for (int l = 0; l < processedText.Count; l++)
                    {
                        string currentLine = processedText[l];
                        //If the first character of a line is a space we don't count it.
                        if (processedText[l].ToCharArray().First() == ' ')
                        {
                            currentLine = processedText[l].Substring(1);
                        }
                        //Add the offset.
                        ts_spaceX.Add((int)(Width - Font.MeasureString(currentLine).X) / 2);
                    }

                    break;
                case RenderMode.Justified: //In this mode text is stretched to fill the current line as much as possible.

                    //Find the longest line.
                    float temp_width = 0;
                    for (int l = 0; l < processedText.Count; l++)
                    {
                        if(Font.MeasureString(processedText[l]).X > temp_width)
                        {
                            temp_width = Font.MeasureString(processedText[l]).X;
                        }
                    }
                    //Go through all lines and make their offsets big enough to approximate this line.
                    for (int l = 0; l < processedText.Count; l++)
                    {
                        //Check if the current line is the longest one.
                        if (Font.MeasureString(processedText[l]).X == temp_width)
                        {
                            ts_spaceX.Add(0);
                            continue;
                        }
                        //Check for very short lines.
                        if(Font.MeasureString(processedText[l]).X < temp_width / 2)
                        {
                            ts_spaceX.Add(0);
                            continue;
                        }

                        //Else start incrementing.
                        int temp_offset = 0;
                        while(Font.MeasureString(processedText[l]).X + (temp_offset * processedText[l].Length) <= temp_width
                            && Font.MeasureString(processedText[l]).X + (temp_offset * processedText[l].Length) < Width)
                        {
                            temp_offset++;

                            //Endless loop escape.
                            if(temp_offset > 5)
                            {
                                temp_offset = 0;
                                break;
                            }
                        }
                        ts_spaceX.Add(temp_offset);
                    }
                    break;

                default:
                    //If invalid or non implemented just add an empty array.
                    for (int l = 0; l < processedText.Count; l++)
                    {
                        ts_spaceX.Add(0);
                    }
                    break;
            }
        }
        #region "Tag Process Functions"
        //Finds all tags and records them.
        private string FindTags()
        {
            //The position of the actual text.
            int actualPointer = 0;

            //The processed text.
            string processedString = "";

            //Scour for tags.
            for (int pointer = 0; pointer < Text.Length; pointer++)
            {
                //Get the current character.
                char curChar = Text[pointer];

                //Check if opening a tag and the sign is not being escaped.
                if (curChar == '<')
                {
                    pointer = TagOpen(pointer, actualPointer);
                    continue;
                }

                //Add the character to the processing string.
                processedString += curChar;

                //Check if a new line so we don't increment the actual position.
                if (curChar != '\r' && curChar != '\n')
                {
                    //If not a new line increase the actual length. This is done because we will be placing
                    //the new lines in a different way later, but we want them in the string to preserve user places
                    //new lines while effect position must remain inpartial.
                    actualPointer++;
                    
                }
            }

            return processedString;
        }
        //Records a tag and it's effect data.
        private int TagOpen(int pos, int actpos)
        {
            //The tag's effect as being recorded.
            string effect = "";

            //Search from this position until the end of the text for the tag closing symbol (>).
            for (int i = pos; i < Text.Length; i++)
            {
                //Get the current character.
                char curChar = Text[i];

                //Write the symbol to the effect.
                effect += curChar;

                //Check if ending.
                if (curChar == '>')
                {
                    procEffects.Add(actpos + ":" + effect);
                    return i; //Return the ending sign to continue reading.
                }
            }

            //If no closing sign is found end reading.
            return Text.Length - 1;
        }
        private void AddToTagPositions(int toAdd, int afterPosition)
        {
            //Go through all effects.
            for (int i = 0; i < procEffects.Count; i++)
            {
                //Get the data.
                string[] data = procEffects[i].Split(':');
                //Check if it's after the position.
                if (int.Parse(data[0]) > afterPosition)
                {
                    //Add the number to the pointer.
                    data[0] = (int.Parse(data[0]) + toAdd).ToString();
                }
                //Combine the data.
                procEffects[i] = data[0] + ":" + data[1];
            }
        }
        #endregion
        #endregion
        #region "Rendering"
        //Render passes.
        private void RenderPass_Outlines()
        {
            
        }
        private void RenderText()
        {
            //The position of the current letter, in the overall text as they are written in the effects list.
            int pointerPosition = 0;

            //The default values.
            Color color = Color;
            bool outline = Outline;
            Color outlineColor = this.outlineColor;
            int outlineSize = this.outlineSize;

            //The stack of effects.
            List<string> effectsStack = new List<string>();

            //The offsets of the current letter.
            int Xoffset = 0;
            int Yoffset = 0;

            //Run through all lines.
            for (int l = 0; l < processedText.Count; l++)
            {
                //Run through all letters.
                for (int p = 0; p < processedText[l].Length; p++)
                {

                    //Updates the effect stack up to the effects of the current position.
                    UpdateEffectStack(effectsStack, pointerPosition);

                    //Reset to defaults.
                    color = Color;
                    outline = Outline;
                    outlineColor = this.outlineColor;
                    outlineSize = this.outlineSize;

                    //Go through the effects stack to see if we should have the outline on for the current character.
                    for (int i = 0; i < effectsStack.Count; i++)
                    {
                        //Read effects. This is in a try catch because we might have invalid input.
                        try
                        {
                            //Check for a outline size effect.
                            if (GetEffectName(effectsStack[i]).ToLower() == "color")
                            {
                                int[] tempColor = GetEffectData(effectsStack[i]).Split('-').Select(int.Parse).ToArray();
                                color = new Color(tempColor[0], tempColor[1], tempColor[2]);
                            }
                            //Check if the effect is an outline.
                            if (GetEffectName(effectsStack[i]).ToLower() == "outline")
                            {
                                outline = bool.Parse(GetEffectData(effectsStack[i]));
                            }
                            //Check for a outline size effect.
                            if (GetEffectName(effectsStack[i]).ToLower() == "outlinesize")
                            {
                                outlineSize = int.Parse(GetEffectData(effectsStack[i]));
                            }
                            //Check for a outline size effect.
                            if (GetEffectName(effectsStack[i]).ToLower() == "ocolor")
                            {
                                int[] tcolor = GetEffectData(effectsStack[i]).Split('-').Select(int.Parse).ToArray();
                                outlineColor = new Color(tcolor[0], tcolor[1], tcolor[2]);
                            }
                        } catch { }

                    }

                    //If the first letter on the line is a space then we don't draw it.
                    if (!(p == 0 && processedText[l][p] == ' '))
                    {
                        //Draw outline
                        if (outline == true)
                            DrawOutline(outlineSize, outlineColor, new Vector2(ts_spaceX[l] + X + Xoffset, Y + Yoffset), Font, processedText[l][p].ToString());

                        //Draw the letter.
                        Core.ink.DrawString(Font, processedText[l][p].ToString(), new Vector2(ts_spaceX[l] + X + Xoffset, Y + Yoffset), color * Opacity);

                        //Add to the Xoffset, if justification then add the line's offset to the offset too.
                        if (TextStyle == RenderMode.Justified)
                            Xoffset += (int)Font.MeasureString(processedText[l][p].ToString()).X + ts_spaceX[l];
                        else
                            Xoffset += (int)Font.MeasureString(processedText[l][p].ToString()).X;
                    }

                    //Increment the pointer position.
                    pointerPosition++;
                }
                //Reset offsets.
                Xoffset = 0;
                Yoffset += (int)Font.MeasureString(" ").Y;
            }
        }
        //Returns all effects that are present at the requested position.
        private string[] GetEffectsAt(int position)
        {
            List<string> results = new List<string>();

            //Go through all effects.
            for (int e = 0; e < procEffects.Count; e++)
            {
                //Split position and effect.
                string[] effectData = procEffects[e].Split(':');

                //Check if the position is the right one.
                if(position == int.Parse(effectData[0]))
                {
                    results.Add(effectData[1]);
                }

            }

            //No effect found.
            return results.ToArray();
        }
        //Returns the name of the effect. (Outline, oColor...)
        private string GetEffectName(string effect)
        {
            effect = effect.Replace("<", "").Replace(">", "");

            if(effect.Contains("="))
            {
                effect = effect.Substring(0, effect.IndexOf("="));
            }
            return effect;
        }
        //Returns the data of the effect.
        private string GetEffectData(string effectData)
        {
            effectData = effectData.Replace("<", "").Replace(">", "");

            if (effectData.Contains("="))
            {
                effectData = effectData.Substring(effectData.IndexOf("="), effectData.Length - effectData.IndexOf("=")).Replace("=","");
            }
            return effectData;
        }
        //Updates the effects stack.
        private void UpdateEffectStack(List<string> effectsStack, int pointerPosition)
        {
            //Get the effects at this position.
            string[] effectsAtPosition = GetEffectsAt(pointerPosition);
            //Go through all effects at this position and modify the effect stack.
            for (int i = 0; i < effectsAtPosition.Length; i++)
            {
                //Check if ending an effect.
                if (GetEffectName(effectsAtPosition[i]) == "/")
                {
                    try
                    {
                        effectsStack.RemoveAt(effectsStack.Count - 1);
                    }
                    catch { }
                }
                else
                {
                    //Add the effect.
                    effectsStack.Add(effectsAtPosition[i]);
                }
            }
        }
        //Draws an outline.
        private void DrawOutline(int size, Color color, Vector2 offsetLocation, SpriteFont font, string text)
        {
            //Draws an outline.
            for (int i = 1; i <= size; i++)
            {
                Core.ink.DrawString(font, text, new Vector2(offsetLocation.X - i, offsetLocation.Y), color * Opacity);
                Core.ink.DrawString(font, text, new Vector2(offsetLocation.X + i, offsetLocation.Y), color * Opacity);
                Core.ink.DrawString(font, text, new Vector2(offsetLocation.X, offsetLocation.Y - i), color * Opacity);
                Core.ink.DrawString(font, text, new Vector2(offsetLocation.X, offsetLocation.Y + i), color * Opacity);
            }
        }
        #endregion
    }
}
