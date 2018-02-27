// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine.ECS
{
    /// <summary>
    /// The base for a component object, to be attached to an entity.
    /// </summary>
    public class ComponentBase
    {
        /// <summary>
        /// The entity this component is attached to.
        /// </summary>
        public Entity Parent;
    }
}