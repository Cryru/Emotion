#region Using

using System.Threading.Tasks;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// Used to wait for a wait in a coroutine.
    /// </summary>
    public class TaskRoutineWaiter : IRoutineWaiter
    {
        public bool Finished
        {
            get => _task.IsCompleted;
        }

        private Task _task;

        public TaskRoutineWaiter(Task task)
        {
            _task = task;
        }

        public void Update()
        {
        }
    }
}