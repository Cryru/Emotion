namespace Emotion.Graphics.Command
{
    public abstract class RenderCommand
    {
        public abstract void Process();
        public abstract void Execute(RenderComposer composer);
    }
}