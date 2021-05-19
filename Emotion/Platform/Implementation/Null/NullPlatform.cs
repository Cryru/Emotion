#region Using

using System;
using System.Numerics;
using Emotion.Common;

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

        protected override void SetupInternal(Configurator config)
        {
            Audio = new NullAudioAdapter();
        }

        public override IntPtr LoadLibrary(string path)
        {
            return IntPtr.Zero;
        }

        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return IntPtr.Zero;
        }

        public override WindowState WindowState { get; set; }
        private Vector2 _position { get; set; }
        private Vector2 _size { get; set; }

        protected override void SetPosition(Vector2 position)
        {
            _position = position;
        }

        protected override Vector2 GetPosition()
        {
            return _position;
        }

        protected override void SetSize(Vector2 size)
        {
            _size = size;
        }

        protected override Vector2 GetSize()
        {
            return _size;
        }

        protected override void UpdateDisplayMode()
        {
        }
    }
}