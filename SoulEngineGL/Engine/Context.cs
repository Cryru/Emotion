using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Soul.Engine
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Context : Game
    {
        /// <summary>
        /// The graphics device manager for this context.
        /// </summary>
        public GraphicsDeviceManager GraphicsManager;

        /// <summary>
        /// The spritebatch used to draw.
        /// </summary>
        public SpriteBatch Ink;

        #region Callbacks

        /// <summary>
        /// The action to call after context creation is complete.
        /// </summary>
        private Action _onCreate;

        /// <summary>
        /// Called once per tick.
        /// </summary>
        private Action _onUpdate;

        /// <summary>
        /// Called once per frame.
        /// </summary>
        private Action _onDraw;


        #endregion

        /// <summary>
        /// Initialize a new Monogame context.
        /// </summary>
        /// <param name="onCreate">The function to call when creation has finished.</param>
        /// <param name="onUpdate">The function to call each tick.</param>
        /// <param name="onDraw">The function to call each frame.</param>
        public Context(Action onCreate, Action onUpdate, Action onDraw)
        {
            GraphicsManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _onCreate = onCreate;
            _onUpdate = onUpdate;
            _onDraw = onDraw;
        }

        /// <summary>
        /// Called once owhen the context has been created.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch from the graphics device.
            Ink = new SpriteBatch(GraphicsDevice);

            // Trigger the creation callback.
            _onCreate?.Invoke();
        }

        /// <summary>
        /// Called each tick.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            _onUpdate?.Invoke();

            base.Update(gameTime);
        }

        /// <summary>
        /// Called each frame.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            _onDraw?.Invoke();

            base.Draw(gameTime);
        }
    }
}
