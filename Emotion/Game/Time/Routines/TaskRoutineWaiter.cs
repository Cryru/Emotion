#region Using

using System.Threading.Tasks;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// Used to wait for a task in a coroutine continuously.
    /// Better used for shorter running tasks.
    /// </summary>
    public class TaskRoutineWaiter : IRoutineWaiter
    {
        public bool Finished
        {
            get => _task.IsCompleted;
        }

        private readonly Task _task;

        public TaskRoutineWaiter(Task task)
        {
            _task = task;
        }

        public void Update()
        {
        }
    }
}