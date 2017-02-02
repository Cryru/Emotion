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
    /// An event object for SE's custom event system.
    /// </summary>
    public class Event
    {
        #region "Declarations"
        public string Type;
        public object Sender;
        public object Data;
        #endregion

        /// <summary>
        /// Create a new event with the specified parameters.
        /// </summary>
        /// <param name="type">The type of event, in order for a listener to detect the event it needs to be of the same type.</param>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="data">Additional data, usually specified by the type.</param>
        public Event(string type, object sender, object data = null)
        {
            Type = type;
            Sender = sender;
            Data = data;
        }
    }
}