// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Raya.Graphics;
using Soul.IO;
using Soul.Encryption;
using Soul.Engine.Internal;

#endregion

namespace Soul.Engine.Modules
{
    public static class TimerManager
    {
        #region Properties

        /// <summary>
        /// Currently running timers.
        /// </summary>
        private static List<Timer> _loadedTimers;

        #endregion

        /// <summary>
        /// Setup the module.
        /// </summary>
        public static void Setup()
        {
            _loadedTimers = new List<Timer>();
        }

        /// <summary>
        /// Update the timer.
        /// </summary>
        public static void Update()
        {
            // Update all timers.
            for (int i = _loadedTimers.Count - 1; i >= 0 ; i--)
            {
                // Check if the timer has finished.
                if (_loadedTimers[i].Finished)
                {
                    // Remove it.
                    _loadedTimers.Remove(_loadedTimers[i]);
                }
                else
                {
                    // Otherwise update it.
                    _loadedTimers[i].Update();
                }
            }
        }
    }
}