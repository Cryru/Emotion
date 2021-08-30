#region Using

using System.Numerics;

#endregion

namespace Emotion.Game.RopeSim
{
    public class RopeSimConnection3D
    {
        public RopeSimPoint3D Start;
        public RopeSimPoint3D End;
        public float Length;

        public RopeSimConnection3D(RopeSimPoint3D start, RopeSimPoint3D end, float length)
        {
            Start = start;
            End = end;
            Length = length;
        }
    }

    public class RopeSimConnection2D
    {
        public RopeSimPoint2D Start;
        public RopeSimPoint2D End;
        public float Length;

        public RopeSimConnection2D(RopeSimPoint2D start, RopeSimPoint2D end, float length = -1)
        {
            Start = start;
            End = end;

            if (length == -1) length = Vector2.Distance(start.Position, end.Position);
            Length = length;
        }
    }
}