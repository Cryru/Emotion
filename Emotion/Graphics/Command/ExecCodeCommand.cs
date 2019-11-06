#region Using

#endregion

using System;

namespace Emotion.Graphics.Command
{
    /// <summary>
    /// Executes code.
    /// </summary>
    public class ExecCodeCommand : RecyclableCommand
    {
        public Action Func;

        public override void Process()
        {
            
        }

        public override void Execute(RenderComposer _)
        {
            Func?.Invoke();
        }

        public override void Recycle()
        {
            Func = null;
        }
    }
}