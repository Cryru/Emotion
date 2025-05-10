namespace Emotion.Game.Time.Routines;

public class CoroutineManagerGameTime : CoroutineManager
{
    //public float GameTimeFactor = 1;

    public float GameTimeAdvanceLimit = -1;
    public float GameTimeMaxTimeBehindLimit = 100;

    public CoroutineManagerGameTime() : base(false)
    {

    }

    public override void Update(float dt)
    {
        if (GameTimeAdvanceLimit == -1)
        {
            base.Update(dt);
        }
        else
        {
            float newTime = Time + dt;
            if (newTime > GameTimeAdvanceLimit) newTime = GameTimeAdvanceLimit;
            if (newTime + GameTimeMaxTimeBehindLimit < GameTimeAdvanceLimit) newTime = GameTimeAdvanceLimit;

            float diff = newTime - Time;
            base.Update(MathF.Floor(diff));
        }
    }

    public void ResetGameTime(float startingTime)
    {
        StopAll();
        Time = startingTime;
    }
}
