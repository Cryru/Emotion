using Emotion.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.TimeUpdate;

public enum FactorMethod
{
    Linear,
    Cubic,
    Exponential,
    Bounce
}

public enum FactorType
{
    In,
    Out,
    InOut
}

public struct Timer
{
    public float Duration;
    public FactorMethod Method;
    public FactorType Type;

    public bool Finished;
    public float TimePassed;

    public Timer(float milliseconds, FactorMethod method = FactorMethod.Linear, FactorType type = FactorType.In)
    {
        Duration = milliseconds;
        Method = method;
        Type = type;
    }

    public void Update(float ms)
    {
        TimePassed += ms;
        if (TimePassed >= Duration) Finished = true;
    }

    public void Reset()
    {
        TimePassed = 0;
        Finished = false;
    }

    public float GetFactor()
    {
        return GetFactorInternal(TimePassed / Duration);
    }

    public float GetFactorClamped()
    {
        return GetFactorInternal(Maths.Clamp01(TimePassed / Duration));
    }

    private float GetFactorInternal(float t)
    {
        switch (Type)
        {
            case FactorType.In:
                t = t;
                break;
            case FactorType.Out:
                t = 1.0f - t;
                break;
            case FactorType.InOut:
                if (t <= 0.5)
                    t *= 2;
                else if (t > 0.5)
                    t = 1.0f - (2f * t - 1.0f);
                break;
        }

        switch (Method)
        {
            case FactorMethod.Linear:
                t = t;
                break;
            case FactorMethod.Cubic:
                t = t * t * t;
                break;
            case FactorMethod.Exponential:
                t = 1.0f - MathF.Exp(-10 * t);
                break;
            case FactorMethod.Bounce:
                const float a = 7.5625f;
                const float b = 1 / 2.75f;

                float res1 = MathF.Min(a * MathF.Pow(t, 2), a * MathF.Pow(t - 1.5f * b, 2) + 0.75f);
                float res2 = MathF.Min(a * MathF.Pow(t - 2.25f * b, 2) + 0.9375f, a * MathF.Pow(t - 2.625f * b, 2));

                float Bounciness = 1.70158f;
                t = t * t * ((Bounciness + 1) * t - Bounciness);
                break;
        }

        return t;
    }
}

public static class Interpolation
{
    // Delta is in seconds (so ms should be delta / 1000f)

    public static float SmoothLerp(float current, float target, int speed, float deltaMs)
    {
        float delta = deltaMs / 1000f;
        return target + (current - target) * MathF.Exp(-speed * delta);
    }

    public static float SmoothLerpAngle(float current, float target, int speed, float delta)
    {
        float num = Maths.Repeat(current - target, 360f);
        if (num > 180f)
            num -= 360f;

        return target + num * MathF.Exp(-speed * delta);
    }

    public static Vector2 SmoothLerp(Vector2 current, Vector2 target, int speed, float deltaMs)
    {
        float delta = deltaMs / 1000f;
        return target + (current - target) * MathF.Exp(-speed * delta);
    }

    public static Vector3 SmoothLerp(Vector3 current, Vector3 target, int speed, float deltaMs)
    {
        float delta = deltaMs / 1000f;
        return target + (current - target) * MathF.Exp(-speed * delta);
    }
}
