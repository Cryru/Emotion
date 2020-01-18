namespace Emotion.Graphics.Command
{
    public abstract class RenderCommand
    {
        public virtual void Process(RenderComposer composer)
        {
            // no-op
        }

        public abstract void Execute(RenderComposer composer);
    }
}