<img src="Resources/SoulEngine2018.png" width=30%>

[![Build status](https://ci.appveyor.com/api/projects/status/yv7u2a04tp1pgmew?svg=true)](https://ci.appveyor.com/project/Cryru/soulengine)

## What is it?

SoulEngine is a 2D Monogame orientated game engine based on the idea of low-level code writing, flexibility, DIY, and control. The project's goal is to provide easy to use and understand APIs and concepts for programmers in order to shorted the amount of time it takes between coming up with an idea, getting a prototype ready, and eventually releasing a product.

## Updates

While I try to update the engine as often as possible, most of the big features, refactorings, and rewrites are done in the form of big yearly updates. The newest version is found on the [master branch](https://github.com/Cryru/SoulEngine), while older versions can be found on the other branches, or at these links:

* [SoulEngine 2017](https://github.com/Cryru/SoulEngine/tree/2017)
* [SoulEngine 2016](https://github.com/Cryru/SoulEngine/tree/2016)
* [SoulEngine 2016 Android](https://github.com/Cryru/SoulEngine/tree/2016Android)

## This Year's Idea

The objective of the 2018 version is to iterate and improve on last year's version by introducing the concept of "systems". Classes which group entities based on components and execute code on them. Upgrading from 2017's EC (entity-component) model to a ECS (entity-component-system) model where components are pure data.

## Features and Progress

- Resolution Adaptation with Boxing [&#10003;]
- Borderless, Fullscreen, and Windowed Mode Switching and Support [&#10003;]
- Tiled Integration
- Camera System [&#10003;]
- FPS Independent Timing [&#10003;]
- Logging [&#10003;] (SoulLib)
- Debugging Console [&#10003;]
- Debugging and Diagnosing API
- Error Handling System [&#10003;]
- Javascript Scripting [&#10003;] (Jint)
- Entity-Component-System Pattern [&#10003;]
- Scene System [&#10003;]
- Physics Engine [&#10003;] (SoulPhysics)
- Asset Packing, and Protection [&#10003;]
- Easy Animation [&#10003;] (Breath)
- Text Wrapping and Formatting
- Custom Markup

## Own Libraries Used:

[SoulLib](https://github.com/Cryru/SoulLib) - IO, Encryption, Hashing, Compression, Extensions, ManagedFile and more...

[SoulPhysics](https://github.com/Cryru/SoulPhysics) - Physics.

## External Projects

[TiledSharp](https://github.com/marshallward/TiledSharp) - Tiled integration.

[MonoGame](https://github.com/MonoGame/MonoGame) - Base-framework.

[Jint](https://github.com/sebastienros/jint) - Javascript engine.

## Tools:

SoulEngine AssetPacker - Built-in tool to help with building and protecting your assets.