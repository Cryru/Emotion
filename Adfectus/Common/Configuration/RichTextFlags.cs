namespace Adfectus.Common.Configuration
{
    /// <summary>
    /// Flags related to the RichText class.
    /// </summary>
    public class RichTextFlags
    {
        /// <summary>
        /// Characters not to render. Updates when the RichText text is changed.
        /// </summary>
        public char[] CharactersToNotRender { get; set; } = {'\n'};

        /// <summary>
        /// The character to separate a tag from its attributes. Updates when the RichText parses tags.
        /// </summary>
        public char TagAttributeSeparator { get; set; } = '-';
    }
}