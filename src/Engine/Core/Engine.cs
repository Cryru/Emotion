using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // The game instance for the engine.                                        //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    public class Engine : Game
    {
        public Engine()
        {
            //Setup the graphics device.
            Core.graphics = new GraphicsDeviceManager(this);
            
            //Setup the Content root folder. The root for this folder is the exe.
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent ()
        {
            //Setup the brush for drawing.
            Core.ink = new SpriteBatch(GraphicsDevice);
            //Start the Core's start up sequence.
            Core.StartSequence ();
        }

        #region "Main Loops"
        //Is executed every frame on the CPU.
        protected override void Update(GameTime gameTime)
        {
            //Run the core's frame update code.
            Core.Update(gameTime);
            //Run the master screen's update code.
            Core.master.Update(gameTime);
            //Run the screen's update code.
            Core.ScreenUpdate?.Invoke(gameTime);
            //Run the core's update ending code.
            Core.Update_End(gameTime);
        }
        //Is executed every frame on the GPU.
        protected override void Draw(GameTime gameTime)
        {
            //Run the core's drawing code.
            Core.Draw(gameTime);
            //Run the screen's draw code.
            Core.ScreenDraw?.Invoke(gameTime);
            //Run the master screen's draw code.
            Core.master.Draw(gameTime);
            //Run the core's drawing ending code.
            Core.Draw_End(gameTime);
        }
        #endregion
    }
}

