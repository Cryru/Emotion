#region Using

using System.Numerics;

#endregion

namespace Emotion.Utility
{
    /// <summary>A 2-by-2 matrix. Stored in column-major order.</summary>
    public struct Matrix2x2
    {
        public Vector2 Ex, Ey;

        /// <summary>Construct this matrix using columns.</summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        public Matrix2x2(Vector2 c1, Vector2 c2)
        {
            Ex = c1;
            Ey = c2;
        }

        /// <summary>
        /// Construct this matrix using scalars.
        /// </summary>
        public Matrix2x2(float a11, float a12, float a21, float a22)
        {
            Ex = new Vector2(a11, a21);
            Ey = new Vector2(a12, a22);
        }

        public Matrix2x2 Inverse
        {
            get
            {
                float a = Ex.X, b = Ey.X, c = Ex.Y, d = Ey.Y;
                float det = a * d - b * c;
                if (det != 0.0f)
                    det = 1.0f / det;

                Matrix2x2 result = new Matrix2x2();
                result.Ex.X = det * d;
                result.Ex.Y = -det * c;

                result.Ey.X = -det * b;
                result.Ey.Y = det * a;

                return result;
            }
        }

        /// <summary>Initialize this matrix using columns.</summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        public void Set(Vector2 c1, Vector2 c2)
        {
            Ex = c1;
            Ey = c2;
        }

        /// <summary>
        /// Set this to the identity matrix.
        /// </summary>
        public void SetIdentity()
        {
            Ex.X = 1.0f;
            Ey.X = 0.0f;
            Ex.Y = 0.0f;
            Ey.Y = 1.0f;
        }

        /// <summary>
        /// Set this matrix to all zeros.
        /// </summary>
        public void SetZero()
        {
            Ex.X = 0.0f;
            Ey.X = 0.0f;
            Ex.Y = 0.0f;
            Ey.Y = 0.0f;
        }

        /// <summary>
        /// Solve A * x = b, where b is a column vector.
        /// This is more efficient than computing the inverse in one-shot cases.
        /// </summary>
        public Vector2 Solve(Vector2 b)
        {
            float a11 = Ex.X, a12 = Ey.X, a21 = Ex.Y, a22 = Ey.Y;
            float det = a11 * a22 - a12 * a21;
            if (det != 0.0f)
                det = 1.0f / det;

            return new Vector2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
        }

        public Vector2 Transform(Vector2 v)
        {
            return new Vector2(Ex.X * v.X + Ey.X * v.Y, Ex.Y * v.X + Ey.Y * v.Y);
        }

        public static void Add(ref Matrix2x2 A, ref Matrix2x2 B, out Matrix2x2 R)
        {
            R.Ex = A.Ex + B.Ex;
            R.Ey = A.Ey + B.Ey;
        }
    }
}