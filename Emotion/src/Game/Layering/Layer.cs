// Emotion - https://github.com/Cryru/Emotion

using Emotion.Engine;
using Emotion.GLES;

namespace Emotion.Game.Layering
{
    public abstract class Layer : ContextObject
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

        internal bool ToUnload = false;

        public abstract void Load();
        public abstract void Update(float frameTime);
        public abstract void Draw(Renderer renderer);
        public abstract void Unload();
    }
}