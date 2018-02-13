// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Breath.Objects;
using Breath.Systems;
using Soul.Engine.ECS;
using Soul.Engine.Graphics.Text;

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

        /// <summary>
        /// Whether the text render needs to be rendered.
        /// </summary>
        public bool NeedRender { get; internal set; }

        /// <summary>
        /// A cached render of the text.
        /// </summary>
        public RenderTarget CachedRender;

        /// <summary>
        /// The font to use.
        /// </summary>
        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                HasUpdated = true;
                _font = value;
            }
        }

        private Font _font;
    }
}