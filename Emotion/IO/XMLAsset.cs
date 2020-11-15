#region Using

using System;
using Emotion.Common;
using Emotion.Standard.XML;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// A file in XML structure.
    /// </summary>
    /// <typeparam name="T">The class to deserialize to.</typeparam>
    public class XMLAsset<T> : Asset
    {
        /// <summary>
        /// The contents of the file.
        /// </summary>
        public T Content { get; protected set; }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            try
            {
                Content = XMLFormat.From<T>(data);
            }
            catch (Exception ex)
            {
                Engine.Log.Error(new Exception($"Couldn't parse XML asset of type {GetType()}!", ex));
            }
        }

        protected override void DisposeInternal()
        {
        }
    }
}