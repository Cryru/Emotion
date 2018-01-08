// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Systems;
using Soul.Engine.Enums;
using Soul.Engine.Graphics;
using Soul.Engine.Graphics.Systems;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Scenography
{
    /// <summary>
    /// A game scene.
    /// </summary>
    public abstract class Scene
    {
        #region System Management

        /// <summary>
        /// Systems that are running.
        /// </summary>
        internal List<SystemBase> RunningSystems;

        /// <summary>
        /// Current scene entities.
        /// </summary>
        protected Dictionary<string, Entity> RegisteredEntities;

        /// <summary>
        /// The drawing hook for the scene.
        /// </summary>
        internal Action DrawHook;

        #endregion

        protected Scene()
        {
            // Setup system manager.
            RunningSystems = new List<SystemBase>();
            RegisteredEntities = new Dictionary<string, Entity>();
        }

        internal void InternalSetup()
        {
            // Load core systems.
            AddSystem(new RenderSystem());
            AddSystem(new ComponentUpdateCleanup());

            // Load user setup.
            Setup();
        }

        /// <summary>
        /// The scene initialization code.
        /// </summary>
        protected abstract void Setup();

        /// <summary>
        /// Is run every tick.
        /// </summary>
        internal void Run()
        {
            // Run update function.
            Update();

            // Run systems.
            foreach (SystemBase sys in RunningSystems)
            {
                sys.Run();
            }
        }

        /// <summary>
        /// Draw the scene.
        /// </summary>
        internal void Draw()
        {
            DrawHook?.Invoke();
        }

        /// <summary>
        /// Is run every tick before systems.
        /// </summary>
        protected virtual void Update()
        {
            
        }

        #region Entity Related

        /// <summary>
        /// Returns the attached entity under the provided name.
        /// </summary>
        /// <param name="name">The name of the entity to look for.</param>
        /// <returns>The entity whose name matches the provided one, or an error if such an entity hasn't been attached.</returns>
        public Entity GetEntity(string name)
        {
            return RegisteredEntities[name];
        }

        /// <summary>
        /// Adds an entity to the scene.
        /// </summary>
        /// <param name="entity">The entity object to add.</param>
        public void AddEntity(Entity entity)
        {
#if DEBUG
            if (RegisteredEntities.ContainsValue(entity))
            {
                Debugging.DebugMessage(DebugMessageType.Error, "Duplicate entity registration!");
            }

            Debugging.DebugMessage(DebugMessageType.InfoGreen, "Registered entity (" + RegisteredEntities.Count + ") [" + entity.Name + "]");
#endif
            // Set parent.
            entity.SceneParent = this;

            // Add to list.
            RegisteredEntities.Add(entity.Name, entity);

            // Update it.
            UpdateEntity(entity);
        }

        /// <summary>
        /// Removes an entity from the scene.
        /// </summary>
        /// <param name="name">The name of the entity to remove.</param>
        public void RemoveEntity(string name)
        {
#if DEBUG
            Debugging.DebugMessage(DebugMessageType.InfoGreen, "Removed entity [" + name + "]");
#endif
            RegisteredEntities.Remove(name);
        }

        #endregion

        #region System Related

        public void AddSystem(SystemBase system)
        {
#if DEBUG
            Debugging.DebugMessage(DebugMessageType.InfoBlue,
                "Registered system (" + RunningSystems.Count + ") [" + system + "]");
#endif

            // Set the system's parent to this scene.
            system.Parent = this;

            // Get requirements.
            Type[] requirements = system.GetRequirements();

            // If no requirements, skip.
            if (requirements != null)
            {
                foreach (Entity ent in RegisteredEntities.Values)
                {
                    // Check if the entity has the required components attached.
                    if (!requirements.Except(ent.Components.Select(x => x.GetType())).Any())
                        system.Links.Add(ent);
                }
            }
            else
            {
                // If null, add all.
                system.Links.AddRange(RegisteredEntities.Values);
            }

            // Add the system to the running systems.
            RunningSystems.Add(system);

            // Run system setup code.
            system.Setup();

            // Order systems by priority.
            RunningSystems = RunningSystems.OrderBy(x => x.Priority).ToList();
        }

        public void RemoveSystem(SystemBase system)
        {
#if DEBUG
            Debugging.DebugMessage(DebugMessageType.InfoBlue, "Removed system [" + system + "]");
#endif

            RunningSystems.Remove(system);
        }

        #endregion

        #region Internal

        /// <summary>
        /// Update the entity system links.
        /// </summary>
        /// <param name="entity">A reference to the attached entity to update.</param>
        internal void UpdateEntity(Entity entity)
        {
            // Check if the provided entity is attached to this scene at all.
            if (!RegisteredEntities.ContainsValue(entity))
            {
                ErrorHandling.Raise(ErrorOrigin.SceneLogic, "Tried to update an entity which isn't attached.");
                return;
            }

            // Go through all systems.
            foreach (SystemBase system in RunningSystems)
            {
                // If the system contains the entity, skip.
                if (system.Links.IndexOf(entity) != -1) continue;

                // Get requirements.
                Type[] requirements = system.GetRequirements();

                // If null requirements or the requirements for the system are met - add.
                if (requirements == null || !requirements.Except(entity.Components.Select(x => x.GetType())).Any())
                    system.Links.Add(entity);
            }
        }

        #endregion
    }
}