#region Using

using Emotion.Utility;

#endregion

//#nullable enable

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/kern
    /// </summary>
    public class KernTable
    {
        public Dictionary<(int, int), int> KernMap;

        private KernTable()
        {
        }

        public static KernTable ParseKern(ByteReader reader)
        {
            int tableVersion = reader.ReadUShortBE();
            if (tableVersion == 0) // Windows Kern table
            {
            }
            else if (tableVersion == 1) // Mac Kern Table
            {
            }

            return null;
        }
    }
}