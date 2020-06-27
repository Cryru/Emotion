namespace Emotion.Standard.TMX.Object
{
    public class TmxAlignment
    {
        public TmxHorizontalAlignment Horizontal { get; private set; }
        public TmxVerticalAlignment Vertical { get; private set; }

        public TmxAlignment(string hAlign, string vAlign)
        {
            string xHorizontal = hAlign ?? "Left";
            Horizontal = (TmxHorizontalAlignment) System.Enum.Parse(typeof(TmxHorizontalAlignment),
                FirstLetterToUpperCase(xHorizontal));

            string xVertical = vAlign ?? "Top";
            Vertical = (TmxVerticalAlignment) System.Enum.Parse(typeof(TmxVerticalAlignment),
                FirstLetterToUpperCase(xVertical));
        }

        private static string FirstLetterToUpperCase(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str[0].ToString().ToUpper() + str.Substring(1);
        }
    }

    public enum TmxHorizontalAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    public enum TmxVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }
}