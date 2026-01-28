#nullable enable

namespace Emotion.Core.Utility.Coroutines;

public class CoroutineManagerGameTime : CoroutineManager
{
    //public float GameTimeFactor = 1;

    public float DeltaTime { get; protected set; }

    public float GameTimeAdvanceLimit { get; protected set; } = -1;
    public float GameTimeBehindLimitSpeedUp = 100;

    public CoroutineManagerGameTime() : base(false)
    {

    }

    public void SetGameTimeAdvanceLimit(float newLimit, bool force = false)
    {
        if (newLimit < GameTimeAdvanceLimit && !force)
            return;
        GameTimeAdvanceLimit = newLimit;
    }

    public override void Update(float dt)
    {
        if (GameTimeAdvanceLimit == -1)
        {
            DeltaTime = dt;
            base.Update(dt);
        }
        else
        {
            float factor = 1f;
            if (GameTimeAdvanceLimit - Time > GameTimeBehindLimitSpeedUp) factor = 1.1f;
            dt *= factor;

            float newTime = Math.Min(Time + dt, GameTimeAdvanceLimit);
            float diff = newTime - Time;
            float deltaAllowed = MathF.Round(diff); // hmm
            DeltaTime = deltaAllowed;
            base.Update(deltaAllowed);
        }
    }

    public void ResetGameTime(float startingTime)
    {
        StopAll();
        Time = startingTime;
    }
}
