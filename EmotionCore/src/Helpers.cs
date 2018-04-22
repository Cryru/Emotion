// Emotion - https://github.com/Cryru/Emotion

namespace Emotion
{
    public static class Helpers
    {
        /// <summary>
        /// Safely parses the text to an int. If the parse fails returns a default value.
        /// </summary>
        /// <param name="text">The text to parse to an int.</param>
        /// <param name="invalidValue">The value to return if the parsing fails. 0 by default.</param>
        /// <returns>The text parsed as an int, or a default value.</returns>
        public static int SafeIntParse(string text, int invalidValue = 0)
        {
            bool parsed = int.TryParse(text, out int result);
            return parsed ? result : invalidValue;
        }

    }
}