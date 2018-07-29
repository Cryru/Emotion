// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

namespace Emotion.Engine
{
    public abstract class ContextObject
    {
        /// <summary>
        /// The context the object belongs to.
        /// </summary>
        public Context Context;

        protected ContextObject()
        {
        }

        protected ContextObject(Context context)
        {
            Context = context;
        }
    }
}