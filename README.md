<img src="Resources/SoulEngine2018.png" width=30%>


## What is it?

SoulEngine is a 2D game engine based on the idea of low level code writing, flexibiliy, and DIY, as opposed to the script based, high level, tool approach big engines have, while still providing some of those features. Despite this, the point is not to complicate development, but rather make it easier and simpler while allowing the developer to maintain control and avoid reinventing the wheel. The true end goal of this project is to shorten the time it takes between coming up with an idea, getting a prototype ready, and eventually releasing a product.

## Updates

While I try to update the engine as often as possible, that is not always possible as life gets in the way, so most of the big changes and rewrites are left to big yearly updates. This year's version can be found here and is in development, while older versions can be found at these links:

* [SoulEngine 2017](https://github.com/Cryru/SoulEngine-2017)
* [SoulEngine 2016](https://github.com/Cryru/SoulEngine-2016)
* [SoulEngine 2016 Android](https://github.com/Cryru/SoulEngine-2016-Android)

## This Year's Idea

SoulEngine 2017 set the grounds for a clean engine structure, and even had some game demos released in it. This year I'm planning to replace XNA/Monogame with a custom framework - [Raya](https://github.com/Cryru/Raya) to increase performance and allow me to build things at a lower level than before. In addition to that there will be improvements to the actor system with a more general abstract approach, and rethinking the debugging, error handling, and scripting systems. Other goals include better documentation, being able to cover the platforms last year's version could - Linux and Android, and demos for different functionality.

## Features and Progress

- Resolution Adaptation with Boxing [&#10003;]
- Camera System [&#10003;]
- FPS Independent Timing [&#10003;]
- Logging [&#10003;]
- Debugging Console [&#10003;]
- Error Handling System [&#10003;]
- Asynchronous Javascript Scripting [&#10003;]
- Actor System [&#10003;]
- Scene System [&#10003;]
- Drawing Primitive Shapes and Custom Polygons [&#10003;] - ShapeTest
- Simple Input System [&#10003;]
- Physics Engine [In Progress]

## Libraries Used:

[Raya](https://github.com/Cryru/Raya) - Drawing and graphics, input, sound, and window management.

[SoulPhysics](https://github.com/Cryru/SoulPhysics) - Physics.

## Tools:

SoulEngine Build Helper - Built-in tool to help with building your projects.

[Polygon-Tool](https://github.com/Cryru/Polygon-Tool) - Tool to help you create custom polygons.

## External Projects

[TiledSharp](https://github.com/marshallward/TiledSharp) - Tiled integration.

[Jint](https://github.com/sebastienros/jint) - Javascript engine.
