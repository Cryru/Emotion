// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

namespace Emotion.IO
{
    public sealed class Other : Asset
    {
        public byte[] Content { get; private set; }

        internal override void Create(byte[] data)
        {
            Content = data;
        }

        internal override void Destroy()
        {
            Content = null;
        }
    }
}