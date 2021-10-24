#region Using

using System;
using System.Collections.Generic;
using Emotion.Utility;

#endregion

namespace Emotion.Platform.Input
{
    public class EmotionKeyEventPair
    {
        public bool[] KeysDown = new bool[(int)Key.Last];
        public Func<Key, KeyStatus, bool> Func;
    }

    public class EmotionKeyEvent
    {
        private ObjectPool<EmotionKeyEventPair> _pairPool = new ObjectPool<EmotionKeyEventPair>();
        private readonly List<EmotionKeyEventPair> _listeners = new List<EmotionKeyEventPair>();

        public bool Invoke(Key key, KeyStatus status)
        {
            lock (_listeners)
            {
                var propagate = true;
                for (var i = 0; i < _listeners.Count; i++)
                {
                    EmotionKeyEventPair listener = _listeners[i];

                    // We want to propagate Up events regardless of what the higher priority handler
                    // has requested, when the handler considers that key pressed. Basically if you
                    // get a down, you will always get a up.
                    // But you won't get an up if you didn't get a down.
                    if (status == KeyStatus.Up && !listener.KeysDown[(int)key]) continue;

                    bool funcPropagate = listener.Func(key, status);
                    if (status == KeyStatus.Down || status == KeyStatus.Up) listener.KeysDown[(int)key] = status == KeyStatus.Down;

                    // Stop propagation if the event handler said so.
                    if (!funcPropagate) propagate = false;

                    // If the event is not up, we can stop calling handlers.
                    if (status != KeyStatus.Up && !propagate) return true;
                }
            }

            return false;
        }

        public void AddListener(Func<Key, KeyStatus, bool> func)
        {
            lock (this)
            {
                EmotionKeyEventPair pair = _pairPool.Get();
                pair.Func = func;
                _listeners.Add(pair);
            }
        }

        public void RemoveListener(Func<Key, KeyStatus, bool> func)
        {
            lock (this)
            {
                for (var i = 0; i < _listeners.Count; i++)
                {
                    EmotionKeyEventPair listener = _listeners[i];
                    if (listener.Func != func) continue;

                    // Call all downs as ups, and clear tracker.
                    for (var j = 0; j < listener.KeysDown.Length; j++)
                    {
                        if (!listener.KeysDown[j]) continue;
                        listener.Func((Key)j, KeyStatus.Up);
                        listener.KeysDown[j] = false;
                    }

                    _listeners.RemoveAt(i);
                    _pairPool.Return(listener);
                }
            }
        }
    }
}