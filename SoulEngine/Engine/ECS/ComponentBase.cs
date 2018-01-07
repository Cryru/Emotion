// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine.ECS
{
    /// <summary>
    /// The base for a component object, to be attached to an entity.
    /// </summary>
    public class ComponentBase
    {
        /// <summary>
        /// Whether the component has updated since the last time this was called.
        /// </summary>
        public bool HasUpdated
        {
            get
            {
                // If it hasn't been updated return false.
                if (!_hasUpdated) return false;
                // If it has then set the flag to false and return true.
                _hasUpdated = false;
                return true;
            }
        }

        protected bool _hasUpdated = true;
    }
}