# Microsoft Windows

The Windows platform is automatically used when Windows XP or higher is detected. It uses the Win32 API to create a window and handle its events. Additionally it uses gdi and wgl to create the OpenGL context.

## Graphics Fallback

Should wgl context creation fail, if the "OSMesa" library is present it will be used instead as a fallback to increase compatibility. To use this fallback download a release package from [here](https://github.com/pal1000/mesa-dist-win) and put the appropriate (x64/x86) opengl32 file in a new directory called "mesa" located in the same directory as your game.

## References

- [Win32 Platform Folder]([CodeRoot]Platform/Implementation/Win32)