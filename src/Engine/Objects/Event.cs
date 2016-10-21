using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Internal
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // An event object for trigerring events when an event occurs               //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Event<T>
    {
        #region "Declarations"
        //A list of events to be invoked when the event is triggered that will return the object that triggered it.
        private List<Action<T>> hookedMethods_Arg = new List<Action<T>>();
        //A list of events to be invoked when the event is triggered that will not return anything.
        private List<Action> hookedMethods_NoArg = new List<Action>();
        #endregion

        /// <summary>
        /// Adds a method to be invoked when the event is triggered.
        /// This method can also contain a single parameter that will return the object that called it.
        /// </summary>
        /// <param name="m">The method to be invoked.</param>
        public void Add(Action m)
        {
            hookedMethods_NoArg.Add(m);
        }
        /// <summary>
        /// Adds a method to be invoked when the event is triggered.
        /// This method can be without a parameter
        /// </summary>  
        /// <param name="m">The method to be invoked.</param>
        public void Add(Action<T> m)
        {
            hookedMethods_Arg.Add(m);
        }
        /// <summary>
        /// Removes a method from the list of methods to be triggered.
        /// </summary>
        /// <param name="m">The method to be removed.</param>
        public void Remove(Action m)
        {
            if(hookedMethods_NoArg.IndexOf(m) != -1) hookedMethods_NoArg.Remove(m);
        }
        /// <summary>
        /// Removes a method from the list of methods to be triggered.
        /// </summary>
        /// <param name="m">The method to be removed.</param>
        public void Remove(Action<T> m)
        {
            if (hookedMethods_Arg.IndexOf(m) != -1) hookedMethods_Arg.Remove(m);
        }
        /// <summary>
        /// Trigger the event.
        /// </summary>
        /// <param name="obj">The object that triggered the event.</param>
        public void Trigger(T obj)
        {
            //Trigger events that are hooked with arguments first.
            for (int i = 0; i < hookedMethods_Arg.Count; i++)
            {
                hookedMethods_Arg[i].Invoke(obj);
            }
            //Trigger events that are hooked without arguments.
            for (int i = 0; i < hookedMethods_NoArg.Count; i++)
            {
                hookedMethods_NoArg[i].Invoke();
            }
        }
    }
}
