using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Events;
using SoulEngine.Objects.Components;
using Microsoft.Xna.Framework;

namespace SoulEngine.Debugging
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A scene-like object that manages debugging components.
    /// </summary>
    public static class DebugScene
    {
        private static GameObject logDebug;
        private static int lastLogLine = -1;
        private static List<string> entriesDisplayed = new List<string>();
        public static List<string> blacklistedEvents = new string[]
        {
            EType.GAME_FRAMESTART,
            EType.GAME_FRAMEEND,
            EType.GAME_TICKSTART,
            EType.GAME_TICKEND,
            EType.TICKER_TICK,
            EType.TICKER_DONE
        }.ToList();

        /// <summary>
        /// Setups the scene as the current scene.
        /// </summary>
        public static void Setup()
        {
            logDebug = GameObject.GenericTextObject;
            logDebug.AddComponent(new ActiveTexture());

            logDebug.Component<ActiveTexture>().Texture = AssetManager.BlankTexture;
            logDebug.Component<ActiveTexture>().Tint = Color.Black;
            logDebug.Component<ActiveTexture>().Opacity = 0.1f;

            ESystem.Add(new Listen(EType.GAME_TICKEND, Update));
            ESystem.Add(new Listen(EType.GAME_FRAMESTART, Compose));
            ESystem.Add(new Listen(EType.GAME_FRAMEEND, DrawHook));

            entriesDisplayed.Add("SoulEngine " + Info.Version);
            entriesDisplayed.Add("");
            entriesDisplayed.Add("");
            entriesDisplayed.Add("");
        }
        #region "Hooks"
        /// <summary>
        /// The core's hook for updating.
        /// </summary>
        private static void Update(Event e)
        {
            FPSCounterUpdate((GameTime) e.Data);
            entriesDisplayed[1] = "<border=#000000><color=#e2a712>FPS: " + FPS + "</></>";
            entriesDisplayed[2] = "Scene: " + Context.Core.Scene.ToString();
            entriesDisplayed[3] = "Objects: " + Context.Core.Scene.ObjectCount;

            logDebug.Component<Transform>().Width = 200;
            logDebug.Component<Transform>().Height = logDebug.Component<ActiveText>().Height;

            logDebug.Component<ActiveText>().Text = string.Join("\n", entriesDisplayed);

            logDebug.Update();
        }
        /// <summary>
        /// Composes component textures on linked objects.
        /// </summary>
        private static void Compose()
        {
            //Don't log events caused by the debugger.
            Logger.Enabled = false;
            logDebug.Compose();
            Logger.Enabled = true;
        }
        /// <summary>
        /// The core's hook for drawing.
        /// </summary>
        private static void DrawHook()
        {
            Context.ink.Start(Enums.DrawChannel.Screen);
            logDebug.Draw();
            Context.ink.End();
        }
        #endregion

        #region "FPS Counter"
        /// <summary>
        /// The frames rendered in the current second.
        /// </summary>
        private static int curFrames = 0;
        /// <summary>
        /// The frames rendered in the last second.
        /// </summary>
        public static int FPS = 0;
        /// <summary>
        /// The current second number.
        /// </summary>
        private static int curSec = 0;
        private static void FPSCounterUpdate(GameTime gameTime)
        {
            //Check if the current second has passed.
            if (!(curSec == gameTime.TotalGameTime.Seconds))
            {
                curSec = gameTime.TotalGameTime.Seconds; //Assign the current second to a variable.
                FPS = curFrames; //Set the current second's frames to the last second's frames as a second has passed.
                curFrames = 0;
            }
            curFrames += 1;
        }
        #endregion
    }
}
