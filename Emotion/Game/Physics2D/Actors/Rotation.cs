#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Game.Physics2D.Actors
{
    /// <summary>
    /// Stores a rotation as a sin and cos values.
    /// </summary>
    public struct Rotation
    {
        /// Sine and cosine
        public float Sin, Cos;

        /// <summary>
        /// Initialize from an angle in radians
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        public Rotation(float angle)
        {
            Sin = (float) Math.Sin(angle);
            Cos = (float) Math.Cos(angle);
        }

        /// <summary>
        /// Set using an angle in radians.
        /// </summary>
        public void SetAngle(float angle)
        {
            if (angle == 0)
            {
                Sin = 0;
                Cos = 1;
            }
            else
            {
                Sin = (float) Math.Sin(angle);
                Cos = (float) Math.Cos(angle);
            }
        }

        /// <summary>
        /// Set to the identity rotation
        /// </summary>
        public void SetIdentity()
        {
            Sin = 0.0f;
            Cos = 1.0f;
        }

        /// <summary>
        /// Get the angle in radians
        /// </summary>
        public float GetAngle()
        {
            return (float) Math.Atan2(Sin, Cos);
        }

        /// <summary>
        /// Get the x-axis
        /// </summary>
        public Vector2 GetXAxis()
        {
            return new Vector2(Cos, Sin);
        }

        /// <summary>
        /// Get the y-axis
        /// </summary>
        public Vector2 GetYAxis()
        {
            return new Vector2(-Sin, Cos);
        }
    }
}