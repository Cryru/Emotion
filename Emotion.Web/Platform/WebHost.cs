#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Web.RazorTemplates;

#endregion

namespace Emotion.Web.Platform
{
    public class WebHost : PlatformBase
    {
        private Vector2 _size;

        public WebHost(RenderCanvas webGlHost)
        {
            Context = new WebGLContext(webGlHost.JsRuntime);
        }

        protected override void SetupPlatform(Configurator config)
        {
        }

        public override void DisplayMessageBox(string message)
        {
        }

        protected override bool UpdatePlatform()
        {
            return true;
        }

        public override WindowState WindowState { get; set; }

        protected override Vector2 GetPosition()
        {
            return Vector2.Zero;
        }

        protected override void SetPosition(Vector2 position)
        {
        }

        protected override Vector2 GetSize()
        {
            return _size;
        }

        protected override void SetSize(Vector2 size)
        {
            _size = size;
        }

        public override IntPtr LoadLibrary(string path)
        {
            return IntPtr.Zero;
        }

        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return IntPtr.Zero;
        }

        protected override void UpdateDisplayMode()
        {
        }
    }
}