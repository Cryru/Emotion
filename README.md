# Emotion
<img src="EmotionLogo.png" width="128px" />

## What is it?

Emotion is a cross-platform 2D oriented game engine written in C# using .Net Core. Its goal is to have *no native* dependencies and be truly portable (apart from the platform specific code) and ensure compatibility. The core principle is that making games as a programmer should be about coding, and not about drag and drop interfaces, or wrestling with linking libraries and making the same interfaces over and over again. I made this to provide indie developers (and mostly myself) with a foundation which can be extended and adapted to a game's needs.

## Documentation

You can find the documentation in the ["Documentation" folder](/Documentation).

It could be useful to check the tests in Emotion.Tests as examples, the comments in the code,
or the annotations that come with the nuget package (which also exist as comments in the code).

## Requirements:

- OpenGL 3.3 or higher
- Be able to run the Net Core runtime.
	- If older than Windows 10 you'll need the [C++ Redistributable 2015 Update 3](https://www.microsoft.com/en-us/download/details.aspx?id=52685)
- A supported platform.

That's it.

## Platforms

Supported platforms are mostly decided by what .Net Core supports (which is a lot) and whatever is implemented in Emotion.Platform. Refer to the documentation for more information.

Tested platforms are: 
  - MacOS 10.12 Sierra x64 +
  - Debian 9+ x64 +
    - Linux Mint 18 +
    - Ubuntu 17.10 +
  - Windows 7 x64 +

### Configurations Tested On:

- Windows 10 x64
  - Nvidia GeForce 1060
  - Nvidia GeForce 1050
  - Intel UHD Graphics 620
  - Nvidia GeForce 940MX

## Features So Far

- Window creation and input on desktop platforms (Win, Linux, Mac)
  - Fullscreen mode.
  - Pause when focus is lost, lowering resource usage in the background.
  - Software renderer fallback. (Windows only, through Gallium, check platform documentation)
- Mouse and keyboard input.
  - Detect key down, key up, and key held independently.
  - Detect text input.
- SIMD vector and matrix math, through System.Numerics.
- File Support
  - Reading and writing of various BMP formats and PNG files.
	- BMP: 8/16/24/32bit
	- PNG: GreyScale/PalleteIndex/Color, Adam7 Interlacing (color only), All PNG supported bit depths. 
  - Parsing of "CFF" and "TTF" font formats, rasterizing and preparing an atlas of glyphs using an Emotion parser and rasterizer.
	- Optionally the font can be parsed and rasterized using FreeType as a native library. (Requires FreeType compilation symbol and the inluded native library)
    	- Optionally the font can also be rasterized using StbTrueTypeSharp.
- Asset loading and managements
 - Load assets from different sources, and manage their lifecycle.
- Camera system.
- Rendering
  - Drawing of things like lines, rectangles, triangles, and circles.
	- These are all drawn as triangles, no GL_LINES here, monsieur!
  - Super fast rendering of many objects at once (less draw calls) through the magic of mapping buffers, batching, and streaming.
  - Draw order independent texture alpha testing and blending. (Mostly, you'll still need to order semi-transparent objects. This feature is achieved by discarding invisible fragments.)
  - Model matrix stack.
  - Spritesheet based animation.
  - Text.
    - Includes advanced font drawing with control over each individual glyph.
    - Extensible Richtext class featuring auto wrapping, alignment, markup, and more.
  - Cache draw commands to reexecute them faster.
  - Different types of scaling, with pixel art in mind, allowing your game to look the same on all resolutions.
- Shader pipeline
  - Try multiple preprocessors to increase the compatibility of your shader.
  - Specify fallbacks
  - Predefined uniforms based on ShaderToy
- An implementation of A*.
  - With the ability to add a custom heuristics function, and perform other customizations.
- Logging.
  - Runs on another thread as not to interrupt your game.
  - Remote logging to PaperTrail and other services which support the protocol.
  - Easily create your own logger!
- Framerate independent timing, semi-fixed step and free-step based on configuration.
  - Managed delta time and total time access in the shader allow for cool effects.
  - The update loop will always be up to date when a target fps is selected.
  - Multithreaded update loop (on by default)!
- Easy tilemap drawing.
  - Integration with the "Tiled" application.
  - Includes layer opacity, multiple tilesets, animated tiles, and more.
- QuadTree Implementation
- Runtime configuration.
- ImGui Integration through a plugin.
- Framebuffer stack and easy framebuffer (RenderTarget) management.
- Execute C# files as scripts in runtime!

## Building and Using

To use the engine just include the "Emotion" nuget package to your project.
To build just clone and build using Visual Studio 2019 or higher. It shouldn't take more than that.

## Projects Used

.NetCore 3.0
 - System.Numerics (Data Structures)
 - Roslyn (C# Scripting)

TiledSharp
 - Support for .tmx files.

OpenGL.Net (Forked)
 - Used as a wrapper for OpenGL.
 - Based on [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net)

Serilog
 - Used for logging.
 - [Website](https://serilog.net/)

CimGui and CimGuiNet
 - Dev Mode UI
 - Included native libraries for MacOS, Linux, Windows64

## Optional Dependencies (These will probably be removed later down the line)

StbTrueTypeSharp
 - Provided by [StbTrueTypeSharp](https://github.com/zwcloud/StbTruetypeSharp)

Emotion.Standard.TrueType (Found in the Plugins folder)
 - Custom wrapper utilizing .NetCore 3.0, based on [SharpFontStandard](https://github.com/jmazouri/SharpFontStandard/)
 - Included native libraries for MacOS, Linux, Windows64

## Inspired Fully, Or In Parts By:

- Processing
- MonoGame
- Mononoke
- LOVE Framework
- Sparky
- OpenTK
