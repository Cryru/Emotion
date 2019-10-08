#region Using

using System;
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

        public override bool Update()
        {
            return true;
        }

        protected override void SetupPlatform()
        {
        }

        protected override Window CreateWindowPlatform()
        {
            return new NullWindow(this);
        }

        public override bool GetKeyDown(Key key)
        {
            return false;
        }

        public override bool GetMouseKeyDown(MouseKey key)
        {
            return false;
        }
    }
}