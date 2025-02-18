#nullable enable

using Emotion.Utility;

namespace Emotion.WIPUpdates.TimeUpdate;

public struct Timer
{
    public float Duration { get; private set; }
    public FactorMethod Method { get; private set; }
    public FactorType Type { get; private set; }

    public bool Finished { get; private set; }
    public float TimePassed { get; private set; }

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
#pragma warning disable CS1717 // Assignment made to same variable
                t = t;
#pragma warning restore CS1717 // Assignment made to same variable
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
#pragma warning disable CS1717 // Assignment made to same variable
                t = t;
#pragma warning restore CS1717 // Assignment made to same variable
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