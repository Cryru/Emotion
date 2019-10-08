#region Using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Emotion.Utility
{
    public class EmotionEvent<T, T2>
    {
        private Dictionary<int, Func<T, T2, bool>> _listeners = new Dictionary<int, Func<T, T2, bool>>();
        private int _nextId;

        /// <summary>
        /// Invokes all listeners, until one returns false.
        /// </summary>
        /// <param name="t">The event parameter.</param>
        /// <param name="t2">The event's second parameter.</param>
        public void Invoke(T t, T2 t2)
        {
            lock (_listeners)
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (KeyValuePair<int, Func<T, T2, bool>> listener in _listeners)
                {
                    if (!listener.Value(t, t2)) break;
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
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Action func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, (_, __) =>
                {
                    func();
                    return true;
                });
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Action<T> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, (t, _) =>
                {
                    func(t);
                    return true;
                });
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Func<bool> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, (_, __) => func());
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Func<T, bool> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, (t, __) => func(t));
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Func<T, T2, bool> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, func);
            }

            return _nextId;
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        /// <param name="id">The listening function's id - returned when it was added.</param>
        public void RemoveListener(int id)
        {
            lock (this)
            {
                _listeners.Remove(id);
            }
        }
    }

    public class EmotionEvent<T>
    {
        private Dictionary<int, Func<T, bool>> _listeners = new Dictionary<int, Func<T, bool>>();
        private int _nextId;

        /// <summary>
        /// Invokes all listeners, until one returns false.
        /// </summary>
        /// <param name="t">The event parameter.</param>
        public void Invoke(T t)
        {
            lock (_listeners)
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (KeyValuePair<int, Func<T, bool>> listener in _listeners)
                {
                    if (!listener.Value(t)) break;
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
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Action func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, _ =>
                {
                    func();
                    return true;
                });
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Action<T> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, t =>
                {
                    func(t);
                    return true;
                });
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Func<bool> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, _ => func());
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Func<T, bool> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, func);
            }

            return _nextId;
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        /// <param name="id">The listening function's id - returned when it was added.</param>
        public void RemoveListener(int id)
        {
            lock (this)
            {
                _listeners.Remove(id);
            }
        }
    }

    public class EmotionEvent
    {
        private Dictionary<int, Func<bool>> _listeners = new Dictionary<int, Func<bool>>();
        private int _nextId;

        /// <summary>
        /// Invokes all listeners, until one returns false.
        /// </summary>
        public void Invoke()
        {
            lock (_listeners)
            {
                _listeners.Where(listeners => !listeners.Value());
            }
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Action func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, () =>
                {
                    func();
                    return true;
                });
            }

            return _nextId;
        }

        /// <summary>
        /// Add a new listener to the event.
        /// </summary>
        /// <param name="func">
        /// The function to be invoked when the event is triggered.
        /// If the function returns false the propagation of the event is stopped.
        /// </param>
        /// <returns>The event listener's id - can be used to remove it.</returns>
        public int AddListener(Func<bool> func)
        {
            lock (this)
            {
                _listeners.Add(_nextId++, func);
            }

            return _nextId;
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        /// <param name="id">The listening function's id - returned when it was added.</param>
        public void RemoveListener(int id)
        {
            lock (this)
            {
                _listeners.Remove(id);
            }
        }
    }
}