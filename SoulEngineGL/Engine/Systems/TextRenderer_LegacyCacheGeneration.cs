// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.Components;
using Soul.Engine.Legacy;

#endregion

namespace Soul.Engine.Systems
{
    internal partial class TextRenderer
    {
        /// <summary>
        /// Generates a line cache for textData which dictates how lines are ordered in order to fit inside a rectangle.
        /// </summary>
        /// <param name="textData">The textData to generate a cache for.</param>
        /// <param name="size">The size to fit the text in.</param>
        private static void GenerateCache(TextData textData, Vector2 size)
        {
            textData.LinesCache.Clear();
            textData.CachedSize = size;

            int spaceOnLine = (int) size.X;
            TextLine currentLine = new TextLine();
            List<Tag> tagStack = new List<Tag>();

            for (int i = 0; i < textData.Text.Length; i++)
            {
                // Define data for the current character.
                CharData current = new CharData(textData.Text[i].ToString(), textData.Color, textData.Font);

                // Check if opening a tag.
                if (current.Content == "<")
                {
                    // Read the tag.
                    i = ReadTag(textData.Text, i, out string tagInfo);
                    // Process the tag stack.
                    ProcessTagStack(tagStack, tagInfo);
                    continue;
                }

                // Apply effects stack.
                foreach (Tag tag in tagStack)
                {
                    current.Tags.Add(tag);
                }

                //Define a trigger for forcing a new line.
                bool newLine = false;

                //If the current character is space and it isn't the last character...
                if (current.Content == " " && i != textData.Text.Length - 1)
                {
                    // Find the location of the next space.
                    int locationOfNextSpace = textData.Text.IndexOf(' ', i + 1);
                    // Get the text to the next space.
                    string textBetweenCurrentCharAndNextSpace = locationOfNextSpace != -1
                        ? textData.Text.Substring(i + 1, locationOfNextSpace - i - 1)
                        : textData.Text.Substring(i + 1);

                    // Check if manual new line symbol is present between this and the next space.
                    if (textBetweenCurrentCharAndNextSpace.IndexOf('\n') != -1)
                        textBetweenCurrentCharAndNextSpace =
                            textBetweenCurrentCharAndNextSpace.Substring(0,
                                textBetweenCurrentCharAndNextSpace.IndexOf('\n'));

                    //Check if there is no space on the line for the next word, in which case force a new line.
                    if (spaceOnLine - StringWidth(" " + textBetweenCurrentCharAndNextSpace, textData.Font) <= 0)
                        newLine = true;
                }


                //If the character is not a space and there is not enough space on the next line or it's a new line character, set offsets to new line.
                if (current.Content != " " && spaceOnLine - StringWidth(current.Content, textData.Font) <= 0 ||
                    current.Content == "\n" || newLine)
                {
                    // New line code.
                    currentLine.Manual = current.Content == "\n"; // Set whether the newline was caused manually.
                    currentLine.SpaceLeft = spaceOnLine; // Set he space left on the line.
                    textData.LinesCache.Add(currentLine); // Add the line to the cached lines.

                    // Resets.
                    spaceOnLine = (int) size.X; // Reset the space on the line.
                    currentLine = new TextLine(); // Reset the current line cache.
                }

                // Update the offset if not going to a new line directed from a space to prevent trailing spaces.
                if (newLine) continue;

                //Add the character to the current line.
                currentLine.Chars.Add(current);
                spaceOnLine = (int) size.X - StringWidth(currentLine.ToString(), textData.Font);
            }

            // Check if any characters are left to be rendered.
            if (currentLine.Chars.Count > 0)
            {
                currentLine.SpaceLeft = spaceOnLine;
                textData.LinesCache.Add(currentLine);
            }

            // Determine text sizes from cached data.
            if (textData.LinesCache.Count > 0)
                textData.TextSize = new Vector2(
                    StringWidth(textData.LinesCache.OrderBy(x => x.SpaceLeft).First().ToString(), textData.Font),
                    textData.LinesCache.Count * StringHeight(" ", textData.Font)
                );
        }

        /// <summary>
        /// Read the tag.
        /// </summary>
        /// <param name="text">The total text.</param>
        /// <param name="position">The position of the tag opening character.</param>
        /// <param name="tagInformation">The collected tag's information.</param>
        /// <returns>The position of the tag closing character</returns>
        private static int ReadTag(string text, int position, out string tagInformation)
        {
            tagInformation = "";

            //Read through the text from the position of the opening character.
            for (int i = position + 1; i < text.Length; i++)
            {
                //If the character is the closing tag return the position to continue reading after it. 
                //It's incremented by one by the 'continue' statement.
                if (text[i] == '>') return i;

                //Add the character to the tag's information.
                tagInformation += text[i];
            }

            //If a closing character is not found then everything up to the text's ending is considered part of the tag.
            return text.Length - 1;
        }

        /// <summary>
        /// Updates the tag stack with new tag information.
        /// </summary>
        /// <param name="tagStack">The collected tags,</param>
        /// <param name="tagInformation">The tag information to process. Shouldn't include the opening and closing characters.</param>
        private static void ProcessTagStack(IList<Tag> tagStack, string tagInformation)
        {
            //Check if the tag information collected contains data in addition to the identifier and separate them if so.
            string identifier = tagInformation.Contains("=") ? tagInformation.Split('=')[0] : tagInformation;
            string data = tagInformation.Contains("=") ? tagInformation.Split('=')[1] : "";

            //Check if ending tag, if yes remove the last tag.
            if (identifier == "/")
            {
                if (tagStack.Count > 0) tagStack.RemoveAt(tagStack.Count - 1);
            }
            else
            {
                Tag temp = TagFactory.Build(identifier.ToLower(), data);
                if (temp != null) tagStack.Add(temp);
            }
        }

        /// <summary>
        /// Returns the width of a string.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <param name="font">The font to measure.</param>
        /// <returns>The width of the input string.</returns>
        public static int StringWidth(string text, SpriteFont font)
        {
            return (int) Math.Ceiling(font.MeasureString(text).X);
        }

        /// <summary>
        /// Returns the height of a string.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <param name="font">The font to measure.</param>
        /// <returns>The height of the input string.</returns>
        public static int StringHeight(string text, SpriteFont font)
        {
            return (int) Math.Ceiling(font.MeasureString(text).Y);
        }
    }
}