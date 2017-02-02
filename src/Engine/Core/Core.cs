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

            //Apply settings.
            IsMouseVisible = Settings.RenderMouse;
            IsFixedTimeStep = Settings.capFPS;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / Settings.FPS);
            Context.GraphicsManager.SynchronizeWithVerticalRetrace = Settings.vSync;
            Window.AllowUserResizing = Settings.ResizableWindow;
            Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
            Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;

            //Apply hardcoded settings.
            Window.AllowAltF4 = true;
            Window.Title = Settings.WName;

            //Add system events.
            ESystem.Add(new Listen(EType.GAME_SIZECHANGED, SystemEvents.RefreshScreenSettings));

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
            //Setup the brush for drawing, and the brush for texture setup.
            Context.ink = new SpriteBatch(GraphicsDevice);
            Context.preInk = new SpriteBatch(GraphicsDevice);

            //Setup the Screen adapter.
            Context.Screen = new ViewAdapter(Context.Core.Window, Context.Graphics, Settings.Width, Settings.Height);

            //Continue the start sequence.
            Starter.ContinueStart();

            //Load global resources.
            AssetManager.LoadGlobal();

            //Measure boot time.
            Starter.bootPerformance.Stop();
            Console.WriteLine(">>>> Engine loaded in: " + Starter.bootPerformance.ElapsedMilliseconds + "ms");

            //temp debug initialization here
            t = GameObject.GenericDrawObject;
            t.Component<Transform>().Size = new Vector2(10, 10);
            t.Component<Transform>().RotationDegree = 45;
            t.Component<Transform>().ObjectCenter();
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

            t.Update(); //temp debug
            t.Component<Transform>().Rotation += 0.01f; //temp

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
            Context.ink.Begin(transformMatrix: Context.Screen.GetScaleMatrix());
            Context.ink.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, Settings.Width, Settings.Height), Settings.FillColor);
            Context.ink.End();

            //Draw the current scene. (NYI)
            //Draw debug objects on top. (NYI)

            Context.ink.Start(Context.Screen.GetScaleMatrix());
            //Context.ink.Draw();
            t.Draw(); //temp debug
            Context.ink.End();
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
        /// </summary>
        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            ESystem.Add(new Event(EType.INPUT_TEXT, this, e.Character));
        }
        /// <summary>
        /// Triggered when the size of the window changes. If Settings.ResizableWindow is true
        /// this can be quite useful. Has a system event hooked to it that regenerates the window.
        /// </summary>
        private void Window_SizeChanged(object sender, EventArgs e)
        {
            ESystem.Add(new Event(EType.GAME_SIZECHANGED, this, null));
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

