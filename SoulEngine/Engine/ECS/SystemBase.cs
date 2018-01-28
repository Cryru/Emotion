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
        /// Updates a system link.
        /// </summary>
        /// <param name="link">The link to update the system for.</param>
        protected abstract void Update(Entity link);

        /// <summary>
        /// Draws a system link.
        /// </summary>
        /// <param name="link">The link to draw.</param>
        protected virtual void Draw(Entity link) {}

        /// <summary>
        /// Get the requirement for this system.
        /// </summary>
        /// <returns>The type components attached to an entity this system requires.</returns>
        protected internal abstract Type[] GetRequirements();

        /// <summary>
        /// The running priority of the system. The higher the later it will be run.
        /// </summary>
        public int Priority = 0;

        /// <summary>
        /// Whether the system draws.
        /// </summary>
        public bool Draws { get; protected set; } = false;

        #region Internals

        /// <summary>
        /// Updates all links.
        /// </summary>
        protected internal virtual void Run()
        {
            foreach (Entity link in Links)
            {
                Update(link);
            }
        }

        /// <summary>
        /// Draws all entities if the system is drawable.
        /// </summary>
        protected internal virtual void DrawCycle()
        {
            // Check if the system is drawable.
            if (!Draws) return;

            foreach (Entity link in Links)
            {
                Draw(link);
            }
        }

        #endregion
    }
}