#region Using

using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Helpers
{
    /// <summary>
    /// The default float marshalling is buggy.
    /// Box the float to work around it.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoxedFloat
    {
        public float Value;

        public BoxedFloat(float val)
        {
            Value = val;
        }
    }
}