#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
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
            var plat = PlatformBase.CreateDetectedPlatform(new Configurator
            {
                HostSize = new Vector2(320, 260),
                RenderSize = new Vector2(320, 260)
            });
            Assert.True(plat != null);
            if (plat == null) return;

            var deskPlat = (DesktopPlatform) plat;
            Monitor primaryMonitor = deskPlat.Monitors[0];
            Assert.True(plat.Context != null);
            Assert.Equal(plat.Size, new Vector2(320, 260));
            Assert.True(plat.IsFocused);

            var resizes = new List<Vector2>();
            plat.OnResize += t =>
            {
                resizes.Add(t);
            };

            plat.Position = new Vector2(0, 0);
            Assert.Equal(plat.Position, new Vector2(0, 0));
            Assert.Equal(plat.Size, new Vector2(320, 260));

            plat.Size = new Vector2(960, 540);
            Assert.Equal(plat.Size, new Vector2(960, 540));

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
            Assert.Equal(resizes[0], new Vector2(960, 540)); // initial size set
            Assert.True(resizes[1].X == primaryMonitor.Width); // maximized
            Assert.Equal(resizes[2], new Vector2(960, 540)); // restoring from the minimized state.

            Vector2 oldSize = plat.Size;
            plat.DisplayMode = DisplayMode.Fullscreen;
            Task.Delay(1000).Wait();
            Assert.Equal(plat.Size, new Vector2(primaryMonitor.Width, primaryMonitor.Height));
            plat.DisplayMode = DisplayMode.Windowed;
            Assert.Equal(plat.Size, oldSize);

            plat.Close();
        }
    }
}