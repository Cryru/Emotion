#region Using

using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Models
{
    /// <summary>
    /// Used for various functions which require the passing of 4 ints, or multiple ints with other
    /// arguments mixed in.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct IntegerVector4
    {
        public int X;
        public int Y;
        public int Z;
        public int W;
    }
}