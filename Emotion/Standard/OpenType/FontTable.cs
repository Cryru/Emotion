namespace Emotion.Standard.OpenType
{
    public class FontTable
    {
        public string Tag;
        public int Checksum;
        public int Offset;
        public int Length;
        public string Compression = null;
    }
}