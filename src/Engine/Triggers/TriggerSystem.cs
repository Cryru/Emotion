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
        private static Dictionary<Action<Trigger>, string> ListenerQueue = new Dictionary<Action<Trigger>, string>();
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

            //Trigger the trigger by adding it as an instant.
            AddInstant(currentTrigger);
        }

        /// <summary>
        /// Add a trigger to the trigger queue, to be triggered when ready.
        /// </summary>
        /// <param name="Trigger"The trigger to trigger.></param>
        public static void Add(Trigger Trigger)
        {
            TriggerQueue.Add(Trigger);
        }

        /// <summary>
        /// Instantly triggers a trigger.
        /// </summary>
        /// <param name="Trigger">The trigger to trigger.</param>
        public static void AddInstant(Trigger Trigger)
        {
            //Get listeners for the current event type.
            List<Action<Trigger>> matches = ListenerQueue.Where((x, y) => x.Value == Trigger.Type).ToList().Select(x => x.Key).ToList();
            //Invoke them.
            for (int i = 0; i < matches.Count; i++)
            {
                matches[i].Invoke(Trigger);
            }
        }

        /// <summary>
        /// Adds a listener for particular trigger type.
        /// </summary>
        /// <param name="triggerType">The type of trigger to listen for.</param>
        /// <param name="listenerAction">The action to execute when the trigger is triggered.</param>
        public static void Listen(string triggerType, Action<Trigger> listenerAction)
        {
            ListenerQueue.Add(listenerAction, triggerType);
        }

        /// <summary>
        /// Adds a listener for particular trigger type from a particular sender.
        /// </summary>
        /// <param name="triggerType">The type of trigger to listen for.</param>
        /// <param name="listenerAction">The action to execute when the trigger is triggered.</param>
        /// <param name="targetedSender">The sender the trigger should be from.</param>
        public static void Listen(string triggerType, Action<Trigger> listenerAction, object targetedSender)
        {
            //TODO
        }

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listenerAction">The listener action to remove.</param>
        public static void StopListening(Action<Trigger> listenerAction)
        {
            ListenerQueue.Remove(listenerAction);
        }
    }
}
