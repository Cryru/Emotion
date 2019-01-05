// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;

#endregion

namespace Emotion.Engine.Scenography
{
    /// <summary>
    /// A single scene.
    /// </summary>
    public abstract class Scene
    {
        /// <summary>
        /// Whether focus loss was called.
        /// </summary>
        public bool FocusLossCalled { get; set; } = false;

        /// <summary>
        /// Is run when the scene is loading.
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
        /// Is run when the scene is unloading.
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// Is run when the host is unfocused.
        /// </summary>
        public virtual void FocusLoss()
        {
        }
    }
}