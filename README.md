<img src="windowslogo.png" width=30%>

# SoulEngine Windows
## Main Repository

SoulEngine is a 2D XNA/Monogame based game engine which provides another layer of abstraction while maintaining the hands-on code writing associated with XNA and Monogame as opposed to the script based approach big engines have. The intent is to make game development easier. The point is NOT to squeeze performance, but rather to shorten the amount of time it takes between you coming up with an idea for a game, getting a prototype ready, and eventually even releasing a product.

While the plan is to update the engine often, that is not always possible, so most of the big changes and rewrites are left to big yearly updates. Currently version 2017 is in development and not ready to be used but older versions can be found here:

* ["SoulEngine 2016"](https://github.com/Cryru/SoulEngine-2016)
* ["SoulEngine 2016 Android"](https://github.com/Cryru/SoulEngine-2016-Android)

## Idea

SoulEngine 2016 wasn't based on any particular idea and was pretty much over all the place. The 2017 version's philosophy is based on an entity-component system similar to the one Unity3D has, as well as a single "Scene", as opposed to the old Photoshop-like layer model.

## Features and Progress

- Object-Component System [&#10003;]
- Simple Animated Textures
- Asset Loading and Unloading Manager
- Scene System
- Unified Event System [&#10003;]
- Tiled Integration
- FPS Independent Timing [&#10003;]
- Asset and Save File Tamper Prevention [&#10003;]
- Camera System [&#10003;]
- Resolution Adaptation with Boxing [&#10003;]
- JSON Based File Management (For Settings, Save Files etc.)
- Error Logging
- Custom Text Rendering with Markup

and more...


## External Projects

["TiledSharp"](https://github.com/marshallward/TiledSharp) - Parsing .tmx files.

["SoulLib"](https://github.com/Cryru/SoulLib) - Encryption, JSON, Managed Files, and IO.

["Polygon-Tool"](https://github.com/Cryru/Polygon-Tool) - Polygon collision boxes for physics engine.

["Farseer Physics"](https://github.com/tinco/Farseer-Physics) - Physics engine.

["Asset-Meta-Generator"](https://github.com/Cryru/Asset-Meta-Generator) - Generating Meta files for asset validation.
