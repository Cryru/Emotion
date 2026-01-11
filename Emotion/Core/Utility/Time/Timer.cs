#nullable enable

#region Using

using Emotion.Core.Utility.Coroutines;

#endregion

namespace Emotion.Core.Utility.Time;

public class Timer : IRoutineWaiter
{
    public float Duration { get; private set; }
    public FactorMethod Method { get; private set; }
    public FactorType Type { get; private set; }

    public bool Finished { get => TimePassed >= Duration; }
    public float TimePassed { get; private set; }

#if DEBUG
    private readonly Guid _dbgTimerId;
    private float _dbgLastUpdate;
#endif

    public Timer(float duration, FactorMethod method = FactorMethod.Linear, FactorType type = FactorType.In)
    {
        Duration = duration;
        Method = method;
        Type = type;

#if DEBUG
        _dbgTimerId = Guid.NewGuid();
#endif
    }

    public void Update(float timeToAdd)
    {
        TimePassed = Math.Min(TimePassed + timeToAdd, Duration);

#if DEBUG
        if (_dbgLastUpdate == Engine.TotalTime)
            Engine.Log.Warning($"Timer {_dbgTimerId} is being updated twice in one tick.", nameof(Timer), true);
        _dbgLastUpdate = Engine.TotalTime;
#endif
    }

    public void End()
    {
        TimePassed = Duration;
    }

    public void Reset()
    {
        TimePassed = 0;
    }

    public float GetFactor()
    {
        if (TimePassed >= Duration) return 1.0f; // Handles delay of zero and other weird values.
        return Timer.GetInterpolationFactor(TimePassed / Duration, Method, Type);
    }

    #region Routine Waiter API

    public void Update()
    {
        Update(Engine.DeltaTime);
    }

    #endregion

    public override string ToString()
    {
        return $"Duration: {Duration}, Progress: {Math.Round(GetFactor(), 2)}, Finished: {Finished}";
    }

    #region Static API

    public static float GetInterpolationFactor(float t, FactorMethod method, FactorType type)
    {
        switch (type)
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

        switch (method)
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

    #endregion
}