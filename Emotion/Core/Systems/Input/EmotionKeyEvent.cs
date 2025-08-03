#nullable enable

namespace Emotion.Core.Systems.Input;

/// <summary>
/// This kind of event is used by the Engine in order to:
/// - Ensure that a key up event will be received for each key that was pressed down for each listener.
/// - Ensure that listeners will be called in priority order (KeyListenerType) and higher priorities (lower numbers)
///     can stop event propagation, except for key ups which will always be propagated to listeners who received a down for that key.
/// 
/// This event is thread safe.
/// </summary>
public class EmotionKeyEvent
{
    private class EmotionKeyEventPair
    {
        // Holds an array of all key down states in order to ensure that each listener will
        // always receive an "Up" after they've received a "Down" for a given key.
        public bool[] KeysDown = null!;
        public Func<Key, KeyState, bool> Func = null!;

        public EmotionKeyEventPair()
        {
            // Default constructor - will be called by object pool.
            KeysDown = new bool[(int)Key.Last];
        }

        public EmotionKeyEventPair(bool noKeyRetention)
        {
            // Dont create KeysDown array.
            // Used by the System listener type.
        }
    }

    private ObjectPool<EmotionKeyEventPair> _pairPool = new ObjectPool<EmotionKeyEventPair>();
    private readonly List<EmotionKeyEventPair>?[] _listenerArray = new List<EmotionKeyEventPair>?[(int) KeyListenerType.Last];

    public bool Invoke(Key key, KeyState status)
    {
        lock (this)
        {
            // System events first. No key retention here.
            List<EmotionKeyEventPair>? systemList = _listenerArray[(int) KeyListenerType.System];
            if (systemList != null)
                for (var i = 0; i < systemList.Count; i++)
                {
                    EmotionKeyEventPair listener = systemList[i];
                    if (!listener.Func(key, status)) return true;
                }

            // Mouse wheel events treat up and down differently.
            if (key != Key.MouseWheel)
                return InvokeKeyEvents(key, status);
            else
                return InvokeKeyEventsMouseWheel(status);
        }
    }

    private bool InvokeKeyEvents(Key key, KeyState status)
    {
        // Call rest of events in order.
        bool isUp = status == KeyState.Up;
        for (int l = (int)KeyListenerType.System + 1; l < _listenerArray.Length; l++)
        {
            if (l == _blockingType) continue;

            List<EmotionKeyEventPair>? list = _listenerArray[l];
            if (list == null) continue;

            for (var i = 0; i < list.Count; i++)
            {
                EmotionKeyEventPair listener = list[i];
                bool listenerDown = listener.KeysDown[(int)key];

                // You won't get an up if you didn't get a down.
                if (isUp && !listenerDown) continue;

                // Set new state and call listener.
                listener.KeysDown[(int)key] = !isUp;
                bool allowPropagation = listener.Func(key, status);

                // Stop propagation if the event handler said so, and the event isn't an up.
                // This is because we want to unpress those who received a down.
                if (!allowPropagation && !isUp)
                    return true;
            }
        }
        return false;
    }

    private bool InvokeKeyEventsMouseWheel(KeyState status)
    {
        Key key = Key.MouseWheel;

        for (int l = (int)KeyListenerType.System + 1; l < _listenerArray.Length; l++)
        {
            if (l == _blockingType) continue;

            List<EmotionKeyEventPair>? list = _listenerArray[l];
            if (list == null) continue;

            for (var i = 0; i < list.Count; i++)
            {
                EmotionKeyEventPair listener = list[i];

                // if the listener said not to propagate - don't.
                bool allowPropagation = listener.Func(key, status);
                if (!allowPropagation)
                    return true;
            }
        }

        return false;
    }

    public void AddListener(Func<Key, KeyState, bool> func, KeyListenerType listenerType = KeyListenerType.UI)
    {
        lock (this)
        {
            EmotionKeyEventPair pair;
            if (listenerType == KeyListenerType.System)
            {
                pair = new EmotionKeyEventPair(true)
                {
                    Func = func
                };
            }
            else
            {
                pair = _pairPool.Get();
                pair.Func = func;
            }

            List<EmotionKeyEventPair>? list = _listenerArray[(int) listenerType];
            if (list == null)
            {
                list = new List<EmotionKeyEventPair>();
                _listenerArray[(int) listenerType] = list;
            }

            list.Add(pair);
        }
    }

    public void RemoveListener(Func<Key, KeyState, bool> func)
    {
        lock (this)
        {
            for (var l = 1; l < _listenerArray.Length; l++)
            {
                List<EmotionKeyEventPair>? list = _listenerArray[l];
                if (list == null) continue;

                for (var i = 0; i < list.Count; i++)
                {
                    EmotionKeyEventPair listener = list[i];
                    if (listener.Func != func) continue;

                    // Call all downs as ups, and clear tracker.
                    for (var j = 0; j < listener.KeysDown.Length; j++)
                    {
                        if (!listener.KeysDown[j]) continue;
                        listener.KeysDown[j] = false;
                        listener.Func((Key) j, KeyState.Up);
                    }

                    list.RemoveAt(i);
                    _pairPool.Return(listener);
                    return;
                }
            }
        }
    }

    private int _blockingType;

    // Used by the editor to block game inputs while its open.
    public void BlockListenersOfType(KeyListenerType? typ)
    {
        if (typ == null)
            _blockingType = -1;
        else
            _blockingType = (int)typ;

        for (var l = 1; l < _listenerArray.Length; l++)
        {
            if (l != _blockingType) continue; // Should it block up and down from it?

            List<EmotionKeyEventPair>? list = _listenerArray[l];
            if (list == null) continue;

            for (var i = 0; i < list.Count; i++)
            {
                EmotionKeyEventPair listener = list[i];

                // Call all downs as ups
                for (var j = 0; j < listener.KeysDown.Length; j++)
                {
                    if (!listener.KeysDown[j]) continue;
                    listener.KeysDown[j] = false;
                    listener.Func((Key)j, KeyState.Up);
                }
            }
        }
    }
}