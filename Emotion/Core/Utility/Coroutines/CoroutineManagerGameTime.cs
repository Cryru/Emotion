#nullable enable

namespace Emotion.Core.Utility.Coroutines;

public class CoroutineManagerGameTime : CoroutineManager
{
    //public float GameTimeFactor = 1;

    public float DeltaTime { get; protected set; }

    public float GameTimeAdvanceLimit { get; protected set; } = -1;
    public float GameTimeBehindLimitSpeedUp = 100;

    private float _accumulatedTime;

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
        DeltaTime = dt;

        if (GameTimeAdvanceLimit == -1)
        {
            base.Update(dt);
            return;
        }

        bool tooMuchBehind = GameTimeAdvanceLimit - Time > GameTimeBehindLimitSpeedUp;
        int loops = tooMuchBehind ? 2 : 1;

        for (int i = 0; i < loops; i++)
        {
            float newTime = Math.Min(Time + dt, GameTimeAdvanceLimit);
            float diff = newTime - Time;
            _accumulatedTime += diff;
        }

        // We don't want to change the timestep size even when limited.
        while (_accumulatedTime >= dt)
        {
            base.Update(dt);
            _accumulatedTime -= dt;
        }
    }

    public void ResetGameTime(float startingTime)
    {
        StopAll();
        Time = startingTime;
    }
}
