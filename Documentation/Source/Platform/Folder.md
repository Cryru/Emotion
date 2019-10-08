# Platform

Platform specific code is located in [Emotion/Platform]([CodeRoot]Platform) and is covered by tests in Emotion.Test for basic functionality. Each platform inherits [PlatformBase]([CodeRoot]Platform/Implementation/PlatformBase) and is in charge of creating a window (or the platform equivalent), OpenGL/graphics context, audio context, handling input, and everything else which is platform specific. This is one of the few places where Emotion is forced to interface with native code.

Some platforms may force specific loop configurations (such as single threaded loops), and some may ignore some settings. A platform is automatically created by the Engine when the "Setup" function is called, and the settings described in the Configurator are passed to the "Loader". Once created it can be accessed with "Engine.Host"

The following settings can be provided to platform creation:

- Major and Minor OpenGL version. On platforms where certain versions are not supported (like MacOS) this is automatically handled by the default value - "1.0".
- ForwardCompatible and Debug context. Once again, this is automatically handled (and even forced) on specific platforms.
- OpenGL Profile (Core, Compat, Any). This too is automatically handled where needed, but the option is there.
- Title. The title of the window if the platform can display it.
- Width/Height. The size of the window. Generally you don't want to configure this as the player is free to resize it and toggle fullscreen. Some platforms will ignore this.
- Min/Max Width and Height. The minimum size is always set to whatever your render size is.

## Updating

The platform is automatically updated in the render loop by the engine. In general platforms are expected to sleep if the window is not focused, and the update function should return whether the window is considered "alive" so the engine can close if needed.