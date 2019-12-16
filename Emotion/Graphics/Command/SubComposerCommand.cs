#region Using

#endregion

namespace Emotion.Graphics.Command
{
    /// <summary>
    /// Command for nesting a composer into another composer.
    /// </summary>
    public class SubComposerCommand : RecyclableCommand
    {
        public RenderComposer Composer;

        public override void Process(RenderComposer composer)
        {
            if (!Composer.Processed) Composer.Process();
        }

        public override void Execute(RenderComposer _)
        {
            Composer.Execute();
        }

        public override void Recycle()
        {
        }
    }
}