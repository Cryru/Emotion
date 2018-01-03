// SoulEngine - https://github.com/Cryru/SoulEngine

using System;
using System.Collections.Generic;
using Soul.Engine.Scenography;

namespace Soul.Engine.ECS
{
    /// <summary>
    /// The base for a system object, which operate on groups of entities.
    /// </summary>
    public abstract class SystemBase
    {
        /// <summary>
        /// Components linked to this system.
        /// </summary>
        protected internal List<Entity> Links = new List<Entity>();

        /// <summary>
        /// The scene this system is running under.
        /// </summary>
        protected internal Scene Parent;

        /// <summary>
        /// Setup the system.
        /// </summary>
        protected internal abstract void Setup();

        /// <summary>
        /// Updates the system.
        /// </summary>
        /// <param name="link">The link to update the system for.</param>
        protected abstract void Update(Entity link);

        /// <summary>
        /// Get the requirement for this system.
        /// </summary>
        /// <returns>The type components attached to an entity this system requires.</returns>
        protected internal abstract Type[] GetRequirements();

        #region Pure Internals

        /// <summary>
        /// Updates all links.
        /// </summary>
        internal void Run()
        {
            foreach (Entity link in Links)
            {
                Update(link);
            }
        }

        #endregion

    }
}