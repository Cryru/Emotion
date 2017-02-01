using SoulEngine.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Triggers
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The SoulEngine trigger system that handles trigger queuing and listener raising.
    /// </summary>
    public static class TriggerSystem
    {
        #region "Queues"
        /// <summary>
        /// 
        /// </summary>
        private static List<Trigger> TriggerQueue = new List<Trigger>();
        /// <summary>
        /// 
        /// </summary>
        private static List<Listen> ListenerQueue = new List<Listen>();
        #endregion

        /// <summary>
        /// Is run every tick to process events in the trigger queue.
        /// </summary>
        public static void Update()
        {
            //Check if there are any events to process.
            if (TriggerQueue.Count == 0) return;

            //Get the next trigger.
            Trigger currentTrigger = TriggerQueue[0];
            TriggerQueue.RemoveAt(0);

            //Trigger the trigger.
            Trigger(currentTrigger);
        }

        /// <summary>
        /// Add a trigger to the trigger queue, to be triggered when ready.
        /// </summary>
        /// <param name="Trigger">The trigger to trigger.</param>
        public static void Add(Trigger Trigger)
        {
            TriggerQueue.Add(Trigger);
        }

        /// <summary>
        /// Adds a listener for triggers.
        /// </summary>
        /// <param name="Trigger">The trigger to trigger.</param>
        public static void Add(Listen Listen)
        {
            ListenerQueue.Add(Listen);
        }

        /// <summary>
        /// Triggers a trigger instantly, without adding it to the queue.
        /// </summary>
        /// <param name="Trigger">The trigger to trigger.</param>
        public static void Trigger(Trigger Trigger)
        {
            //Get listeners for the current event type.
            List<Listen> matches = ListenerQueue.Where((x, y) => x.Type == Trigger.Type).ToList().ToList();
            //Invoke them.
            for (int i = 0; i < matches.Count; i++)
            {
                matches[i].Invoke(Trigger);
            }

            //Report trigger.
            Console.WriteLine(">>>> Trigger " + Trigger.Type + " from " + Trigger.Sender.ToString() + " got to " + matches.Count + " listeners.");
        }

        #region "Stopping"
        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="Listen">The listener action to remove.</param>
        public static void StopListening(Listen Listen)
        {
            ListenerQueue.Remove(Listen);
        }
        /// <summary>
        /// Removes all listeners hooked to a specific action.
        /// </summary>
        /// <param name="listenerAction">The listener action to remove listeners by.</param>
        public static void StopListening(Action<Trigger> listenerAction)
        {
            List<Listen> matches = ListenerQueue.Where(x => x.ListenerAction == listenerAction).ToList();
            for (int i = 0; i < matches.Count; i++)
            {
                ListenerQueue.Remove(matches[i]);
            }
        }
        #endregion
    }
}
