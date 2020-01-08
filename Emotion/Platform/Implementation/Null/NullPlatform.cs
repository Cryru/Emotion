#region Using

using System;
using Emotion.Common;
using Emotion.Platform.Input;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public class NullPlatform : PlatformBase
    {
        public override void DisplayMessageBox(string message)
        {
            Console.WriteLine($"NullPlatform MessageBox: {message}");
        }

        protected override bool UpdatePlatform()
        {
            return true;
        }

        protected override void SetupPlatform(Configurator config)
        {
            Audio = new NullAudioContext();
            Window = new NullWindow(this);
        }

        public override IntPtr LoadLibrary(string path)
        {
            return IntPtr.Zero;
        }

        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return IntPtr.Zero;
        }
    }
}