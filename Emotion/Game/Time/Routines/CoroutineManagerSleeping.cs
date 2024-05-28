using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emotion.Game.Time.Routines;

public class CoroutineManagerSleeping : CoroutineManager
{
    private ManualResetEventSlim _lock = new ManualResetEventSlim();

    public override bool Update(float timePassed = 0)
    {
        if (_runningRoutines.Count == 0)
            _lock.Wait();
        _lock.Reset();

        return base.Update(timePassed);
    }

    public override Coroutine StartCoroutine(IEnumerator enumerator)
    {
        var newRoutine = base.StartCoroutine(enumerator);
        _lock.Set();
        return newRoutine;
    }
}
