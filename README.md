# Emotion
<img src="EmotionLogo.png" width="128px" />

[![Build status](https://ci.appveyor.com/api/projects/status/qur90gc2wdhmd5ff/branch/master?svg=true)](https://ci.appveyor.com/project/Cryru/emotion/branch/master)

Debug Package: [![nuget debug](https://img.shields.io/nuget/v/AdfectusDebug.svg)](https://www.nuget.org/packages/AdfectusDebug)

Release Package: [![nuget release](https://img.shields.io/nuget/v/Adfectus.svg)](https://www.nuget.org/packages/Adfectus)

## What is it?

Adfectus is a cross-platform game engine written in C# using .NetCore, with the intent of abstracting the lower levels of game programming, without taking away control from the developer. The core principle is that making games as a programmer should be about coding, and not about drag and drop interfaces, or wrestling with linking libraries and making the same interfaces over and over again. I made this to provide indie developers (and mostly myself) with a foundation which can be extended and adapted to a game's needs.

## Platforms

Supported platforms are: 
  - MacOS 10.12 Sierra x64 +
  - Debian 9+ x64 +
    - Linux Mint 18 +
    - Ubuntu 17.10 +
  - Windows 7 x64/x86 +

For additional supported platforms, lookup .NetCore 3.0 support.

### Requirements:

- At least OpenGL 3.2 Support
  - Note: Shader support for 2.0 is included, but I wouldn't list the requirements that low.
  - Lower versions need testing.
- If older than Windows 10
  - [C++ Redistributable 2015 Update 3](https://www.microsoft.com/en-us/download/details.aspx?id=52685)

### Platforms Tested On:

- Windows 10 x64
  - Intel HD Graphics 620
  - Nvidia 940MX
  - AMD R9 200
  - Nvidia 970M
  - Nvidia 1060
- Windows 7 x64
  - Nvidia 970M
- Linux Mint 19.1
  - Nvidia 970M NV124 Nouveau Driver
  - Nvidia 960M Unknown Driver
- MacOSX 10.13 High Sierra
  - MacBook Air (Intel 4000)

## Features So Far

- Window creation and configuration.
- SIMD vector and matrix math, through System.Numerics.
- Mouse and keyboard input.
  - Detect key down, key up, and key held independently.
  - Detect text input.
- Asset loading and management.
  - Textures: All FreeImage supported formats. ex. BMP/PNG/JPEG/GIF
  - Fonts: All FreeType supported formats. ex. TTF/OTF
  - Sounds: RIFF WAV
  - Text Files
  - Binaries
  - Custom asset sources.
- Camera system.
  - Default cameras include one which follows the mouse and one which follows a target transform. 
  - Easy to create your own.
- Rendering
  - Drawing of things like lines, rectangles, triangles, and circles.
    - These are all drawn as triangles, no GL_LINES here, monsieur!
  - Super fast rendering of many objects at once (less draw calls) through the magic of MapBuffers, batching, and streaming.
  - Model matrix stack.
  - Textures with alpha testing and blending.
   - Draw order independent.
  - Spritesheet based animation.
  - Text.
    - Includes advanced font drawing with control over each individual glyph.
    - Extensible Richtext class featuring auto wrapping, alignment, markup, and more.
  - Draw arbitrary vertices, the wrapper gives you control over your own model matrix and buffers.
  - Easy shader creation with fallbacks and helper uniforms.
- Sound engine with fading effects.
  - Play on multiple layers with individual control over them.
  - Queue sounds one after another.
  - Seemless looping through streaming buffers.
- UI system.
  - Customize base controls through inheritance or use them straight away.
  - Layouting and anchoring.
  - Parenting system.
  - Comes with basic controls such as labels, buttons, scrollbar, and more.
- Easy tilemap drawing.
  - Integration with the Tiled application.
  - Includes layer opacity, multiple tilesets, animated tiles, and more.
- An implementation of A*.
  - With the ability to add a custom heuristics function, and perform other customizations.
- A Javascript scripting engine.
- Logging.
  - Runs on another thread as not to interrupt your game.
  - Remote logging to PaperTrail and other services which support the protocol.
- Graphical debugging and a command console.
  - Debug the camera, or UI through the engine.
  - Execute scripting commands at real time.
- Scenes.
- Framerate independent timing, semi-fixed step and free-step based on configuration.
  - Managed delta time and total time access in the shader for cool effects.
  - The update loop will always be up to date when a target fps is selected.
- Lots of configuration through settings and flags.
- Steam integration.
- ImGui integration.

## Documentation

The documentation is still a work in progress.

* [Getting Started](./Documents/Start.md)
* Modules
  * [AssetLoader](./Documents/AssetLoader.md)
    * [Shaders](./Documents/Shaders.md)
  * [SoundManager](./Documents/SoundManager.md)
* [Configuration](./Documents/Configuration.md)
* [Special Thanks](./Documents/Thanks.md)

For more information you can refer to the automated tests here - [Adfectus.Tests](Adfectus.Tests).

## Building and Using

Just clone and build. Everything is included, it shouldn't take more than that.
You can also use the Adfectus and AdfectusDebug packages on Nuget.

## Projects Used

.NetCore 3.0

Adfectus.Glfw3
 - Used for input management, window creation, and OpenGL context creation.
 - Custom wrapper utilizing .NetCore 3.0
 - Included native libraries for MacOS, Linux, Windows64, and Windows86
 - Included vcruntime140 dependency on Windowsx64

Adfectus.OpenGLNet
 - Used for drawing.
 - Custom wrapper utilizing .NetCore 3.0, based on [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net)

Adfectus.FreeImage
 - Used for loading and converting image files into drawable textures.
 - Custom wrapper utilizing .NetCore 3.0, based on [FreeImage.Standard](https://github.com/LordBenjamin/FreeImage.Standard)
 - Included native libraries for MacOS, Linux, Windows64, and Windows86
 - Included vcomp120 dependency on Windowsx64
 - Included libpng dependency on MacOS

Adfectus.FreeType
 - Used for loading of fonts and generation of font atlases.
 - Custom wrapper utilizing .NetCore 3.0, based on [SharpFontStandard](https://github.com/jmazouri/SharpFontStandard/)
 - Included native libraries for MacOS, Linux, Windows64, and Windows86
 - Included msvcr100 dependency on Windowsx64

Adfectus.OpenAL
 - Used for playing and controlling sound.
 - Custom wrapper utilizing .NetCore 3.0
 - Included native libraries for MacOS, Linux, Windows64, and Windows86
 - Included sndio dependency on MacOS

TiledSharp
 - Used for loading .tmx files as tile maps
 - Provided by [TiledSharp](https://github.com/marshallward/TiledSharp)

Jint
 - Javascript script engine used for providing a scripting interface if one is needed.
 - Provided by [Jint](https://github.com/sebastienros/jint)

Serilog
 - Used for logging.
 - [Website](https://serilog.net/)

## Plugins

- https://www.nuget.org/packages/Adfectus.ImGuiNet/
- https://www.nuget.org/packages/Adfectus.Steam/

## Inspired By

- Processing
- MonoGame
- LOVE Framework
- Sparky
