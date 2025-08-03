#nullable enable

#region Using

using Emotion;
using System.Runtime.InteropServices;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Core.Platform.Implementation.Win32.Native;

[StructLayout(LayoutKind.Sequential)]
public struct Point
{
    public int X;
    public int Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    /// <summary>
    /// Get the top left point of the rectangle.
    /// </summary>
    /// <returns></returns>
    public Point LeftTop()
    {
        return new Point
        {
            X = Left,
            Y = Top
        };
    }

    /// <summary>
    /// Get the bottom right point of the rectangle.
    /// </summary>
    /// <returns></returns>
    public Point RightBottom()
    {
        return new Point
        {
            X = Right,
            Y = Bottom
        };
    }

    /// <summary>
    /// Converts a client-area rectangle to screen coordinates.
    /// </summary>
    /// <param name="winInstance">
    /// Handle to the window whose client coordinates are to be converted.
    /// This does mean that this rectangle is expected to be within the window.
    /// </param>
    public void ClientToScreen(nint winInstance)
    {
        Point leftTop = LeftTop();
        Point rightBottom = RightBottom();
        User32.User32.ClientToScreen(winInstance, ref leftTop);
        User32.User32.ClientToScreen(winInstance, ref rightBottom);

        Left = leftTop.X;
        Top = leftTop.Y;
        Right = rightBottom.X;
        Bottom = rightBottom.Y;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct RectS
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}