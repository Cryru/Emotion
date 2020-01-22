#region Using

using Emotion.Common;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Command
{
    /// <summary>
    /// Command for nesting a composer into another composer.
    /// </summary>
    public class FramebufferModificationCommand : RecyclableCommand
    {
        public FrameBuffer Buffer;
        public bool RebindPrevious = true;

        public override void Execute(RenderComposer _)
        {
            if (Buffer != null)
                Engine.Renderer.PushFramebuffer(Buffer);
            else
                Engine.Renderer.PopFramebuffer(RebindPrevious);
        }

        public override void Recycle()
        {
            RebindPrevious = true;
        }
    }
}