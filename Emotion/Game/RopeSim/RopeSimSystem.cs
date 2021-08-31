#region Using

using System.Collections.Generic;
using System.Numerics;

#endregion

namespace Emotion.Game.RopeSim
{
    public static class RopeSimSystem
    {
        public static void Simulate3D(IEnumerable<RopeSimPoint3D> points, IEnumerable<RopeSimConnection3D> connections, Vector3 force, int maxIterations = 5)
        {
            foreach (RopeSimPoint3D point in points)
            {
                if (point.Locked) continue;
                Vector3 oldPos = point.Position;
                Vector3 motion = point.Position - point.PrevPosition;
                if (motion.Length() > 0.01f)
                    point.Position += motion; // Objects in motion stay in motion.
                point.Position += force;
                point.PrevPosition = oldPos;
            }

            // Simulating a connection can invalidate the constraints of another connection, so do multiple iterations.
            for (var i = 0; i < maxIterations; i++)
            {
                foreach (RopeSimConnection3D connection in connections)
                {
                    Vector3 center = (connection.Start.Position + connection.End.Position) / 2;

                    Vector3 diff = connection.Start.Position - connection.End.Position;
                    if (diff == Vector3.Zero) continue;
                    Vector3 dir = Vector3.Normalize(diff);

                    float length = connection.Length / 2;
                    if (!connection.Start.Locked)
                        connection.Start.Position = center + dir * length;

                    if (!connection.End.Locked)
                        connection.End.Position = center - dir * length;
                }
            }
        }

        public static void Simulate2D(IEnumerable<RopeSimPoint2D> points, IEnumerable<RopeSimConnection2D> connections, Vector2 force, int maxIterations = 5)
        {
            foreach (RopeSimPoint2D point in points)
            {
                if (point.Locked) continue;
                Vector2 oldPos = point.Position;
                Vector2 motion = point.Position - point.PrevPosition;
                if (motion.Length() > 0.01f)
                    point.Position += motion; // Objects in motion stay in motion.
                point.Position += force;
                point.PrevPosition = oldPos;
            }

            // Simulating a connection can invalidate the constraints of another connection, so do multiple iterations.
            for (var i = 0; i < maxIterations; i++)
            {
                foreach (RopeSimConnection2D connection in connections)
                {
                    Vector2 center = (connection.Start.Position + connection.End.Position) / 2;

                    Vector2 diff = connection.Start.Position - connection.End.Position;
                    if (diff == Vector2.Zero) continue;
                    Vector2 dir = Vector2.Normalize(diff);

                    float length = connection.Length / 2;
                    if (!connection.Start.Locked)
                        connection.Start.Position = center + dir * length;

                    if (!connection.End.Locked)
                        connection.End.Position = center - dir * length;
                }
            }
        }
    }
}