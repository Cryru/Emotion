# Window

The window is literally the window on Desktop-like platforms, while on others it can be something completely different. It is in charge of the graphics context and the "viewing area". It can be accessed by "Engine.Host.Window".

In general to provide consistant behavior Emotion's windows are expected to (where possible):
- Toggle fullscreen when Alt+Enter is pressed, and change sizes when Ctrl+F1-F9 are pressed in debug mode.
- Will minimize if they lose focus in fullscreen mode.
- Are created in windowed display mode, in the normal state. (Where possible)
- Have the cursor centered on the screen when switching to fullscreen mode.
- Have the window be centered on the screen when switching to windowed mode. (The size of the window should be restored to whatever the player resized it to.)
- Have the window revert to windowed mode if a monitor is disconnected while it is in fullscreen mode.
- Fullscreen mode should be implemented as Fullscreen Window.
- Shouldn't be able to be resized beneath the RenderSize property of the Configurator, where possible.

## Properties

After the window is created it can be modified by changing its properties. 

- Size and Position will modify the window's size and position
- DisplayMode (Windowed, Fullscreen) allow you to set the window's display mode.
- WindowState (Normal, Minimized, Maximized) This shouldn't be explicitly set, but can be useful for information.

## Events

The window also provides events such as:
- OnResize is called when the window's size is changed. This is not called when the window is minimized, but is called when entering fullscreen.