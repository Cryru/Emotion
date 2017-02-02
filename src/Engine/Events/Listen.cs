using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Events
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A listener object for SE's custom event system.
    /// </summary>
    public class Listen
    {
        #region "Variables"
        #region "Public"
        /// <summary>
        /// The type of event to listen for.
        /// </summary>
        public string Type;
        /// <summary>
        /// The action to execute when the event is triggered.
        /// </summary>
        public Action<Event> ListenerAction;
        /// <summary>
        /// The sender the event should be from.
        /// </summary>
        public object TargetedSender;
        /// <summary>
        /// The number of times the listener should listen.
        /// </summary>
        public int TriggerCount;
        /// <summary>
        /// The number of times the listener has been triggered.
        /// </summary>
        public int TimesTriggered
        {
            get
            {
                return _timesTriggered;
            }
        }
        #endregion
        #region "Privates"
        /// <summary>
        /// The number of times the listener has been triggered.
        /// </summary>
        private int _timesTriggered = 0;
        #endregion
        #endregion

        /// <summary>
        /// Creates a new event listener object, which will execute an action if an event is triggered.
        /// </summary>
        /// <param name="type">The type of event to listen for.</param>
        /// <param name="listenerAction">The action to execute when the event is triggered.</param>
        /// <param name="targetedSender">The sender the event should be from.</param>
        /// <param name="triggerCount">The number of times the listener should trigger.</param>
        public Listen(string type, Action<Event> listenerAction, object targetedSender = null, int triggerCount = -1)
        {
            Type = type;
            ListenerAction = listenerAction;
            TargetedSender = targetedSender;
            TriggerCount = triggerCount;
        }

        /// <summary>
        /// Triggers the listener using the specified event.
        /// </summary>
        /// <param name="Event">The event that has been triggered and delegated to this listener.</param>
        public void Invoke(Event Event)
        {
            //Check if reached trigger limit.
            if (_timesTriggered >= TriggerCount && TriggerCount != -1) return;

            //Check if waiting for a specific sender.
            if(TargetedSender != null && TargetedSender == Event.Sender)
            {
                ListenerAction.Invoke(Event);
            }
            else if(TargetedSender == null && Type == Event.Type)
            {
                ListenerAction.Invoke(Event);
            }

            //Increment trigger count.
            _timesTriggered++;
        }
    }
}
