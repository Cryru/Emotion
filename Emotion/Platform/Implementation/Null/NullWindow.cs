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
        private Vector2 _size { get; set; }

        internal NullWindow(PlatformBase platform) : base(platform)
        {
        }

        protected override void SetSize(Vector2 size)
        {
            _size = size;   
        }

        protected override Vector2 GetSize()
        {
            return _size;
        }

        internal override void UpdateDisplayMode()
        {
        }
    }
}