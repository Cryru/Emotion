#region Using

using System.Numerics;

#endregion

namespace Emotion.Game.RopeSim
{
    public class RopeSimPoint3D
    {
        public Vector3 Position;
        public Vector3 PrevPosition;
        public bool Locked;

        public RopeSimPoint3D(Vector3 pos)
        {
            Position = pos;
            PrevPosition = pos;
        }
    }

    public class RopeSimPoint2D
    {
        public Vector2 Position;
        public Vector2 PrevPosition;
        public bool Locked;

        public RopeSimPoint2D(Vector2 pos)
        {
            Position = pos;
            PrevPosition = pos;
        }
    }
}