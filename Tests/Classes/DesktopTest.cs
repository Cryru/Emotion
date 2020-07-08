#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Test;

#endregion

namespace Tests.Classes
{
    [Test("EmotionDesktop", true)]
    public class DesktopTest
    {
        [Test]
        public void PlatformTests()
        {
            var config = new Configurator
            {
                HostSize = new Vector2(320, 260)
            };

            PlatformBase plat = Engine.GetInstanceOfDetectedPlatform(config);

            Assert.True(plat != null);
            if (plat == null) return;

            plat.Setup(config);

            var deskPlat = (DesktopPlatform) plat;
            Monitor primaryMonitor = deskPlat.Monitors[0];
            Assert.True(plat.Context != null);
            Assert.True(plat.Size == new Vector2(320, 260));
            Assert.True(plat.IsFocused);

            var resizes = new List<Vector2>();
            plat.OnResize.AddListener(t =>
            {
                resizes.Add(t);
                return true;
            });

            plat.Position = new Vector2(0, 0);
            Assert.True(plat.Position == new Vector2(0, 0));
            Assert.True(plat.Size == new Vector2(320, 260));

            plat.Size = new Vector2(960, 540);
            Assert.True(plat.Size == new Vector2(960, 540));

            plat.WindowState = WindowState.Minimized;
            Assert.True(!plat.IsFocused);

            plat.WindowState = WindowState.Maximized;
            Assert.True(plat.Size.X == primaryMonitor.Width);
            Assert.True(plat.IsFocused);

            plat.WindowState = WindowState.Minimized;
            Assert.True(!plat.IsFocused);

            plat.WindowState = WindowState.Normal;
            Assert.True(plat.IsFocused);

            // Check that the on resize function was called correctly and with the correct sizes.
            Assert.True(resizes.Count == 3);
            Assert.True(resizes[0] == new Vector2(960, 540)); // initial size set
            Assert.True(resizes[1].X == primaryMonitor.Width); // maximized
            Assert.True(resizes[2] == new Vector2(960, 540)); // restoring from the minimized state.

            Vector2 oldSize = plat.Size;
            plat.DisplayMode = DisplayMode.Fullscreen;
            Task.Delay(1000).Wait();
            Assert.True(plat.Size == new Vector2(primaryMonitor.Width, primaryMonitor.Height));
            plat.DisplayMode = DisplayMode.Windowed;
            Assert.True(plat.Size == oldSize);
        }
    }
}