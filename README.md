# Emotion
<img src="EmotionLogo.png" width="128px" />

![CI-Windows](https://github.com/Cryru/Emotion/workflows/CI-Windows/badge.svg?branch=master)
[![Gitter](https://badges.gitter.im/EmotionGameEngine/community.svg)](https://gitter.im/EmotionGameEngine/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## What is it?

Emotion is a cross-platform 2D oriented game engine written in C#. Its goal is to have *no native* dependencies and be truly portable (apart from the platform specific code of course). The core principle is that making games as a programmer should be about coding. Not about drag and drop interfaces and navigating menus, nor wrestling with linking libraries and making the same interfaces over and over again. I made this to provide indie developers (and mostly myself) with a foundation which can be extended and adapted to a game's needs.

## Documentation

Documentation is something which I really want to get around to doing. The existing one in the ["Documentation" folder](/Documentation) is not up to date and is a work in progress.

I would recommend checking out the tests in the "Tests" project as examples, and the comments in the code.

## Requirements for Developers and Players:

- OpenGL 3.0 or higher
  - Or DirectX 11 if ANGLE is enabled
  - Or a multi-core CPU if the Mesa software renderer is enabled
  - WebGL 2.0 on Web
- Be able to run the Net 5 runtime.
	- If older than Windows 10 you'll need the [C++ Redistributable 2015 Update 3](https://www.microsoft.com/en-us/download/details.aspx?id=52685)
	- WASM support on Web
- A supported platform.

That's it.

## Platforms

Supported platforms are those implemented in Emotion.Platform. You are free to implement one yourself. Refer to the code for more information.

The goal is for the following platforms to be supported: 
  - Debian 9+ x64 +
    - Linux Mint 18 +
    - Ubuntu 17.10 +
  - Chromium 70+

Currently supported:
  - Windows 7+ x64 & x86
  - MacOSX 10.13 x64+ (Compile with "GLFW;OpenAL")

### Configurations Tested On:

- Windows 10 x64
  - Nvidia GeForce 1070
  - Nvidia GeForce 1060
  - Nvidia GeForce 1050
  - Intel UHD Graphics 620
  - Nvidia GeForce 940MX
  - AMD Radeon HD 5700 Series
  - GPU-less (Microsoft Basic Render Driver)
- Windows 7 x86
  - Intel 4 Series Express (using Mesa)
- MacOSX 10.13 x64 High Sierra
  - MacBook Air (Intel 4000)
- Chromium 86 (Not feature complete)

## Features So Far

- Custom platform code.
  - Window creation, keyboard, mouse, and text input.
  - Borderless fullscreen, and windowed support. (terms and conditions may apply)
  - Pause when focus is lost, lowering resource usage in the background.
  - Google ANGLE support
  - Software renderer (llvmpipe) fallback on Windows
  - GLFW support
- File Support
  - Reading and writing of various BMP formats and PNG files.
	- BMP: 8/16/24/32bit
	- PNG: All png formats and bit depths are supported. Tested against the PNGSuite.
  - Parsing and rasterizing of "CFF" and "TTF" font formats using a custom parser and rasterizer.
	- Optionally the font can be parsed and rasterized using FreeType as a native library. (Requires FreeType compilation symbol and the included native library)
	- Optionally the font can also be rasterized using StbTrueTypeSharp.
  - "WAV" files for audio.
- Asset loading and management, virtual file system.
  - Reading and writing of files - for custom editors.
  - Packing files into binary blobs for obfuscation, easy transport, and potentially compression.
  - Keeps track of which assets are loaded.
- Layer-based audio system with playlists and a high quality sinc sample-rate converter.
  - OpenAL support
- Extensible camera system.
- Super fast rendering of many objects at once (less draw calls) through the magic of mapping buffers, batching, and streaming.
  - Unsynchronized rendering
  - Sensible defaults
  - Easy drawing of any triangle list, and 2D primitives like lines, rectangles, triangles, and ellipses (or circles).
- Spritesheet based animation in either a grid or freeform format.
  - The animation editor allows you to easily detect frames in a spritesheet and visually create animation controllers.
  - The animation controller keeps a set of animations and handles switching between them.
- Custom text rendering with atlases created at runtime.
  - Extensible RichText and TextLayouter classes allowing control over each glyph, and featuring auto wrapping, alignment, markup, and more.
- Automatic scaling, making your game look reasonably the same on all resolutions.
  - Integer scaling mode for pixel art games.
- Shader Pipeline
  - Automatically tries multiple preprocessors to increase the compatibility of your shader.
  - Specify custom fallbacks!
  - Predefined uniforms based on ShaderToy allow for easily making cool effects.
  - Easily switch between backends such as ANGLE and Mesa3D to check your shader compatibility.
- Various data structures and algorithms implemented.
  - A* with custom heuristic support
  - Generic QuadTree
  - 2D binning
  - Sprite stacking - generate voxel models from a spritesheet!
- Logging, easily extendible and modifiable.
  - Runs on another thread and generates log files.
  - Remote logging to PaperTrail and other services which support UDP logging.
- Framerate independent timing with a configurable semi-fixed step.
- Easy tilemap drawing and handling, animated tiles, object handling and lookup, and more.
  - .TMX support and integration with [Tiled](https://www.mapeditor.org/)
- Custom fast XML serializer/deserializer with support for derived types, dictionaries, and others.
  - Compliant with .Net System.Text.XML
- Basic scene system, where new scenes load in a new thread while a loading screen scene plays.

and many more!

## Building and Using

If you want to use all of Emotion's features such as, the testing library (Emotion.Test) to create unit/intergration tests for your game, the tools library (Emotion.Tools) to easily create developer tools, or any of the plugins, you should clone the repo and build using Visual Studio 2019 or higher. Then reference the "Emotion" project in your project. It shouldn't take more than that.

If you just want to write some code or take it for a spin you can use the Nuget package - https://www.nuget.org/packages/Emotion
The package includes a precompiled debug version of Emotion, but doesn't include any of the native libraries. You don't really need those for most use cases, but you can download them seperately from the repo at [Emotion/AssetsNativeLibs](https://github.com/Cryru/Emotion/tree/master/Emotion/AssetsNativeLibs).

## Projects Used

This includes dependencies and projects which were used for research references.

| Library | License | Used For | Inclusion |
| -- | -- | -- | -- |
| .Net Core | MIT | Runtime | Nuget
| System.Numerics | MIT | Data structures and hardware intrinsics | Nuget
| Forks
| [WinApi](https://github.com/prasannavl/WinApi) | Apache | Windows API Interop Headers | Platform/Implementation/Win32/Native
| [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net) | MIT | OpenGL API | Platform/OpenGL
| [StbTrueType](https://github.com/nothings/stb/blob/master/stb_truetype.h) & [StbTrueTypeSharp](https://github.com/zwcloud/StbTruetypeSharp) | MIT & GPL3 | Font Rendering Comparison | Referenced by Tests @ Tests/StbTrueType
| [TiledSharp](https://github.com/marshallward/TiledSharp) | Apache 2.0 | .TMX Support | Uses custom XML and engine integration @ Standard/TMX
| Optional
| [Roslyn/Microsoft.CodeAnalysis.CSharp](https://github.com/dotnet/roslyn) | MIT | Runtime C# Script Compilation | Emotion.Plugins.CSharpScripting
| [CimGui](https://github.com/cimgui/cimgui) & [CimGui.Net](https://github.com/mellinoe/ImGui.NET) | MIT | Developer UI | Emotion.Plugins.ImGuiNet, Precompiled for Mac64, Linux64, and Win64
| [ANGLE](https://github.com/google/angle) | Google License | Compatibility | Precompiled for Win32 and Win64, Add "ANGLE" symbol
| [llvmpipe / Gallium / Mesa](https://mesa3d.org/) | MIT | Compatibility via Software Renderer | Precompiled for Win32 and Win64
| [Glfw](https://github.com/glfw/glfw) & [Glfw.Net](https://github.com/Chman/Glfw.Net) | Zlib | Mac and Linux Window Creation | Precompiled for Mac64, Linux64, Win32, and Win64, Add "GLFW" symbol
| [OpenAL-Soft](https://github.com/kcat/openal-soft) & [OpenAL.NetCore](https://github.com/nsglover/OpenAL.NETCore) | LGPL & MIT | Mac and Linux Audio | Precompiled for Mac64, Linux64, Win64, Add "OpenAL" symbol
| References
| [McGill Engineering](http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/Samples.html) | X | Hardening WAV and Audio Implementation | None
| [PNGSuite](http://www.schaik.com/pngsuite/) | X | Hardening PNG Implementation | None
| [OpenType.JS](https://opentype.js.org/) | X | Font Parsing Reference | None
| [Nine.Imagine](https://github.com/yufeih/Nine.Imaging) | X | Image Parsing Comparison | None
| [ImageSharp](https://github.com/SixLabors/ImageSharp) | X | Quirky Image Format Reference | None
| [OpenAL-Soft](https://github.com/kcat/openal-soft/) | X | Audio Code Reference | None
| [NAudio](https://github.com/naudio/NAudio) | X | Audio Code Reference | None
| [Audacity](https://github.com/audacity) | X | Audio Code Reference | None

If you're distributing code using this project include the "LICENSE THIRD-PARTY" file from the repository.

## Inspired Fully, Or In Parts By:

- Processing
- MonoGame
- Mononoke
- LOVE Framework
- Sparky
- OpenTK
