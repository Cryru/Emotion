#region Using

using System.Numerics;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public sealed class NullWindow : Window
    {
        public override WindowState WindowState { get; set; }
        public override DisplayMode DisplayMode { get; set; }
        public override Vector2 Position { get; set; }
        public override Vector2 Size { get; set; }

        internal NullWindow(PlatformBase platform) : base(platform)
        {
        }

        internal override void UpdateDisplayMode()
        {
            
        }
    }
}