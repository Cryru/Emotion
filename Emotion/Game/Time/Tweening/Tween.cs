#region Using

using System;

#endregion

namespace Emotion.Game.Time.Tweening
{
    public static class Tween
    {
        public static float Bounciness = 1.70158f;

        public static Func<float, float> In(TweenMethod method)
        {
            return method switch
            {
                TweenMethod.Linear => Linear,
                TweenMethod.Quad => Quad,
                TweenMethod.Cubic => Cubic,
                TweenMethod.Quart => Quart,
                TweenMethod.Quint => Quint,
                TweenMethod.Sine => Sine,
                TweenMethod.Expo => Expo,
                TweenMethod.Circle => Circle,
                TweenMethod.Back => Back,
                TweenMethod.Bounce => Bounce,
                _ => Linear
            };
        }

        public static Func<float, float> Out(TweenMethod method)
        {
            Func<float, float> methodFunc = In(method);

            return s => 1f - methodFunc(s);
        }

        public static Func<float, float> InOut(TweenMethod method)
        {
            Func<float, float> methodFuncIn = In(method);
            Func<float, float> methodFuncOut = Out(method);

            return s =>
            {
                if (s < 0.5f)
                    return methodFuncIn(2 * s) * 0.5f;
                return 1 + methodFuncOut(2 * s - 1) * 0.5f;
            };
        }

        public static Func<float, float> OutIn(TweenMethod method)
        {
            Func<float, float> methodFuncIn = In(method);
            Func<float, float> methodFuncOut = Out(method);

            return s =>
            {
                if (s < 0.5f)
                    return 1 + methodFuncOut(2 * s - 1) * 0.5f;
                return methodFuncIn(2 * s) * 0.5f;
            };
        }

        public static float Linear(float s)
        {
            return s;
        }

        public static float Quad(float s)
        {
            return s * s;
        }

        public static float Cubic(float s)
        {
            return s * s * s;
        }

        public static float Quart(float s)
        {
            return s * s * s * s;
        }

        public static float Quint(float s)
        {
            return s * s * s * s * s;
        }

        public static float Sine(float s)
        {
            return 1.0f + MathF.Sin(3 * (MathF.PI / 2) + s * (MathF.PI / 2));
        }

        public static float Expo(float s)
        {
            return MathF.Pow(2, 10 * (s - 1));
        }

        public static float Circle(float s)
        {
            return 1f - MathF.Sqrt(1 - s * s);
        }

        public static float Back(float s)
        {
            return s * s * ((Bounciness + 1) * s - Bounciness);
        }

        public static float Bounce(float s)
        {
            const float a = 7.5625f;
            const float b = 1 / 2.75f;

            float res1 = MathF.Min(a * MathF.Pow(s, 2), a * MathF.Pow(s - 1.5f * b, 2) + 0.75f);
            float res2 = MathF.Min(a * MathF.Pow(s - 2.25f * b, 2) + 0.9375f, a * MathF.Pow(s - 2.625f * b, 2));

            return MathF.Min(res1, res2) + 0.984375f;
        }
    }
}