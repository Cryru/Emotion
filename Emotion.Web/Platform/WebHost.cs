#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Platform;

#endregion

namespace Emotion.Web.Platform
{
    public class WebHost : PlatformBase
    {
        public GraphicsContext ContextSetter
        {
            set => Context = value;
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
            return new Vector2(960, 540);
        }

        protected override void SetSize(Vector2 size)
        {
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