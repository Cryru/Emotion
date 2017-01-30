using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects.Components;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The base for engine objects.
    /// </summary>
    public class GameObject
    {
        #region "Variables"
        #endregion

        #region "Components"
        /// <summary>
        /// The list of components attached to this object. Accessed through functions.
        /// </summary>
        private List<Component> Components = new List<Component>();
        #endregion

        /// <summary>
        /// Is run every tick.
        /// </summary>
        public virtual void Update()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update();
            }
        }

        /// <summary>
        /// Is run every frame.
        /// </summary>
        public virtual void Draw()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Draw();
            }
        }

        #region "Components Functions"
        /// <summary>
        /// Returns an attached component, will throw a NullReference if not attached.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The component object.</returns>
        public T Component<T>()
        {
            return (T)Convert.ChangeType(Components[IdComponent<T>()], typeof(T));
        }
        /// <summary>
        /// Returns an attached component, will throw a NullReference if not attached.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        /// <returns>The component object, needs to be cast.</returns>
        public object Component(string componentName)
        {
            return Components[IdComponent(componentName)];
        }

        /// <summary>
        /// Attaches a component to the object, if the object already has a component of this type it is overwritten.
        /// </summary>
        /// <param name="ComponentObject">The component object.</param>
        public void AddComponent(Component ComponentObject)
        {
            if (HasComponent(ComponentObject.GetType().Name))
            { Components[IdComponent(ComponentObject.GetType().Name)] = ComponentObject; ComponentObject.attachedObject = this; }
            else
            { Components.Add(ComponentObject); ComponentObject.attachedObject = this; }
        }

        /// <summary>
        /// Removes a component from the object, if such a component isn't attached nothing happens.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        public void RemoveComponent<T>()
        {
            RemoveComponent(typeof(T).Name);
        }
        /// <summary>
        /// Removes a component from the object, if such a component isn't attached nothing happens.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        public void RemoveComponent(string componentName)
        {
            int id = IdComponent(componentName);
            if (id != -1) { Components[id].Dispose(); Components.RemoveAt(id); }
        }

        /// <summary>
        /// Returns whether the specified component is attached to the object.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>True if attached, false if not.</returns>
        public bool HasComponent<T>()
        {
            return HasComponent(typeof(T).Name);
        }
        /// <summary>
        /// Returns whether the specified component is attached to the object.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        /// <returns>True if attached, false if not.</returns>
        public bool HasComponent(string componentName)
        {
            return IdComponent(componentName) != -1;
        }

        /// <summary>
        /// Returns the id of the component within the internal component list.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>The id of the component, -1 if not attached.</returns>
        private int IdComponent<T>()
        {
            return IdComponent(typeof(T).Name);
        }
        /// <summary>
        /// Returns the id of the component within the internal component list.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        /// <returns>The id of the component, -1 if not attached.</returns>
        private int IdComponent(string componentName)
        {
            //Note: This is the primary function other component functions rely on, optimization is to be done HERE.
            for (int i = 0; i < Components.Count; i++)
                if (Components[i].GetType().Name.ToLower() == componentName.ToLower())
                    return i;

            return -1;
        }
        #endregion
    }
}
