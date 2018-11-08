# Getting Started

Download the release or debug packages and include the `EmotionCore.dll` in your project. The other dlls and the library folder must be set to copy to the output directory.
Include `Context.Setup()` in your main method. This prepares the engine and allows you to make sure of its APIs.

## Engine Structure

Game engines run on loops, usually there is an update and a draw loop which on some engines can either be one loop or run on different threads. These loops update the world state and perform rendering respectively. Some engines have additional loops like a physics simulation loop and so on.

In Emotion there are the following loops:

- Logging (Constant) (LoggingThread)
- Console Thread (Debug Only) (ConsoleThread)
- Update (Run Every Tick) (GLThread)
- Draw (Run Every Frame) (GLThread)
- Audio (Constant) (ALThread)

## Layering

In order to execute code on the update and draw loops you need to have a layer class. Each layer is a part of the user section of the loops, and they are run one after another based on their `Priority` property. Creating your own layer is as simple as inheriting the layer class.

[Layer Class]("https://github.com/Cryru/Emotion/blob/master/EmotionCore/src/Game/Layering/Layer.cs")

---
Code Example:

```
#region Using

using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Graphics;

#endregion

namespace MyEmotionProject
{
    internal class Program
    {
        public static void Main()
        {
            Context.Setup();
        }
    }

    internal class MyLayer : Layer
    {
        public override void Load()
        {
        }

        public override void Draw(Renderer renderer)
        {
        }

        public override void Update(float frameTime)
        {
        }

        public override void Unload()
        {
        }
    }
}
```
---

### Load

The load function is called when the layer is loaded and is run on another "Layer Loading" thread. This means that others layers can render and update while yours is loading, this allows for a "loading screen" layer and for your code to be responsive. In this function you should load assets and initiate objects, but remember that some operations can only be performed on the GL Thread and will interrupt the other layers' execution.