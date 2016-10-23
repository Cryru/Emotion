using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The game instance for use by the engine.
    /// </summary>
    public class Engine : Game
    {
        /// <summary>
        /// The initializer.
        /// </summary>
        public Engine()
        {
            //Setup the graphics device.
            Core.graphics = new GraphicsDeviceManager(this);        
            //Setup the Content root folder. The root for this folder is the exe.
            Content.RootDirectory = "Content";
        }
        /// <summary>
        /// Setups the spritebatch and starts the start sequence.
        /// </summary>
        protected override void LoadContent()
        {
            //Setup the brush for drawing.
            Core.ink = new SpriteBatch(GraphicsDevice);
            //Start the Core's start up sequence.
            Core.StartSequence();
        }

        #region "Loops"
        /// <summary>
        /// Is executed every frame on the CPU.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //Run the core's frame update code.
            Core.Update(gameTime);
            //Run screen's update code.
            Core.UpdateScreens();
            //Run the core's update ending code.
            Core.Update_End(gameTime);
        }
        /// <summary>
        /// Is executed every frame on the GPU.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //Run the core's drawing code.
            Core.Draw(gameTime);
            //Run screen's draw code.
            Core.DrawScreens();
            //Run the core's drawing ending code.
            Core.Draw_End(gameTime);
        }
        #endregion
    }
}

