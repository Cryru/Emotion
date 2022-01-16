#region Using

using System;

#endregion

namespace Emotion.Game.Physics2D.Actors
{
    [Flags]
    public enum BodyFlags : byte
    {
        Unknown = 0,

        Island = 1,

        /// <summary>
        /// The body is being simulated.
        /// </summary>
        Awake = 2 << 0,

        /// <summary>
        /// The body will be simulated more often to ensure it doesn't tunnel through other objects when moving fast.
        /// All bodies are prevented from tunneling through kinematic and static bodies, but not dynamic.
        /// Warning: Heavy performance cost.
        /// </summary>
        Bullet = 2 << 2,

        /// <summary>
        /// The body cannot rotate.
        /// </summary>
        FixedRotation = 2 << 3,

        /// <summary>
        /// The body is enabled and will be simulated.
        /// </summary>
        Enabled = 2 << 4,
    }
}