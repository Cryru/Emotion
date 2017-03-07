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
        private List<Tag> textTags = new List<Tag>();
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
            for (int i = 0; i < tagFreeText.Length + 1; i++)
            {
                //Get the data of the current character, if past the length create an empty dummy.
                CharData current;
                if (i < tagFreeText.Length) current = new CharData(tagFreeText[i].ToString());
                else current = new CharData("");

                //check if any tags begin here
                for (int tag = 0; tag < textTags.Count; tag++)
                {
                    /*
                        Note: Tags are tied to a character's position which means tags with no characters between them will apply their onStart and
                        onEnd on the next character. TODO - PREVENT THIS
                    */

                    if (textTags[tag].Start == i)
                    {
                        textTags[tag].Active = true;
                        textTags[tag].onStart(current);
                    }
                    if (textTags[tag].End == i)
                    {
                        textTags[tag].Active = false;
                        textTags[tag].onEnd(current);
                    }
                }

                //apply effect stack
                Tag[] activeTags = textTags.Where(x => x.Active == true).ToArray();
                for (int effect = 0; effect < activeTags.Length; effect++)
                {
                    activeTags[effect].onDuration(current);
                }

                //render the text using the curdata
                Console.Write(current.Content);
            }
        }
        #region "Process"
        private string ExtractTags(string Text)
        {
            //Clear the array holding tag data.
            textTags.Clear();

            //The text being built without tags.
            string tagFreeText = "";

            //Loop through the text looking for tags.
            for (int pointer = 0; pointer < Text.Length; pointer++)
            {
                //Get the current and previous character.
                char curChar = Text[pointer];
                char prevChar = Text[Math.Max(0, pointer - 1)];

                //Check if opening a tag and the character is not being escaped.
                if (curChar == '<' && prevChar != '\\')
                {
                    //Read the discovered tag and move the point to its end so it doesn't get recorded to the tag free text.
                    pointer = ReadTag(pointer, tagFreeText.Length);
                    //Continue to the next character, which should be after the tag.
                    continue;
                }

                //If the current character is an escape symbol, and so is the previous one, then an escape symbol is being escaped.
                if (curChar == '\\' && prevChar == '\\')
                    tagFreeText += curChar;
                //If the current character is an escape symbol, then don't add it to the tag free string.
                else if (curChar == '\\') continue;
                //If the current character is anything else, add it to the tag free string.
                else tagFreeText += curChar;
            }

            return tagFreeText;
        }

        /// <summary>
        /// Reads a records a tag's data.
        /// </summary>
        /// <param name="pos">The position where the tag starts within the text.</param>
        /// <param name="actpos">The position where the tag will be in the tag free text.</param>
        /// <returns>The position at the end of the tag.</returns>
        private int ReadTag(int pos, int actPos)
        {
            //The tag's information.
            string information = "";

            //Search from this position, plus one so we don't capture the opening character, until the end of the text for the tag closing symbol - '>'.
            for (int pointer = pos + 1; pointer < Text.Length; pointer++)
            {
                //Get the current and previous characters.
                char curChar = Text[pointer];
                char prevChar = Text[Math.Max(0, pointer - 1)];

                //Check if closing a tag and the character is not being escaped.
                if (curChar == '>' && prevChar != '\\')
                {
                    //Check if the tag information collected contains data in addition to the identifier and separate them if so.
                    string identifier = information.Contains("=") ? information.Split('=')[0] : information;
                    string data = information.Contains("=") ? information.Split('=')[1] : "";

                    /*
                     * Check if the current tag is a closing tag, in addition to there being tags collected and that there
                     * are tags with unknown endings to prevent the ending from moving from the first found closing tag.
                     */
                    if (identifier == "/" && textTags.Count > 0 && textTags.Where(x => x.End == null).ToArray().Length > 0)
                    {
                        //Find the last tag with no closing pair.
                        Tag pair = textTags.Where(x => x.End == null).Last();
                        int endingPos = Math.Max(0, actPos - 1);

                        /*
                         * Assign the ending point to the last collected tag. We subtract one from the actual position because
                         * otherwise we are grabbing the character after the tag into the ending.
                         */
                         pair.End = endingPos < pair.Start ? pair.Start : endingPos;
                    }
                    /*
                     * If not a closing tag then add the tag to the tag list with the captured 
                     * identifier, data, position, and an empty ending to be assigned later.
                     */
                    else if(information != "/")
                    {
                        Tag newTag = TagFactory.Build(identifier, data, actPos, null);
                        //If the identifier is not present a null will be returned, so we need to end tag extraction.
                        if (newTag == null) return Text.Length - 1;
                        textTags.Add(newTag);
                    }
                        
                    /*
                     * Return the position of the ending sign to continue reading. 
                     * Reading will resume from the next character to avoid capturing the closing character.
                     */
                    return pointer; 
                }
                //If not then continue add the current character to the information.
                else
                {
                    information += curChar;
                }
            }

            /*
             * If no closing character is found then the tag spans the rest of the text.
             * This is not intended behaviour, but is the logical one, and is the default error handling here.
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
