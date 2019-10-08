#region Using

using System.Numerics;
using Emotion.Common;

#endregion

namespace Emotion.Graphics.Command
{
    /// <summary>
    /// Command for modifying the current model matrix.
    /// </summary>
    public class ModelMatrixModificationCommand : RecyclableCommand
    {
        public Matrix4x4? Matrix;
        public bool Multiply;

        public override void Process()
        {
        }

        public override void Execute(RenderComposer _)
        {
            if (Matrix != null)
                Engine.Renderer.PushModelMatrix((Matrix4x4) Matrix, Multiply);
            else
                Engine.Renderer.PopModelMatrix();
        }

        public override void Recycle()
        {
        }
    }
}