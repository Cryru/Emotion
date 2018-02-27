// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Soul.Engine.Diagnostics;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Systems;
using Soul.Engine.Systems.UI;

#endregion

namespace Soul.Engine.Scenography
{
    /// <summary>
    /// A game scene.
    /// </summary>
    public abstract class Scene
    {
        #region Scene Objects

        /// <summary>
        /// Systems that are running.
        /// </summary>
        internal List<SystemBase> RunningSystems;

        /// <summary>
        /// Current scene entities.
        /// </summary>
        protected internal Dictionary<string, Entity> RegisteredEntities;

        #endregion

        protected Scene()
        {
            // Setup system manager.
            RunningSystems = new List<SystemBase>();
            RegisteredEntities = new Dictionary<string, Entity>();
        }

        /// <summary>
        /// Performs a setup of the scene.
        /// </summary>
        /// <param name="full">Whether to load all systems or a lite version.</param>
        internal void InternalSetup(bool full = true)
        {
            // Load core systems.
            AddSystem(new Renderer());

            if (full)
            {
                AddSystem(new Animator());
                AddSystem(new TextRenderer());
                AddSystem(new MouseEvents());
            }

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
            foreach (KeyValuePair<string, Entity> ent in RegisteredEntities)
            {
                ent.Value.Update();
            }
        }

        /// <summary>
        /// Draw the scene.
        /// </summary>
        internal void Draw()
        {
            // Run systems.
            foreach (KeyValuePair<string, Entity> ent in RegisteredEntities)
            {
                ent.Value.Draw();
            }
        }

        /// <summary>
        /// Is run every tick before systems.
        /// </summary>
        protected abstract void Update();

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
                Debugging.DebugMessage(DiagnosticMessageType.Error, "Duplicate entity registration!");

            Debugging.DebugMessage(DiagnosticMessageType.Scene,
                "Registered entity (" + RegisteredEntities.Count + ") [" + entity.Name + "] to scene " + this);
#endif
            // Set parent.
            entity.SceneParent = this;

            // Add to list.
            RegisteredEntities.Add(entity.Name, entity);

            // Order entities by priority.
            OrderEntities();

            // Update it.
            UpdateEntity(entity);
        }

        /// <summary>
        /// Ensures entity priority.
        /// </summary>
        public void OrderEntities()
        {
            // Reorder entities.
            RegisteredEntities = RegisteredEntities.OrderBy(x => x.Value.Priority).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Removes an entity from the scene.
        /// </summary>
        /// <param name="name">The name of the entity to remove.</param>
        public void RemoveEntity(string name)
        {
#if DEBUG
            Debugging.DebugMessage(DiagnosticMessageType.Scene, "Removed entity [" + name + "] from scene " + this);
#endif

            //TODO
        }

        #endregion

        #region System Related

        /// <summary>
        /// Adds a system and links all relevant entities to it.
        /// </summary>
        /// <param name="system">The system to add.</param>
        public void AddSystem(SystemBase system)
        {
            // Set the system's parent to this scene.
            system.Parent = this;

            // Add the system to the running systems.
            RunningSystems.Add(system);

            // Get requirements.
            Type[] requirements = system.GetRequirements();

            // Go through all entities and link them to the new system if they have the requirements or if it has no requirement.
            foreach (Entity ent in RegisteredEntities.Values)
            {
                // Check if the entity has the required components attached.
                if (requirements == null || !requirements.Except(ent.Components.Select(x => x.GetType())).Any())
                    ent.LinkToSystem(system);
            }

            // Run system setup code.
            system.Setup();

            // Order systems by priority.
            //RunningSystems = RunningSystems.OrderBy(x => x.Order).ToList();

#if DEBUG
            Debugging.DebugMessage(DiagnosticMessageType.Scene,
                "Registered system (" + RunningSystems.Count + ") [" + system + "] with priority " + system.Order + " to scene " + this);
#endif
        }

        /// <summary>
        /// Returns a running system of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of system to return, or the type default if no system of that type is running.</typeparam>
        public T GetSystem<T>()
        {
            foreach (SystemBase sys in RunningSystems)
            {
                if (sys is T) return (T)System.Convert.ChangeType(sys, typeof(T));
            }

            return default(T);
        }

        /// <summary>
        /// Removes and unlinks the specified system from the Scene.
        /// </summary>
        /// <param name="system"></param>
        public void RemoveSystem(SystemBase system)
        {
#if DEBUG
            Debugging.DebugMessage(DiagnosticMessageType.Scene, "Removed system [" + system + "] from scene " + this);
#endif

            //TODO
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
                ErrorHandling.Raise(DiagnosticMessageType.Scene, "Tried to update an entity which isn't attached with name " + entity.Name);
                return;
            }

            foreach (SystemBase sys in RunningSystems)
            {
                // Check if the system is already linked.
                if (entity.LinkedSystems.Contains(sys)) continue;

                // Get requirements.
                Type[] requirements = sys.GetRequirements();

                // If null requirements or the requirements for the system are met - add.
                if (requirements == null || !requirements.Except(entity.Components.Select(x => x.GetType())).Any())
                    entity.LinkToSystem(sys);
            }
        }

        #endregion
    }
}