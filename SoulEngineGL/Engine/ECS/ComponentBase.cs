// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine.ECS
{
    /// <summary>
    /// The base for a component object, to be attached to an entity.
    /// </summary>
    public class ComponentBase
    {
        /// <summary>
        /// Whether the component has updated this frame.
        /// </summary>
        public bool HasUpdated;
    }
}