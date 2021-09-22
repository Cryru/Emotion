#region Using

using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// https://www.khronos.org/opengl/wiki/Sync_Object
    /// </summary>
    public class Fence
    {
        /// <summary>
        /// The OpenGL pointer to the fence.
        /// </summary>
        public int Pointer { get; protected set; }

        /// <summary>
        /// Whether the fence has been signaled.
        /// </summary>
        public bool Finished { get; protected set; }

        /// <summary>
        /// Create a new GL fence object which lets you know when certain GL commands have executed on the GPU.
        /// </summary>
        public Fence()
        {
            Pointer = Gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
        }

        /// <summary>
        /// Update the fence's Finished status.
        /// </summary>
        public bool IsSignaled()
        {
            var result = (SyncStatus) Gl.GetSync(Pointer, SyncParameterName.SyncStatus);
            return result == SyncStatus.Signaled;
        }

        /// <summary>
        /// Reset the fence, creating a new unsignaled fence at this position.
        /// </summary>
        public void Reset()
        {
            Gl.DeleteSync(Pointer);
            Pointer = Gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
            Finished = false;
        }
    }
}