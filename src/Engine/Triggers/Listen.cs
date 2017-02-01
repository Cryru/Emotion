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
    /// A listener object for SE's custom trigger system.
    /// </summary>
    public class Listen
    {
        #region "Variables"
        #region "Public"
        /// <summary>
        /// The type of trigger to listen for.
        /// </summary>
        public string Type;
        /// <summary>
        /// The action to execute when the trigger is triggered.
        /// </summary>
        public Action<Trigger> ListenerAction;
        /// <summary>
        /// The sender the trigger should be from.
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
        /// Creates a new trigger listener object, which will execute an action if a trigger is triggered.
        /// </summary>
        /// <param name="type">The type of trigger to listen for.</param>
        /// <param name="listenerAction">The action to execute when the trigger is triggered.</param>
        /// <param name="targetedSender">The sender the trigger should be from.</param>
        /// <param name="triggerCount">The number of times the listener should trigger.</param>
        public Listen(string type, Action<Trigger> listenerAction, object targetedSender = null, int triggerCount = -1)
        {
            Type = type;
            ListenerAction = listenerAction;
            TargetedSender = targetedSender;
            TriggerCount = triggerCount;
        }

        /// <summary>
        /// Triggers the listener using the specified trigger.
        /// </summary>
        /// <param name="Trigger"></param>
        public void Invoke(Trigger Trigger)
        {
            //Check if reached trigger limit.
            if (_timesTriggered >= TriggerCount && TriggerCount != -1) return;

            //Check if waiting for a specific sender.
            if(TargetedSender != null && TargetedSender == Trigger.Sender)
            {
                ListenerAction.Invoke(Trigger);
            }
            else if(TargetedSender == null && Type == Trigger.Type)
            {
                ListenerAction.Invoke(Trigger);
            }

            //Increment trigger count.
            _timesTriggered++;
        }
    }
}
