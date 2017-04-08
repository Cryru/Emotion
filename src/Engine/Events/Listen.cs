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
        #region "Declarations"
        #region "Public"
        /// <summary>
        /// The type of event to listen for.
        /// </summary>
        public string Type;
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
        /// <summary>
        /// The action to execute when the event is triggered.
        /// </summary>
        public Action<Event> ListenerAction { private set; get; }
        /// <summary>
        /// The action to execute when the event is triggered that doesn't require an event object.
        /// </summary>
        public Action ListenerActionWithNoEvent { private set; get; }
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
        /// Creates a new event listener object, which will execute an action if an event is triggered.
        /// </summary>
        /// <param name="type">The type of event to listen for.</param>
        /// <param name="listenerAction">The action to execute when the event is triggered.</param>
        /// <param name="targetedSender">The sender the event should be from.</param>
        /// <param name="triggerCount">The number of times the listener should trigger.</param>
        public Listen(string type, Action listenerAction, object targetedSender = null, int triggerCount = -1)
        {
            Type = type;
            ListenerActionWithNoEvent = listenerAction;
            TargetedSender = targetedSender;
            TriggerCount = triggerCount;
        }

        /// <summary>
        /// Triggers the listener using the specified event.
        /// </summary>
        /// <param name="Event">The event that has been triggered and delegated to this listener.</param>
        public void Invoke(Event Event)
        {
            //Check if broken listener.
            if (ListenerAction != null && ListenerActionWithNoEvent != null) throw new Exception("Invalid event listener, ");

            //Check if waiting for a specific sender.
            if((TargetedSender != null && TargetedSender.Equals(Event.Sender)) || (TargetedSender == null))
            {
                if (ListenerActionWithNoEvent == null) ListenerAction.Invoke(Event); else ListenerActionWithNoEvent.Invoke();
            }

            //Increment trigger count.
            _timesTriggered++;

            //Check if reached trigger limit.
            if (_timesTriggered >= TriggerCount && TriggerCount != -1)
            {
                ESystem.Remove(this);
                return;
            }
        }
    }
}
