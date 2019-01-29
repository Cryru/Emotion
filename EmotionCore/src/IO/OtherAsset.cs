// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

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

        internal override void CreateAsset(byte[] data)
        {
            Content = data;
        }

        internal override void DestroyAsset()
        {
            Content = null;
        }
    }
}