#region Using

using System.Runtime.InteropServices;

#endregion

namespace Emotion.Core.Platform.Implementation.Web.Models;

/// <summary>
/// Used for various functions which require the passing of 4 ints, or multiple ints with other
/// arguments mixed in.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct IntegerVector4
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int W { get; set; }
}