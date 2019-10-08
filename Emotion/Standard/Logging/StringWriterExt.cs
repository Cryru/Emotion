#region Using

using System;
using System.IO;

#endregion

namespace Emotion.Standard.Logging
{
    /// <summary>
    /// String writer which automatically flushes the buffer and reports events when it is written to.
    /// </summary>
    public class StringWriterExt : StringWriter
    {
        /// <summary>
        /// The event to trigger when updated.
        /// </summary>
        public event Action OnUpdate;

        /// <summary>
        /// Whether the string writer will automatically flush the buffer.
        /// </summary>
        public virtual bool AutoFlush { get; set; }

        /// <summary>
        /// Create a new extended string writer.
        /// </summary>
        /// <param name="autoFlush">Whether the string writer should flush automatically.</param>
        public StringWriterExt(bool autoFlush)
        {
            AutoFlush = autoFlush;
        }

        /// <inheritdoc />
        public override void Flush()
        {
            base.Flush();
            OnUpdate?.Invoke();
        }

        /// <inheritdoc />
        public override void Write(char value)
        {
            base.Write(value);
            if (AutoFlush) Flush();
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            base.Write(value);
            if (AutoFlush) Flush();
        }

        /// <inheritdoc />
        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            if (AutoFlush) Flush();
        }
    }
}