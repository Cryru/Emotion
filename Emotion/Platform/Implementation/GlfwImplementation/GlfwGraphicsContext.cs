#region Using

using System;
using Emotion.GLFW;

#endregion

namespace Emotion.Platform.Implementation.GlfwImplementation
{
    public class GlfwGraphicsContext : RenderDocGraphicsContext
    {
        private IntPtr _win;

        public GlfwGraphicsContext(IntPtr window)
        {
            _win = window;
        }

        protected override void SetSwapIntervalPlatform(int interval)
        {
            Glfw.SwapInterval(interval);
        }

        public override void MakeCurrent()
        {
            Glfw.MakeContextCurrent(_win);
        }

        public override void SwapBuffers()
        {
            Glfw.SwapBuffers(_win);
        }

        public override IntPtr GetProcAddress(string func)
        {
            return Glfw.GetProcAddress(func);
        }
    }
}