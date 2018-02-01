<img src="Resources/SoulEngine2018.png" width=30%>

[![Build status](https://ci.appveyor.com/api/projects/status/yv7u2a04tp1pgmew?svg=true)](https://ci.appveyor.com/project/Cryru/soulengine)

## What is it?

SoulEngine is a 2D orientated game engine based on the idea of low-level code writing, flexibility, and control as opposed to the higher level GUI approach big engines have. The project's goal is to provide easy to use and understand, APIs and concepts for programmers which prefer to write code over dragging and dropping objects and digging through menus.

## Updates

While I try to update the engine as often as possible, most of the big features, refactorings, and rewrites are done in the form of big yearly updates. The newest version is found on the [master branch](https://github.com/Cryru/SoulEngine), while older versions can be found on the other branches, or at these links:

* [SoulEngine 2017](https://github.com/Cryru/SoulEngine/tree/2017)
* [SoulEngine 2016](https://github.com/Cryru/SoulEngine/tree/2016)
* [SoulEngine 2016 Android](https://github.com/Cryru/SoulEngine/tree/2016Android)

## This Year's Idea

The objective of the 2018 version is to move away from XNA/Monogame by using a small library called "Breath". It is based on OpenTK which allows for more control over the OpenGL and OpenAL APIs, in addition to making cross-platform compatibility more straight-forward. This version also features a custom asset packer based on last version's asset loading extensions.

On the game development side 2018 introduces the concept of "systems". Classes which group entities based on components and execute code on them. Upgrading from 2017's EC (entity-component) model to a ECS (entity-component-system) model where components are pure data.

## Features and Progress

- Window Control [&#10003;] (OpenTK)
- Resolution Adaptation with Boxing [&#10003;] (Breath)
- Borderless, Fullscreen, and Windowed Mode Switching and Support
- Camera System
- FPS Independent Timing [&#10003;] (OpenTK)
- Logging [&#10003;] (SoulLib)
- Debugging Console [&#10003;]
- Error Handling System [&#10003;]
- Asynchronous Javascript Scripting [&#10003;] (Jint)
- Entity-Component-System Pattern [&#10003;] - [Example](/SoulEngine/Examples/Basic/ECSTest.cs)
- Scene System [&#10003;]
- Drawing Primitive Shapes and Custom Polygons [&#10003;] (Breath) - [Example](/SoulEngine/Examples/Basic/ShapeTest.cs)
  - Texturing shapes [&#10003;] (Breath)
- Mouse/Keyboard Input System [&#10003;] (OpenTK) - [Example](/SoulEngine/Examples/Basic/InputTest.cs)
- Physics Engine [&#10003;] (SoulPhysics) - [Example](/SoulEngine/Examples/Basic/PhysicsTest.cs)
- Asset Packing, and Protection [&#10003;]
- Texture Rendering and Animation [&#10003;] (Breath) - [Example](/SoulEngine/Examples/Basic/TextureTest.cs)
  - Supported: PNG, JPEG, BMP
  - Unsupported: ICO
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
