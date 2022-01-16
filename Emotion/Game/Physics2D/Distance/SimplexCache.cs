﻿#region Using

using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Distance
{
    /// <summary>Used to warm start ComputeDistance. Set count to zero on first call.</summary>
    public struct SimplexCache
    {
        /// <summary>Length or area</summary>
        public ushort Count;

        /// <summary>Vertices on shape A</summary>
        public FixedArray3<byte> IndexA;

        /// <summary>Vertices on shape B</summary>
        public FixedArray3<byte> IndexB;

        public float Metric;
    }
}