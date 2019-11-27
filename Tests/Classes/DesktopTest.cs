#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Platform;
using Emotion.Platform.Config;
using Emotion.Platform.Implementation;
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
            PlatformBase plat = Loader.Setup(new InitConfig(),
                new PlatformConfig
                {
                    Width = 320,
                    Height = 260
                }
            );

            Assert.True(plat != null);
            Assert.True(plat.Window != null);
            Assert.True(plat.Window.Context != null);
            Assert.True(plat.Window.Size == new Vector2(320, 260));
            Assert.True(plat.Window.Focused);

            var resizes = new List<Vector2>();
            plat.Window.OnResize.AddListener(t => { resizes.Add(t); return true; });

            plat.Window.Position = new Vector2(0, 0);
            Assert.True(plat.Window.Position == new Vector2(0, 0));
            Assert.True(plat.Window.Size == new Vector2(320, 260));

            plat.Window.Size = new Vector2(960, 540);
            Assert.True(plat.Window.Size == new Vector2(960, 540));

            plat.Window.WindowState = WindowState.Minimized;
            Assert.True(!plat.Window.Focused);

            plat.Window.WindowState = WindowState.Maximized;
            Assert.True(plat.Window.Size.X == plat.Monitors[0].Width);
            Assert.True(plat.Window.Focused);

            plat.Window.WindowState = WindowState.Minimized;
            Assert.True(!plat.Window.Focused);

            plat.Window.WindowState = WindowState.Normal;
            Assert.True(plat.Window.Focused);

            // Check that the on resize function was called correctly and with the correct sizes.
            Assert.True(resizes.Count == 3);
            Assert.True(resizes[0] == new Vector2(960, 540)); // initial size set
            Assert.True(resizes[1].X == plat.Monitors[0].Width); // maximized
            Assert.True(resizes[2] == new Vector2(960, 540)); // restoring from the minimized state.

            Vector2 oldSize = plat.Window.Size;
            plat.Window.DisplayMode = DisplayMode.Fullscreen;
            Task.Delay(1000).Wait();
            Assert.True(plat.Window.Size == new Vector2(plat.Monitors[0].Width, plat.Monitors[0].Height));
            plat.Window.DisplayMode = DisplayMode.Windowed;
            Assert.True(plat.Window.Size == oldSize);
        }
    }
}