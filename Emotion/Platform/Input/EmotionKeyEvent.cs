#region Using

using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Platform.Input;

public enum KeyListenerType : byte
{
    None = 0, // Dont use!
    System = 1,
    EditorUI = 2,
    Editor = 3,
    EditorCamera = 4,

    UI = 6,
    Game = 7,
    Last
}

public class EmotionKeyEventPair
{
    // Holds an array of all key down states in order to ensure that each listener will
    // always receive an "Up" after they've received a "Down" for a given key.
    public bool[] KeysDown = null!;
    public Func<Key, KeyState, bool> Func = null!;

    public EmotionKeyEventPair()
    {
        // Default constructor - will be called by object pool.
        KeysDown = new bool[(int) Key.Last];
    }

    public EmotionKeyEventPair(bool noKeyRetention)
    {
        // Dont create KeysDown array.
        // Used by the System listener type.
    }
}

public class EmotionKeyEvent
{
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

            // Call rest of events in order.
            var propagate = true;
            for (int l = (int) KeyListenerType.System + 1; l < _listenerArray.Length; l++)
            {
                if (l == _blockingType) continue;

                List<EmotionKeyEventPair>? list = _listenerArray[l];
                if (list == null) continue;

                for (var i = 0; i < list.Count; i++)
                {
                    EmotionKeyEventPair listener = list[i];

                    // We want to propagate Up events regardless of what the higher priority handler
                    // has requested, when the handler considers that key pressed. Basically if you
                    // get a down, you will always get a up.
                    // But you won't get an up if you didn't get a down.
                    if (status == KeyState.Up && !listener.KeysDown[(int) key]) continue;

                    if (status == KeyState.Down || status == KeyState.Up) listener.KeysDown[(int)key] = status == KeyState.Down;
                    bool funcPropagate = listener.Func(key, status);

                    // Stop propagation if the event handler said so.
                    if (!funcPropagate) propagate = false;

                    // If the event is not up, we can stop calling handlers.
                    if (status != KeyState.Up && !propagate) return true;
                }
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
    }
}