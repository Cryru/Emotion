namespace Emotion.IO
{
    /// <summary>
    /// An asset of an another type.
    /// </summary>
    public class OtherAsset : Asset
    {
        /// <summary>
        /// The context of the file as a byte array.
        /// </summary>
        public byte[] Content { get; private set; }

        protected override void CreateInternal(byte[] data)
        {
            Content = data;
        }

        protected override void DisposeInternal()
        {
            Content = null;
        }
    }
}