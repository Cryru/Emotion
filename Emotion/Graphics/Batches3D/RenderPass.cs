#nullable enable

#region Using


#endregion

namespace Emotion.Graphics.Batches3D
{
    [Flags]
    public enum RenderPass : byte
    {
        None = 0,

        Main = 2 << 0,
        Transparent = 2 << 1,

        Count = 2
    }
}