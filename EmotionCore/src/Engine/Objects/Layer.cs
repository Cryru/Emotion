// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Engine.Objects
{
    public abstract class Layer
    {
        #region Properties

        /// <summary>
        /// Whether to draw and update the layer.
        /// </summary>
        public bool Active { get; set; } = true;

        #endregion

        internal string Name = "";
        internal int Priority = 0;
        internal bool ToUnload = false;
        internal bool Loaded = false;

        public abstract void Load();
        public abstract void Update();
        public abstract void Draw();
        public abstract void Unload();
    }
}