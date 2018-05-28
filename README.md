# Emotion
<img src="EmotionLogo.png" width="128px" />

[![Build status](https://ci.appveyor.com/api/projects/status/qur90gc2wdhmd5ff/branch/master?svg=true)](https://ci.appveyor.com/project/Cryru/emotion/branch/master)

## What is it?

Emotion is a cross-platform 2D game engine written in C#, with the intent of removing fat and overhead without giving up too much control. The idea is to make game development about game development, and not about engine or low-level backend development. Written by a game developer for game developers.  

This project is a successor to [SoulEngine](Documents/SoulEngine.md).

## Platforms Tested On:

- Windows x64/x86
- Ubuntu x64 (!) Latest version not tested.

## Features So Far

- Window creation.
- Mouse and keyboard input.
- Asset loading and management.
  - Textures: All FreeImage supported formats. ex. BMP/PNG/JPEG/GIF
  - Fonts: All FreeType supported formats. ex. TTF
  - Sounds: RIFF WAV
- Camera system.
  - Default cameras include one which follows the mouse and one which follows a target object.
- Rendering
  - Primitives like lines and rectangles.
  - Square textures.
  - Text.
    - Includes advanced font drawing with control over each individual glyph.
    - Richtext featuring auto wrapping, alignment, markup, and more.
- Sound engine with fading effects.
- UI system.
- Tiled integration and rendering.
  - Includes layer opacity, multiple tilesets, animated tiles, and more.
- An implementation of A*.
  - With the ability to add a custom heuristics function, and perform other customizations.
- A Javascript scripting engine.
- Logging and a debug module.

## How to use it?

1. Download a release packet from the GitHub releases page, or compile the engine yourself.
2. Create a C# project, reference EmotionCore.dll, and setup the library files to copy on compilation. You will need them in the same folder as your executable.
3. Write a game.

For examples you can refer to the [EmotionSandbox](EmotionSandbox) project.

## Projects Used

OpenTK - OpenGL/OpenAL bindings and context creation.

FreeImage/FreeImage-DotNet-Core - Loading and converting images.

FreeType/SharpFont - Loading fonts and glyphs.

TiledSharp [Modified] - Loading .tmx Tiled files.

Jint - Javascript execution.

Soul - Logging.

## Inspired By

- Processing
- MonoGame
- LOVE Framework
