// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Soul.Engine.ECS;

#endregion

namespace Soul.Engine
{
    /// <summary>
    /// A game scene.
    /// </summary>
    public abstract class Scene : Actor
    {
        /// <summary>
        /// Whether the scene has physics.
        /// </summary>
        public bool HasPhysics = false;
    }
}