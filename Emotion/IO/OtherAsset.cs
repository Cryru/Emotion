#region Using

using System;

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
        public ReadOnlyMemory<byte> Content { get; private set; }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            Content = data;
        }

        protected override void DisposeInternal()
        {
            Content = null;
        }
    }
}