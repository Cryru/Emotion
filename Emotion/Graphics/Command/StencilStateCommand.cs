#region Using

using System.Diagnostics;
using Emotion.Common;
using OpenGL;

#endregion

namespace Emotion.Graphics.Command
{
    public class StencilStateCommand : RecyclableCommand
    {
        public uint? Mask;
        public StencilFunction? Func;
        public int? Threshold;

        public override void Execute(RenderComposer composer)
        {
            Debug.Assert(Engine.Renderer.CurrentState.StencilTest == true);

            if (Mask != null) Gl.StencilMask(Mask.Value);
            if (Func != null) Gl.StencilFunc(Func.Value, Threshold ?? 0xFF, 0xFF);
        }

        public override void Recycle()
        {
            Mask = 0;
            Func = null;
            Threshold = null;
        }
    }
}