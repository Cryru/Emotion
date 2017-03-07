using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using SoulEngine.Enums;
using SoulEngine.Events;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The game instance for use by the engine.
    /// </summary>
    public class Core : Game
    {
        #region "Variables"
        /// <summary>
        /// The time in milliseconds it took for the last frame to render.
        /// </summary>
        public float frameTime = 0;
        #region "Systems"
        /// <summary>
        /// 
        /// </summary>
        public Scene Scene;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// The initializer.
        /// </summary>
        public Core()
        {
            //Setup the graphics device.
            Context.GraphicsManager = new GraphicsDeviceManager(this);

            //Setup the Content root folder for the master scene. The root for this folder is the exe.
            Content.RootDirectory = "Content";

            //Connect the C# native events to the SE trigger system.
            Exiting += Engine_Exiting;
            Window.TextInput += Window_TextInput;
            Window.ClientSizeChanged += Window_SizeChanged;
        }

        /// <summary>
        /// Setups the spritebatch and starts the start sequence.
        /// </summary>
        protected override void LoadContent()
        {
            //Apply settings.
            IsMouseVisible = Settings.RenderMouse;
            IsFixedTimeStep = Settings.FPS > 0 ? true : false; //Check whether to cap FPS based on the fps target.
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / Settings.FPS);
            Context.GraphicsManager.SynchronizeWithVerticalRetrace = Settings.vSync;

            //Apply hardcoded settings.
            Window.AllowAltF4 = true;
            Window.Title = Settings.WName;

            //Setup the brush for drawing.
            Context.ink = new SpriteBatch(GraphicsDevice);

            //Setup the window manager.
            WindowManager.Initialize();

            //Connect system events.
            SystemEvents.ConnectSystemEvents();

            //Continue the start sequence.
            Starter.ContinueStart();

            //Load global resources.
            AssetManager.LoadGlobal();

            //Measure boot time.
            Starter.bootPerformance.Stop();
            Console.WriteLine(">>>> Engine loaded in: " + Starter.bootPerformance.ElapsedMilliseconds + "ms");

            //THE BELOW CODE NEEDS TO BE ATTACHED TO A SCENE LOADER
            //Load the starting scene.
            Scene = new ScenePrim();
            //Run the scene's setup code.
            Scene.Start();
        }
        #endregion
        GameObject t;
        #region "Loops"
        /// <summary>
        /// Is executed every tick.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //If the game is not focused, don't update.
            if (IsActive == false) return;

            //Trigger tick start event.
            ESystem.Add(new Event(EType.GAME_TICKSTART, this, gameTime));

            //Trigger tick end event.
            ESystem.Add(new Event(EType.GAME_TICKEND, this, gameTime));
        }

        /// <summary>
        /// Is executed every frame.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //If the game is not focused, don't update.
            if (IsActive == false) return;
            FPSCounterUpdate(gameTime);
            //Trigger frame start event.
            ESystem.Add(new Event(EType.GAME_FRAMESTART, this, gameTime));

            //Record frametime.
            frameTime = gameTime.ElapsedGameTime.Milliseconds;

            //Start drawing frame by first clearing the screen, first the behind and then the front.
            Context.Graphics.Clear(Color.Black);
            Context.ink.Begin(transformMatrix: Context.Screen.View);
            Context.ink.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, Settings.Width, Settings.Height), Settings.FillColor);
            Context.ink.End();

            //Draw the current scene. (NYI)
            //Draw debug objects on top. (NYI)


            //Trigger frame end event.
            ESystem.Add(new Event(EType.GAME_FRAMEEND, this, gameTime));
        }
        #endregion

        #region "Functions"

        #endregion

        #region "Triggers for Internal Events"
        /// <summary>
        /// Is triggered before the game is closed.
        /// </summary>
        private void Engine_Exiting(object sender, System.EventArgs e)
        {
            ESystem.Add(new Event(EType.GAME_CLOSED, this, null));
        }
        /// <summary>
        /// Is triggered when text input is detected to the game.
        /// This returns the character as input, for instance capital letters etc. 
        /// and doesn't capture non text characters.
        /// </summary>
        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            ESystem.Add(new Event(EType.INPUT_TEXT, this, e.Character.ToString()));
        }
        /// <summary>
        /// Triggered when the size of the window changes. If Settings.ResizableWindow is true
        /// this can be quite useful. Has a system event hooked to it that regenerates the window.
        /// </summary>
        private void Window_SizeChanged(object sender, EventArgs e)
        {
            ESystem.Add(new Event(EType.WINDOW_SIZECHANGED, this, null));
        }
        #endregion


        /// <summary>
        /// The frames rendered in the current second.
        /// </summary>
        public static int curFrames = 0;
        /// <summary>
        /// The frames rendered in the last second.
        /// </summary>
        public static int lastFrames = 0;
        /// <summary>
        /// The current second number.
        /// </summary>
        public static int curSec = 0;
        public static void FPSCounterUpdate(GameTime gameTime)
        {
            //Check if the current second has passed.
            if (!(curSec == gameTime.TotalGameTime.Seconds))
            {
                curSec = gameTime.TotalGameTime.Seconds; //Assign the current second to a variable.
                lastFrames = curFrames; //Set the current second's frames to the last second's frames as a second has passed.
                curFrames = 0;
            }
            curFrames += 1;
        }
    }
}

