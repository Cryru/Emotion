// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

namespace Emotion.IO
{
    public class Other : Asset
    {
        public byte[] Content;

        internal override void Process(byte[] data)
        {
            Content = data;

            base.Process(data);
        }
    }
}