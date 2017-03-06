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
                if(text != "") Process();
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
                if (text != "") Process();
            }
        }

        public TextStyle Style;
        #endregion
        //Private variables.
        #region "Private"
        private string text;
        private SpriteFont font;
        #endregion
        #region "Processed Text Data"
        /// <summary>
        /// 
        /// </summary>
        private List<string> textTags = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        private List<string> textLines = new List<string>();
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

        }   
        #endregion
        //Private functions.
        #region "Internal Functions"
        private void Process()
        {
            //Extract tags from the text to populate the array and get a clean string.
            string tagFreeText = ExtractTags(Text);

            //debug
            Console.WriteLine(tagFreeText);
        }
        #region "Process"
        private string ExtractTags(string Text)
        {
            //Clear the array holding tag data.
            textTags.Clear();

            //The position inside the text being built without tags.
            int actualPointer = 0;
            //The text being built without tags.
            string tagFreeText = "";

            //Loop through the text looking for tags.
            for (int pointer = 0; pointer < Text.Length; pointer++)
            {
                //Get the current and previous characters.
                char curChar = Text[pointer];
                char prevChar = Text[Math.Max(0, pointer - 1)];

                //Check if opening a tag and the character is not being escaped.
                if (curChar == '<' && prevChar != '\\')
                {
                    //Read the discovered tag and move the point to its end so it doesn't get recorded to the tag free text.
                    pointer = ReadTag(pointer, actualPointer);
                    //Continue to the next character, which should be after the tag.
                    continue;
                }

                //If the current character is an escape symbol, and so is the previous one, then an escape symbol is being escaped.
                if (curChar == '\\' && prevChar == '\\')
                    tagFreeText += curChar;
                //If the current character is an escape symbol, then don't add it to the tag free string.
                else if (curChar == '\\') ;
                //If the current character is anything else, add it to the tag free string.
                else tagFreeText += curChar;

                //CHECK THIS - TODO
                //Check if a new line so we don't increment the actual position.
                if (curChar != '\r' && curChar != '\n')
                {
                    //If not a new line increase the actual length. This is done because we will be placing
                    //the new lines in a different way later, but we want them in the string to preserve user places
                    //new lines while effect position must remain inpartial.
                    actualPointer++;
                }
            }

            return tagFreeText;
        }

        /// <summary>
        /// Reads a records a tag's data.
        /// </summary>
        /// <param name="pos">The position where the tag starts within the text.</param>
        /// <param name="actpos">The position where the tag will be in the tag free text.</param>
        /// <returns>The position at the end of the tag.</returns>
        private int ReadTag(int pos, int actpos)
        {
            //The tag's data.
            string data = "";

            //Search from this position until the end of the text for the tag closing symbol '>'.
            for (int pointer = pos; pointer < Text.Length; pointer++)
            {
                //Get the current and previous characters.
                char curChar = Text[pointer];
                char prevChar = Text[Math.Max(0, pointer - 1)];

                //Check if closing a tag and the character is not being escaped.
                if (curChar == '>' && prevChar != '\\')
                {
                    //Record the position of the tag within the tag free text and the tag's data.
                    textTags.Add(actpos + ":" + data);
                    //Return the position of the ending sign to continue reading.
                    return pointer; 
                }

                //If continuing add the character to the tag data.
                data += curChar;
            }

            /*
             * If no closing character is found then the tag spans the rest of the text.
             * This is not intended behaviour, but is the logical one.
            */
            return Text.Length - 1;
        }
        #endregion
        #endregion

        //Other
        #region "Component Interface"
        public override void Draw() { }
        #endregion
    }
}
