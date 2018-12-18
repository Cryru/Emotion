# Emotion
<img src="EmotionLogo.png" width="128px" />

[![Build status](https://ci.appveyor.com/api/projects/status/qur90gc2wdhmd5ff/branch/master?svg=true)](https://ci.appveyor.com/project/Cryru/emotion/branch/master)

Development: [![debug artifact](https://img.shields.io/badge/Download-%20Debug%20Build-brightgreen.svg)](https://ci.appveyor.com/api/projects/Cryru/Emotion/artifacts/EmotionCore%2Fbin%2FEmotion%20Built%20Debug.zip?branch=master&job=Configuration%3A%20Debug-GLES) [![nuget debug](https://img.shields.io/nuget/v/Emotion.svg)](https://www.nuget.org/packages/Emotion/)

Deployment: [![release artifact](https://img.shields.io/badge/Download-%20Release%20Build-brightgreen.svg)](https://ci.appveyor.com/api/projects/Cryru/Emotion/artifacts/EmotionCore%2Fbin%2FEmotion%20Built.zip?branch=master&job=Configuration%3A%20Release-GLES)

## What is it?

Emotion is a cross-platform game engine written in C#, with the intent of removing overhead and providing abstraction without sacrificing control. The idea is, to make game development about game development and not about engine or low-level backend development, but allowing those who want to do that with the option to do so. The goal is to save time, and provide indie developers (and mostly myself) with a foundation.

## Requirements:

- At least .Net Framework 4.6.2 or the equivalent Mono runtime.
- OpenGL Support
  - OpenGL ES 3
  - 3.3 Core on MacOS, Windows and Linux (This means it won't run on Linux VMs)
- GLSL Support Options
  - 300 es support on Windows or Linux
  - 300 support on MacOS
- Dynamically Uniform Expression Support Options
  - The "GL_ARB_gpu_shader5" extension
  - GLSL 400 on Windows or Linux

## Features So Far

- Window creation.
- SIMD vector and matrix math, through System.Numerics.
- Mouse and keyboard input.
  - Captured only while the window has focus preventing rogue clicks.
  - Detect key down, key up, and key held independently.
- Asset loading and management.
  - Textures: All FreeImage supported formats. ex. BMP/PNG/JPEG/GIF
  - Fonts: All FreeType supported formats. ex. TTF/OTF
  - Sounds: RIFF WAV
  - Text Files
  - Binaries
- Camera system.
  - Default cameras include one which follows the mouse and one which follows a target transform. Create your own cameras and share!
- Rendering
  - Drawing of things like lines, rectangles, triangles, and circles.
    - These are all drawn as triangles, no GL_LINES here, monsieur!
    - Super fast rendering of many objects at once through the magic of MapBuffers.
  - Transformation matrix stack.
  - Batching and buffer mapping.
  - Textures and alpha.
  - Spritesheet based animation.
  - Text.
    - Includes advanced font drawing with control over each individual glyph.
    - Extensible Richtext class featuring auto wrapping, alignment, markup, and more.
  - Draw arbitrary vertices, the wrapper gives you control over your own model matrix and buffers.
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
- Graphical debugging and a command console.
  - Debug the camera, or UI through the engine.
  - Execute scripting commands at real time.
- Scenes in the form of layers. Have your UI, pause menu, levels, or anything on separate scenes.
- Framerate independent timing, semi-fixed step and free-step based on configuration.
  - Managed delta time and total time access in the shader for cool effects.
  - The update loop will always be up to date when a target fps is selected.
- Lots of configuration through settings and flags.

## How to develop for it?

1. Go on Nuget and add the "Emotion" package to your project.
2. Write a game.
3. Before releasing, download the release packet and use that instead of the nuget package.

### Resources

You can refer to the unit tests for specific information here - [Emotion.Tests](Emotion.Tests).

For a playground of examples go here - [EmotionSandbox](https://github.com/Cryru/EmotionSandbox).

Detailed documentation is a work in progress [here](Documents).

## Platforms

### Platforms Tested On:

The latest build is always tested on the configurations listed as `Primary`.

- Windows 10 x64/x86
  - Intel HD Graphics 620 `(Primary)`
  - Nvidia 940MX `(Primary)`
  - AMD R9 200
  - 970M
- Ubuntu Xenial-Xerus x64 `(Last Commit Tested On: 5e7bea38bb197f85376d43e164f41ce7a6a5c341) Nov 6th`
- MacOS High-Sierra x64 `(Last Test On: Build 210) Dec 4th`

### Supported Platforms:

Due to lack of developers or testers, these platforms are considered technically supported.

- Windows Vista/7/8/8.1 x64/x86
- Debian 9+ x64
- MacOS High-Sierra+ x64

### Planned Platforms:

- Android 5.0+

## Building:

Just clone and build. Everything is included.

For information on how to build for other platforms check out: https://github.com/Cryru/The-Struggles-Of-Running-And-Statically-Linking-Mono

On Linux build without a machine config, and make sure the .exe **doesn't** carry a config specifying the runtime.

## Testing and QA

The engine has unit and integration tests, but they aren't automatically run as the CI doesn't support OpenGL 3.3+ Despite this I ensure that all tests pass before merging into master. Take note however that the tests are only ran on Windows 10 and do not test other platforms. There I rely on user and my own game development testing. Other issues like difference in driver behavior are also hard to catch using these tests and rely on user feedback.

## Projects Used

OpenTK [OpenGL/OpenAL] : Context and host creation, input capturing, GL API and AL api.
- OpenAL32.dll included. x64/x86
- openal.so included. x64
    - libsndio.so.6.1 included.

FreeImage-DotNet-Core [FreeImage] : Loading and converting images.
- FreeImage.dll included. x64/x86
- FreeImage.so included. x64
    - libpng14.14.dylib included.
- FreeImage.dylib included. x64

SharpFont [FreeType] : Loading fonts and glyphs, and rendering them.
- freetype6.dll included. x64/x86
- freetype.so included. x64
- libfreetype.6.dylib included. x64

TiledSharp [Modified] : Loading .tmx Tiled files.

Jint : Javascript script engine.

System.Numerics : Vector and matrix math.

### Linux

Most installations should include the proper libraries to run the engine, but some repos I've tested have some missing ones. Here is a list of ones I've found missing which the engine depends on:

- libjxr0 (https://packages.debian.org/sid/libjxr0)

- libopenjp2-7 (https://packages.debian.org/stretch/libopenjp2-7)

## Inspired By

- Processing
- MonoGame
- LOVE Framework
- Sparky