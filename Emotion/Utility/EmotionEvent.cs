#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Custom event implementation which handles propagation.
    /// </summary>
    public class EmotionEvent<T, T2>
    {
        private readonly List<Func<T, T2, bool>> _listeners = new List<Func<T, T2, bool>>();

        /// <summary>
        /// Invokes all listeners, until one returns false.
        /// </summary>
        /// <param name="t">The event parameter.</param>
        /// <param name="t2">The event's second parameter.</param>
        public void Invoke(T t, T2 t2)
        {
            lock (_listeners)
            {
                for (var i = 0; i < _listeners.Count; i++)
                {
                    if (!_listeners[i](t, t2)) break;
                }
            }
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        public void AddListener(Func<T, T2, bool> func)
        {
            lock (this)
            {
                _listeners.Add(func);
            }
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        public void RemoveListener(Func<T, T2, bool> func)
        {
            lock (this)
            {
                _listeners.Remove(func);
            }
        }
    }

    public class EmotionEvent<T>
    {
        private readonly List<Func<T, bool>> _listeners = new List<Func<T, bool>>();

        /// <summary>
        /// Invokes all listeners, until one returns false.
        /// </summary>
        /// <param name="t">The event parameter.</param>
        public void Invoke(T t)
        {
            lock (_listeners)
            {
                for (var i = 0; i < _listeners.Count; i++)
                {
                    if (!_listeners[i](t)) break;
                }
            }
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        public void AddListener(Func<T, bool> func)
        {
            lock (this)
            {
                _listeners.Add(func);
            }
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        public void RemoveListener(Func<T, bool> func)
        {
            lock (this)
            {
                _listeners.Remove(func);
            }
        }
    }

    public class EmotionEvent
    {
        private readonly List<Func<bool>> _listeners = new List<Func<bool>>();

        /// <summary>
        /// Invokes all listeners, until one returns false.
        /// </summary>
        public void Invoke()
        {
            lock (_listeners)
            {
                for (var i = 0; i < _listeners.Count; i++)
                {
                    if (!_listeners[i]()) break;
                }
            }
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        public void AddListener(Func<bool> func)
        {
            lock (this)
            {
                _listeners.Add(func);
            }
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        public void RemoveListener(Func<bool> func)
        {
            lock (this)
            {
                _listeners.Remove(func);
            }
        }
    }
}