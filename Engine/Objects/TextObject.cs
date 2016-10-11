using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // A text rendering object.                                                 //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
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
        public bool CenterText = false; //Whether the text should be centered. (UNTESTED)

        //Other.
        public bool noEffects = false; //Whether to skip rendering effect tags.
        public bool autoSizeX = false; //Whether to automatically set the width of the object to the text's size.
        public bool autoSizeY = false; //Whether to automatically set the height of the object based on the text's size.

        //Render Modes
        public enum RenderMode
        {
            Left,
            Center,
            Right,
            Justified
        }
        public RenderMode TextStyle = RenderMode.Left; //The way to render the text.
        private int ts_offsetX = 0; //The offset for rendering the text.
        private List<int> ts_spaceX = new List<int>(); //The offset for space between letters. Each line corresponds to a line within the processedtext.
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
            txt = txt.Replace("\r\n", "<- !<NEWLINE>! ->");
            txt = txt.Replace("\n", "\r\n");
            txt = txt.Replace("<- !<NEWLINE>! ->", "\r\n");
        }

        public void Process()
        {
            //Clear old variables.
            procEffects.Clear();
            processedText.Clear();

            string stringInProcess = "";

            //Find the tags.
            stringInProcess = FindTags();

            //Fix new line in text.
            FixNewLine(ref stringInProcess);

            //Calculate the lines. This also includes the auto height and width code.
            CalculateLines(stringInProcess);

            //Note: At this point the lines have been separated into the processedText variable.

            //Fit the width based on the longest line if auto width is on.
            if(autoSizeX)
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
            //First we render the outlines.
            RenderPass_Outlines();
            //Then we render the text.
            RenderPass_Text();
        }
        #region "Processing"
        private void CalculateLines(string stringInProcess)
        {
            //If empty string.
            if(stringInProcess == "")
            {
                processedText.Add("");
                return;
            }

            List<string> words = stringInProcess.Split(' ').ToList(); //A list to hold all words.

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i] == "") words[i] = " ";
            }

            int lineWidth = 0; //The width of the current line which we are checking agaisnt the total available width.
            string lineString = ""; //The current line as a string.

            //Check if auto sizing width.
            if (autoSizeX)
            {
                Width = int.MaxValue;
            }

            //Note: In earlier versions some infinite loops were created when the width of the object is smaller
            //than that of a single character. This doesn't seem to be the case anymore, but in case this bug
            //resurfaces I'm leaving this fix in, commented out.
            ////Check if width is smaller than one letter.
            //if (Width < (int)Font.MeasureString(" ").X)
            //{
            //    stringInProcess = "";
            //    return;
            //}

            for (int i = 0; i < words.Count; i++)
            {
                string newLineWord = ""; //The text for a queued new line.
                bool newLine = false; //The bool that signifies new lines. We need a bool cuz we can't check by newLineWord != "" since empty lines are "".

                //Check if new line.
                if (words[i].Contains("\r\n"))
                {
                    newLine = true;
                    newLineWord = words[i].Substring(words[i].IndexOf('\n') + 1);
                    words[i] = words[i].Substring(0, words[i].IndexOf('\n') - 1);

                    if(newLineWord.Contains("\r\n"))
                    {
                        words.Insert(i + 1, newLineWord.Substring(newLineWord.IndexOf('\r'), newLineWord.Length - newLineWord.IndexOf('\r')));
                        newLineWord = newLineWord.Substring(0, newLineWord.IndexOf('\n') - 1);
                    }
                }

                //Check if the line's width plus the next word are too much to fit on this line.
                if (lineWidth + (int)Font.MeasureString(" ").X + (int)Font.MeasureString(words[i]).X > Width)
                {
                    //Check if the word is longer than the total width.
                    if ((int)Font.MeasureString(words[i]).X > Width)
                    {
                        string lettersFit = ""; //The letters that fit on this line.
                        for (int p = 0; p < words[i].Length; p++)
                        {

                            if (lineWidth - (int)Font.MeasureString(" ").X + Font.MeasureString(words[i][p].ToString()).X > Width)
                            {
                                //Check if no letters fit. This is an infinite loop prevention.
                                if (lettersFit == "" && lineString == "")
                                {
                                    return;
                                }
                                //Put the letters that fit on this line. A space is present at the end so no need to add one.
                                processedText.Add(lineString + lettersFit);
                                lineString = "";
                                lineWidth = 0;
                                //Transfer the rest of the world on a new line.
                                if(newLine == true)
                                {
                                    words.Insert(i + 1, words[i].Substring(p) + "\r\n" + newLineWord);
                                }
                                else
                                {
                                    words.Insert(i + 1, words[i].Substring(p));
                                }
                                

                                break;
                            }
                            else
                            {
                                lettersFit += words[i][p]; //If we can fit more letters add them to the string.
                                lineWidth += (int)Font.MeasureString(words[i][p].ToString()).X; //Add the fitting letters to the width.
                            }
                        }
                    }
                    else //If ye just newline.
                    {
                        processedText.Add(lineString.Substring(0, lineString.Length)); //Add the current data to the line.
                        lineString = words[i] + " "; //Transfer the current word.
                        lineWidth = (int)Font.MeasureString(words[i] + " ").X; //Assign the current line's width.

                        //Check if a newline is queued.
                        if (newLine == true)
                        {
                            processedText.Add(lineString.Substring(0, lineString.Length - 1)); //Add the current data to the line.
                            lineString = newLineWord + " "; //Transfer the current word.
                            lineWidth = (int)Font.MeasureString(newLineWord + " ").X; //Assign the current line's width.
                        }

                        //Check if last.
                        if (i == words.Count - 1)
                        {
                            processedText.Add(lineString.Substring(0, lineString.Length - 1)); //Add the current data to the line.

                            if (newLine == true)
                            {
                                processedText.Add(newLineWord);
                            }

                            break;
                        }
                    }
                }
                else //If not fit it on this line.
                {
                    if(words[i] != "") //Empty new lines create this case.
                    {
                        lineWidth += (int)Font.MeasureString(words[i] + " ").X; //Add the width of the word to the current line's.
                        lineString += words[i] + " "; //Add the word to the string.
                    }

                    //Check if last.
                    if (i == words.Count - 1)
                    {
                        processedText.Add(lineString.Substring(0, lineString.Length - 1)); //Add the current data to the line.

                        if(newLine == true)
                        {
                            processedText.Add(newLineWord);
                        }

                        break;
                    }

                    //Check if a newline is queued.
                    if (newLine == true)
                    {
                        processedText.Add(lineString.Substring(0, Math.Max(lineString.Length - 1,0))); //Add the current data to the line. Empty new lines create lengths shorter than 0.
                        lineString = newLineWord + " "; //Transfer the current word.
                        lineWidth = (int)Font.MeasureString(newLineWord + " ").X; //Assign the current line's width.
                    }
                }
            }
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
            int linesCount = (int)(Height / Font.MeasureString(" ").Y);
            processedText.RemoveRange(Math.Min(linesCount, processedText.Count), processedText.Count - Math.Min(linesCount, processedText.Count));
        }
        //Decides on the offsets of the text based on the text style.
        private void TextStyleCalculate()
        {
            ts_offsetX = 0;
            ts_spaceX.Clear();
            switch(TextStyle)
            {
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
                            if(temp_offset > 10)
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
            //The position of the current letter, in the overall text as they are written in the effects list.
            int pointerPosition = 0;

            //The default values.
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
                    outline = Outline;
                    outlineColor = this.outlineColor;
                    outlineSize = this.outlineSize;

                    //Go through the effects stack to see if we should have the outline on for the current character.
                    for (int i = 0; i < effectsStack.Count; i++)
                    {
                        //Read effects. This is in a try catch because we might have invalid input.
                        try
                        {
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
                                int[] color = GetEffectData(effectsStack[i]).Split('-').Select(int.Parse).ToArray();
                                outlineColor = new Color(color[0], color[1], color[2]);
                            }
                        }
                        catch { }
                    }

                    //Draw the outline.
                    if (outline == true)
                    {
                        int centerOffset = 0;

                        if(CenterText == true && autoSizeX == false)
                        {
                            centerOffset = Width - (int) Font.MeasureString(processedText[l]).X / 2;
                        }

                        DrawOutline(outlineSize, outlineColor, new Vector2(centerOffset + X + Xoffset, Y + Yoffset), Font, processedText[l][p].ToString());
                    }

                    //Add to the X offset.
                    Xoffset += (int) Font.MeasureString(processedText[l][p].ToString()).X;

                    //Increment the pointer position.
                    pointerPosition++;
                }
                //Reset offsets.
                Xoffset = 0;
                Yoffset += (int) Font.MeasureString(" ").Y;
            }
        }
        private void RenderPass_Text()
        {
            //The position of the current letter, in the overall text as they are written in the effects list.
            int pointerPosition = 0;

            //The default values.
            Color color = Color;

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
                        } catch { }

                    }
                    //Draw the outline.
                    int centerOffset = 0;

                    if (CenterText == true && autoSizeX == false)
                    {
                        centerOffset = Width - (int)Font.MeasureString(processedText[l]).X / 2;
                    }

                    Core.ink.DrawString(Font, processedText[l][p].ToString(), new Vector2(centerOffset + X + Xoffset + ts_spaceX[l], Y + Yoffset), color * Opacity);

                    //Add to the X offset.
                    Xoffset += (int)Font.MeasureString(processedText[l][p].ToString()).X + ts_spaceX[l];

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
