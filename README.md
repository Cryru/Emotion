# Emotion
<img src="EmotionLogo.png" width="128px" />

![CI-Windows](https://github.com/Cryru/Emotion/workflows/CI-Windows/badge.svg?branch=master)

## What is it?

Emotion is a cross-platform 2D oriented game engine written in C#. Its goal is to have *no native* dependencies and be truly portable (apart from the platform specific code of course). The core principle is that making games as a programmer should be about coding. Not about drag and drop interfaces and navigating menus, nor wrestling with linking libraries and making the same interfaces over and over again. I made this to provide indie developers (and mostly myself) with a foundation which can be extended and adapted to a game's needs.

## Documentation

Documentation is something which I really want to get around to doing. The existing one in the ["Documentation" folder](/Documentation) is not up to date and is a work in progress.

I would recommend checking out the tests in the "Tests" project as examples, and the comments in the code.

## Requirements for Developers and Players:

- OpenGL 3.3 or higher (might work on 3.0, but needs testing)
	- If your GPU doesn't support it, you might still be able to run it using [Mesa](https://github.com/pal1000/mesa-dist-win/releases). Place the opengl32.dll file in a "mesa" folder next to the exe.
- Be able to run the Net Core runtime.
	- If older than Windows 10 you'll need the [C++ Redistributable 2015 Update 3](https://www.microsoft.com/en-us/download/details.aspx?id=52685)
- A supported platform.

That's it.

## Platforms

Supported platforms are those implemented in Emotion.Platform. You are free to implement one yourself. Refer to the code for more information.

The goal is for the following platforms to be supported: 
  - MacOS 10.12 Sierra x64 +
  - Debian 9+ x64 +
    - Linux Mint 18 +
    - Ubuntu 17.10 +
  - Windows 7 x64 +

Currently supported:
  - Windows 7 x64 +

### Configurations Tested On:

- Windows 10 x64
  - Nvidia GeForce 1070
  - Nvidia GeForce 1060
  - Nvidia GeForce 1050
  - Intel UHD Graphics 620
  - Nvidia GeForce 940MX
  - AMD Radeon HD 5700 Series
- Windows 7 x86
  - Intel 4 Series Express (using Mesa)

## Features So Far

- Custom platform code.
  - Window creation, keyboard, mouse, and text input.
  - Borderless fullscreen, and windowed support.
  - Pause when focus is lost, lowering resource usage in the background.
  - Software renderer fallback. Windows only
- File Support
  - Reading and writing of various BMP formats and PNG files.
	- BMP: 8/16/24/32bit
	- PNG: GreyScale/PalleteIndex/Color, Adam7 Interlacing (color only), All PNG supported bit depths. 
  - Parsing of "CFF" and "TTF" font formats, rasterizing and preparing an atlas of glyphs using an Emotion parser and rasterizer.
	- Optionally the font can be parsed and rasterized using FreeType as a native library. (Requires FreeType compilation symbol and the inluded native library)
    	- Optionally the font can also be rasterized using StbTrueTypeSharp.
  - "WAV" files for audio.
- Asset loading and managements
- Layer-based audio system with playlists and a custom resampler.
- Camera system, and a model matrix stack.
- Draw order independent texture alpha testing and blending. You'll still need to order semi-transparent objects.
- Super fast rendering of many objects at once (less draw calls) through the magic of mapping buffers, batching, and streaming.
- Unsynchronized rendering.
- Easy drawing of vertices, and 2D primitives like lines, rectangles, triangles, and circles.
- Spritesheet based animation in either a grid or freeform format.
  - The animation editor allows you to easily detect frames in a spritesheet and name animations.
- Custom text rendering with atlases created at runtime.
  - Extensible Richtext and TextLayouter classes allowing control over each glyph, and featuring auto wrapping, alignment, markup, and more.
- Different types of scaling, with pixel art in mind, allowing your game to look good on all resolutions.
- Framebuffer (RenderTarget) stack allowing for easy rendering to textures and side buffers.
- Shader Pipeline
  - Try multiple preprocessors to increase the compatibility of your shader.
  - Specify fallbacks!
  - Predefined uniforms based on ShaderToy allow for easily making cool effects.
- An implementation of A*, with the ability to add a custom heuristics function, and perform other customizations.
- QuadTree Implementation
- Logging, easily extendible and modifiable.
  - Runs on another thread as not to interrupt your game.
  - Remote logging to PaperTrail and other services which support the protocol.
- Framerate independent timing with a semi-fixed step. Automatically switches to a faster mode and slower mode depending on runtime performance.
- Easy tilemap drawing and handling, including layer opacity, multiple tilesets, animated tiles, and more
  - Integration with the "Tiled" application.
- ImGui Integration through a plugin.
- Custom fast XML serializer/deserializer with support for derived types, dictionaries, and others. Compliant with the .Net XML one.

and many more!

## Building and Using

Clone and build using Visual Studio 2019 or higher. Then reference the "Emotion" project in your project. It shouldn't take more than that.

## Dependencies

.NetCore 3.1 and .Net Standard 2.1 [MIT]
 - System.Numerics (Data Structures)

TiledSharp (https://github.com/marshallward/TiledSharp) [Apache 2.0]
 - Support for .tmx files.

Serilog (https://github.com/serilog/serilog) [Apache 2.0]
 - Used for logging.

## Optional Dependencies

Emotion.Plugins.CSharpScripting
 - Uses Roslyn
 - Allows you to easily write scripts in C#

Emotion.Plugins.ImGuiNet
 - Uses CimGui and CimGuiNet
 - Dev Mode UI
 - Included native libraries for MacOS_x64, Linux_x64, Windows_x64

Emotion.Standard.FreeType (Found in the Plugins folder)
 - Custom wrapper utilizing .NetCore 3.0, based on [SharpFontStandard](https://github.com/jmazouri/SharpFontStandard/) which itself is based on [SharpFont](https://github.com/Robmaister/SharpFont)
 - Included native libraries for MacOS_x64, Linux_x64, Windows_x64
 - Requires compiling Emotion with the "FreeType" symbol.

StbTrueTypeSharp (https://github.com/zwcloud/StbTruetypeSharp) [GPL 3.0]
 - Provided by [StbTrueTypeSharp](https://github.com/zwcloud/StbTruetypeSharp)
 - Used by unit tests.

## Projects Referenced

WinApi (https://github.com/prasannavl/WinApi) [Apache]
 - Windows API Interop Headers
 - Forked

OpenGL.Net (https://github.com/luca-piccioni/OpenGL.Net) [MIT]
 - Used as a wrapper for OpenGL.
 - Heavily forked, stripped down, and modified.

OpenType.JS (https://opentype.js.org/)
 - Font parsing reference.

StbTrueType (https://github.com/nothings/stb/blob/master/stb_truetype.h)
 - Font rendering reference.

Nine.Imagine (https://github.com/yufeih/Nine.Imaging)
 - Image parsing reference.

ImageSharp (https://github.com/SixLabors/ImageSharp)
 - Quirky image formats reference.

OpenAL-Soft (https://github.com/kcat/openal-soft/)
 - Audio code reference

NAudio (https://github.com/naudio/NAudio)
 - Audio code reference

Audacity (https://github.com/audacity)
 - Audio code reference

## Inspired Fully, Or In Parts By:

- Processing
- MonoGame
- Mononoke
- LOVE Framework
- Sparky
- OpenTK
