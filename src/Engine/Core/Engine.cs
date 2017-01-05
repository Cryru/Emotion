using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The game instance for use by the engine.
    /// </summary>
    public class Engine : Game
    {
        #region "Initialization"
        /// <summary>
        /// The initializer.
        /// </summary>
        public Engine()
        {
            //Setup the graphics device.
            Context.graphics = new GraphicsDeviceManager(this);

            //Setup the Content root folder for the master scene. The root for this folder is the exe.
            Content.RootDirectory = "Content";

            //Apply relevant settings.
            IsFixedTimeStep = Settings.capFPS;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / Settings.FPS);
            Context.graphics.SynchronizeWithVerticalRetrace = Settings.vSync;
            Window.AllowUserResizing = Settings.ResizableWindow;

            //Apply hardcoded settings.
            Window.AllowAltF4 = true;

            //TODO EVENT MANAGER
            //Attach event for when closing.
            //TODO
            Exiting += Engine_Exiting;
        }
        /// <summary>
        /// Setups the spritebatch and starts the start sequence.
        /// </summary>
        protected override void LoadContent()
        {
            //Setup the brush for drawing.
            Context.ink = new SpriteBatch(GraphicsDevice);

            //Refresh the screen settings.
            Functions.RefreshScreenSettings();

            //Load loading screen assets.
            LoadingLogo = Content.Load<Texture2D>("Engine/loadingLogo");

            //Continue the start sequence.
            Starter.ContinueStart();
        }
        #endregion


        //TODO BELOW THIS



        #region "Loops"
        /// <summary>
        /// Is executed every tick.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //Check if loading, in which case we want to run the loading cycle.
            if (Starter.Loading) { Loading_Update(gameTime); return; }

            //Check if a core has been loaded, and if not do nothing.
            if (Context.Core == null) return;
        
            //If the game is not focused, don't update.
            if (IsActive == false) return;

            //Run the physics engine.
            Physics.Engine.Update();
            //Run the core's frame update code.
            Core.Update(gameTime);
            //Run screen's update code.
            Core.UpdateScreens();
            ////Run the core's update ending code.
            Core.Update_End(gameTime);
        }

        /// <summary>
        /// Is executed every frame.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //Check if loading, in which case we want to run the loading cycle.
            if (Starter.Loading) { Loading_Draw(gameTime); return; }

            //Check if a core has been loaded, and if not do nothing.
            if (Context.Core == null) return;
          
            //Run the core's drawing code.
            Core.Draw(gameTime);
            //Run screen's draw code.
            Core.DrawScreens();
            //Run the core's drawing ending code.
            Core.Draw_End(gameTime);
        }
        #endregion

        #region "Loading Screen Loops"
        #region "Loading Screen Assets"
        Texture2D LoadingLogo;
        float LoadingLogoOpacity = 0.1f;
        bool LoadingLogoGlowFadeOut = false;
        int LoadingLogoFadeTimer = 0;
        #endregion
        int X = 0;
        int Y = 0;
        /// <summary>
        /// The update cycle of the loading screen.
        /// </summary>
        public void Loading_Update(GameTime gameTime)
        {
            LoadingLogoFadeTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (LoadingLogoFadeTimer < 150) return;

            LoadingLogoFadeTimer = 0;

            //Glow effect.
            if (LoadingLogoGlowFadeOut)
            {
                LoadingLogoOpacity -= 0.01f;

                if (LoadingLogoOpacity < 0.05f) LoadingLogoGlowFadeOut = false;
            }
            else
            {
                LoadingLogoOpacity += 0.01f;

                if (LoadingLogoOpacity > 0.1f) LoadingLogoGlowFadeOut = true;
            }
            
        }
        /// <summary>
        /// The draw cycle of the loading screen.
        /// </summary>
        public void Loading_Draw(GameTime gameTime)
        {
            Context.graphics.GraphicsDevice.Clear(new Color(56, 56, 56));
            Context.ink.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, RasterizerState.CullNone,
   null, Context.Screen.GetScaleMatrix());
            Context.ink.Draw(LoadingLogo, new Rectangle(0, 0, Settings.Width, Settings.Height), Color.White * LoadingLogoOpacity);
            Context.ink.End();
        }
        #endregion

        /// <summary>
        /// Is executed before the game is closed.
        /// </summary>
        private void Engine_Exiting(object sender, System.EventArgs e)
        {
            Core.onClosing.Trigger();
        }
    }
}

