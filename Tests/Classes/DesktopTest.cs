#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
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
        public static void EventualConsistencyHostWait(PlatformBase plat = null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            plat ??= Engine.Host;
            plat?.Update();
            Task.Delay(100).Wait();
            plat?.Update();
            Task.Delay(100).Wait();
            plat?.Update();
            Task.Delay(100).Wait();
            plat?.Update();
        }

        [Test]
        public void PlatformTests()
        {
            var plat = PlatformBase.CreateDetectedPlatform(new Configurator
            {
                HostSize = new Vector2(320, 260),
                RenderSize = new Vector2(320, 260),
                DebugMode = true
            });
            Assert.True(plat != null);
            if (plat == null) return;

            var deskPlat = (DesktopPlatform) plat;
            Monitor primaryMonitor = deskPlat.Monitors[0];
            Assert.True(primaryMonitor != null);
            Assert.True(plat.Context != null);
            Assert.Equal(plat.Size, new Vector2(320, 260));
            Assert.True(plat.IsFocused);

            var resizes = new List<Vector2>();
            plat.OnResize += t => { resizes.Add(t); };

            plat.Position = new Vector2(10, 100);
            EventualConsistencyHostWait(plat);
            Assert.Equal(plat.Position, new Vector2(10, 100));
            Assert.Equal(plat.Size, new Vector2(320, 260));

            plat.Size = new Vector2(960, 540);
            EventualConsistencyHostWait(plat);
            Assert.Equal(plat.Size, new Vector2(960, 540));

            plat.WindowState = WindowState.Minimized;
            EventualConsistencyHostWait(plat);
            Assert.True(!plat.IsFocused);

            plat.WindowState = WindowState.Maximized;
            EventualConsistencyHostWait(plat);
            Assert.Equal(plat.Size.X, primaryMonitor.Width); // Don't check Y as taskbars can be different size.
            Assert.True(plat.IsFocused);

            plat.WindowState = WindowState.Minimized;
            EventualConsistencyHostWait(plat);
            Assert.True(!plat.IsFocused);

            plat.WindowState = WindowState.Normal;
            EventualConsistencyHostWait(plat);
            Assert.True(plat.IsFocused);

            // Check that the on resize function was called correctly and with the correct sizes.
            Assert.Equal(resizes[0], new Vector2(960, 540)); // initial size set
            Assert.Equal(resizes[1].X, primaryMonitor.Width); // maximized
            Assert.Equal(resizes[^1], new Vector2(960, 540)); // restoring from the maximized state.

            Vector2 oldSize = plat.Size;
            plat.DisplayMode = DisplayMode.Fullscreen;
            EventualConsistencyHostWait(plat);
            Assert.Equal(plat.Size, new Vector2(primaryMonitor.Width, primaryMonitor.Height));
            plat.DisplayMode = DisplayMode.Windowed;
            EventualConsistencyHostWait(plat);
            Assert.Equal(plat.Size, oldSize);

            plat.Close();
        }
    }
}