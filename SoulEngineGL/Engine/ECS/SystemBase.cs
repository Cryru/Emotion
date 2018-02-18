// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using Soul.Engine.Scenography;

#endregion

namespace Soul.Engine.ECS
{
    /// <summary>
    /// The base for a system object, which operate on groups of entities.
    /// </summary>
    public abstract class SystemBase
    {
        /// <summary>
        /// The scene this system is running under.
        /// </summary>
        protected internal Scene Parent;

        /// <summary>
        /// Setup the system.
        /// </summary>
        protected internal abstract void Setup();

        /// <summary>
        /// Updates a system link.
        /// </summary>
        /// <param name="link">The link to update the system for.</param>
        internal abstract void Update(Entity link);

        /// <summary>
        /// Draws a system link.
        /// </summary>
        /// <param name="link">The link to draw.</param>
        internal virtual void Draw(Entity link) { }

        /// <summary>
        /// Get the requirement for this system.
        /// </summary>
        /// <returns>The type components attached to an entity this system requires.</returns>
        protected internal abstract Type[] GetRequirements();

        /// <summary>
        /// The running order of the system. The higher the later it will be run.
        /// </summary>
        public int Order = 0;
    }
}