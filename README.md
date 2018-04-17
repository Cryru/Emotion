# Emotion
<img src="EmotionLogo.png" width="128px" />

[![Build status](https://ci.appveyor.com/api/projects/status/qur90gc2wdhmd5ff/branch/master?svg=true)](https://ci.appveyor.com/project/Cryru/emotion/branch/master)

## What is it?

Emotion is a cross-platform 2D game engine written in C#, with the intent of removing fat and overhead without giving up too much control. The idea is to make game development about game development, and not about engine or low-level backend development. Written by a game developer for game developers.  

This project is a successor to [SoulEngine](Documents/SoulEngine.md).

## Implemented Features So Far

- SDL Platform.
  - A backend which uses SDL2.
- Window creation.
- Mouse and keyboard input.
- Asset loading and management.
  - Textures: All FreeImage supported formats. ex. BMP/PNG/JPEG/GIF
  - Fonts: All FreeType supported formats. ex. TTF
- Camera system.
  - Default cameras include one which follows the mouse and one which follows the target.
- Rendering
  - Primitives like lines and rectangles.
  - Textures.
  - Text.
    - Includes advanced font drawing with control over each individual glyph.
- Tiled integration and rendering.
  - Includes layer opacity, multiple tilesets, animated tiles, and more.
- An implementation of A*.
  - With the ability to add a custom heuristics function, and perform other customizations.
- A Javascript engine.

## How to use it?

1. Download a release packet from the GitHub releases page, or compile the engine yourself.
2. Create a C# project, reference EmotionCore.dll, and set the library files to copy on compilation. You need them in the same folder as your executable.
3. Write a game.

For examples you can refer to the [EmotionSandbox](EmotionSandbox) project, or you can read the documentation within the source folder starting [here](/EmotionCore/src).

## Notes

- It is recommended when cloning Emotion to use ```git clone --depth=1 https://github.com/Cryru/Emotion.git``` because of the huge repository size.

## Projects Used

SDL2 - The SDL platform is based entirely on it.

SDL_TTF - Text drawing on the SDL platform.

FreeImage - Loading and converting images.

FreeType - Loading fonts and glyphs.

TiledSharp [Modified] - Loading .tmx Tiled files.

Jint - Javascript execution.

Soul - Logging.

## Inspired By

- Processing
- MonoGame
- LOVE Framework