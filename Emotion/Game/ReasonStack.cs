#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Emotion.Game
{
    public class ReasonStack
    {
        public event EventHandler OnChange;
        private HashSet<string> _stack = new HashSet<string>();

        public void AddReason(string reason)
        {
            _stack.Add(reason);
            OnChange?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveReason(string reason)
        {
            _stack.Remove(reason);
            OnChange?.Invoke(this, EventArgs.Empty);
        }

        public bool AnyReason()
        {
            return _stack.Count > 0;
        }
    }
}