using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Events = SoulEngine.Events;

namespace SoulEngine.Events
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The SoulEngine event system.
    /// </summary>
    public static class ESystem
    {
        #region "Queues"
        /// <summary>
        /// 
        /// </summary>
        private static List<Listen> ListenerQueue = new List<Listen>();
        #endregion

        #region "Adding"
        /// <summary>
        /// Triggers an event.
        /// </summary>
        /// <param name="Event">The event to trigger.</param>
        public static void Add(Event Event)
        {
            //Get listeners for the current event type.
            List<Listen> matches = ListenerQueue.Where((x, y) => x.Type == Event.Type).ToList().ToList();
            //Invoke them.
            for (int i = 0; i < matches.Count; i++)
            {
                matches[i].Invoke(Event);
            }
        }
        /// <summary>
        /// Adds a listener for events.
        /// </summary>
        /// <param name="Listen">The listener to add.</param>
        public static void Add(Listen Listen)
        {
            ListenerQueue.Add(Listen);
        }
        #endregion
        #region "Removing"
        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="Listen">The listener action to remove.</param>
        public static void Remove(Listen Listen)
        {
            ListenerQueue.Remove(Listen);
        }
        /// <summary>
        /// Removes all listeners hooked to a specific action.
        /// </summary>
        /// <param name="listenerAction">The listener action to remove listeners by.</param>
        public static void Remove(Action<Event> listenerAction)
        {
            List<Listen> matches = ListenerQueue.Where(x => x.ListenerAction == listenerAction).ToList();
            for (int i = 0; i < matches.Count; i++)
            {
                ListenerQueue.Remove(matches[i]);
            }
        }
        /// <summary>
        /// Removes all listeners hooked to a specific action.
        /// </summary>
        /// <param name="listenerAction">The listener action to remove listeners by.</param>
        public static void Remove(Action listenerAction)
        {
            List<Listen> matches = ListenerQueue.Where(x => x.ListenerActionWithNoEvent == listenerAction).ToList();
            for (int i = 0; i < matches.Count; i++)
            {
                ListenerQueue.Remove(matches[i]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TargetedSender"></param>
        public static void Remove(object TargetedSender)
        {
            List<Listen> matches = ListenerQueue.Where(x => x.TargetedSender == TargetedSender).ToList();
            for (int i = 0; i < matches.Count; i++)
            {
                ListenerQueue.Remove(matches[i]);
            }
        }
        #endregion
    }
}
