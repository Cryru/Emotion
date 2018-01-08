<img src="Resources/SoulEngine2018.png" width=30%>

[![Build status](https://ci.appveyor.com/api/projects/status/yv7u2a04tp1pgmew?svg=true)](https://ci.appveyor.com/project/Cryru/soulengine)

## What is it?

todo

## Updates

While I try to update the engine as often as possible, that is not always possible as life gets in the way, so most of the big changes and rewrites are left to big yearly updates. This year's version can be found here and is in development, while older versions can be found at these links:

* [SoulEngine 2017](https://github.com/Cryru/SoulEngine-2017)
* [SoulEngine 2016](https://github.com/Cryru/SoulEngine-2016)
* [SoulEngine 2016 Android](https://github.com/Cryru/SoulEngine-2016-Android)

## This Year's Idea

todo

## Features and Progress

- Resolution Adaptation with Boxing [&#10003;] (Breath)
- Borderless, Fullscreen, and Windowed Mode Switching
- Camera System
- FPS Independent Timing [&#10003;] (OpenTK)
- Logging [&#10003;] (SoulLib)
- Debugging Console [&#10003;]
- Error Handling System [&#10003;]
- Asynchronous Javascript Scripting [&#10003;] (Jint)
- Entity-Component-System Pattern [&#10003;]
- Scene System [&#10003;]
- Drawing Primitive Shapes and Custom Polygons [&#10003;] (Breath) - [ShapeTest](/SoulEngine/Examples/Basic/ShapeTest.cs)
  - Texturing shapes [&#10003;] (Breath)
- Simple Input System
- Physics Engine [&#10003;] - [PhysicsTest](/SoulEngine/Examples/Basic/PhysicsTest.cs)
- Asset Packing, and Protection [&#10003;]
- Texture Rendering and Animation - [TextureTest](/SoulEngine/Examples/Basic/TextureTest.cs)
  - Supported: PNG, JPEG, BMP
  - Unsupported: ICO
- Window Icon 
- Advanced Text Rendering With Markup

## Libraries Used:

[Breath](https://github.com/Cryru/Breath) - Drawing and graphics, input, sound, and window management.

[SoulLib](https://github.com/Cryru/SoulLib) - IO, Encryption, Hashing, Compression, Extensions, ManagedFile and more...

[SoulPhysics](https://github.com/Cryru/SoulPhysics) - Physics.

## Tools:

SoulEngine AssetPacker - Built-in tool to help with building your project assets.

[Polygon-Tool](https://github.com/Cryru/Polygon-Tool) - Tool for creating polygon vertices.

## External Projects

[TiledSharp](https://github.com/marshallward/TiledSharp) - Tiled integration.

[Jint](https://github.com/sebastienros/jint) - Javascript engine.
