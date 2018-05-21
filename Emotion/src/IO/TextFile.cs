// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Text;

#endregion

namespace Emotion.IO
{
    public class TextFile : Asset
    {
        public string[] Content;

        internal override void Process(byte[] data)
        {
            Content = Encoding.Default.GetString(data).Replace("\r", "").Split('\n');

            base.Process(data);
            ProcessNative();
        }
    }
}