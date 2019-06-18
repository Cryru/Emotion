# Getting Started

_Last Updated: Version 0.0.15_

First you have to add Adfectus to your project. You can do this by looking up "Adfectus" on Nuget - and then choosing the packet for your platform (Adfectus.Platform.DesktopGL for example).

Once that's done invoke the `Engine.Setup()` _(Adfectus.Common.Engine)_ function. This loads libraries and performs platform dependent bootstrapping. 90% of the time this should be the first Adfectus related function you invoke. After the setup you can now use Adfectus APIs and modules, most of which are found as members of the Engine class. 

You can also pass an `EngineBuilder` _(Adfectus.Common.EngineBuilder)_ to the setup, which allows you to configure various properties. Another way to configure the engine is to use the `Engine.Flags` property which is accessible even before setup is called, but is not to be relied on as it includes some lower level settings and is much more volatile. For more information refer to the [Configuration](./Configuration.md) documentation.

## Scenography

Your game code should be organized in objects called scenes. These objects have a load function which is ran at the start once, on another thread, and an update and draw function which will be executed as part of the main loop. To create a scene you need to inherit the `Scene` _(Adfectus.Scenography.Scene)_ class. Once you have created your scene all that is required for it to be loaded is invoking `Engine.SceneManager.SetScene(new MyScene())`.

For more information refer to the [Scenography](./Scenography.md) documentation.

---
Code Example:

```
#region Using

using Adfectus.Common;
using Adfectus.Scenography;

#endregion

namespace MyGame
{
    public class MyScene : Scene
    {
        public override void Load()
        {
            // Run on another thread, once. Loading assets and other resources should happen here.
        }

        public override void Update()
        {
            // If run on the game loop according to the timestep. Update your game's step here - physics etc.
        }

        public override void Draw()
        {
            // Perform drawing here using the `Engine.Renderer` object.
        }

        public override void Unload()
        {
            // Perform cleanup of assets and resources here.
            // Is ran on another thread when another scene is loaded, before the new scene's *load* function is called.
        }
    }
}
```
---

## The Game Loop

Once you have setup your scene you can start the game loop by invoking `Engine.Run()`, this function is expected to be blocking. Your *scene's update function* will be called in a semi fixed step according to the EngineBuilder's `TargetTPS` setting (60 by default). If the settings is set to 0, it will be executed as many times as possible. The *draw function* on the other hand will be called less often. It is advised not to perform any state updating in the draw function,
and to use `Engine.FrameTime` for time tracking. When the host is unfocused neither function will be run.

## You're Done Now

This is all that is required to start using the engine.

---
Your main function will look something like this now:

```
    public static void Main(string[] args)
    {
        Engine.Setup();
        Engine.SceneManager.SetScene(new MyScene());
        Engine.Run();
    }
```
---