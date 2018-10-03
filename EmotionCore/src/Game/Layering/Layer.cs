// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;

#endregion

namespace Emotion.Game.Layering
{
    public abstract class Layer
    {
        #region Properties

        /// <summary>
        /// The name of the layer. Is set when loaded.
        /// </summary>
        public string Name { get; internal set; } = "";

        /// <summary>
        /// The priority of the layer. Is set when loaded.
        /// </summary>
        public int Priority { get; internal set; }

        /// <summary>
        /// Whether to draw and update the layer.
        /// </summary>
        public bool Active { get; set; } = true;

        #endregion

        /// <summary>
        /// Whether to unload the layer on the next layer update.
        /// </summary>
        internal bool ToUnload = false;

        /// <summary>
        /// Is run when the layer is loading.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Is run every tick while the window is focused.
        /// </summary>
        /// <param name="frameTime">The time passed since the last update.</param>
        public abstract void Update(float frameTime);

        /// <summary>
        /// Is run every frame while the window is focused.
        /// </summary>
        /// <param name="renderer">The context's renderer.</param>
        public abstract void Draw(Renderer renderer);

        /// <summary>
        /// Is run when the layer is unloaded.
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// Is run when the window is not focused.
        /// </summary>
        /// <param name="frameTime">The time passed since the last update or light update.</param>
        public virtual void LightUpdate(float frameTime)
        {
        }
    }
}