#region Using

using System;

#endregion

namespace Emotion.Graphics.Command
{
    public abstract class RecyclableCommand : RenderCommand, IDisposable
    {
        public abstract void Recycle();

        public virtual void Dispose()
        {
            // no-op
        }
    }
}