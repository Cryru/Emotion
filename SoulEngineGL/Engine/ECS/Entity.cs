// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Soul.Engine.Components;
using Soul.Engine.Scenography;

#if DEBUG

#endif

#endregion

namespace Soul.Engine.ECS
{
    /// <summary>
    /// An entity object, to be controlled by systems.
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// The name of the entity.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The priority of the entity. The larger it is the sooner it will be executed.
        /// </summary>
        public int Priority
        {
            get => _priority;
            set
            {
                _priority = value;

                // Reorder entities.
                SceneParent?.OrderEntities();
            }
        }

        private int _priority;

        #region Internals

        /// <summary>
        /// Components attached to this entity.
        /// </summary>
        internal List<ComponentBase> Components = new List<ComponentBase>();

        /// <summary>
        /// Systems this entity is linked to.
        /// </summary>
        internal SystemBase[] LinkedSystems = {};

        /// <summary>
        /// The parental scene of the entity.
        /// </summary>
        internal Scene SceneParent;

        #endregion

        public Entity(string name)
        {
            Name = name;
        }

        #region Component Control

        /// <summary>
        /// Attach a component to the entity of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to add.</typeparam>
        public void AttachComponent<T>()
        {
            // Instance the component.
            ComponentBase component = (ComponentBase) Activator.CreateInstance(typeof(T));

            // Attach the component.
            Components.Add(component);

            // Update the entity within the listing.
            SceneParent?.UpdateEntity(this);
        }

        /// <summary>
        /// Returns the number of components attached to the entity.
        /// </summary>
        /// <returns>The number of components attached to the entity.</returns>
        public int GetComponentCount()
        {
            return Components.Count;
        }

        /// <summary>
        /// Returns an attached component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to return.</typeparam>
        /// <returns>The child itself or the default type value if not found.</returns>
        public T GetComponent<T>()
        {
            // Loop through all components until we find one of the requested type.
            foreach (ComponentBase comp in Components)
            {
                if (comp is T) return (T)System.Convert.ChangeType(comp, typeof(T));
            }

            // If one wasn't found return default T.
            return default(T);
        }

        /// <summary>
        /// Returns an attached component of the specified id.
        /// </summary>
        /// <param name="id">The id of the component</param>
        /// <returns>The component or null if not found.</returns>
        public ComponentBase GetComponent(int id)
        {
            return id > Components.Count ? null : Components[id];
        }

        public void RemoveComponent(ComponentBase component)
        {
            // Remove the component from the entity list.
            Components.Remove(component);

            // Update the entity within the listing.
            SceneParent?.UpdateEntity(this);
        }

        #endregion

        #region System Linking

        public void LinkToSystem(SystemBase sys)
        {
            // Resize the array so the new system can fit in.
            Array.Resize(ref LinkedSystems, LinkedSystems.Length + 1);

            // Add the system.
            LinkedSystems[LinkedSystems.Length - 1] = sys;

            // Order.
            LinkedSystems = LinkedSystems.OrderBy(x => x.Order).ToArray();
        }

        public void UnlinkSystem(SystemBase sys)
        {
            // Remove the system from the linked systems.
            LinkedSystems = LinkedSystems.Where(x => x != sys).ToArray();
        }

        #endregion

        #region Loops

        /// <summary>
        /// Updates all linked systems with this entity.
        /// </summary>
        internal void Update()
        {
            foreach (SystemBase sys in LinkedSystems)
            {
                sys.Update(this);
            }
        }

        /// <summary>
        /// Draws the entity with the linked systems.
        /// </summary>
        internal void Draw()
        {
            foreach (SystemBase sys in LinkedSystems)
            {
                sys.Draw(this);
            }
        }

        #endregion

        /// <summary>
        /// Creates a basic drawable entity.
        /// </summary>
        /// <param name="name">The name of the entity to create.</param>
        /// <returns>An entity with a transform and render data with an applied rectangle.</returns>
        public static Entity CreateBasicDrawable(string name, Vector2 position, Vector2 size)
        {
            Entity temp = new Entity(name)
            {
                Position = position,
                Size = size
            };
            temp.AttachComponent<RenderData>();
            return temp;
        }

        /// <summary>
        /// Creates a basic text entity.
        /// </summary>
        /// <param name="name">The name of the entity to create.</param>
        /// <returns>An entity with a transform and render data and text data.</returns>
        public static Entity CreateBasicText(string name)
        {
            Entity temp = new Entity(name);
            //temp.AttachComponent<RenderData>();
            //temp.AttachComponent<TextData>();
            //temp.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            return temp;
        }
    }
}