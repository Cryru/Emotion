#region Using

using System.Numerics;
using Adfectus.Graphics;
using OpenGL;

#endregion

namespace Adfectus.Platform.DesktopGL
{
    public class GlRenderTarget : RenderTarget
    {
        public uint Pointer { get; set; }

        public GlRenderTarget(uint pointer, Vector2 size, Texture texture) : base(size, texture)
        {
            Pointer = pointer;
        }

        public override void Dispose()
        {
            Gl.DeleteFramebuffers(Pointer);
            Texture.Dispose();
        }
    }
}