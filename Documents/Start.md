# Getting Started

First you have to add Emotion to your project. You can do this either by looking up "Emotion" on Nuget, or by downloading the debug/release packages from the ReadMe. If you opt for the second option you need to include the `EmotionCore.dll` in your project, and set all other files to copy to the output directory.

Once that's done invoke the `Context.Setup()` _(Emotion.Engine.Context)_ function. This loads libraries and performs platform dependent bootstrapping. 90% of the time this should be the first Emotion related function you invoke. After the setup you can now use Emotion APIs and modules, most of which are found as members of the Context class. 

You can also pass a function which takes in a `Settings` _(Emotion.Engine.Configuration.Settings)_ class to the Setup to configure some user related properties. Another way to configure the engine is to use the `Context.Flags` property which will be loaded and available from the start. It includes some lower level settings and is much more volatile. For more information refer to the [Configuration](./Configuration.md) documentation.

## Scenography

Your game code should be organized in objects called scenes. These objects have a load function which is ran at the start once, on another thread, and an update and draw function which will be executed as part of the draw loop. To create a scene you need to inherit the `Scene` _(Emotion.Engine.Scenography.Scene)_ class. Once you have created your scene all that is required for it to be loaded is invoking `Context.SceneManager.SetScene(new MainScene())`.

For more information refer to the [Scenography](./Scenography.md) documentation.

---
Code Example:

```
#region Using

using Emotion.Engine;
using Emotion.Engine.Scenography;
using Emotion.Graphics;

#endregion

namespace MyGame
{
    public class MainScene : Scene
    {
        public override void Load()
        {
            // Run on another thread, once. Loading assets and other resources should happen here.
        }

        public override void Update(float frameTime)
        {
            // If run on the game loop according to the timestep. Update your game's step here - physics etc.
            // The frametime argument passed is `Context.FrameTime`.
        }

        public override void Draw(Renderer renderer)
        {
            // Perform drawing here.
            // The renderer argument passed is `Context.Renderer`.
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

Once you have setup your scene you can start the game loop by invoking `Context.Run()`, this function is expected to be blocking. Your *scene's update function* will be called in a semi fixed step according to the `Settings.RenderSettings.CapFPS` setting. If the settings is set to 0, it will be executed as many times as possible. The *draw function* on the other hand will be called much more often. It is advised not to perform any state updating in the draw function,
and to use `Context.FrameTime` for time tracking. When the host is unfocused neither function will be run.

Note: The game loop is handled by the IHost instance. The above description is for the default host.

## You're Done Now

This is all that is required to start using the engine.

---
Your main function will look something like this now:

```
    public static void Main(string[] args)
    {
        Context.Setup();
        Context.SceneManager.SetScene(new MainScene());
        Context.Run();
    }
```
---