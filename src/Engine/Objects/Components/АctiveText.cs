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
        private void Compose()
        {
            //Determine how to wrap the text to fit the bounds.
            determineBounds();
        }
        #region "Compose"
        /// <summary>
        /// 
        /// </summary>
        private void determineBounds()
        {

        }
        #endregion
        private void Process()
        {
            //Reset the tag array.
            textTags.Clear();
            //Clean the text from tags and record some info while doing it.
            List<TagData> tempTagData = new List<TagData>();
            string tagFreeText = CleanTags(tempTagData);
            //Process the captured info.
            ProcessTags(tempTagData);



            //debug
            for (int i = 0; i < tagFreeText.Length + 1; i++)
            {
                //Get the data of the current character, if past the length create an empty dummy.
                CharData current;
                CharData emptyOP = new CharData("");
                if (i < tagFreeText.Length) current = new CharData(tagFreeText[i].ToString());
                else current = new CharData("");

                //check if any tags begin here
                for (int tag = 0; tag < textTags.Count; tag++)
                {
                    if (textTags[tag] == null) break;

                    if (textTags[tag].Start == i)
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

                    if (textTags[tag].Start == i)
                    {
                        textTags[tag].onStart(textTags[tag].Empty ? emptyOP : current);
                    }
                    if (textTags[tag].End == i)
                    {
                        textTags[tag].Active = false;
                        textTags[tag].onEnd(textTags[tag].Empty ? emptyOP : current);
                    }
                }

                //combine the empty tag content and the current char content.
                Console.Write(emptyOP.Content + current.Content);
            }
        }
        #region "Process"
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
