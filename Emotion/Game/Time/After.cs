#region Using

using System.Runtime.CompilerServices;
using Emotion.Game.Time.Routines;

#endregion

namespace Emotion.Game.Time
{
    public class After : ITimer, IRoutineWaiter
    {
        public virtual float Delay { get; set; }

        public virtual float Progress
        {
            get => GetProgress();
        }

        protected float _timePassed;
        protected Action _function;

#if DEBUG
        private string _dbgTimerId;
        private float _dbgLastUpdate;
#endif

        public After(float delay, Action function = null)
        {
            Delay = delay;
            _function = function;

#if DEBUG
            _dbgTimerId = Guid.NewGuid().ToString();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetProgress()
        {
            if (_timePassed == Delay) return 1.0f; // Handles delay of zero and other weird values.
            return _timePassed / Delay;
        }

        public virtual void Update(float timePassed)
        {
            if (Finished) return;
            if (GetProgress() == 1.0f)
            {
                End();
                return;
            }

#if DEBUG
            if (_dbgLastUpdate == Engine.TotalTime) Engine.Log.Warning($"Timer {_dbgTimerId} is being updated twice in one tick.", "After.cs", true);
            _dbgLastUpdate = Engine.TotalTime;
#endif

            _timePassed += timePassed;
            if (_timePassed > Delay) _timePassed = Delay;
        }

        public virtual void End()
        {
            _timePassed = Delay;
            _function?.Invoke();
            Finished = true;
        }

        public virtual void Restart()
        {
            _timePassed = 0;
            Finished = false;
        }

        public ITimer Clone()
        {
            return new After(Delay, _function);
        }

        #region Routine Waiter API

        public bool Finished { get; protected set; }

        public void Update()
        {
            Update(Engine.DeltaTime);
        }

        #endregion

        public override string ToString()
        {
            return $"Delay: {Delay}, Progress: {Math.Round(Progress, 2)}, Finished: {Finished}";
        }
    }
}