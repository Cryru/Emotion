#region Using

using System;
using System.Diagnostics;

#endregion

namespace Emotion.Test
{
    public static class Assert
    {
        public static void Equal(object a, object b)
        {
            if (!a.Equals(b)) throw new Exception($"Assert equal failed. Left is {a} and right is {b}");
        }

        public static void Equal(float a, float b)
        {
            if (a != b) throw new Exception($"Assert equal failed. Left is {a} and right is {b}");
        }

        public static void Equal(int a, int b)
        {
            if (a != b) throw new Exception($"Assert equal failed. Left is {a} and right is {b}");
        }

        public static void True(bool condition)
        {
            Debug.Assert(condition);

            if (!condition) throw new Exception("Assert failed.");
        }

        public static void False(bool condition)
        {
            Debug.Assert(!condition);

            if (condition) throw new Exception("Assert failed.");
        }
    }
}