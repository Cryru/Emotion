#nullable enable

namespace Emotion.Core.Utility.Time;

public struct ValueTimer
{
    public float Duration { get; private set; }
    public FactorMethod Method { get; private set; }
    public FactorType Type { get; private set; }

    public readonly bool Finished { get => TimePassed >= Duration; }
    public float TimePassed { get; private set; }

    public ValueTimer(float duration, FactorMethod method = FactorMethod.Linear, FactorType type = FactorType.In)
    {
        Duration = duration;
        Method = method;
        Type = type;
    }

    public void Update(float timeToAdd)
    {
        TimePassed = Math.Min(TimePassed + timeToAdd, Duration);
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

    public override string ToString()
    {
        return $"Duration: {Duration}, Progress: {Math.Round(GetFactor(), 2)}, Finished: {Finished}";
    }
}