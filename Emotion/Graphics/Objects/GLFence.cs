#region Using

using Emotion.Common.Threading;
using Emotion.Game.Time.Routines;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    public class GLFence : IRoutineWaiter
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
        /// <param name="finished">Whether to create the fence as a finished one.</param>
        public GLFence(bool finished = false)
        {
            if (finished)
            {
                Finished = true;
                return;
            }

            Pointer = Gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
        }

        /// <summary>
        /// Update the fence's Finished status.
        /// </summary>
        public void Update()
        {
            if (Finished) return;

            GLThread.ExecuteGLThread(() =>
            {
                var result = (SyncStatus) Gl.GetSync(Pointer, SyncParameterName.SyncStatus);
                if (result != SyncStatus.Signaled) return;
                Finished = true;
            });
        }

        public void Reset()
        {
            Pointer = Gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
            Finished = false;
        }
    }
}