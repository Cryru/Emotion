#nullable enable

namespace Emotion.Core.Systems.Input;

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
