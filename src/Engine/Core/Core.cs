using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using SoulEngine.Triggers;
using SoulEngine.Enums;

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
        /// The tickers running.
        /// </summary>
        public List<Ticker> Tickers = new List<Ticker>();
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

            //Apply relevant settings.
            IsFixedTimeStep = Settings.capFPS;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / Settings.FPS);
            Context.GraphicsManager.SynchronizeWithVerticalRetrace = Settings.vSync;
            Window.AllowUserResizing = Settings.ResizableWindow;

            //Apply hardcoded settings.
            Window.AllowAltF4 = true;
            Window.Title = Settings.WName;

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

            //Refresh the screen settings.
            Functions.RefreshScreenSettings();

            //Continue the start sequence.
            Starter.ContinueStart();

            //Load global resources.
            AssetManager.LoadGlobal();

            //Measure boot time.
            Starter.bootPerformance.Stop();
            Console.WriteLine(">>>> Engine loaded in: " + Starter.bootPerformance.ElapsedMilliseconds + "ms");
        }
        #endregion

        List<GameObject> a = new List<GameObject>();
        #region "Loops"
        /// <summary>
        /// Is executed every tick.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //If the game is not focused, don't update.
            if (IsActive == false) return;

            //Update event system.
            TriggerSystem.Update();

            //Cont here


            if (a.Count < 1) {
                GameObject testc = new GameObject();
                testc.AddComponent(new Renderer());
                testc.AddComponent(new ActiveTexture(AssetManager.MissingTexture, TextureMode.Tile));
                testc.AddComponent(new Transform(new Vector2(5, 5), new Vector2(100, 100)));
                testc.Component<Transform>().MoveTo(5000, new Vector3(500, 500, 0));
                a.Add(testc); }

            for (int i = 0; i < a.Count; i++)
            {
                a[i].Update();
            }
        }

        /// <summary>
        /// Is executed every frame.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //If the game is not focused, don't update.
            if (IsActive == false) return;

            //Trigger frame start trigger.
            TriggerSystem.Trigger(new Trigger("GAME_FRAMESTART", this, gameTime));

            //Record frametime.
            frameTime = gameTime.ElapsedGameTime.Milliseconds;

            //Update tickers.
            UpdateTickers();

            //Start drawing frame by first clearing the screen.
            Context.Graphics.Clear(Color.CornflowerBlue);

            //Cont here
            Context.ink.Begin();
            for (int i = 0; i < a.Count; i++)
            {
                a[i].Draw();
            }
            Context.ink.End();
        }
        #endregion

        #region "Functions"
        /// <summary>
        /// Updates tickers.
        /// </summary>
        private void UpdateTickers()
        {
            for (int i = Tickers.Count - 1; i >= 0; i--)
            {
                //Check if the timer has finished running.
                if (Tickers[i].State == Enums.TickerState.Done)
                {
                    //If finished, purge it.
                    Tickers.RemoveAt(i);
                }
                else
                {
                    //if not finished, update it.
                    Tickers[i].Update();
                }
            }
        }
        #endregion

        #region "Triggers"
        /// <summary>
        /// Is executed before the game is closed, used to trigger the internal event.
        /// </summary>
        private void Engine_Exiting(object sender, System.EventArgs e)
        {
            TriggerSystem.Trigger(new Trigger(TriggerType.GAME_CLOSED, this, null));
        }
        /// <summary>
        /// 
        /// </summary>
        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            TriggerSystem.Add(new Trigger(TriggerType.INPUT_TEXT, this, e.Character));
        }
        /// <summary>
        /// 
        /// </summary>
        private void Window_SizeChanged(object sender, EventArgs e)
        {
            TriggerSystem.Add(new Trigger(TriggerType.GAME_SIZECHANGED, this, null));
        }
        #endregion
    }
}

