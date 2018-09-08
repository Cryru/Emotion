# Emotion
<img src="EmotionLogo.png" width="128px" />

[![Build status](https://ci.appveyor.com/api/projects/status/qur90gc2wdhmd5ff/branch/master?svg=true)](https://ci.appveyor.com/project/Cryru/emotion/branch/master)

## What is it?

Emotion is a cross-platform 2D game engine written in C#, with the intent of removing fat and overhead without giving up too much control. The idea is to make game development about game development, and not about engine or low-level backend development. Written by a game developer for game developers.  

This project is a successor to [SoulEngine](Documents/SoulEngine.md).

## Platforms Tested On:

- Windows 10 x64/x86
- Ubuntu Xenial-Xerus x64 (Latest version is not tested)
- MacOS High-Sierra x64

## Supported Platforms:

- Windows Vista/7/8/8.1/10 x64/x86
- Debian 9+ x64
- MacOS High-Sierra+ x64
- Android 5.0+ (Planned)

For information on how to build for other platforms check out: https://github.com/Cryru/The-Struggles-Of-Running-And-Statically-Linking-Mono

#### Linux

Most installations should include the proper libraries to run the engine, but some repos I've tested have some missing ones. Here is a list of ones I've found missing which the engine depends on:

- libjxr0 (https://packages.debian.org/sid/libjxr0)

- libopenjp2-7 (https://packages.debian.org/stretch/libopenjp2-7)

## Features So Far

- Window creation.
- Mouse and keyboard input.
  - Focus captured.
- Asset loading and management.
  - Textures: All FreeImage supported formats. ex. BMP/PNG/JPEG/GIF
  - Fonts: All FreeType supported formats. ex. TTF
  - Sounds: RIFF WAV
- Camera system.
  - Default cameras include one which follows the mouse and one which follows a target transform.
- Rendering
  - Primitives like lines and rectangles.
  - Batching and buffer mapping.
  - Square textures.
  - Text.
    - Includes advanced font drawing with control over each individual glyph.
    - Richtext featuring auto wrapping, alignment, markup, and more.
- Sound engine with effects.
- UI system.
  - Customize base controls through inheritance or use them straight away.
  - Inter
- Tiled integration and rendering.
  - Includes layer opacity, multiple tilesets, animated tiles, and more.
- An implementation of A*.
  - With the ability to add a custom heuristics function, and perform other customizations.
- A Javascript scripting engine.
- Logging and additional debugging.

## How to use it?

1. Download a release packet from the GitHub releases page, or compile the engine yourself.
2. Create a C# project, reference EmotionCore.dll, and setup the library files to copy on compilation. You will need them in the same folder as your executable.
3. Write a game.

For examples you can refer to the [EmotionSandbox](EmotionSandbox) project.

## Projects Used

OpenTK [OpenGL/OpenAL] - Rendering and audio.
	- OpenAL32.dll included.
	- openal.so included.

FreeImage-DotNet-Core [FreeImage] - Loading and converting images.
	- FreeImage.dll included.
	- FreeImage.so included.
	- FreeImage.dylib included.

SharpFont [FreeType] - Loading fonts and glyphs, and rendering them.
	- freetype6.dll included.
	- freetype.so included.

TiledSharp [Modified] - Loading .tmx Tiled files.

Jint - Javascript script engine.

Soul - Logging.

## Inspired By

- Processing
- MonoGame
- LOVE Framework
