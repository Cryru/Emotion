namespace Emotion.IO.AssetPack
{
    public class BlobFile
    {
        public int Offset;
        public int Length;

        protected BlobFile()
        {
            // for serialization
        }

        public BlobFile(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }
    }
}