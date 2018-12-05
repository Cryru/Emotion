using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Flags related to the RichText class.
    /// </summary>
    public class RichTextFlags
    {
        /// <summary>
        /// Characters not to render. Updates when the RichText text is changed.
        /// </summary>
        public char[] CharactersToNotRender = {'\n'};

        /// <summary>
        /// The character to separate a tag from its attributes. Updates when the RichText parses tags.
        /// </summary>
        public char TagAttributeSeparator = '-';
    }
}
