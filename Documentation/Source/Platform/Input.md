# Input

Input is defined as keyboard, mouse, and controller. Other methods of input based on the platform should simulate one of these. There are two ways to receive input in Emotion, whichever you choose input implementations are expected to:

- Release all pressed keys on focus loss.

## Event Driven (New)

The recommended way to check for player input is by attaching a function to one of the platform's input events. These include:

- Emotion.Host.OnKey is called when a key's status changes (Up, Down, Held).
- Emotion.Host.OnTextInput is called when text input is detected - along with the char that was pressed. On some platforms this may be a duplicate of OnKey.
- Emotion.Host.OnMouseKey is called when a key on the mouse (Left, Right, Middle) changes its status (Up, Down). Take note that the KeyStatus enum contains "Held" but it isn't valid for OnMouseKey.
- Emotion.Host.OnMouseScroll is called when the mouse wheel is scrolled. The number received is negative when scrolling down and positive when scrolling up.

Tip: If you return "false" in any of the events above you will stop event propagation/bubbling. The event hierarchy is based on attachment order.

To check the current cursor position query "Emotion.Host.MousePosition". You'll receive a position scaled relative to the render size.

## Immediate Mode (Old)

This is the way input was handled previously in Emotion and has been kept not only for legacy reasons, but also because it is a better fit in some cases. The "InputManager" class is accessible through "Engine.Input" and provides you with a variety of functions which react to input.

Mouse:

- Emotion.Input.IsMouseKeyDown returns true if the mouse key provided was pressed down in this tick.
- Emotion.Input.IsMouseKeyHeld returns true if the mouse key provided is still being held this tick.
- Emotion.Input.IsMouseKeyUp returns true if the mouse key provided was let go in this tick.
- Emotion.Input.GetMouseScroll returns the mouse scroll value since the start of the game, starting at 0.
- Emotion.Input.GetMouseScrollRelative returns the mouse scroll change since last tick.

Keyboard:

Tip: These functions also have overloads where you can provide a string value instead of a key. If the provided string doesn't match any key false will be returned.

- Emotion.Input.IsKeyHeld returns true if the key provided is still being held this tick.
- Emotion.Input.IsKeyDown returns true if the key provided was pressed down in this tick.
- Emotion.Input.IsKeyUp returns true if the key provided was let go in this tick.

Keyboard Other:

- Emotion.Input.GetAllKeysHeld returns a list of all keys currently held.
- Emotion.Input.GetAllKeysDown returns a list of all keys which were pressed down in this tick.

Tip: You are not guaranteed to receive all key events, as it depends on when you check for it.