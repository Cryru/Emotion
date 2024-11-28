using Emotion.Game.Time.Routines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Common;

public static class GameTime
{
    public static CoroutineManager CoroutineManager = new CoroutineManager(false);

    public static float GameTimeFactor = 1;

    public static float GameTimeAdvanceLimit = -1;

    public static void Update(float dt)
    {
        if (GameTimeAdvanceLimit == -1)
        {
            CoroutineManager.Update(dt);
        }
        else
        {
            while (CoroutineManager.Time + dt < GameTimeAdvanceLimit)
            {
                float newTimeMax = CoroutineManager.Time + dt;
                if (newTimeMax > GameTimeAdvanceLimit) newTimeMax = GameTimeAdvanceLimit;
                float diff = newTimeMax - CoroutineManager.Time;
                CoroutineManager.Update(MathF.Floor(diff));
            }
        }
    }

    public static void ResetGameTime(float startingTime)
    {
        CoroutineManager.Time = startingTime;
        CoroutineManager.StopAll();
    }
}
