// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Text;

#endregion

namespace Emotion.IO
{
    public sealed class TextFile : Asset
    {
        public string[] Content { get; private set; }

        internal override void Create(byte[] data)
        {
            Content = Encoding.UTF8.GetString(data).Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "").Split('\n');
        }

        internal override void Destroy()
        {
            Content = null;
        }
    }
}