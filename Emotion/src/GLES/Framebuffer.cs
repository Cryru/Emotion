// Emotion - https://github.com/Cryru/Emotion

#region Using

using OpenTK.Graphics.OpenGL;

#endregion

namespace Emotion.GLES
{
    /// <summary>
    /// A framebuffer (frame buffer) is a portion of RAM containing a bitmap that drives a video
    /// display.
    /// </summary>
    public class Framebuffer
    {
        public int Pointer;

        /// <summary>
        /// Create new.
        /// </summary>
        public Framebuffer()
        {
            Pointer = GL.GenFramebuffer();
        }

        public void Use()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Pointer);
        }

        public void Destroy()
        {
            GL.DeleteFramebuffer(Pointer);
        }

        public void StopUsing()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}