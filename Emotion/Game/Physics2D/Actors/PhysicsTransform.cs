#region Using

using System.Numerics;

#endregion

namespace Emotion.Game.Physics2D.Actors
{
    public class PhysicsTransform
    {
        /// <summary>
        /// The position of the origin of the body.
        /// This is usually the body's center.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The rotation of the body, in the origin.
        /// </summary>
        public Rotation Rotation;

        public Vector2 TransformVector(Vector2 vec)
        {
            float sin = Rotation.Sin;
            float cos = Rotation.Cos;

            float x = cos * vec.X - sin * vec.Y + Position.X;
            float y = sin * vec.X + cos * vec.Y + Position.Y;

            return new Vector2(x, y);
        }

        public Vector2 TransformTransposeVector(Vector2 vec)
        {
            float sin = Rotation.Sin;
            float cos = Rotation.Cos;

            float px = vec.X - Position.X;
            float py = vec.Y - Position.Y;
            float x = cos * px + sin * py;
            float y = -sin * px + cos * py;

            return new Vector2(x, y);
        }

        public Rotation TransformTransposeRotation(Rotation rotationB)
        {
            float sinA = Rotation.Sin;
            float cosA = Rotation.Cos;

            float sinB = rotationB.Sin;
            float cosB = rotationB.Cos;

            Rotation rot;
            rot.Sin = cosA * sinB - sinA * cosB;
            rot.Cos = cosA * cosB + sinA * sinB;

            return rot;
        }

        public Vector2 RotateVector(Vector2 vector)
        {
            float sinA = Rotation.Sin;
            float cosA = Rotation.Cos;

            return new Vector2(cosA * vector.X - sinA * vector.Y, sinA * vector.X + cosA * vector.Y);
        }

        public Vector2 RotateTransposeVector(Vector2 vector)
        {
            float sinA = Rotation.Sin;
            float cosA = Rotation.Cos;

            return new Vector2(cosA * vector.X + sinA * vector.Y, -sinA * vector.X + cosA * vector.Y);
        }

        public PhysicsTransform TransposeTransform(PhysicsTransform otherTransform)
        {
            var newTrans = new PhysicsTransform
            {
                Rotation = TransformTransposeRotation(otherTransform.Rotation),
                Position = RotateTransposeVector(otherTransform.Position - Position)
            };
            return newTrans;
        }
    }
}