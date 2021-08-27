#region Using

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emotion.Common;

#endregion

namespace Emotion.UI
{
    public class UILoadingContext
    {
        public bool Running { get; protected set; }
        private List<Task> _loadingTasks = new List<Task>();

        private bool _log;

        public UILoadingContext(bool log = true)
        {
            _log = log;
        }

        public void AddLoadingTask(Task t)
        {
            if (t.IsCompleted) return;
            _loadingTasks.Add(t);
        }

        public async Task LoadWindows()
        {
            Running = true;
            try
            {
                await Task.WhenAll(_loadingTasks);
            }
            catch (Exception ex)
            {
                Engine.Log.Error($"Window loading failed. {ex}", "UI");
            }

            if (_log) Engine.Log.Trace($"Loaded {_loadingTasks.Count} windows.", "UI");
            _loadingTasks.Clear();
            Running = false;
        }
    }
}