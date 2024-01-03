#nullable enable

#region Using


#endregion

namespace Emotion.Graphics.Batches3D
{
    [Flags]
    public enum RenderPass
    {
        None = 0,
        Shadow = 2 << 0,
        Main = 2 << 1,
        Transparent = 2 << 2,
        All = Shadow | Main | Transparent
    }
}