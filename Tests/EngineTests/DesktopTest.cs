#region Using

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Core;
using Emotion.Core.Platform;
using Emotion.Core.Platform.Implementation.CommonDesktop;
using Emotion.Platform;
using Emotion.Testing;

#endregion

namespace Tests.EngineTests;

[Test]
public class DesktopTest
{
    [Test]
    public IEnumerator PlatformTests()
    {
        PlatformBase plat = Engine.Host;
        Assert.True(plat != null);
        if (plat == null) yield break;

        Vector2 startingPos = plat.Position;

        var deskPlat = (DesktopPlatform)plat;
        MonitorScreen primaryMonitor = deskPlat.Monitors[0];
        Assert.True(primaryMonitor != null);
        Assert.True(plat.Context != null);
        Assert.Equal(plat.Size, Engine.Configuration.HostSize);
        Assert.True(plat.IsFocused);

        var resizes = new List<Vector2>();
        plat.OnResize += t => { resizes.Add(t); };

        plat.Position = new Vector2(10, 100);
        yield return 100;
        Assert.Equal(plat.Position, new Vector2(10, 100));
        Assert.Equal(plat.Size, Engine.Configuration.HostSize);

        plat.Size = new Vector2(960, 540);
        yield return 100;
        Assert.Equal(plat.Size, new Vector2(960, 540));

        plat.WindowState = WindowState.Minimized;
        yield return 100;
        Assert.True(!plat.IsFocused);

        plat.WindowState = WindowState.Maximized;
        yield return 100;
        Assert.Equal(plat.Size.X, primaryMonitor.Width); // Don't check Y as taskbars can be different size.
        Assert.True(plat.IsFocused);

        plat.WindowState = WindowState.Minimized;
        yield return 100;
        Assert.True(!plat.IsFocused);

        plat.WindowState = WindowState.Normal;
        yield return 100;
        Assert.True(plat.IsFocused);

        // Check that the on resize function was called correctly and with the correct sizes.
        Assert.Equal(resizes[0], new Vector2(960, 540)); // initial size set
        Assert.Equal(resizes[1].X, primaryMonitor.Width); // maximized
        Assert.Equal(resizes[^1], new Vector2(960, 540)); // restoring from the maximized state.

        Vector2 oldSize = plat.Size;
        plat.DisplayMode = DisplayMode.Fullscreen;
        yield return 100;
        Assert.Equal(plat.Size, new Vector2(primaryMonitor.Width, primaryMonitor.Height));
        plat.DisplayMode = DisplayMode.Windowed;
        yield return 100;
        Assert.Equal(plat.Size, oldSize);

        // Restore for future tests
        plat.Size = Engine.Configuration.HostSize;
        plat.Position = startingPos;
    }
}