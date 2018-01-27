// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Soul.Engine.ECS;

#endregion

namespace Soul.Engine.Graphics.Components
{
    /// <summary>
    /// Object containing data about rendering text.
    /// </summary>
    public class TextData : ComponentBase
    {
        /// <summary>
        /// The text to render.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                HasUpdated = true;
                _text = value;
            }
        }

        private string _text = "";

        /// <summary>
        /// The character size.
        /// </summary>
        public int Size
        {
            get { return _size; }
            set
            {
                HasUpdated = true;
                _size = value;
            }
        }

        private int _size = 10;
    }
}